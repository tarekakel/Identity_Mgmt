namespace AmlScreening.Application.DTOs.SanctionLists;

/// <summary>One alias name with the source list's quality marker (e.g. "Good", "Low", "a.k.a.").</summary>
public class SanctionAliasDto
{
    public string Name { get; set; } = string.Empty;
    public string? Quality { get; set; }
}

/// <summary>One declared date of birth: a full date, year-only, or BETWEEN year range.</summary>
public class SanctionDobDto
{
    public DateTime? Date { get; set; }
    public int? Year { get; set; }
    public int? FromYear { get; set; }
    public int? ToYear { get; set; }
    public string? TypeOfDate { get; set; }
    public string? Note { get; set; }
}

/// <summary>One physical address record.</summary>
public class SanctionAddressDto
{
    public string? Street { get; set; }
    public string? City { get; set; }
    public string? StateProvince { get; set; }
    public string? Country { get; set; }
    public string? Note { get; set; }
}

/// <summary>One declared place of birth.</summary>
public class SanctionPlaceOfBirthDto
{
    public string? City { get; set; }
    public string? StateProvince { get; set; }
    public string? Country { get; set; }
}

/// <summary>One identity document on record (passport, national ID, etc.).</summary>
public class SanctionDocumentDto
{
    public string? Type { get; set; }
    public string? Type2 { get; set; }
    public string? Number { get; set; }
    public string? IssuingCountry { get; set; }
    public DateTime? DateOfIssue { get; set; }
    public string? Note { get; set; }
}
