using AmlScreening.Domain.Interfaces;

namespace AmlScreening.Domain.Entities;

public class CorporateBulkUploadBatch : IEntity, IAuditable, ISoftDelete, ITenantEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }

    public string OriginalFileName { get; set; } = string.Empty;
    public int MatchThreshold { get; set; } = 85;

    public bool CheckPepUkOnly { get; set; }
    public bool CheckDisqualifiedDirectorUkOnly { get; set; }
    public bool CheckSanctions { get; set; }
    public bool CheckProfileOfInterest { get; set; }
    public bool CheckReputationalRiskExposure { get; set; }
    public bool CheckRegulatoryEnforcementList { get; set; }
    public bool CheckInsolvencyUkIreland { get; set; }

    public bool ScreeningFinished { get; set; }
    public int TotalRowCount { get; set; }
    public int FailedRowCount { get; set; }
    public int QueuedRowCount { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }
    public bool IsActive { get; set; }

    public ICollection<CorporateBulkUploadLine> Lines { get; set; } = new List<CorporateBulkUploadLine>();
}
