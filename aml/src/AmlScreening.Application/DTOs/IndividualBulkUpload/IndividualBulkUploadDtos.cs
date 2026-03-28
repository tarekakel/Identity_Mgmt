namespace AmlScreening.Application.DTOs.IndividualBulkUpload;

public class IndividualBulkUploadReportRowDto
{
    public string CustomerId { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string Dob { get; set; } = string.Empty;
    public string PlaceOfBirth { get; set; } = string.Empty;
    public string? Error { get; set; }
}

public class IndividualBulkUploadResultDto
{
    public string Mode { get; set; } = string.Empty;
    public Guid BatchId { get; set; }
    public IReadOnlyList<IndividualBulkUploadReportRowDto> Rows { get; set; } = Array.Empty<IndividualBulkUploadReportRowDto>();
}

public class IndividualBulkUploadBatchListItemDto
{
    public Guid Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public DateTime UploadedOn { get; set; }
    public string UploadedBy { get; set; } = string.Empty;
    public bool ScreeningFinished { get; set; }
}

public class IndividualBulkUploadLineDetailDto
{
    public int Index { get; set; }
    public string CustomerId { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string Nationality { get; set; } = string.Empty;
    public string DateOfBirth { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Source { get; set; } = "Bulk";
    public string Uid { get; set; } = string.Empty;
    public string Date { get; set; } = string.Empty;
}
