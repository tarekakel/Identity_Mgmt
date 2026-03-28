namespace AmlScreening.Domain.Entities;

public class CorporateScreeningCompanyDocument
{
    public Guid Id { get; set; }
    public Guid CorporateScreeningRequestId { get; set; }

    public string? DocumentNo { get; set; }
    public DateTime? IssuedDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string? Details { get; set; }
    public string? Remarks { get; set; }

    public CorporateScreeningRequest CorporateScreeningRequest { get; set; } = null!;
}
