using AmlScreening.Domain.Interfaces;

namespace AmlScreening.Domain.Entities;

public class CorporateBulkUploadLine : IEntity, IAuditable, ISoftDelete
{
    public Guid Id { get; set; }
    public Guid BatchId { get; set; }

    public int LineIndex { get; set; }

    public string CustomerId { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string IncorporatedCountry { get; set; } = string.Empty;
    public string DateOfIncorporationRaw { get; set; } = string.Empty;
    public DateTime? DateOfIncorporationParsed { get; set; }
    public string CompanyReferenceCode { get; set; } = string.Empty;
    public string TradeLicense { get; set; } = string.Empty;

    public string? IncorporatedCountryResolvedCode { get; set; }

    public string? ErrorMessage { get; set; }
    public bool QueuedForScreening { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }
    public bool IsActive { get; set; }

    public CorporateBulkUploadBatch Batch { get; set; } = null!;
}
