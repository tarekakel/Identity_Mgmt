using AmlScreening.Application.Common;
using AmlScreening.Application.DTOs.IndividualBulkUpload;

namespace AmlScreening.Application.Interfaces;

public interface IIndividualBulkUploadService
{
    Task<ApiResponse<IndividualBulkUploadResultDto>> UploadAsync(
        Stream fileStream,
        string fileName,
        IndividualBulkUploadOptionsDto options,
        CancellationToken cancellationToken = default);

    byte[] GetSampleWorkbookBytes();

    Task<ApiResponse<IReadOnlyList<IndividualBulkUploadBatchListItemDto>>> GetBatchesAsync(
        DateTime? fromUtc,
        DateTime? toUtc,
        string? uploadedBy,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<IReadOnlyList<IndividualBulkUploadLineDetailDto>>> GetBatchLinesAsync(
        Guid batchId,
        string? caseStatus,
        CancellationToken cancellationToken = default);
}
