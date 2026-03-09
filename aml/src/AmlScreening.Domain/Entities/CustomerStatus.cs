namespace AmlScreening.Domain.Entities;

public class CustomerStatus
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}
