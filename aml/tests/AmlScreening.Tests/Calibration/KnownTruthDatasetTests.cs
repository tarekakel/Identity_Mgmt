namespace AmlScreening.Tests.Calibration;

public class KnownTruthDatasetTests
{
    [Fact]
    public void DatasetLoads_AndIsNotEmpty()
    {
        var dataset = KnownTruthDataset.Load();

        Assert.NotNull(dataset);
        Assert.NotEmpty(dataset.Cases);
        Assert.True(dataset.Thresholds.MinNormalizedScore is > 0 and <= 100);
        Assert.True(dataset.Thresholds.ExpectedTopRank > 0);
    }

    [Fact]
    public void EveryCase_HasUniqueId_NonEmptyQuery_AndCategory()
    {
        var dataset = KnownTruthDataset.Load();
        var ids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var c in dataset.Cases)
        {
            Assert.False(string.IsNullOrWhiteSpace(c.Id), $"Case is missing Id: {c.QueryName}");
            Assert.False(string.IsNullOrWhiteSpace(c.QueryName), $"Case '{c.Id}' has empty QueryName.");
            Assert.False(string.IsNullOrWhiteSpace(c.Category), $"Case '{c.Id}' has empty Category.");
            Assert.True(ids.Add(c.Id), $"Duplicate case id: {c.Id}");
        }
    }

    [Fact]
    public void EveryPositiveCase_HasExpectedFullNameOrAlias()
    {
        var dataset = KnownTruthDataset.Load();

        foreach (var c in dataset.Cases.Where(x => !x.IsNegativeCase))
        {
            var hasFullName = !string.IsNullOrWhiteSpace(c.ExpectedFullName);
            var hasAlias = c.ExpectedAliases is { Count: > 0 } && c.ExpectedAliases.Any(a => !string.IsNullOrWhiteSpace(a));
            Assert.True(hasFullName || hasAlias,
                $"Positive case '{c.Id}' must define ExpectedFullName or ExpectedAliases.");
        }
    }

    [Fact]
    public void Dataset_CoversAllCalibrationCategories()
    {
        var dataset = KnownTruthDataset.Load();
        var categories = dataset.Cases
            .Select(c => c.Category.ToLowerInvariant())
            .ToHashSet();

        var required = new[] { "exact", "typo", "alias-only", "transliteration" };
        foreach (var r in required)
        {
            Assert.Contains(r, categories);
        }
    }
}
