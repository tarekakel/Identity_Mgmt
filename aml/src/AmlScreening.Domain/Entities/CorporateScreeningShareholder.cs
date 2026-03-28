namespace AmlScreening.Domain.Entities;

public class CorporateScreeningShareholder
{
    public Guid Id { get; set; }
    public Guid CorporateScreeningRequestId { get; set; }

    public string FullName { get; set; } = string.Empty;
    public Guid? NationalityId { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public decimal SharePercent { get; set; }

    public CorporateScreeningRequest CorporateScreeningRequest { get; set; } = null!;
    public Nationality? Nationality { get; set; }

    public ICollection<CorporateScreeningShareholderDocument> Documents { get; set; } = new List<CorporateScreeningShareholderDocument>();
}
