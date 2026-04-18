namespace AmlScreening.Domain.Entities.SanctionList;

/// <summary>
/// One alias name with optional source-list quality.
/// UN XML: <c>QUALITY</c> = "Good"/"Low" for individuals or "a.k.a."/"f.k.a." for entities.
/// </summary>
public class SanctionAlias
{
    public string Name { get; set; } = string.Empty;
    public string? Quality { get; set; }
}
