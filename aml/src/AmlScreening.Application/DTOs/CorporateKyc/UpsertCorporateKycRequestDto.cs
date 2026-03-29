using System.Text.Json;

namespace AmlScreening.Application.DTOs.CorporateKyc;

public class UpsertCorporateKycRequestDto
{
    public Guid TenantId { get; set; }

    public JsonElement? FormPayload { get; set; }
}
