using AmlScreening.Application.Common;
using AmlScreening.Application.DTOs.CorporateKyc;

namespace AmlScreening.Application.Interfaces;

public interface ICorporateKycDocumentService
{
    Task<ApiResponse<IReadOnlyList<CorporateKycDocumentDto>>> GetByCustomerIdAsync(
        Guid customerId,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<CorporateKycDocumentDto>> UploadAsync(
        Guid customerId,
        UploadCorporateKycDocumentRequestDto dto,
        Stream fileContent,
        string fileName,
        string? contentType,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<(Stream Content, string FileName, string ContentType)>> GetDownloadAsync(
        Guid customerId,
        Guid documentId,
        CancellationToken cancellationToken = default);

    Task<ApiResponse> DeleteAsync(Guid customerId, Guid documentId, CancellationToken cancellationToken = default);
}
