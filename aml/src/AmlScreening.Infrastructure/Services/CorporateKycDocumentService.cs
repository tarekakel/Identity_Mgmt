using System.IO;
using AmlScreening.Application.Common;
using AmlScreening.Application.DTOs.CorporateKyc;
using AmlScreening.Application.Interfaces;
using AmlScreening.Domain.Entities;
using AmlScreening.Infrastructure.Options;
using AmlScreening.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace AmlScreening.Infrastructure.Services;

public class CorporateKycDocumentService : ICorporateKycDocumentService
{
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase) { ".pdf", ".jpg", ".jpeg", ".png" };

    private readonly ApplicationDbContext _context;
    private readonly IFileStorageService _fileStorage;
    private readonly ICurrentUserService _currentUser;
    private readonly long _maxFileSizeBytes;

    public CorporateKycDocumentService(
        ApplicationDbContext context,
        IFileStorageService fileStorage,
        ICurrentUserService currentUser,
        IOptions<FileStorageOptions> options)
    {
        _context = context;
        _fileStorage = fileStorage;
        _currentUser = currentUser;
        _maxFileSizeBytes = options.Value.MaxFileSizeBytes > 0 ? options.Value.MaxFileSizeBytes : 10 * 1024 * 1024;
    }

    public async Task<ApiResponse<IReadOnlyList<CorporateKycDocumentDto>>> GetByCustomerIdAsync(
        Guid customerId,
        CancellationToken cancellationToken = default)
    {
        var activeKyc = await _context.CorporateKyc
            .AsNoTracking()
            .Where(k => k.CustomerId == customerId && k.IsActive)
            .OrderByDescending(k => k.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (activeKyc == null)
            return ApiResponse<IReadOnlyList<CorporateKycDocumentDto>>.Ok(Array.Empty<CorporateKycDocumentDto>());

        var list = await _context.CorporateKycDocuments
            .AsNoTracking()
            .Where(d => d.CorporateKycId == activeKyc.Id)
            .OrderByDescending(d => d.UploadedDate)
            .ToListAsync(cancellationToken);

        var dtos = list.Select(MapToDto).ToList();
        return ApiResponse<IReadOnlyList<CorporateKycDocumentDto>>.Ok(dtos);
    }

    public async Task<ApiResponse<CorporateKycDocumentDto>> UploadAsync(
        Guid customerId,
        UploadCorporateKycDocumentRequestDto dto,
        Stream fileContent,
        string fileName,
        string? contentType,
        CancellationToken cancellationToken = default)
    {
        var ext = Path.GetExtension(fileName);
        if (string.IsNullOrEmpty(ext) || !AllowedExtensions.Contains(ext))
            return ApiResponse<CorporateKycDocumentDto>.Fail("Only PDF, JPG, and PNG are allowed.");

        if (fileContent.CanSeek && fileContent.Length > _maxFileSizeBytes)
            return ApiResponse<CorporateKycDocumentDto>.Fail("File size must be less than 10MB.");

        var activeKyc = await _context.CorporateKyc
            .FirstOrDefaultAsync(k => k.CustomerId == customerId && k.IsActive, cancellationToken);

        if (activeKyc == null)
            return ApiResponse<CorporateKycDocumentDto>.Fail("Corporate KYC not found.");

        string relativePath;
        try
        {
            relativePath = await _fileStorage.SaveAsync(
                fileContent,
                fileName,
                contentType ?? "application/octet-stream",
                customerId.ToString("N"),
                cancellationToken);
        }
        catch (ArgumentException ex)
        {
            return ApiResponse<CorporateKycDocumentDto>.Fail(ex.Message);
        }

        var now = DateTime.UtcNow;
        var doc = new CorporateKycDocument
        {
            Id = Guid.NewGuid(),
            CorporateKycId = activeKyc.Id,
            CustomerId = customerId,

            DocumentNo = dto.DocumentNo?.Trim(),
            IssuedDate = dto.IssuedDate,
            ExpiryDate = dto.ExpiryDate,
            ApprovedBy = dto.ApprovedBy?.Trim(),
            FolderPath = dto.FolderPath?.Trim(),

            FileName = fileName,
            FilePath = relativePath,
            UploadedBy = _currentUser.GetCurrentUserDisplayName(),
            UploadedDate = now
        };

        _context.CorporateKycDocuments.Add(doc);
        await _context.SaveChangesAsync(cancellationToken);

        return ApiResponse<CorporateKycDocumentDto>.Ok(MapToDto(doc));
    }

    public async Task<ApiResponse<(Stream Content, string FileName, string ContentType)>> GetDownloadAsync(
        Guid customerId,
        Guid documentId,
        CancellationToken cancellationToken = default)
    {
        var doc = await _context.CorporateKycDocuments
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == documentId && d.CustomerId == customerId, cancellationToken);

        if (doc == null)
            return ApiResponse<(Stream, string, string)>.Fail("Document not found.");

        var stream = await _fileStorage.GetAsync(doc.FilePath, cancellationToken);
        if (stream == null)
            return ApiResponse<(Stream, string, string)>.Fail("File not found on storage.");

        var contentType = _fileStorage.GetContentTypeFromFileName(doc.FileName);
        return ApiResponse<(Stream, string, string)>.Ok((stream, doc.FileName, contentType));
    }

    public async Task<ApiResponse> DeleteAsync(Guid customerId, Guid documentId, CancellationToken cancellationToken = default)
    {
        var doc = await _context.CorporateKycDocuments
            .FirstOrDefaultAsync(d => d.Id == documentId && d.CustomerId == customerId, cancellationToken);

        if (doc == null)
            return ApiResponse.Fail("Document not found.");

        doc.IsDeleted = true;
        doc.IsActive = false;

        await _context.SaveChangesAsync(cancellationToken);
        return ApiResponse.Ok();
    }

    private static CorporateKycDocumentDto MapToDto(CorporateKycDocument d) => new()
    {
        Id = d.Id,
        CorporateKycId = d.CorporateKycId,
        CustomerId = d.CustomerId,

        DocumentNo = d.DocumentNo,
        IssuedDate = d.IssuedDate,
        ExpiryDate = d.ExpiryDate,
        ApprovedBy = d.ApprovedBy,
        FolderPath = d.FolderPath,

        FileName = d.FileName,
        UploadedDate = d.UploadedDate,
        UploadedBy = d.UploadedBy
    };
}
