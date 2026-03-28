using AmlScreening.Application.Interfaces;
using AmlScreening.Infrastructure.Options;
using AmlScreening.Infrastructure.Persistence;
using AmlScreening.Infrastructure.Services;
using AmlScreening.Infrastructure.Services.SanctionListParsers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace AmlScreening.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.Configure<IdentityApiOptions>(configuration.GetSection(IdentityApiOptions.SectionName));
        services.Configure<FileStorageOptions>(configuration.GetSection(FileStorageOptions.SectionName));
        services.AddHttpClient("IdentityApi", (sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<IdentityApiOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/");
            client.Timeout = TimeSpan.FromSeconds(10);
        });

        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<ICustomerService, CustomerService>();
        services.AddScoped<ISanctionsScreeningService, SanctionsScreeningService>();
        services.AddScoped<IRunSanctionsScreeningService, RunSanctionsScreeningService>();
        services.AddScoped<IIndividualScreeningRunnerService, IndividualScreeningRunnerService>();
        services.AddScoped<IIndividualScreeningService, IndividualScreeningService>();
        services.AddScoped<ICorporateScreeningRunnerService, CorporateScreeningRunnerService>();
        services.AddScoped<ICorporateScreeningService, CorporateScreeningService>();
        services.AddScoped<ISanctionActionAuditLogService, SanctionActionAuditLogService>();
        services.AddScoped<IRiskAssignmentService, RiskAssignmentService>();
        services.AddScoped<ICaseService, CaseService>();
        services.AddScoped<IAuditLogService, AuditLogService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ILookupService, LookupService>();
        services.AddScoped<IFileStorageService, FileStorageService>();
        services.AddScoped<ICustomerDocumentService, CustomerDocumentService>();
        services.AddScoped<IIndividualKycService, IndividualKycService>();
        services.AddScoped<IIndividualKycDocumentService, IndividualKycDocumentService>();
        services.AddScoped<IUnConsolidatedListParser, UnConsolidatedListParser>();
        services.AddScoped<IUaeSanctionListParser, UaeSanctionListParser>();
        services.AddScoped<ISanctionListUploadService, SanctionListUploadService>();

        return services;
    }
}
