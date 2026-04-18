namespace AmlScreening.Infrastructure.Options;

public class ElasticsearchOptions
{
    public const string SectionName = "Elasticsearch";

    public string Url { get; set; } = "http://localhost:9200";
    public string IndexName { get; set; } = "sanction-entries";
    public string? Username { get; set; }
    public string? Password { get; set; }
    public string? ApiKey { get; set; }

    /// <summary>
    /// Reference raw ES score used to normalize hits to a 0-100 scale.
    /// Tune via integration tests against a known-truth dataset.
    /// </summary>
    public double TopScoreReference { get; set; } = 30.0;

    /// <summary>Max candidates returned per ES query.</summary>
    public int MaxCandidates { get; set; } = 50;
}
