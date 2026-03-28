using System.Globalization;
using System.Text;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace AmlScreening.Infrastructure.Services;

public static class IndividualBulkUploadFileParser
{
    public sealed class RawRow
    {
        public int LineIndex { get; init; }
        public string CustomerId { get; init; } = string.Empty;
        public string FullName { get; init; } = string.Empty;
        public string Nationality { get; init; } = string.Empty;
        public string DateOfBirth { get; init; } = string.Empty;
        public string CompanyReferenceCode { get; init; } = string.Empty;
        public string IdType { get; init; } = string.Empty;
        public string IdNumber { get; init; } = string.Empty;
        public string ReferenceId { get; init; } = string.Empty;
        public string PlaceOfBirth { get; init; } = string.Empty;
    }

    public static IReadOnlyList<RawRow> Parse(Stream stream, string fileName)
    {
        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        return ext switch
        {
            ".csv" => ParseCsv(stream),
            ".xlsx" or ".xls" => ParseExcel(stream, fileName),
            _ => throw new InvalidOperationException("Only .csv, .xlsx, and .xls files are supported.")
        };
    }

    private static IReadOnlyList<RawRow> ParseCsv(Stream stream)
    {
        using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, leaveOpen: true);
        var lines = new List<string>();
        while (!reader.EndOfStream)
        {
            var line = reader.ReadLine();
            if (line != null) lines.Add(line);
        }

        if (lines.Count < 2)
            throw new InvalidOperationException("The file must contain a header row and at least one data row.");

        var header = SplitCsvLine(lines[0]);
        var col = MapHeaderColumns(header);
        var result = new List<RawRow>();
        for (var i = 1; i < lines.Count; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;
            var cells = SplitCsvLine(lines[i]);
            result.Add(BuildRow(i + 1, cells, col));
        }

        return result;
    }

    private static string[] SplitCsvLine(string line)
    {
        var parts = line.Split(',');
        for (var i = 0; i < parts.Length; i++)
            parts[i] = parts[i].Trim().Trim('"');
        return parts;
    }

    private sealed class ColumnMap
    {
        public int CustomerId = -1;
        public int FullName = -1;
        public int Nationality = -1;
        public int Dob = -1;
        public int CompanyRef = -1;
        public int IdType = -1;
        public int IdNumber = -1;
        public int ReferenceId = -1;
        public int PlaceOfBirth = -1;
    }

    private static ColumnMap MapHeaderColumns(IReadOnlyList<string> headerCells)
    {
        var map = new ColumnMap();
        for (var i = 0; i < headerCells.Count; i++)
        {
            var h = NormalizeHeader(headerCells[i]);
            if (h.Contains("customer id", StringComparison.OrdinalIgnoreCase) && map.CustomerId < 0)
                map.CustomerId = i;
            else if (h.Contains("full name", StringComparison.OrdinalIgnoreCase) && map.FullName < 0)
                map.FullName = i;
            else if (h.Contains("nationality", StringComparison.OrdinalIgnoreCase) && map.Nationality < 0)
                map.Nationality = i;
            else if ((h.Contains("date of birth", StringComparison.OrdinalIgnoreCase) || h == "dob") && map.Dob < 0)
                map.Dob = i;
            else if (h.Contains("company reference", StringComparison.OrdinalIgnoreCase) && map.CompanyRef < 0)
                map.CompanyRef = i;
            else if (h.Replace(" ", "") == "idtype" && map.IdType < 0)
                map.IdType = i;
            else if (h.Replace(" ", "") == "idnumber" && map.IdNumber < 0)
                map.IdNumber = i;
            else if (h.Contains("reference id", StringComparison.OrdinalIgnoreCase) && !h.Contains("company", StringComparison.OrdinalIgnoreCase) && map.ReferenceId < 0)
                map.ReferenceId = i;
            else if (h.Contains("place of birth", StringComparison.OrdinalIgnoreCase) && map.PlaceOfBirth < 0)
                map.PlaceOfBirth = i;
        }

        if (map.CustomerId < 0 || map.FullName < 0)
            throw new InvalidOperationException("Required columns not found: Customer ID and Full Name of the Customer.");

        return map;
    }

    private static string NormalizeHeader(string? cell) =>
        (cell ?? string.Empty).Trim().ToLowerInvariant();

    private static RawRow BuildRow(int lineIndex, IReadOnlyList<string> cells, ColumnMap col)
    {
        string G(int idx) => idx >= 0 && idx < cells.Count ? cells[idx].Trim() : string.Empty;
        return new RawRow
        {
            LineIndex = lineIndex,
            CustomerId = G(col.CustomerId),
            FullName = G(col.FullName),
            Nationality = G(col.Nationality),
            DateOfBirth = G(col.Dob),
            CompanyReferenceCode = G(col.CompanyRef),
            IdType = G(col.IdType),
            IdNumber = G(col.IdNumber),
            ReferenceId = G(col.ReferenceId),
            PlaceOfBirth = G(col.PlaceOfBirth)
        };
    }

    private static IReadOnlyList<RawRow> ParseExcel(Stream stream, string fileName)
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
            var sheet = workbook.GetSheetAt(0);
            if (sheet == null)
                throw new InvalidOperationException("The workbook has no sheets.");

            var headerRowIndex = FindHeaderRow(sheet);
            var header = ReadRowCells(sheet.GetRow(headerRowIndex));
            var col = MapHeaderColumns(header);

            var result = new List<RawRow>();
            for (var r = headerRowIndex + 1; r <= sheet.LastRowNum; r++)
            {
                var row = sheet.GetRow(r);
                if (row == null || IsEmptyRow(row)) continue;
                var cells = ReadRowCells(row);
                result.Add(BuildRow(r + 1, cells, col));
            }

            if (result.Count == 0)
                throw new InvalidOperationException("No data rows found under the header.");

            return result;
        }
        finally
        {
            workbook.Close();
        }
    }

    private static int FindHeaderRow(ISheet sheet)
    {
        for (var r = 0; r <= Math.Min(10, sheet.LastRowNum); r++)
        {
            var row = sheet.GetRow(r);
            if (row == null) continue;
            var cells = ReadRowCells(row);
            var joined = string.Join(" ", cells.Select(NormalizeHeader));
            if (joined.Contains("customer") && joined.Contains("full name"))
                return r;
        }

        return 0;
    }

    private static bool IsEmptyRow(IRow row)
    {
        for (var c = row.FirstCellNum; c < row.LastCellNum; c++)
        {
            var cell = row.GetCell(c);
            if (cell != null && !string.IsNullOrWhiteSpace(CellString(cell)))
                return false;
        }

        return true;
    }

    private static IReadOnlyList<string> ReadRowCells(IRow? row)
    {
        if (row == null) return Array.Empty<string>();
        var last = (int)row.LastCellNum;
        if (last < 0) last = 0;
        var list = new List<string>();
        for (var c = 0; c < last; c++)
        {
            var cell = row.GetCell(c);
            list.Add(CellString(cell));
        }

        return list;
    }

    private static string FormatExcelDate(ICell cell)
    {
        try
        {
            var date = DateUtil.GetJavaDate(cell.NumericCellValue);
            return date.ToString("M/d/yyyy", CultureInfo.InvariantCulture);
        }
        catch
        {
            return string.Empty;
        }
    }

    private static string CellString(ICell? cell)
    {
        if (cell == null) return string.Empty;
        return cell.CellType switch
        {
            CellType.String => cell.StringCellValue?.Trim() ?? string.Empty,
            CellType.Numeric => DateUtil.IsCellDateFormatted(cell)
                ? FormatExcelDate(cell)
                : cell.NumericCellValue.ToString(CultureInfo.InvariantCulture),
            CellType.Boolean => cell.BooleanCellValue ? "true" : "false",
            CellType.Formula => cell.StringCellValue?.Trim() ?? cell.NumericCellValue.ToString(CultureInfo.InvariantCulture),
            _ => string.Empty
        };
    }
}


