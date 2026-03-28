using System.Globalization;
using AmlScreening.Application.Common;
using AmlScreening.Application.DTOs.CorporateBulkUpload;
using AmlScreening.Application.Interfaces;
using AmlScreening.Domain.Entities;
using AmlScreening.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using NPOI.XSSF.UserModel;

namespace AmlScreening.Infrastructure.Services;

public class CorporateBulkUploadService : ICorporateBulkUploadService
{
    public static readonly Guid DefaultTenantId = Guid.Parse("00000000-0000-0000-0000-000000000001");

    private readonly ApplicationDbContext _db;

    public CorporateBulkUploadService(ApplicationDbContext db)
    {
        _db = db;
    }

    public byte[] GetSampleWorkbookBytes()
    {
        var wb = new XSSFWorkbook();
        var sheet = wb.CreateSheet("Sheet1");
        var row0 = sheet.CreateRow(0);
        var headers = new[]
        {
            "Customer ID (Mandatory field)",
            "Full Name of the Entity (Mandatory field)",
            "Incorporated Country",
            "Date of Incorporation (MM/DD/YYYY or YYYY-MM-DD)",
            "Company Reference Code",
            "Trade License"
        };
        for (var i = 0; i < headers.Length; i++)
            row0.CreateCell(i).SetCellValue(headers[i]);

        var row1 = sheet.CreateRow(1);
        row1.CreateCell(0).SetCellValue("CUST001");
        row1.CreateCell(1).SetCellValue("Sample Entity LLC");
        row1.CreateCell(2).SetCellValue("AE");
        row1.CreateCell(3).SetCellValue("1/15/2020");

        using var ms = new MemoryStream();
        wb.Write(ms, true);
        wb.Close();
        return ms.ToArray();
    }

    public async Task<ApiResponse<CorporateBulkUploadResultDto>> UploadAsync(
        Stream fileStream,
        string fileName,
        CorporateBulkUploadOptionsDto options,
        CancellationToken cancellationToken = default)
    {
        if (fileStream == null || string.IsNullOrWhiteSpace(fileName))
            return ApiResponse<CorporateBulkUploadResultDto>.Fail("File is required.");

        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        if (ext != ".csv" && ext != ".xlsx" && ext != ".xls")
            return ApiResponse<CorporateBulkUploadResultDto>.Fail("Only .csv, .xlsx, and .xls files are allowed.");

        IReadOnlyList<CorporateBulkUploadFileParser.RawRow> rawRows;
        try
        {
            rawRows = CorporateBulkUploadFileParser.Parse(fileStream, fileName);
        }
        catch (Exception ex)
        {
            return ApiResponse<CorporateBulkUploadResultDto>.Fail(ex.Message);
        }

        var countries = await _db.Countries.AsNoTracking().ToListAsync(cancellationToken);

        var batch = new CorporateBulkUploadBatch
        {
            Id = Guid.NewGuid(),
            TenantId = DefaultTenantId,
            OriginalFileName = Path.GetFileName(fileName),
            MatchThreshold = Math.Clamp(options.MatchThreshold, 75, 100),
            CheckPepUkOnly = options.CheckPepUkOnly,
            CheckDisqualifiedDirectorUkOnly = options.CheckDisqualifiedDirectorUkOnly,
            CheckSanctions = options.CheckSanctions,
            CheckProfileOfInterest = options.CheckProfileOfInterest,
            CheckReputationalRiskExposure = options.CheckReputationalRiskExposure,
            CheckRegulatoryEnforcementList = options.CheckRegulatoryEnforcementList,
            CheckInsolvencyUkIreland = options.CheckInsolvencyUkIreland,
            ScreeningFinished = false
        };

        var lines = new List<CorporateBulkUploadLine>();
        var reportRows = new List<CorporateBulkUploadReportRowDto>();

        foreach (var raw in rawRows)
        {
            var errs = new List<string>();
            if (string.IsNullOrWhiteSpace(raw.CustomerId))
                errs.Add("Customer ID is required.");
            if (string.IsNullOrWhiteSpace(raw.FullName))
                errs.Add("Full name of the entity is required.");

            string? resolvedCode = null;
            if (!string.IsNullOrWhiteSpace(raw.IncorporatedCountry))
            {
                if (!TryResolveCountry(raw.IncorporatedCountry, countries, out resolvedCode, out var cErr))
                    errs.Add(cErr);
            }

            DateTime? doiParsed = null;
            var doiDisplay = raw.DateOfIncorporation ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(raw.DateOfIncorporation))
            {
                if (!TryParseDate(raw.DateOfIncorporation, out doiParsed, out var disp))
                    errs.Add("Invalid date of incorporation. Use MM/DD/YYYY or YYYY-MM-DD.");
                else
                    doiDisplay = disp;
            }

            var errMsg = errs.Count > 0 ? string.Join(" ", errs) : null;
            var queued = errMsg == null;

            var line = new CorporateBulkUploadLine
            {
                Id = Guid.NewGuid(),
                BatchId = batch.Id,
                LineIndex = raw.LineIndex,
                CustomerId = raw.CustomerId.Trim(),
                FullName = raw.FullName.Trim(),
                IncorporatedCountry = raw.IncorporatedCountry.Trim(),
                DateOfIncorporationRaw = raw.DateOfIncorporation ?? string.Empty,
                DateOfIncorporationParsed = doiParsed,
                CompanyReferenceCode = raw.CompanyReferenceCode ?? string.Empty,
                TradeLicense = raw.TradeLicense ?? string.Empty,
                IncorporatedCountryResolvedCode = resolvedCode,
                ErrorMessage = errMsg,
                QueuedForScreening = queued
            };
            lines.Add(line);

            reportRows.Add(new CorporateBulkUploadReportRowDto
            {
                CustomerId = line.CustomerId,
                EntityName = line.FullName,
                IncorporatedCountry = resolvedCode ?? raw.IncorporatedCountry.Trim(),
                DateOfIncorporation = doiDisplay,
                CompanyReferenceCode = line.CompanyReferenceCode,
                TradeLicense = line.TradeLicense,
                Error = errMsg ?? string.Empty
            });
        }

        batch.TotalRowCount = lines.Count;
        batch.FailedRowCount = lines.Count(l => l.ErrorMessage != null);
        batch.QueuedRowCount = lines.Count(l => l.QueuedForScreening);
        batch.Lines = lines;

        _db.CorporateBulkUploadBatches.Add(batch);
        await _db.SaveChangesAsync(cancellationToken);

        var mode = batch.FailedRowCount > 0 ? "validationFailed" : "queued";
        var dto = new CorporateBulkUploadResultDto
        {
            BatchId = batch.Id,
            Mode = mode,
            Rows = reportRows
        };

        return ApiResponse<CorporateBulkUploadResultDto>.Ok(dto);
    }

    public async Task<ApiResponse<IReadOnlyList<CorporateBulkUploadBatchListItemDto>>> GetBatchesAsync(
        DateTime? fromUtc,
        DateTime? toUtc,
        string? uploadedBy,
        CancellationToken cancellationToken = default)
    {
        var q = _db.CorporateBulkUploadBatches.AsNoTracking().Where(b => b.TenantId == DefaultTenantId);
        if (fromUtc.HasValue)
            q = q.Where(b => b.CreatedAt >= fromUtc.Value);
        if (toUtc.HasValue)
            q = q.Where(b => b.CreatedAt <= toUtc.Value);
        if (!string.IsNullOrWhiteSpace(uploadedBy))
            q = q.Where(b => b.CreatedBy != null && b.CreatedBy.Contains(uploadedBy));

        var list = await q
            .OrderByDescending(b => b.CreatedAt)
            .Select(b => new CorporateBulkUploadBatchListItemDto
            {
                Id = b.Id,
                FileName = b.OriginalFileName,
                UploadedOn = b.CreatedAt,
                UploadedBy = b.CreatedBy ?? "",
                ScreeningFinished = b.ScreeningFinished
            })
            .ToListAsync(cancellationToken);

        return ApiResponse<IReadOnlyList<CorporateBulkUploadBatchListItemDto>>.Ok(list);
    }

    public async Task<ApiResponse<IReadOnlyList<CorporateBulkUploadLineDetailDto>>> GetBatchLinesAsync(
        Guid batchId,
        string? caseStatus,
        CancellationToken cancellationToken = default)
    {
        var exists = await _db.CorporateBulkUploadBatches.AnyAsync(b => b.Id == batchId, cancellationToken);
        if (!exists)
            return ApiResponse<IReadOnlyList<CorporateBulkUploadLineDetailDto>>.Fail("Batch not found.");

        var q = _db.CorporateBulkUploadLines.AsNoTracking().Where(l => l.BatchId == batchId);
        var lines = await q.OrderBy(l => l.LineIndex).ToListAsync(cancellationToken);

        var result = new List<CorporateBulkUploadLineDetailDto>();
        foreach (var l in lines)
        {
            var status = l.ErrorMessage != null ? "Failed" : (l.QueuedForScreening ? "Queued" : "Pending");
            if (!string.IsNullOrWhiteSpace(caseStatus) && !caseStatus.Equals("all", StringComparison.OrdinalIgnoreCase))
            {
                var want = caseStatus.Trim();
                if (want.Equals("pending", StringComparison.OrdinalIgnoreCase))
                    want = "Queued";
                if (want.Equals("cleared", StringComparison.OrdinalIgnoreCase))
                    continue;
                if (!status.Equals(want, StringComparison.OrdinalIgnoreCase))
                    continue;
            }

            var idx = result.Count + 1;
            result.Add(new CorporateBulkUploadLineDetailDto
            {
                Index = idx,
                CustomerId = l.CustomerId,
                EntityName = l.FullName,
                IncorporatedCountry = l.IncorporatedCountryResolvedCode ?? l.IncorporatedCountry,
                DateOfIncorporation = l.DateOfIncorporationParsed?.ToString("yyyy-MM-dd") ?? l.DateOfIncorporationRaw,
                CompanyReferenceCode = l.CompanyReferenceCode,
                TradeLicense = l.TradeLicense,
                Status = status,
                Source = "Bulk",
                Uid = l.Id.ToString(),
                Date = l.CreatedAt.ToString("yyyy-MM-dd")
            });
        }

        return ApiResponse<IReadOnlyList<CorporateBulkUploadLineDetailDto>>.Ok(result);
    }

    private static bool TryResolveCountry(string input, List<Country> countries, out string? code, out string error)
    {
        code = null;
        var t = input.Trim();
        var byCode = countries.FirstOrDefault(c => c.Code.Equals(t, StringComparison.OrdinalIgnoreCase));
        if (byCode != null)
        {
            code = byCode.Code;
            error = "";
            return true;
        }

        var byName = countries.FirstOrDefault(c => c.Name.Equals(t, StringComparison.OrdinalIgnoreCase));
        if (byName != null)
        {
            code = byName.Code;
            error = "";
            return true;
        }

        error = $"Invalid country: '{t}'";
        return false;
    }

    private static bool TryParseDate(string raw, out DateTime? parsed, out string display)
    {
        parsed = null;
        display = raw.Trim();
        var formats = new[] { "M/d/yyyy", "MM/dd/yyyy", "yyyy-MM-dd", "d/M/yyyy", "dd/MM/yyyy", "M/d/yy" };
        foreach (var f in formats)
        {
            if (DateTime.TryParseExact(raw.Trim(), f, CultureInfo.InvariantCulture, DateTimeStyles.None, out var p))
            {
                parsed = p;
                display = p.ToString("M/d/yyyy", CultureInfo.InvariantCulture);
                return true;
            }
        }

        if (DateTime.TryParse(raw, CultureInfo.InvariantCulture, DateTimeStyles.None, out var p2))
        {
            parsed = p2;
            display = p2.ToString("M/d/yyyy", CultureInfo.InvariantCulture);
            return true;
        }

        return false;
    }
}
