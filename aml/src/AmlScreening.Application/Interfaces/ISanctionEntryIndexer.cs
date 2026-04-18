using AmlScreening.Domain.Entities;

namespace AmlScreening.Application.Interfaces;

public interface ISanctionEntryIndexer
{
    Task EnsureIndexAsync(CancellationToken cancellationToken = default);
    Task IndexAsync(SanctionListEntry entry, CancellationToken cancellationToken = default);
    Task IndexBulkAsync(IEnumerable<SanctionListEntry> entries, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid entryId, CancellationToken cancellationToken = default);
    Task DeleteByListSourceAsync(string listSource, CancellationToken cancellationToken = default);
    Task<long> ReindexAllAsync(IEnumerable<SanctionListEntry> entries, CancellationToken cancellationToken = default);
    Task<long> CountAsync(CancellationToken cancellationToken = default);
}
