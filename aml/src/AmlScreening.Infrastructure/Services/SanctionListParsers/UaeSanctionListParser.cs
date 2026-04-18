using System.Globalization;
using AmlScreening.Domain.Entities;
using AmlScreening.Domain.Entities.SanctionList;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using NPOI.HSSF.UserModel;

namespace AmlScreening.Infrastructure.Services.SanctionListParsers;

public class UaeSanctionListParser : IUaeSanctionListParser
{
    // Header tokens used by the fallback substring matcher (for non-canonical UAE files).
    private static readonly string[] FullNameHeaders = { "name", "full name", "fullname", "full_name", "individual name", "entity name", "first name", "title", "designation", "listed name", "name (english)", "name(en)", "nom", "nombre", "الاسم الكامل", "الاسم" };
    private static readonly string[] NationalityHeaders = { "nationality", "country", "country of nationality", "nationality/citizenship", "citizenship", "الجنسية" };
    private static readonly string[] DobHeaders = { "dob", "date of birth", "dateofbirth", "birth date", "birthdate", "birth date (yyyy-mm-dd)", "تاريخ الميلاد" };
    private static readonly string[] FullNameArabicHeaders = { "full name (arabic)", "name (arabic)", "الاسم الكامل (بالحروف العربية)", "full name arabic" };
    private static readonly string[] FamilyNameArabicHeaders = { "family name (arabic)", "اسم العائلة", "family name arabic" };
    private static readonly string[] FamilyNameLatinHeaders = { "family name (latin)", "family name latin", "اسم العائلة أو الحروف اللاتينية", "اسم العائلة (بالحروف اللاتينية)" };
    private static readonly string[] PlaceOfBirthHeaders = { "place of birth", "placeofbirth", "مكان الميلاد", "pob" };
    private static readonly string[] DocumentNumberHeaders = { "document number", "document no", "رقم الوثيقة", "doc number" };
    private static readonly string[] IssuingAuthorityHeaders = { "issuing authority", "جهة الإصدار", "issuer", "بلد الإصدار" };
    private static readonly string[] IssueDateHeaders = { "issue date", "تاريخ الإصدار", "date of issue" };
    private static readonly string[] EndDateHeaders = { "end date", "expiry", "تاريخ الانتهاء", "expiry date" };
    private static readonly string[] OtherInfoHeaders = { "other information", "معلومات أخرى", "comments", "notes", "remarks", "قرار", "رقم القرار" };
    private static readonly string[] ClassificationHeaders = { "classification", "التصنيف", "صنف" };
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
                if (TryParseCanonicalUaeSheet(sheet, listSourceName, list))
                {
                    if (list.Count > countBefore) break;
                    continue;
                }

                TryParseSheetByHeaders(sheet, listSourceName, list);
                if (list.Count > countBefore) break;
            }

            return list;
        }
        finally
        {
            workbook.Close();
        }
    }

    /// <summary>
    /// Canonical UAE local terror list layout (19 cols, Arabic headers in row 3).
    /// Returns true if this layout was detected and parsed.
    /// </summary>
    private static bool TryParseCanonicalUaeSheet(ISheet sheet, string listSourceName, List<SanctionListEntry> list)
    {
        // Detect by looking for a row that contains the four canonical Arabic name headers in known positions.
        for (int hr = 0; hr <= Math.Min(6, sheet.LastRowNum); hr++)
        {
            var row = sheet.GetRow(hr);
            if (row == null || row.LastCellNum < 19) continue;

            string Cell(int i) => GetCellString(row.GetCell(i))?.Trim() ?? string.Empty;
            var c4 = Cell(3); var c5 = Cell(4); var c6 = Cell(5); var c7 = Cell(6);
            var c11 = Cell(10);

            bool hasFamilyArabic = c4.Contains("اسم العائلة") && c4.Contains("العربية");
            bool hasFamilyLatin = c5.Contains("اسم العائلة") && (c5.Contains("اللاتينية") || c5.Contains("اللاتيني"));
            bool hasFullArabic = c6.Contains("الاسم الكامل") && c6.Contains("العربية");
            bool hasFullLatin = c7.Contains("الاسم الكامل") && (c7.Contains("اللاتينية") || c7.Contains("اللاتيني"));
            bool hasStreet = c11.Contains("الشارع");

            if (!(hasFamilyArabic && hasFamilyLatin && hasFullArabic && hasFullLatin && hasStreet))
                continue;

            var dataStart = hr + 1;
            for (int r = dataStart; r <= sheet.LastRowNum; r++)
            {
                var dr = sheet.GetRow(r);
                if (dr == null) continue;

                string V(int i) => CleanCell(GetCellString(dr.GetCell(i)));
                var fullLatin = V(6);
                var fullArabic = V(5);
                if (string.IsNullOrEmpty(fullLatin) && string.IsNullOrEmpty(fullArabic)) continue;

                var fullName = !string.IsNullOrEmpty(fullLatin) ? fullLatin : fullArabic;
                if (!LooksLikeName(fullName)) continue;

                var classification = V(1);
                var nationality = V(2);
                var familyArabic = V(3);
                var familyLatin = V(4);
                var dob = ParseDateCell(dr.GetCell(7));
                var placeOfBirth = V(8);
                var aliasName = V(9);
                var addrStreet = V(10);
                var addrCity = V(11);
                var addrCountry = V(12);
                var docType = V(13);
                var docNumber = V(14);
                var docIssuingCountry = V(15);
                var issueDate = ParseDateCell(dr.GetCell(16));
                var endDate = ParseDateCell(dr.GetCell(17));
                var otherInfo = V(18);

                var entry = new SanctionListEntry
                {
                    Id = Guid.NewGuid(),
                    ListSource = listSourceName,
                    FullName = Trunc(fullName, 512)!,
                    FullNameArabic = Trunc(fullArabic, 512),
                    FamilyNameArabic = Trunc(familyArabic, 256),
                    FamilyNameLatin = Trunc(familyLatin, 256),
                    Nationality = Trunc(nationality, 128),
                    DateOfBirth = dob,
                    PlaceOfBirthCountry = Trunc(placeOfBirth, 128),
                    AddressCity = Trunc(addrCity, 128),
                    AddressCountry = Trunc(addrCountry, 128),
                    AddressNote = Trunc(addrStreet, 512),
                    DocumentNumber = Trunc(docNumber, 128),
                    IssuingAuthority = Trunc(docIssuingCountry, 256),
                    IssueDate = issueDate,
                    EndDate = endDate,
                    OtherInformation = Trunc(otherInfo, 2000),
                    EntryType = MapEntryType(classification),
                    TypeDetail = Trunc(classification, 64)
                };

                if (!string.IsNullOrEmpty(nationality))
                    entry.Nationalities = new List<string> { nationality! };

                if (!string.IsNullOrEmpty(placeOfBirth))
                {
                    entry.PlacesOfBirth = new List<SanctionPlaceOfBirth>
                    {
                        new() { Country = placeOfBirth }
                    };
                }

                if (!string.IsNullOrEmpty(aliasName) &&
                    !string.Equals(aliasName, fullName, StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(aliasName, fullArabic, StringComparison.OrdinalIgnoreCase))
                {
                    entry.AliasItems = new List<SanctionAlias>
                    {
                        new() { Name = aliasName!, Quality = "a.k.a." }
                    };
                    entry.Aliases = Trunc(aliasName, 1000);
                }

                if (!string.IsNullOrEmpty(addrStreet) || !string.IsNullOrEmpty(addrCity) || !string.IsNullOrEmpty(addrCountry))
                {
                    entry.Addresses = new List<SanctionAddress>
                    {
                        new()
                        {
                            Street = NullIfEmpty(addrStreet),
                            City = NullIfEmpty(addrCity),
                            Country = NullIfEmpty(addrCountry)
                        }
                    };
                }

                if (!string.IsNullOrEmpty(docType) || !string.IsNullOrEmpty(docNumber) || !string.IsNullOrEmpty(docIssuingCountry))
                {
                    entry.Documents = new List<SanctionDocument>
                    {
                        new()
                        {
                            Type = NullIfEmpty(docType),
                            Number = NullIfEmpty(docNumber),
                            IssuingCountry = NullIfEmpty(docIssuingCountry),
                            DateOfIssue = issueDate
                        }
                    };
                }

                if (dob.HasValue)
                {
                    entry.DatesOfBirth = new List<SanctionDob>
                    {
                        new() { Date = dob, Year = dob.Value.Year, TypeOfDate = "EXACT" }
                    };
                }

                list.Add(entry);
            }

            return true;
        }

        return false;
    }

    private struct UaeCols
    {
        public int Name, Nationality, Dob, FullNameArabic, FamilyNameArabic, FamilyNameLatin, PlaceOfBirth;
        public int DocumentNumber, IssuingAuthority, IssueDate, EndDate, OtherInfo, Classification;
    }

    private static void TryParseSheetByHeaders(ISheet sheet, string listSourceName, List<SanctionListEntry> list)
    {
        const int maxHeaderRow = 5;
        var cols = new UaeCols { Name = -1, Nationality = -1, Dob = -1, FullNameArabic = -1, FamilyNameArabic = -1, FamilyNameLatin = -1, PlaceOfBirth = -1, DocumentNumber = -1, IssuingAuthority = -1, IssueDate = -1, EndDate = -1, OtherInfo = -1, Classification = -1 };
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

        for (int r = startRow; r <= sheet.LastRowNum; r++)
        {
            var row = sheet.GetRow(r);
            if (row == null) continue;

            var fullName = CleanCell(GetCellString(row.GetCell(cols.Name)));
            if (string.IsNullOrWhiteSpace(fullName)) continue;

            var nationality = CleanCell(GetCellString(row.GetCell(cols.Nationality)));
            var placeOfBirth = CleanCell(GetCellString(row.GetCell(cols.PlaceOfBirth)));
            var docNumber = CleanCell(GetCellString(row.GetCell(cols.DocumentNumber)));
            var issuingAuthority = CleanCell(GetCellString(row.GetCell(cols.IssuingAuthority)));
            var otherInfo = CleanCell(GetCellString(row.GetCell(cols.OtherInfo)));
            var classification = CleanCell(GetCellString(row.GetCell(cols.Classification)));
            var fullNameArabic = CleanCell(GetCellString(row.GetCell(cols.FullNameArabic)));
            var familyNameArabic = CleanCell(GetCellString(row.GetCell(cols.FamilyNameArabic)));
            var familyNameLatin = CleanCell(GetCellString(row.GetCell(cols.FamilyNameLatin)));

            var entry = new SanctionListEntry
            {
                Id = Guid.NewGuid(),
                ListSource = listSourceName,
                FullName = Trunc(fullName, 512)!,
                Nationality = Trunc(nationality, 128),
                DateOfBirth = ParseDateCell(row.GetCell(cols.Dob))
            };
            if (cols.FullNameArabic >= 0) entry.FullNameArabic = Trunc(fullNameArabic, 512);
            if (cols.FamilyNameArabic >= 0) entry.FamilyNameArabic = Trunc(familyNameArabic, 256);
            if (cols.FamilyNameLatin >= 0) entry.FamilyNameLatin = Trunc(familyNameLatin, 256);
            if (cols.PlaceOfBirth >= 0) entry.PlaceOfBirthCountry = Trunc(placeOfBirth, 128);
            if (cols.DocumentNumber >= 0) entry.DocumentNumber = Trunc(docNumber, 128);
            if (cols.IssuingAuthority >= 0) entry.IssuingAuthority = Trunc(issuingAuthority, 256);
            if (cols.IssueDate >= 0) entry.IssueDate = ParseDateCell(row.GetCell(cols.IssueDate));
            if (cols.EndDate >= 0) entry.EndDate = ParseDateCell(row.GetCell(cols.EndDate));
            if (cols.OtherInfo >= 0) entry.OtherInformation = Trunc(otherInfo, 2000);
            if (cols.Classification >= 0 && !string.IsNullOrEmpty(classification))
            {
                entry.TypeDetail = Trunc(classification, 64);
                entry.EntryType = MapEntryType(classification);
            }
            if (string.IsNullOrEmpty(entry.EntryType)) entry.EntryType = "Individual";

            if (!string.IsNullOrEmpty(nationality))
                entry.Nationalities = new List<string> { nationality! };
            if (!string.IsNullOrEmpty(placeOfBirth))
                entry.PlacesOfBirth = new List<SanctionPlaceOfBirth> { new() { Country = placeOfBirth } };
            if (!string.IsNullOrEmpty(docNumber) || !string.IsNullOrEmpty(issuingAuthority))
            {
                entry.Documents = new List<SanctionDocument>
                {
                    new()
                    {
                        Number = NullIfEmpty(docNumber),
                        IssuingCountry = NullIfEmpty(issuingAuthority),
                        DateOfIssue = entry.IssueDate
                    }
                };
            }
            if (entry.DateOfBirth.HasValue)
            {
                entry.DatesOfBirth = new List<SanctionDob>
                {
                    new() { Date = entry.DateOfBirth, Year = entry.DateOfBirth.Value.Year, TypeOfDate = "EXACT" }
                };
            }

            list.Add(entry);
        }
    }

    private static string MapEntryType(string? classification)
    {
        if (string.IsNullOrWhiteSpace(classification)) return "Individual";
        var lc = classification.ToLowerInvariant();
        if (lc.Contains("entity") || lc.Contains("organization") || lc.Contains("جماعة") || lc.Contains("منظمة") || lc.Contains("شركة"))
            return "Entity";
        return "Individual";
    }

    private static string CleanCell(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return string.Empty;
        var t = value.Trim();
        if (t == "-" || t == "—" || t == "_" || t == "—") return string.Empty;
        return t;
    }

    private static DateTime? ParseDateCell(ICell? cell)
    {
        if (cell == null) return null;
        if (cell.CellType == CellType.Numeric && DateUtil.IsCellDateFormatted(cell))
            return cell.DateCellValue;
        var s = GetCellString(cell);
        if (string.IsNullOrWhiteSpace(s)) return null;
        if (s.Trim() == "-") return null;
        if (DateTime.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.None, out var d)) return d;
        return null;
    }

    private static string? Trunc(string? value, int max)
    {
        if (value == null) return null;
        return value.Length <= max ? value : value[..max];
    }

    private static string? NullIfEmpty(string? s) => string.IsNullOrWhiteSpace(s) ? null : s;

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
