namespace AmlScreening.Domain.Entities.SanctionList;

/// <summary>UN XML <c>INDIVIDUAL_ADDRESS</c> / <c>ENTITY_ADDRESS</c>.</summary>
public class SanctionAddress
{
    public string? Street { get; set; }
    public string? City { get; set; }
    public string? StateProvince { get; set; }
    public string? Country { get; set; }
    public string? Note { get; set; }
}
