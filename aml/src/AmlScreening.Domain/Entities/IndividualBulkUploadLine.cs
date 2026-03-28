using AmlScreening.Domain.Interfaces;

namespace AmlScreening.Domain.Entities;

public class IndividualBulkUploadLine : IEntity, IAuditable, ISoftDelete
{
    public Guid Id { get; set; }
    public Guid BatchId { get; set; }

    public int LineIndex { get; set; }

    public string CustomerId { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Nationality { get; set; } = string.Empty;
    public string DateOfBirthRaw { get; set; } = string.Empty;
    public DateTime? DateOfBirthParsed { get; set; }
    public string CompanyReferenceCode { get; set; } = string.Empty;
    public string IdType { get; set; } = string.Empty;
    public string IdNumber { get; set; } = string.Empty;
    public string ReferenceId { get; set; } = string.Empty;
    public string PlaceOfBirth { get; set; } = string.Empty;

    public string? NationalityResolvedCode { get; set; }

    public string? ErrorMessage { get; set; }
    public bool QueuedForScreening { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }
    public bool IsActive { get; set; }

    public IndividualBulkUploadBatch Batch { get; set; } = null!;
}
