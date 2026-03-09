using AmlScreening.Domain.Interfaces;

namespace AmlScreening.Domain.Entities;

public class CustomerDocument : IEntity, IAuditable
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public Guid DocumentTypeId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string? UploadedBy { get; set; }
    public DateTime UploadedDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }

    public Customer Customer { get; set; } = null!;
    public DocumentType DocumentType { get; set; } = null!;
}
