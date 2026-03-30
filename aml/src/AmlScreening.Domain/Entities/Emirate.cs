namespace AmlScreening.Domain.Entities;

public class Emirate
{
    public Guid Id { get; set; }
    public Guid CountryId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;

    public Country Country { get; set; } = null!;
}
