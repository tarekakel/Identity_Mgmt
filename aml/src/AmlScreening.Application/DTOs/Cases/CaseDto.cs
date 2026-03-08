namespace AmlScreening.Application.DTOs.Cases;

public class CaseDto
{
    public Guid Id { get; set; }
    public Guid? CustomerId { get; set; }
    public Guid? AlertId { get; set; }
    public string? CaseNumber { get; set; }
    public string? Status { get; set; }
    public Guid? AssignedToId { get; set; }
    public string? CreatedByRole { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
    public bool IsActive { get; set; }
    public bool IsDeleted { get; set; }
}

public class CreateCaseDto
{
    public Guid? CustomerId { get; set; }
    public Guid? AlertId { get; set; }
    public string Status { get; set; } = string.Empty;
    public Guid? AssignedToId { get; set; }
    public string? CreatedByRole { get; set; }
}

public class UpdateCaseDto
{
    public Guid? CustomerId { get; set; }
    public Guid? AlertId { get; set; }
    public string Status { get; set; } = string.Empty;
    public Guid? AssignedToId { get; set; }
    public string? CreatedByRole { get; set; }
    public bool IsActive { get; set; }
}
