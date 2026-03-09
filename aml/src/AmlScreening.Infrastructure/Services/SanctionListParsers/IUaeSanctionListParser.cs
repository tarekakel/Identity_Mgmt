using AmlScreening.Domain.Entities;

namespace AmlScreening.Infrastructure.Services.SanctionListParsers;

public interface IUaeSanctionListParser
{
    IReadOnlyList<SanctionListEntry> Parse(Stream stream, string listSourceName, string fileName);
}
