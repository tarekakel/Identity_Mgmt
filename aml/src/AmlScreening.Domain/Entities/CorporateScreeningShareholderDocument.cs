namespace AmlScreening.Domain.Entities;

public class CorporateScreeningShareholderDocument
{
    public Guid Id { get; set; }
    public Guid CorporateScreeningShareholderId { get; set; }

    public string? DocumentNo { get; set; }
    public DateTime? IssuedDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string? Details { get; set; }
    public string? Remarks { get; set; }

    public CorporateScreeningShareholder Shareholder { get; set; } = null!;
}
