using AmlScreening.Application.Common;
using AmlScreening.Application.DTOs.Dashboard;
using AmlScreening.Application.Interfaces;
using AmlScreening.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AmlScreening.Infrastructure.Services;

public class DashboardService : IDashboardService
{
    private readonly ApplicationDbContext _context;

    public DashboardService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<CustomerDashboardKpisDto>> GetCustomerKpisAsync(CancellationToken cancellationToken = default)
    {
        var total = await _context.Customers.AsNoTracking().CountAsync(cancellationToken);

        var countsByCode = await _context.Customers
            .AsNoTracking()
            .GroupBy(c => c.Status.Code)
            .Select(g => new { Code = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Code, x => x.Count, cancellationToken);

        static int Get(Dictionary<string, int> map, string code) =>
            map.TryGetValue(code, out var n) ? n : 0;

        var dto = new CustomerDashboardKpisDto
        {
            TotalCustomers = total,
            AutoApproved = Get(countsByCode, "AutoApproved"),
            Approved = Get(countsByCode, "Approved"),
            Rejected = Get(countsByCode, "Rejected"),
            PendingMaker = Get(countsByCode, "PendingMaker"),
            PendingChecker = Get(countsByCode, "PendingChecker"),
            PendingScheduler = Get(countsByCode, "PendingScheduler")
        };

        return ApiResponse<CustomerDashboardKpisDto>.Ok(dto);
    }
}
