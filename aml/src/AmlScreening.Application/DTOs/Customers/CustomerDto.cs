namespace AmlScreening.Application.DTOs.Customers;

public class CustomerDto
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string CustomerNumber { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? FullName { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public Guid? GenderId { get; set; }
    public string? GenderName { get; set; }
    public Guid? NationalityId { get; set; }
    public string? NationalityName { get; set; }
    public string? PassportNumber { get; set; }
    public DateTime? PassportExpiryDate { get; set; }
    public string? NationalId { get; set; }
    public Guid? CountryOfResidenceId { get; set; }
    public string? CountryOfResidenceName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public Guid? OccupationId { get; set; }
    public string? OccupationName { get; set; }
    public string? EmployerName { get; set; }
    public Guid? SourceOfFundsId { get; set; }
    public string? SourceOfFundsName { get; set; }
    public decimal? AnnualIncome { get; set; }
    public decimal? ExpectedMonthlyTransactionVolume { get; set; }
    public decimal? ExpectedMonthlyTransactionValue { get; set; }
    public Guid CustomerTypeId { get; set; }
    public string? CustomerTypeCode { get; set; }
    public string? CustomerTypeName { get; set; }
    public string? AccountPurpose { get; set; }
    public decimal? RiskScore { get; set; }
    public string? RiskLevel { get; set; }
    public Guid StatusId { get; set; }
    public string? StatusCode { get; set; }
    public string? StatusName { get; set; }
    public bool IsPep { get; set; }
    public string? BusinessActivity { get; set; }
    public string? NationalIdOrPassport { get; set; }
    public string? RiskClassification { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
    public bool IsActive { get; set; }
    public bool IsDeleted { get; set; }
}

public class CreateCustomerDto
{
    public Guid TenantId { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string FullName { get; set; } = string.Empty;
    public DateTime? DateOfBirth { get; set; }
    public Guid? GenderId { get; set; }
    public Guid? NationalityId { get; set; }
    public string? PassportNumber { get; set; }
    public DateTime? PassportExpiryDate { get; set; }
    public string? NationalId { get; set; }
    public Guid? CountryOfResidenceId { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public Guid? OccupationId { get; set; }
    public string? EmployerName { get; set; }
    public Guid? SourceOfFundsId { get; set; }
    public decimal? AnnualIncome { get; set; }
    public decimal? ExpectedMonthlyTransactionVolume { get; set; }
    public decimal? ExpectedMonthlyTransactionValue { get; set; }
    public Guid CustomerTypeId { get; set; }
    public string? AccountPurpose { get; set; }
}

public class UpdateCustomerDto
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string FullName { get; set; } = string.Empty;
    public DateTime? DateOfBirth { get; set; }
    public Guid? GenderId { get; set; }
    public Guid? NationalityId { get; set; }
    public string? PassportNumber { get; set; }
    public DateTime? PassportExpiryDate { get; set; }
    public string? NationalId { get; set; }
    public Guid? CountryOfResidenceId { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public Guid? OccupationId { get; set; }
    public string? EmployerName { get; set; }
    public Guid? SourceOfFundsId { get; set; }
    public decimal? AnnualIncome { get; set; }
    public decimal? ExpectedMonthlyTransactionVolume { get; set; }
    public decimal? ExpectedMonthlyTransactionValue { get; set; }
    public Guid CustomerTypeId { get; set; }
    public string? AccountPurpose { get; set; }
    public bool IsActive { get; set; }
}
