using FuzzySharp;

namespace AmlScreening.Infrastructure.Services;

/// <summary>
/// Shared fuzzy match scoring between customer sanction runs and instant list search.
/// </summary>
public static class SanctionListMatchScoring
{
    public const int ThresholdConfirmedMatch = 90;
    public const int ThresholdPossibleMatch = 70;

    private const string MatchTypeFullName = "FullName";
    private const string MatchTypeFirstName = "FirstName";
    private const string MatchTypeLastName = "LastName";
    private const string MatchTypeNationality = "Nationality";
    private const string MatchTypeDateOfBirth = "DateOfBirth";

    public static (double score, string matchType) ComputeMatchScore(
        string customerFullName,
        string customerFirstName,
        string customerLastName,
        string customerNationality,
        DateTime? customerDob,
        string entryFullName,
        string entryFirstName,
        string entrySecondName,
        string entryNationality,
        DateTime? entryDob)
    {
        double fullNameScore = 0;
        if (customerFullName.Length > 0 && entryFullName.Length > 0)
            fullNameScore = Fuzz.TokenSetRatio(customerFullName, entryFullName);

        double firstNameScore = 0;
        if (customerFirstName.Length > 0 && entryFirstName.Length > 0)
            firstNameScore = Fuzz.TokenSetRatio(customerFirstName, entryFirstName);

        double lastNameScore = 0;
        if (customerLastName.Length > 0 && entrySecondName.Length > 0)
            lastNameScore = Fuzz.TokenSetRatio(customerLastName, entrySecondName);

        var nameScore = fullNameScore;
        if (firstNameScore > 0 || lastNameScore > 0)
        {
            var firstLastAvg = (firstNameScore + lastNameScore) / 2.0;
            if (firstNameScore == 0) firstLastAvg = lastNameScore;
            else if (lastNameScore == 0) firstLastAvg = firstNameScore;
            nameScore = Math.Max(fullNameScore, firstLastAvg);
        }

        double nationalityScore = 0;
        if (customerNationality.Length > 0 && entryNationality.Length > 0)
            nationalityScore = Fuzz.TokenSetRatio(customerNationality, entryNationality);

        double dobScore = 0;
        if (customerDob.HasValue && entryDob.HasValue)
            dobScore = customerDob.Value.Date == entryDob.Value.Date ? 100 : 0;

        var weights = 0.0;
        var total = 0.0;
        if (customerFullName.Length > 0 || entryFullName.Length > 0 || customerFirstName.Length > 0 || entryFirstName.Length > 0 || customerLastName.Length > 0 || entrySecondName.Length > 0)
        {
            weights += 0.5;
            total += 0.5 * nameScore;
        }

        if (customerNationality.Length > 0 || entryNationality.Length > 0)
        {
            weights += 0.3;
            total += 0.3 * nationalityScore;
        }

        if (customerDob.HasValue || entryDob.HasValue)
        {
            weights += 0.2;
            total += 0.2 * dobScore;
        }

        var score = weights > 0 ? total / weights : 0;

        var matchType = MatchTypeFullName;
        if (firstNameScore >= nameScore && firstNameScore >= lastNameScore && firstNameScore >= nationalityScore && firstNameScore >= dobScore)
            matchType = MatchTypeFirstName;
        else if (lastNameScore >= nameScore && lastNameScore >= nationalityScore && lastNameScore >= dobScore)
            matchType = MatchTypeLastName;
        else if (nationalityScore >= nameScore && nationalityScore >= dobScore)
            matchType = MatchTypeNationality;
        else if (dobScore >= nameScore && dobScore >= nationalityScore)
            matchType = MatchTypeDateOfBirth;

        return (Math.Round(score, 2), matchType);
    }

    public static string GetStatusFromScore(double score)
    {
        if (score >= ThresholdConfirmedMatch) return "ConfirmedMatch";
        if (score >= ThresholdPossibleMatch) return "PossibleMatch";
        return "Clear";
    }
}
