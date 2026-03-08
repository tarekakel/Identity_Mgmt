namespace AmlScreening.Domain.Interfaces;

public interface ISoftDelete
{
    bool IsDeleted { get; set; }
    bool IsActive { get; set; }
}
