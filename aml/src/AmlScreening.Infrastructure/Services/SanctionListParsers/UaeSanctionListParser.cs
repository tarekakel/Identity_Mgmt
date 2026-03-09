using System.Globalization;
using AmlScreening.Domain.Entities;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using NPOI.HSSF.UserModel;

namespace AmlScreening.Infrastructure.Services.SanctionListParsers;

public class UaeSanctionListParser : IUaeSanctionListParser
{
    private static readonly string[] FullNameHeaders = { "name", "full name", "fullname", "full_name", "individual name", "entity name", "first name", "title", "designation", "listed name", "name (english)", "name(en)", "nom", "nombre", "الاسم الكامل", "الاسم" };
    private static readonly string[] NationalityHeaders = { "nationality", "country", "country of nationality", "nationality/citizenship", "citizenship", "الجنسية" };
    private static readonly string[] DobHeaders = { "dob", "date of birth", "dateofbirth", "birth date", "birthdate", "birth date (yyyy-mm-dd)", "تاريخ الميلاد" };
    private static readonly string[] FullNameArabicHeaders = { "full name (arabic)", "name (arabic)", "الاسم الكامل (بالحروف العربية)", "full name arabic" };
    private static readonly string[] FamilyNameArabicHeaders = { "family name (arabic)", "اسم العائلة", "family name arabic" };
    private static readonly string[] FamilyNameLatinHeaders = { "family name (latin)", "family name latin", "اسم العائلة أو الحروف اللاتينية" };
    private static readonly string[] PlaceOfBirthHeaders = { "place of birth", "placeofbirth", "مكان الميلاد", "pob" };
    private static readonly string[] DocumentNumberHeaders = { "document number", "document no", "رقم الوثيقة", "doc number" };
    private static readonly string[] IssuingAuthorityHeaders = { "issuing authority", "جهة الإصدار", "issuer" };
    private static readonly string[] IssueDateHeaders = { "issue date", "تاريخ الإصدار", "date of issue" };
    private static readonly string[] EndDateHeaders = { "end date", "expiry", "تاريخ الانتهاء", "expiry date" };
    private static readonly string[] OtherInfoHeaders = { "other information", "معلومات أخرى", "comments", "notes", "remarks" };
    private static readonly string[] TypeDetailHeaders = { "type", "النوع", "type detail", "citizen", "مواطن" };
    private static readonly string[] ClassificationHeaders = { "classification", "التصنيف", "صنف", "person", "شخص ذاتي", "entity" };
    private static readonly string[] IndexLikeHeaders = { "no.", "no ", "number", "id", "index", "#", "num.", "num ", "row", "s.no", "s.no.", "serial" };

    public IReadOnlyList<SanctionListEntry> Parse(Stream stream, string listSourceName, string fileName)
    {
        IWorkbook workbook;
        try
        {
            workbook = fileName.EndsWith(".xls", StringComparison.OrdinalIgnoreCase)
                ? new HSSFWorkbook(stream)
                : new XSSFWorkbook(stream);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Cannot open Excel file: {ex.Message}", ex);
        }

        try
        {
            var list = new List<SanctionListEntry>();
            var numSheets = workbook.NumberOfSheets;

            for (int sheetIndex = 0; sheetIndex < numSheets; sheetIndex++)
            {
                var sheet = workbook.GetSheetAt(sheetIndex);
                if (sheet == null) continue;

                var countBefore = list.Count;
                TryParseSheet(sheet, listSourceName, list, out _);
                if (list.Count > countBefore)
                    break;
            }

            return list;
        }
        finally
        {
            workbook.Close();
        }
    }

    private struct UaeCols
    {
        public int Name, Nationality, Dob, FullNameArabic, FamilyNameArabic, FamilyNameLatin, PlaceOfBirth;
        public int DocumentNumber, IssuingAuthority, IssueDate, EndDate, OtherInfo, TypeDetail, Classification;
    }

    private static void TryParseSheet(ISheet sheet, string listSourceName, List<SanctionListEntry> list, out int dataStartRow)
    {
        dataStartRow = 0;
        const int maxHeaderRow = 5;
        var cols = new UaeCols { Name = -1, Nationality = -1, Dob = -1, FullNameArabic = -1, FamilyNameArabic = -1, FamilyNameLatin = -1, PlaceOfBirth = -1, DocumentNumber = -1, IssuingAuthority = -1, IssueDate = -1, EndDate = -1, OtherInfo = -1, TypeDetail = -1, Classification = -1 };
        int startRow = 0;

        for (int hr = 0; hr <= Math.Min(maxHeaderRow, sheet.LastRowNum); hr++)
        {
            var row = sheet.GetRow(hr);
            if (row == null) continue;

            int nCol = -1;
            var lastCell = row.LastCellNum;
            for (int i = 0; i < lastCell; i++)
            {
                var cell = row.GetCell(i);
                var value = GetCellString(cell)?.Trim().ToLowerInvariant() ?? string.Empty;
                if (IndexLikeHeaders.Any(h => value.Contains(h))) continue;
                if (FullNameHeaders.Any(h => value.Contains(h)) && nCol < 0) nCol = i;
                if (NationalityHeaders.Any(h => value.Contains(h))) cols.Nationality = i;
                if (DobHeaders.Any(h => value.Contains(h))) cols.Dob = i;
                if (FullNameArabicHeaders.Any(h => value.Contains(h))) cols.FullNameArabic = i;
                if (FamilyNameArabicHeaders.Any(h => value.Contains(h))) cols.FamilyNameArabic = i;
                if (FamilyNameLatinHeaders.Any(h => value.Contains(h))) cols.FamilyNameLatin = i;
                if (PlaceOfBirthHeaders.Any(h => value.Contains(h))) cols.PlaceOfBirth = i;
                if (DocumentNumberHeaders.Any(h => value.Contains(h))) cols.DocumentNumber = i;
                if (IssuingAuthorityHeaders.Any(h => value.Contains(h))) cols.IssuingAuthority = i;
                if (IssueDateHeaders.Any(h => value.Contains(h))) cols.IssueDate = i;
                if (EndDateHeaders.Any(h => value.Contains(h))) cols.EndDate = i;
                if (OtherInfoHeaders.Any(h => value.Contains(h))) cols.OtherInfo = i;
                if (TypeDetailHeaders.Any(h => value.Contains(h))) cols.TypeDetail = i;
                if (ClassificationHeaders.Any(h => value.Contains(h))) cols.Classification = i;
            }

            if (nCol >= 0)
            {
                var firstDataRow = sheet.GetRow(hr + 1);
                if (firstDataRow != null)
                {
                    var firstVal = GetCellString(firstDataRow.GetCell(nCol))?.Trim();
                    if (!LooksLikeName(firstVal)) { nCol = -1; continue; }
                }
                cols.Name = nCol;
                startRow = hr + 1;
                break;
            }
        }

        if (cols.Name < 0)
        {
            startRow = 0;
            var firstRow = sheet.GetRow(0);
            int lastCol = firstRow?.LastCellNum ?? 0;
            for (int c = 0; c < lastCol; c++)
            {
                bool hasNameLike = false;
                for (int r = 0; r <= Math.Min(sheet.LastRowNum, 20); r++)
                {
                    var dataRow = sheet.GetRow(r);
                    var cellVal = GetCellString(dataRow?.GetCell(c))?.Trim();
                    if (LooksLikeName(cellVal)) { hasNameLike = true; break; }
                }
                if (hasNameLike) { cols.Name = c; break; }
            }
            if (cols.Name < 0) cols.Name = 0;
        }

        dataStartRow = startRow;
        for (int r = startRow; r <= sheet.LastRowNum; r++)
        {
            var row = sheet.GetRow(r);
            if (row == null) continue;

            var fullName = GetCellString(row.GetCell(cols.Name))?.Trim();
            if (string.IsNullOrWhiteSpace(fullName)) continue;

            var entry = new SanctionListEntry
            {
                Id = Guid.NewGuid(),
                ListSource = listSourceName,
                FullName = Trunc(fullName, 512),
                Nationality = Trunc(GetCellString(row.GetCell(cols.Nationality))?.Trim(), 128),
                DateOfBirth = ParseDateCell(row.GetCell(cols.Dob))
            };
            if (cols.FullNameArabic >= 0) entry.FullNameArabic = Trunc(GetCellString(row.GetCell(cols.FullNameArabic))?.Trim(), 512);
            if (cols.FamilyNameArabic >= 0) entry.FamilyNameArabic = Trunc(GetCellString(row.GetCell(cols.FamilyNameArabic))?.Trim(), 256);
            if (cols.FamilyNameLatin >= 0) entry.FamilyNameLatin = Trunc(GetCellString(row.GetCell(cols.FamilyNameLatin))?.Trim(), 256);
            if (cols.PlaceOfBirth >= 0) entry.PlaceOfBirthCountry = Trunc(GetCellString(row.GetCell(cols.PlaceOfBirth))?.Trim(), 128);
            if (cols.DocumentNumber >= 0) entry.DocumentNumber = Trunc(GetCellString(row.GetCell(cols.DocumentNumber))?.Trim(), 128);
            if (cols.IssuingAuthority >= 0) entry.IssuingAuthority = Trunc(GetCellString(row.GetCell(cols.IssuingAuthority))?.Trim(), 256);
            if (cols.IssueDate >= 0) entry.IssueDate = ParseDateCell(row.GetCell(cols.IssueDate));
            if (cols.EndDate >= 0) entry.EndDate = ParseDateCell(row.GetCell(cols.EndDate));
            if (cols.OtherInfo >= 0) entry.OtherInformation = Trunc(GetCellString(row.GetCell(cols.OtherInfo))?.Trim(), 2000);
            if (cols.TypeDetail >= 0) entry.TypeDetail = Trunc(GetCellString(row.GetCell(cols.TypeDetail))?.Trim(), 64);
            if (cols.Classification >= 0)
            {
                var classification = GetCellString(row.GetCell(cols.Classification))?.Trim();
                if (!string.IsNullOrEmpty(classification))
                    entry.EntryType = Trunc(classification, 32);
            }
            if (string.IsNullOrEmpty(entry.EntryType)) entry.EntryType = "Individual";

            list.Add(entry);
        }

        return;
    }

    private static DateTime? ParseDateCell(ICell? cell)
    {
        if (cell == null) return null;
        if (cell.CellType == CellType.Numeric && DateUtil.IsCellDateFormatted(cell))
            return cell.DateCellValue;
        var s = GetCellString(cell);
        if (string.IsNullOrWhiteSpace(s)) return null;
        if (DateTime.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.None, out var d)) return d;
        return null;
    }

    private static string? Trunc(string? value, int max)
    {
        if (value == null) return null;
        return value.Length <= max ? value : value[..max];
    }

    /// <summary>True if value looks like a person/entity name (non-empty, not purely digits).</summary>
    private static bool LooksLikeName(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return false;
        var t = value.Trim();
        if (t.Length < 2) return false;
        if (long.TryParse(t, out _)) return false;
        if (double.TryParse(t, CultureInfo.InvariantCulture, out _)) return false;
        return true;
    }

    private static string? GetCellString(ICell? cell)
    {
        if (cell == null) return null;
        switch (cell.CellType)
        {
            case CellType.String:
                return cell.StringCellValue;
            case CellType.Numeric:
                return DateUtil.IsCellDateFormatted(cell) ? $"{cell.DateCellValue:yyyy-MM-dd}" : cell.NumericCellValue.ToString(CultureInfo.InvariantCulture);
            case CellType.Boolean:
                return cell.BooleanCellValue.ToString();
            case CellType.Formula:
                try
                {
                    return cell.CachedFormulaResultType switch
                    {
                        CellType.String => cell.StringCellValue,
                        CellType.Numeric => DateUtil.IsCellDateFormatted(cell) ? $"{cell.DateCellValue:yyyy-MM-dd}" : cell.NumericCellValue.ToString(CultureInfo.InvariantCulture),
                        _ => cell.ToString()
                    };
                }
                catch { return cell.ToString(); }
            default:
                return cell.ToString();
        }
    }
}
