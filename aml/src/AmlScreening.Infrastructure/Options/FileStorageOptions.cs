namespace AmlScreening.Infrastructure.Options;

public class FileStorageOptions
{
    public const string SectionName = "FileStorage";

    /// <summary>Base directory path for storing uploaded files (e.g. outside web root).</summary>
    public string BasePath { get; set; } = string.Empty;

    /// <summary>Max file size in bytes (default 10MB).</summary>
    public long MaxFileSizeBytes { get; set; } = 10 * 1024 * 1024;
}
