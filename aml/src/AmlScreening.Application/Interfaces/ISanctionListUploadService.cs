using AmlScreening.Application.Common;
using AmlScreening.Application.DTOs.SanctionLists;

namespace AmlScreening.Application.Interfaces;

public interface ISanctionListUploadService
{
    Task<ApiResponse<IReadOnlyList<SanctionListSourceDto>>> GetSourcesAsync(CancellationToken cancellationToken = default);
    Task<ApiResponse<SanctionListUploadResultDto>> UploadAsync(string listSourceId, Stream fileContent, string fileName, CancellationToken cancellationToken = default);
    Task<ApiResponse<PagedResult<SanctionListEntryDto>>> GetEntriesAsync(string? searchTerm, string? listSource, int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<ApiResponse<int>> DeleteByListSourceAsync(string listSource, CancellationToken cancellationToken = default);
    Task<ApiResponse<SanctionListEntryDto>> CreateEntryAsync(CreateSanctionListEntryDto dto, CancellationToken cancellationToken = default);
}
