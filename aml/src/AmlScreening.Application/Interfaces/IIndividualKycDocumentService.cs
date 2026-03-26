using AmlScreening.Application.Common;
using AmlScreening.Application.DTOs.IndividualKyc;
using System.IO;

namespace AmlScreening.Application.Interfaces;

public interface IIndividualKycDocumentService
{
    Task<ApiResponse<IReadOnlyList<IndividualKycDocumentDto>>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default);

    Task<ApiResponse<IndividualKycDocumentDto>> UploadAsync(
        Guid customerId,
        UploadIndividualKycDocumentRequestDto dto,
        Stream fileContent,
        string fileName,
        string? contentType,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<(Stream Content, string FileName, string ContentType)>> GetDownloadAsync(Guid customerId, Guid documentId, CancellationToken cancellationToken = default);

    Task<ApiResponse> DeleteAsync(Guid customerId, Guid documentId, CancellationToken cancellationToken = default);
}

