namespace AmlScreening.Application.DTOs.Lookups;

public class EmirateDto
{
    public Guid Id { get; set; }
    public Guid CountryId { get; set; }
    public string? CountryName { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}
