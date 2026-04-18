namespace AmlScreening.Domain.Entities.SanctionList;

/// <summary>UN XML <c>INDIVIDUAL_PLACE_OF_BIRTH</c>.</summary>
public class SanctionPlaceOfBirth
{
    public string? City { get; set; }
    public string? StateProvince { get; set; }
    public string? Country { get; set; }
}
