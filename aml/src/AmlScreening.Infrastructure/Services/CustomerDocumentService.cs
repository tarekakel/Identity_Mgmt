using AmlScreening.Application.Common;
using AmlScreening.Application.DTOs.Customers;
using AmlScreening.Application.Interfaces;
using AmlScreening.Domain.Entities;
using AmlScreening.Infrastructure.Options;
using AmlScreening.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace AmlScreening.Infrastructure.Services;

public class CustomerDocumentService : ICustomerDocumentService
{
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase) { ".pdf", ".jpg", ".jpeg", ".png" };
    private const string PassportCode = "Passport";

    private readonly ApplicationDbContext _context;
    private readonly IFileStorageService _fileStorage;
    private readonly ICurrentUserService _currentUser;
    private readonly long _maxFileSizeBytes;

    public CustomerDocumentService(
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

    public async Task<ApiResponse<CustomerDocumentDto>> UploadAsync(Guid customerId, string documentTypeCode, Stream fileContent, string fileName, string? contentType, DateTime? expiryDate, CancellationToken cancellationToken = default)
    {
        var ext = Path.GetExtension(fileName);
        if (string.IsNullOrEmpty(ext) || !AllowedExtensions.Contains(ext))
            return ApiResponse<CustomerDocumentDto>.Fail("Only PDF, JPG, and PNG are allowed.");

        if (fileContent.CanSeek && fileContent.Length > _maxFileSizeBytes)
            return ApiResponse<CustomerDocumentDto>.Fail("File size must be less than 10MB.");

        var customer = await _context.Customers
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == customerId, cancellationToken);
        if (customer == null)
            return ApiResponse<CustomerDocumentDto>.Fail("Customer not found.");

        var docType = await _context.DocumentTypes
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Code == documentTypeCode, cancellationToken);
        if (docType == null)
            return ApiResponse<CustomerDocumentDto>.Fail("Invalid document type.");

        if (string.Equals(docType.Code, PassportCode, StringComparison.OrdinalIgnoreCase))
        {
            var effectiveExpiry = expiryDate ?? customer.PassportExpiryDate;
            if (!effectiveExpiry.HasValue)
                return ApiResponse<CustomerDocumentDto>.Fail("Passport expiry date is required.");
            if (effectiveExpiry.Value.Date < DateTime.UtcNow.Date)
                return ApiResponse<CustomerDocumentDto>.Fail("Passport must not be expired.");
            expiryDate = effectiveExpiry;
        }

        string relativePath;
        try
        {
            relativePath = await _fileStorage.SaveAsync(fileContent, fileName, contentType ?? "application/octet-stream", customerId.ToString("N"), cancellationToken);
        }
        catch (ArgumentException ex)
        {
            return ApiResponse<CustomerDocumentDto>.Fail(ex.Message);
        }

        var now = DateTime.UtcNow;
        var doc = new CustomerDocument
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            DocumentTypeId = docType.Id,
            FileName = fileName,
            FilePath = relativePath,
            UploadedBy = _currentUser.GetCurrentUserDisplayName(),
            UploadedDate = now,
            ExpiryDate = expiryDate,
            CreatedAt = now,
            CreatedBy = _currentUser.GetCurrentUserDisplayName()
        };
        _context.CustomerDocuments.Add(doc);
        await _context.SaveChangesAsync(cancellationToken);

        var dto = MapToDto(doc, docType.Code, docType.Name);
        return ApiResponse<CustomerDocumentDto>.Ok(dto);
    }

    public async Task<ApiResponse<IReadOnlyList<CustomerDocumentDto>>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        var list = await _context.CustomerDocuments
            .AsNoTracking()
            .Include(d => d.DocumentType)
            .Where(d => d.CustomerId == customerId)
            .OrderByDescending(d => d.UploadedDate)
            .ToListAsync(cancellationToken);
        var dtos = list.Select(d => MapToDto(d, d.DocumentType.Code, d.DocumentType.Name)).ToList();
        return ApiResponse<IReadOnlyList<CustomerDocumentDto>>.Ok(dtos);
    }

    public async Task<ApiResponse<(Stream Content, string FileName, string ContentType)>> GetDownloadAsync(Guid customerId, Guid documentId, CancellationToken cancellationToken = default)
    {
        var doc = await _context.CustomerDocuments
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

    private static CustomerDocumentDto MapToDto(CustomerDocument d, string docTypeCode, string docTypeName) => new()
    {
        Id = d.Id,
        CustomerId = d.CustomerId,
        DocumentTypeId = d.DocumentTypeId,
        DocumentTypeCode = docTypeCode,
        DocumentTypeName = docTypeName,
        FileName = d.FileName,
        UploadedBy = d.UploadedBy,
        UploadedDate = d.UploadedDate,
        ExpiryDate = d.ExpiryDate
    };
}
