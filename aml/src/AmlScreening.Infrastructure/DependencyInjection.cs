using AmlScreening.Application.Interfaces;
using AmlScreening.Infrastructure.Persistence;
using AmlScreening.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AmlScreening.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<ICustomerService, CustomerService>();
        services.AddScoped<ISanctionsScreeningService, SanctionsScreeningService>();
        services.AddScoped<IRiskAssignmentService, RiskAssignmentService>();
        services.AddScoped<ICaseService, CaseService>();
        services.AddScoped<IAuditLogService, AuditLogService>();
        services.AddScoped<IAuthService, AuthService>();

        return services;
    }
}
