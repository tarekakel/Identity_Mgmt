using System.Text.Json;

namespace AmlScreening.Application.DTOs.CorporateKyc;

public class CorporateKycDto
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid CustomerId { get; set; }
    public bool IsActive { get; set; }
    public bool IsDeleted { get; set; }

    public JsonElement? FormPayload { get; set; }
}
