using AmlScreening.Application.Common;
using AmlScreening.Application.DTOs.Customers;

namespace AmlScreening.Application.Interfaces;

public interface ICustomerService
{
    Task<ApiResponse<PagedResult<CustomerDto>>> GetPagedAsync(PagedRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<CustomerDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse<CustomerDto>> CreateAsync(CreateCustomerDto dto, CancellationToken cancellationToken = default);
    Task<ApiResponse<CustomerDto>> UpdateAsync(Guid id, UpdateCustomerDto dto, CancellationToken cancellationToken = default);
    Task<ApiResponse> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
