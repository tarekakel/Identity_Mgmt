using AmlScreening.Application.Interfaces;

namespace AmlScreening.Tests.Calibration;

/// <summary>
/// Pluggable harness for running the known-truth dataset against any
/// <see cref="IScreeningEngine"/> implementation. Used for offline calibration
/// of <c>topScoreReference</c>, field boosts and <c>min_score</c> tuning.
///
/// Usage (e.g. from a console runner or an integration test):
///     var report = await new ScreeningCalibrationHarness(engine).RunAsync();
///     foreach (var row in report.Rows) Console.WriteLine(row);
/// </summary>
public sealed class ScreeningCalibrationHarness
{
    private readonly IScreeningEngine _engine;
    private readonly KnownTruthDataset _dataset;

    public ScreeningCalibrationHarness(IScreeningEngine engine, KnownTruthDataset? dataset = null)
    {
        _engine = engine ?? throw new ArgumentNullException(nameof(engine));
        _dataset = dataset ?? KnownTruthDataset.Load();
    }

    public async Task<CalibrationReport> RunAsync(CancellationToken cancellationToken = default)
    {
        var rows = new List<CalibrationRow>();

        foreach (var c in _dataset.Cases)
        {
            var query = new ScreeningQuery
            {
                FullName = c.QueryName,
                Nationality = c.Nationality,
                DateOfBirth = TryParseDate(c.DateOfBirth),
                Threshold0to100 = _dataset.Thresholds.MinNormalizedScore,
                Top = Math.Max(_dataset.Thresholds.ExpectedTopRank * 2, 10)
            };

            var candidates = await _engine.SearchAsync(query, cancellationToken);

            int? matchedRank = null;
            ScreeningCandidate? matched = null;
            for (var i = 0; i < candidates.Count; i++)
            {
                if (IsExpectedHit(c, candidates[i]))
                {
                    matchedRank = i + 1;
                    matched = candidates[i];
                    break;
                }
            }

            var passed = c.IsNegativeCase
                ? candidates.All(x => x.NormalizedScore0to100 < _dataset.Thresholds.MinNormalizedScore)
                : matchedRank.HasValue
                  && matchedRank.Value <= _dataset.Thresholds.ExpectedTopRank
                  && (matched?.NormalizedScore0to100 ?? 0) >= _dataset.Thresholds.MinNormalizedScore;

            rows.Add(new CalibrationRow(
                CaseId: c.Id,
                Category: c.Category,
                Query: c.QueryName,
                Expected: c.ExpectedFullName ?? string.Join(" / ", c.ExpectedAliases),
                MatchedRank: matchedRank,
                MatchedScore: matched?.NormalizedScore0to100,
                Passed: passed));
        }

        return new CalibrationReport(rows, _dataset.Thresholds);
    }

    private static bool IsExpectedHit(KnownTruthCase truth, ScreeningCandidate candidate)
    {
        if (!string.IsNullOrWhiteSpace(truth.ExpectedFullName) &&
            string.Equals(truth.ExpectedFullName, candidate.FullName, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (truth.ExpectedAliases is { Count: > 0 } && !string.IsNullOrWhiteSpace(candidate.MatchedAlias))
        {
            return truth.ExpectedAliases.Any(a =>
                string.Equals(a, candidate.MatchedAlias, StringComparison.OrdinalIgnoreCase));
        }

        return false;
    }

    private static DateTime? TryParseDate(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        return DateTime.TryParse(value, out var dt) ? dt : null;
    }
}

public sealed record CalibrationRow(
    string CaseId,
    string Category,
    string Query,
    string Expected,
    int? MatchedRank,
    double? MatchedScore,
    bool Passed)
{
    public override string ToString() =>
        $"[{(Passed ? "PASS" : "FAIL")}] {CaseId,-20} cat={Category,-15} rank={MatchedRank?.ToString() ?? "-",-3} score={MatchedScore?.ToString("0.0") ?? "-",-5} q='{Query}' -> '{Expected}'";
}

public sealed record CalibrationReport(IReadOnlyList<CalibrationRow> Rows, KnownTruthThresholds Thresholds)
{
    public int Total => Rows.Count;
    public int Passed => Rows.Count(r => r.Passed);
    public int Failed => Rows.Count(r => !r.Passed);
    public double PassRate => Total == 0 ? 0 : (double)Passed / Total;
}
