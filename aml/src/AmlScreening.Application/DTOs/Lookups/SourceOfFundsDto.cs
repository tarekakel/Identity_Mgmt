namespace AmlScreening.Application.DTOs.Lookups;

public class SourceOfFundsDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}
