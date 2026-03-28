using AmlScreening.Application.Common;
using AmlScreening.Application.DTOs.CorporateBulkUpload;

namespace AmlScreening.Application.Interfaces;

public interface ICorporateBulkUploadService
{
    byte[] GetSampleWorkbookBytes();

    Task<ApiResponse<CorporateBulkUploadResultDto>> UploadAsync(
        Stream fileStream,
        string fileName,
        CorporateBulkUploadOptionsDto options,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<IReadOnlyList<CorporateBulkUploadBatchListItemDto>>> GetBatchesAsync(
        DateTime? fromUtc,
        DateTime? toUtc,
        string? uploadedBy,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<IReadOnlyList<CorporateBulkUploadLineDetailDto>>> GetBatchLinesAsync(
        Guid batchId,
        string? caseStatus,
        CancellationToken cancellationToken = default);
}
