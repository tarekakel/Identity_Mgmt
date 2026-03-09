using AmlScreening.Application.Common;
using AmlScreening.Application.DTOs.Customers;

namespace AmlScreening.Application.Interfaces;

public interface ICustomerDocumentService
{
    Task<ApiResponse<CustomerDocumentDto>> UploadAsync(Guid customerId, string documentTypeCode, Stream fileContent, string fileName, string? contentType, DateTime? expiryDate, CancellationToken cancellationToken = default);
    Task<ApiResponse<IReadOnlyList<CustomerDocumentDto>>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task<ApiResponse<(Stream Content, string FileName, string ContentType)>> GetDownloadAsync(Guid customerId, Guid documentId, CancellationToken cancellationToken = default);
}
