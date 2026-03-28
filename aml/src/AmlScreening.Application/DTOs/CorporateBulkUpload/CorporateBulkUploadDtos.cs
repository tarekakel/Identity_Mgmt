namespace AmlScreening.Application.DTOs.CorporateBulkUpload;

public class CorporateBulkUploadReportRowDto
{
    public string CustomerId { get; set; } = string.Empty;
    public string EntityName { get; set; } = string.Empty;
    public string IncorporatedCountry { get; set; } = string.Empty;
    public string DateOfIncorporation { get; set; } = string.Empty;
    public string CompanyReferenceCode { get; set; } = string.Empty;
    public string TradeLicense { get; set; } = string.Empty;
    public string? Error { get; set; }
}

public class CorporateBulkUploadResultDto
{
    public string Mode { get; set; } = string.Empty;
    public Guid BatchId { get; set; }
    public IReadOnlyList<CorporateBulkUploadReportRowDto> Rows { get; set; } = Array.Empty<CorporateBulkUploadReportRowDto>();
}

public class CorporateBulkUploadBatchListItemDto
{
    public Guid Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public DateTime UploadedOn { get; set; }
    public string UploadedBy { get; set; } = string.Empty;
    public bool ScreeningFinished { get; set; }
}

public class CorporateBulkUploadLineDetailDto
{
    public int Index { get; set; }
    public string CustomerId { get; set; } = string.Empty;
    public string EntityName { get; set; } = string.Empty;
    public string IncorporatedCountry { get; set; } = string.Empty;
    public string DateOfIncorporation { get; set; } = string.Empty;
    public string CompanyReferenceCode { get; set; } = string.Empty;
    public string TradeLicense { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Source { get; set; } = "Bulk";
    public string Uid { get; set; } = string.Empty;
    public string Date { get; set; } = string.Empty;
}
