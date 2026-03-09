using AmlScreening.Application.Common;
using AmlScreening.Application.DTOs.SanctionLists;
using AmlScreening.Application.Interfaces;
using AmlScreening.Domain.Entities;
using AmlScreening.Infrastructure.Persistence;
using AmlScreening.Infrastructure.Services.SanctionListParsers;
using Microsoft.EntityFrameworkCore;

namespace AmlScreening.Infrastructure.Services;

public class SanctionListUploadService : ISanctionListUploadService
{
    public const string SourceUn = "United Nations Security Council";
    public const string SourceUae = "UAE Sanction List";
    public const string SourceOfac = "Office of Foreign Assets Control";

    private const int BatchSize = 2000;

    private readonly ApplicationDbContext _context;
    private readonly IUnConsolidatedListParser _unParser;
    private readonly IUaeSanctionListParser _uaeParser;

    public SanctionListUploadService(
        ApplicationDbContext context,
        IUnConsolidatedListParser unParser,
        IUaeSanctionListParser uaeParser)
    {
        _context = context;
        _unParser = unParser;
        _uaeParser = uaeParser;
    }

    public Task<ApiResponse<IReadOnlyList<SanctionListSourceDto>>> GetSourcesAsync(CancellationToken cancellationToken = default)
    {
        var sources = new List<SanctionListSourceDto>
        {
            new() { Id = SourceUn, Name = "United Nations Security Council Consolidated List", FileFormat = "XML" },
            new() { Id = SourceUae, Name = "UAE Sanction List", FileFormat = "XLS / XLSX" },
            new() { Id = SourceOfac, Name = "Office of Foreign Assets Control", FileFormat = "XLS / XLSX" }
        };
        return Task.FromResult(ApiResponse<IReadOnlyList<SanctionListSourceDto>>.Ok(sources));
    }

    public async Task<ApiResponse<SanctionListUploadResultDto>> UploadAsync(string listSourceId, Stream fileContent, string fileName, CancellationToken cancellationToken = default)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(listSourceId))
            return ApiResponse<SanctionListUploadResultDto>.Fail("List source is required.");

        var validIds = new[] { SourceUn, SourceUae, SourceOfac };
        if (!validIds.Contains(listSourceId))
            return ApiResponse<SanctionListUploadResultDto>.Fail("Invalid list source.");

        List<SanctionListEntry> entries;
        try
        {
            if (listSourceId == SourceUn)
            {
                if (!fileName.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
                    return ApiResponse<SanctionListUploadResultDto>.Fail("UN list must be an XML file.");
                entries = _unParser.Parse(fileContent, listSourceId).ToList();
            }
            else
            {
                if (!fileName.EndsWith(".xls", StringComparison.OrdinalIgnoreCase) && !fileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
                    return ApiResponse<SanctionListUploadResultDto>.Fail("UAE/OFAC list must be an Excel file (.xls or .xlsx).");
                entries = _uaeParser.Parse(fileContent, listSourceId, fileName).ToList();
            }
        }
        catch (Exception ex)
        {
            return ApiResponse<SanctionListUploadResultDto>.Fail($"Failed to parse file: {ex.Message}");
        }

        if (entries.Count == 0)
            return ApiResponse<SanctionListUploadResultDto>.Fail("No entries found in the file.");

        var existingCount = await _context.SanctionListEntries
            .Where(e => e.ListSource == listSourceId)
            .CountAsync(cancellationToken);

        await _context.SanctionListEntries
            .Where(e => e.ListSource == listSourceId)
            .ExecuteDeleteAsync(cancellationToken);

        for (int i = 0; i < entries.Count; i += BatchSize)
        {
            var batch = entries.Skip(i).Take(BatchSize).ToList();
            await _context.SanctionListEntries.AddRangeAsync(batch, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        return ApiResponse<SanctionListUploadResultDto>.Ok(new SanctionListUploadResultDto
        {
            ImportedCount = entries.Count,
            ReplacedCount = existingCount,
            Errors = errors
        });
    }

    public async Task<ApiResponse<PagedResult<SanctionListEntryDto>>> GetEntriesAsync(string? searchTerm, string? listSource, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _context.SanctionListEntries.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(listSource))
            query = query.Where(e => e.ListSource == listSource);

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.Trim().ToLower();
            query = query.Where(e =>
                (e.FullName != null && e.FullName.ToLower().Contains(term)) ||
                (e.Nationality != null && e.Nationality.ToLower().Contains(term)) ||
                (e.ReferenceNumber != null && e.ReferenceNumber.ToLower().Contains(term)));
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var page = Math.Max(1, pageNumber);
        var size = Math.Clamp(pageSize, 1, 100);
        var items = await query
            .OrderBy(e => e.ListSource)
            .ThenBy(e => e.FullName)
            .Skip((page - 1) * size)
            .Take(size)
            .Select(e => new SanctionListEntryDto
            {
                Id = e.Id,
                ListSource = e.ListSource,
                FullName = e.FullName,
                Nationality = e.Nationality,
                DateOfBirth = e.DateOfBirth,
                ReferenceNumber = e.ReferenceNumber,
                EntryType = e.EntryType,
                DataId = e.DataId,
                VersionNum = e.VersionNum,
                FirstName = e.FirstName,
                SecondName = e.SecondName,
                UnListType = e.UnListType,
                ListType = e.ListType,
                ListedOn = e.ListedOn,
                LastDayUpdated = e.LastDayUpdated,
                Gender = e.Gender,
                Designation = e.Designation,
                Comments = e.Comments,
                Aliases = e.Aliases,
                AddressCity = e.AddressCity,
                AddressCountry = e.AddressCountry,
                AddressNote = e.AddressNote,
                PlaceOfBirthCountry = e.PlaceOfBirthCountry,
                SortKey = e.SortKey,
                FullNameArabic = e.FullNameArabic,
                FamilyNameArabic = e.FamilyNameArabic,
                FamilyNameLatin = e.FamilyNameLatin,
                DocumentNumber = e.DocumentNumber,
                IssuingAuthority = e.IssuingAuthority,
                IssueDate = e.IssueDate,
                EndDate = e.EndDate,
                OtherInformation = e.OtherInformation,
                TypeDetail = e.TypeDetail
            })
            .ToListAsync(cancellationToken);

        var result = new PagedResult<SanctionListEntryDto>(items, totalCount, page, size);
        return ApiResponse<PagedResult<SanctionListEntryDto>>.Ok(result);
    }

    public async Task<ApiResponse<int>> DeleteByListSourceAsync(string listSource, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(listSource))
            return ApiResponse<int>.Fail("List source is required.");

        var validIds = new[] { SourceUn, SourceUae, SourceOfac };
        if (!validIds.Contains(listSource.Trim()))
            return ApiResponse<int>.Fail("Invalid list source.");

        var count = await _context.SanctionListEntries
            .Where(e => e.ListSource == listSource.Trim())
            .ExecuteDeleteAsync(cancellationToken);
        return ApiResponse<int>.Ok(count);
    }

    public async Task<ApiResponse<SanctionListEntryDto>> CreateEntryAsync(CreateSanctionListEntryDto dto, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(dto.ListSource))
            return ApiResponse<SanctionListEntryDto>.Fail("List source is required.");
        if (string.IsNullOrWhiteSpace(dto.FullName))
            return ApiResponse<SanctionListEntryDto>.Fail("Full name is required.");

        var validIds = new[] { SourceUn, SourceUae, SourceOfac };
        if (!validIds.Contains(dto.ListSource.Trim()))
            return ApiResponse<SanctionListEntryDto>.Fail("Invalid list source.");

        var entry = new SanctionListEntry
        {
            Id = Guid.NewGuid(),
            ListSource = dto.ListSource.Trim(),
            FullName = Trunc(dto.FullName.Trim(), 512),
            Nationality = Trunc(dto.Nationality?.Trim(), 128),
            DateOfBirth = dto.DateOfBirth,
            ReferenceNumber = Trunc(dto.ReferenceNumber?.Trim(), 128),
            EntryType = Trunc(dto.EntryType?.Trim(), 32),
            FirstName = Trunc(dto.FirstName?.Trim(), 256),
            SecondName = Trunc(dto.SecondName?.Trim(), 256),
            Gender = Trunc(dto.Gender?.Trim(), 32),
            Designation = Trunc(dto.Designation?.Trim(), 256),
            Comments = Trunc(dto.Comments?.Trim(), 2000),
            Aliases = Trunc(dto.Aliases?.Trim(), 1000),
            AddressCity = Trunc(dto.AddressCity?.Trim(), 128),
            AddressCountry = Trunc(dto.AddressCountry?.Trim(), 128),
            AddressNote = Trunc(dto.AddressNote?.Trim(), 512),
            PlaceOfBirthCountry = Trunc(dto.PlaceOfBirthCountry?.Trim(), 128),
            FullNameArabic = Trunc(dto.FullNameArabic?.Trim(), 512),
            FamilyNameArabic = Trunc(dto.FamilyNameArabic?.Trim(), 256),
            FamilyNameLatin = Trunc(dto.FamilyNameLatin?.Trim(), 256),
            DocumentNumber = Trunc(dto.DocumentNumber?.Trim(), 128),
            IssuingAuthority = Trunc(dto.IssuingAuthority?.Trim(), 256),
            IssueDate = dto.IssueDate,
            EndDate = dto.EndDate,
            OtherInformation = Trunc(dto.OtherInformation?.Trim(), 2000),
            TypeDetail = Trunc(dto.TypeDetail?.Trim(), 64)
        };
        _context.SanctionListEntries.Add(entry);
        await _context.SaveChangesAsync(cancellationToken);

        return ApiResponse<SanctionListEntryDto>.Ok(MapToDto(entry));
    }

    private static string? Trunc(string? value, int maxLen)
    {
        if (value == null || value.Length <= maxLen) return value;
        return value[..maxLen];
    }

    private static SanctionListEntryDto MapToDto(SanctionListEntry e)
    {
        return new SanctionListEntryDto
        {
            Id = e.Id,
            ListSource = e.ListSource,
            FullName = e.FullName,
            Nationality = e.Nationality,
            DateOfBirth = e.DateOfBirth,
            ReferenceNumber = e.ReferenceNumber,
            EntryType = e.EntryType,
            DataId = e.DataId,
            VersionNum = e.VersionNum,
            FirstName = e.FirstName,
            SecondName = e.SecondName,
            UnListType = e.UnListType,
            ListType = e.ListType,
            ListedOn = e.ListedOn,
            LastDayUpdated = e.LastDayUpdated,
            Gender = e.Gender,
            Designation = e.Designation,
            Comments = e.Comments,
            Aliases = e.Aliases,
            AddressCity = e.AddressCity,
            AddressCountry = e.AddressCountry,
            AddressNote = e.AddressNote,
            PlaceOfBirthCountry = e.PlaceOfBirthCountry,
            SortKey = e.SortKey,
            FullNameArabic = e.FullNameArabic,
            FamilyNameArabic = e.FamilyNameArabic,
            FamilyNameLatin = e.FamilyNameLatin,
            DocumentNumber = e.DocumentNumber,
            IssuingAuthority = e.IssuingAuthority,
            IssueDate = e.IssueDate,
            EndDate = e.EndDate,
            OtherInformation = e.OtherInformation,
            TypeDetail = e.TypeDetail
        };
    }
}
