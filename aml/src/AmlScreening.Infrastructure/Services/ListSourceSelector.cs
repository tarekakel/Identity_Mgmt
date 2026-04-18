using AmlScreening.Domain.Entities;

namespace AmlScreening.Infrastructure.Services;

/// <summary>
/// Maps the boolean category toggles on Individual / Corporate screening requests
/// to concrete <see cref="SanctionListUploadService"/> source identifiers used in the
/// Elasticsearch index. If nothing is selected, all known sources are returned so a
/// run is never silently empty.
/// </summary>
internal static class ListSourceSelector
{
    public static IReadOnlyCollection<string> GetSelectedSources(IndividualScreeningRequest request)
    {
        var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (request.CheckSanctions)
        {
            set.Add(SanctionListUploadService.SourceUn);
            set.Add(SanctionListUploadService.SourceUae);
            set.Add(SanctionListUploadService.SourceOfac);
        }
        if (request.CheckPepUkOnly) set.Add(SanctionListUploadService.SourcePepUk);
        if (request.CheckProfileOfInterest) set.Add(SanctionListUploadService.SourceProfileOfInterest);
        if (request.CheckReputationalRiskExposure) set.Add(SanctionListUploadService.SourceAdverseMedia);

        return EnsureNonEmpty(set);
    }

    public static IReadOnlyCollection<string> GetSelectedSources(CorporateScreeningRequest request)
    {
        var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (request.CheckSanctions)
        {
            set.Add(SanctionListUploadService.SourceUn);
            set.Add(SanctionListUploadService.SourceUae);
            set.Add(SanctionListUploadService.SourceOfac);
        }
        if (request.CheckPepUkOnly) set.Add(SanctionListUploadService.SourcePepUk);
        if (request.CheckProfileOfInterest) set.Add(SanctionListUploadService.SourceProfileOfInterest);
        if (request.CheckReputationalRiskExposure) set.Add(SanctionListUploadService.SourceAdverseMedia);

        return EnsureNonEmpty(set);
    }

    private static HashSet<string> EnsureNonEmpty(HashSet<string> set)
    {
        if (set.Count == 0)
        {
            set.Add(SanctionListUploadService.SourceUn);
            set.Add(SanctionListUploadService.SourceUae);
            set.Add(SanctionListUploadService.SourceOfac);
        }
        return set;
    }
}
