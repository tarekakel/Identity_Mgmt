namespace AmlScreening.Application.DTOs.Customers;

public class CustomerDocumentDto
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public Guid DocumentTypeId { get; set; }
    public string? DocumentTypeCode { get; set; }
    public string? DocumentTypeName { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string? UploadedBy { get; set; }
    public DateTime UploadedDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
}
