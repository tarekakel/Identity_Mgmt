using AmlScreening.Domain.Interfaces;

namespace AmlScreening.Domain.Entities;

public class CorporateKycDocument : IEntity, IAuditable, ISoftDelete
{
    public Guid Id { get; set; }
    public Guid CorporateKycId { get; set; }
    public Guid CustomerId { get; set; }

    public bool IsActive { get; set; }
    public bool IsDeleted { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }

    public string? DocumentNo { get; set; }
    public DateTime? IssuedDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string? ApprovedBy { get; set; }
    public string? FolderPath { get; set; }

    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;

    public DateTime UploadedDate { get; set; }
    public string? UploadedBy { get; set; }

    public CorporateKyc CorporateKyc { get; set; } = null!;
}
