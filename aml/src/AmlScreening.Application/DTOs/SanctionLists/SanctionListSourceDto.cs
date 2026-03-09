namespace AmlScreening.Application.DTOs.SanctionLists;

public class SanctionListSourceDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string FileFormat { get; set; } = string.Empty; // e.g. "XML", "XLS/XLSX"
}
