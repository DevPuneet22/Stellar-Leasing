using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StellarLeasing.Application.Abstractions;
using StellarLeasing.Infrastructure.Persistence;
using StellarLeasing.Infrastructure.Tenancy;

namespace StellarLeasing.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Connection string 'Default' is not configured.");
        }

        services.AddDbContext<StellarLeasingDbContext>(options =>
            options.UseNpgsql(connectionString));
        services.AddHttpContextAccessor();
        services.Configure<TenantAccessOptions>(configuration.GetSection(TenantAccessOptions.SectionName));

        services.AddScoped<IWorkflowDefinitionRepository, PostgresWorkflowDefinitionRepository>();
        services.AddScoped<ICurrentTenantAccessor, CurrentTenantAccessor>();
        return services;
    }
}
