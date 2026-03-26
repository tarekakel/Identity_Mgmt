namespace AmlScreening.Application.DTOs.IndividualKyc;

public class UploadIndividualKycDocumentRequestDto
{
    public string? DocumentNo { get; set; }
    public DateTime? IssuedDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string? ApprovedBy { get; set; }
    public string? FolderPath { get; set; }
}

