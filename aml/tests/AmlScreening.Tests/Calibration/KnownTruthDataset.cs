using System.Text.Json;
using System.Text.Json.Serialization;

namespace AmlScreening.Tests.Calibration;

public class KnownTruthDataset
{
    [JsonPropertyName("comment")] public string? Comment { get; set; }
    [JsonPropertyName("thresholds")] public KnownTruthThresholds Thresholds { get; set; } = new();
    [JsonPropertyName("cases")] public List<KnownTruthCase> Cases { get; set; } = new();

    public static KnownTruthDataset Load()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Calibration", "known-truth.json");
        if (!File.Exists(path))
            throw new FileNotFoundException($"Known-truth dataset not found at: {path}");

        using var stream = File.OpenRead(path);
        var dataset = JsonSerializer.Deserialize<KnownTruthDataset>(stream, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return dataset ?? throw new InvalidOperationException("Failed to parse known-truth.json");
    }
}

public class KnownTruthThresholds
{
    [JsonPropertyName("minNormalizedScore")] public int MinNormalizedScore { get; set; } = 60;
    [JsonPropertyName("expectedTopRank")] public int ExpectedTopRank { get; set; } = 3;
}

public class KnownTruthCase
{
    [JsonPropertyName("id")] public string Id { get; set; } = string.Empty;
    [JsonPropertyName("category")] public string Category { get; set; } = string.Empty;
    [JsonPropertyName("queryName")] public string QueryName { get; set; } = string.Empty;
    [JsonPropertyName("expectedFullName")] public string? ExpectedFullName { get; set; }
    [JsonPropertyName("expectedAliases")] public List<string> ExpectedAliases { get; set; } = new();
    [JsonPropertyName("nationality")] public string? Nationality { get; set; }
    [JsonPropertyName("dateOfBirth")] public string? DateOfBirth { get; set; }

    public bool IsNegativeCase => string.Equals(Category, "negative", StringComparison.OrdinalIgnoreCase);
}
