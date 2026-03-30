namespace AmlScreening.Application.DTOs.Lookups;

public class UpsertEmirateRequest
{
    public Guid CountryId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}
