namespace AmlScreening.Domain.Entities;

public class CustomerType
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}
