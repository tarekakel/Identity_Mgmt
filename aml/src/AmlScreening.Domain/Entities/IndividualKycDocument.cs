using AmlScreening.Domain.Interfaces;

namespace AmlScreening.Domain.Entities;

public class IndividualKycDocument : IEntity, IAuditable, ISoftDelete
{
    public Guid Id { get; set; }
    public Guid IndividualKycId { get; set; }
    public Guid CustomerId { get; set; } // Convenience for filtering and upload route

    // Repeatable documents: use IsDeleted for "Remove"
    public bool IsActive { get; set; }
    public bool IsDeleted { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }

    // Metadata from the screenshot (document row)
    public string? DocumentNo { get; set; }
    public DateTime? IssuedDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string? ApprovedBy { get; set; }
    public string? FolderPath { get; set; }

    // Uploaded file information (stored via IFileStorageService)
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;

    public DateTime UploadedDate { get; set; }

    public string? UploadedBy { get; set; }

    public IndividualKyc IndividualKyc { get; set; } = null!;
}

