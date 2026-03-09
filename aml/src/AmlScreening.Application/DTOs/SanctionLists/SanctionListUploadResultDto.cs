namespace AmlScreening.Application.DTOs.SanctionLists;

public class SanctionListUploadResultDto
{
    public int ImportedCount { get; set; }
    public int ReplacedCount { get; set; }
    public IReadOnlyList<string> Errors { get; set; } = Array.Empty<string>();
}
