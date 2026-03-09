using AmlScreening.Domain.Entities;

namespace AmlScreening.Infrastructure.Services.SanctionListParsers;

public interface IUnConsolidatedListParser
{
    IReadOnlyList<SanctionListEntry> Parse(Stream xmlStream, string listSourceName);
}
