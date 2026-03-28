using System.Globalization;
using AmlScreening.Application.Common;
using AmlScreening.Application.DTOs.IndividualBulkUpload;
using AmlScreening.Application.Interfaces;
using AmlScreening.Domain.Entities;
using AmlScreening.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using NPOI.XSSF.UserModel;

namespace AmlScreening.Infrastructure.Services;

public class IndividualBulkUploadService : IIndividualBulkUploadService
{
    public static readonly Guid DefaultTenantId = Guid.Parse("00000000-0000-0000-0000-000000000001");

    private readonly ApplicationDbContext _db;

    public IndividualBulkUploadService(ApplicationDbContext db)
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
            "Full Name of the Customer (Mandatory field)",
            "Nationality",
            "Date of Birth (MM/DD/YYYY or YYYY-MM-DD)",
            "Company Reference Code",
            "IDType",
            "IDNumber",
            "Reference Id",
            "Place of Birth"
        };
        for (var i = 0; i < headers.Length; i++)
            row0.CreateCell(i).SetCellValue(headers[i]);

        var row1 = sheet.CreateRow(1);
        row1.CreateCell(0).SetCellValue("AAAVV");
        row1.CreateCell(1).SetCellValue("AAAVV");
        row1.CreateCell(2).SetCellValue("SY");
        row1.CreateCell(3).SetCellValue("1/1/1994");

        using var ms = new MemoryStream();
        wb.Write(ms, true);
        wb.Close();
        return ms.ToArray();
    }

    public async Task<ApiResponse<IndividualBulkUploadResultDto>> UploadAsync(
        Stream fileStream,
        string fileName,
        IndividualBulkUploadOptionsDto options,
        CancellationToken cancellationToken = default)
    {
        if (fileStream == null || string.IsNullOrWhiteSpace(fileName))
            return ApiResponse<IndividualBulkUploadResultDto>.Fail("File is required.");

        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        if (ext != ".csv" && ext != ".xlsx" && ext != ".xls")
            return ApiResponse<IndividualBulkUploadResultDto>.Fail("Only .csv, .xlsx, and .xls files are allowed.");

        IReadOnlyList<IndividualBulkUploadFileParser.RawRow> rawRows;
        try
        {
            rawRows = IndividualBulkUploadFileParser.Parse(fileStream, fileName);
        }
        catch (Exception ex)
        {
            return ApiResponse<IndividualBulkUploadResultDto>.Fail(ex.Message);
        }

        var countries = await _db.Countries.AsNoTracking().ToListAsync(cancellationToken);

        var batch = new IndividualBulkUploadBatch
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

        var lines = new List<IndividualBulkUploadLine>();
        var reportRows = new List<IndividualBulkUploadReportRowDto>();

        foreach (var raw in rawRows)
        {
            var errs = new List<string>();
            if (string.IsNullOrWhiteSpace(raw.CustomerId))
                errs.Add("Customer ID is required.");
            if (string.IsNullOrWhiteSpace(raw.FullName))
                errs.Add("Full name is required.");

            string? resolvedCode = null;
            if (!string.IsNullOrWhiteSpace(raw.Nationality))
            {
                if (!TryResolveCountry(raw.Nationality, countries, out resolvedCode, out var natErr))
                    errs.Add(natErr);
            }

            DateTime? dobParsed = null;
            var dobDisplay = raw.DateOfBirth ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(raw.DateOfBirth))
            {
                if (!TryParseDob(raw.DateOfBirth, out dobParsed, out var disp))
                    errs.Add("Invalid date of birth. Use MM/DD/YYYY or YYYY-MM-DD.");
                else
                    dobDisplay = disp;
            }

            var errMsg = errs.Count > 0 ? string.Join(" ", errs) : null;
            var queued = errMsg == null;

            var line = new IndividualBulkUploadLine
            {
                Id = Guid.NewGuid(),
                BatchId = batch.Id,
                LineIndex = raw.LineIndex,
                CustomerId = raw.CustomerId.Trim(),
                FullName = raw.FullName.Trim(),
                Nationality = raw.Nationality.Trim(),
                DateOfBirthRaw = raw.DateOfBirth ?? string.Empty,
                DateOfBirthParsed = dobParsed,
                CompanyReferenceCode = raw.CompanyReferenceCode ?? string.Empty,
                IdType = raw.IdType ?? string.Empty,
                IdNumber = raw.IdNumber ?? string.Empty,
                ReferenceId = raw.ReferenceId ?? string.Empty,
                PlaceOfBirth = raw.PlaceOfBirth ?? string.Empty,
                NationalityResolvedCode = resolvedCode,
                ErrorMessage = errMsg,
                QueuedForScreening = queued
            };
            lines.Add(line);

            reportRows.Add(new IndividualBulkUploadReportRowDto
            {
                CustomerId = line.CustomerId,
                FullName = line.FullName,
                Country = resolvedCode ?? raw.Nationality.Trim(),
                Dob = dobDisplay,
                PlaceOfBirth = line.PlaceOfBirth,
                Error = errMsg ?? string.Empty
            });
        }

        batch.TotalRowCount = lines.Count;
        batch.FailedRowCount = lines.Count(l => l.ErrorMessage != null);
        batch.QueuedRowCount = lines.Count(l => l.QueuedForScreening);
        batch.Lines = lines;

        _db.IndividualBulkUploadBatches.Add(batch);
        await _db.SaveChangesAsync(cancellationToken);

        var mode = batch.FailedRowCount > 0 ? "validationFailed" : "queued";
        var dto = new IndividualBulkUploadResultDto
        {
            BatchId = batch.Id,
            Mode = mode,
            Rows = reportRows
        };

        return ApiResponse<IndividualBulkUploadResultDto>.Ok(dto);
    }

    public async Task<ApiResponse<IReadOnlyList<IndividualBulkUploadBatchListItemDto>>> GetBatchesAsync(
        DateTime? fromUtc,
        DateTime? toUtc,
        string? uploadedBy,
        CancellationToken cancellationToken = default)
    {
        var q = _db.IndividualBulkUploadBatches.AsNoTracking().Where(b => b.TenantId == DefaultTenantId);
        if (fromUtc.HasValue)
            q = q.Where(b => b.CreatedAt >= fromUtc.Value);
        if (toUtc.HasValue)
            q = q.Where(b => b.CreatedAt <= toUtc.Value);
        if (!string.IsNullOrWhiteSpace(uploadedBy))
            q = q.Where(b => b.CreatedBy != null && b.CreatedBy.Contains(uploadedBy));

        var list = await q
            .OrderByDescending(b => b.CreatedAt)
            .Select(b => new IndividualBulkUploadBatchListItemDto
            {
                Id = b.Id,
                FileName = b.OriginalFileName,
                UploadedOn = b.CreatedAt,
                UploadedBy = b.CreatedBy ?? "",
                ScreeningFinished = b.ScreeningFinished
            })
            .ToListAsync(cancellationToken);

        return ApiResponse<IReadOnlyList<IndividualBulkUploadBatchListItemDto>>.Ok(list);
    }

    public async Task<ApiResponse<IReadOnlyList<IndividualBulkUploadLineDetailDto>>> GetBatchLinesAsync(
        Guid batchId,
        string? caseStatus,
        CancellationToken cancellationToken = default)
    {
        var exists = await _db.IndividualBulkUploadBatches.AnyAsync(b => b.Id == batchId, cancellationToken);
        if (!exists)
            return ApiResponse<IReadOnlyList<IndividualBulkUploadLineDetailDto>>.Fail("Batch not found.");

        var q = _db.IndividualBulkUploadLines.AsNoTracking().Where(l => l.BatchId == batchId);
        var lines = await q.OrderBy(l => l.LineIndex).ToListAsync(cancellationToken);

        var result = new List<IndividualBulkUploadLineDetailDto>();
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
            result.Add(new IndividualBulkUploadLineDetailDto
            {
                Index = idx,
                CustomerId = l.CustomerId,
                CustomerName = l.FullName,
                Nationality = l.NationalityResolvedCode ?? l.Nationality,
                DateOfBirth = l.DateOfBirthParsed?.ToString("yyyy-MM-dd") ?? l.DateOfBirthRaw,
                Status = status,
                Source = "Bulk",
                Uid = l.Id.ToString(),
                Date = l.CreatedAt.ToString("yyyy-MM-dd")
            });
        }

        return ApiResponse<IReadOnlyList<IndividualBulkUploadLineDetailDto>>.Ok(result);
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

    private static bool TryParseDob(string raw, out DateTime? parsed, out string display)
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


