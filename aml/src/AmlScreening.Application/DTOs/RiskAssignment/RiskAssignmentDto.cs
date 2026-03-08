namespace AmlScreening.Application.DTOs.RiskAssignment;

public class RiskAssignmentDto
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public int CountryRisk { get; set; }
    public int CustomerTypeRisk { get; set; }
    public int PepRisk { get; set; }
    public int TransactionRisk { get; set; }
    public int IndustryRisk { get; set; }
    public int TotalScore { get; set; }
    public string? RiskLevel { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
    public bool IsActive { get; set; }
    public bool IsDeleted { get; set; }
}

public class CreateRiskAssignmentDto
{
    public Guid CustomerId { get; set; }
    public int CountryRisk { get; set; }
    public int CustomerTypeRisk { get; set; }
    public int PepRisk { get; set; }
    public int TransactionRisk { get; set; }
    public int IndustryRisk { get; set; }
    public int TotalScore { get; set; }
    public string RiskLevel { get; set; } = string.Empty;
}

public class UpdateRiskAssignmentDto
{
    public int CountryRisk { get; set; }
    public int CustomerTypeRisk { get; set; }
    public int PepRisk { get; set; }
    public int TransactionRisk { get; set; }
    public int IndustryRisk { get; set; }
    public int TotalScore { get; set; }
    public string RiskLevel { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}
