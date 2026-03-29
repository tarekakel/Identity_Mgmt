using AmlScreening.Application.Common;
using AmlScreening.Application.DTOs.Dashboard;

namespace AmlScreening.Application.Interfaces;

public interface IDashboardService
{
    Task<ApiResponse<CustomerDashboardKpisDto>> GetCustomerKpisAsync(CancellationToken cancellationToken = default);
}
