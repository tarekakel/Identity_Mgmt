namespace AmlScreening.Application.DTOs.CorporateKyc;

public class CorporateKycDocumentDto
{
    public Guid Id { get; set; }
    public Guid CorporateKycId { get; set; }
    public Guid CustomerId { get; set; }

    public string? DocumentNo { get; set; }
    public DateTime? IssuedDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string? ApprovedBy { get; set; }
    public string? FolderPath { get; set; }

    public string FileName { get; set; } = string.Empty;
    public DateTime UploadedDate { get; set; }
    public string? UploadedBy { get; set; }
}
