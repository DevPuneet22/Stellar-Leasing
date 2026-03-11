using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using StellarLeasing.Domain.WorkflowDefinitions;
using StellarLeasing.Infrastructure.Tenancy;

namespace StellarLeasing.Infrastructure.Persistence;

public static class DatabaseInitializationExtensions
{
    public static async Task InitializeStellarLeasingDatabaseAsync(
        this IServiceProvider services,
        DatabaseInitializationOptions options,
        CancellationToken cancellationToken = default)
    {
        using var scope = services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<StellarLeasingDbContext>();
        var tenantOptions = scope.ServiceProvider.GetRequiredService<IOptions<TenantAccessOptions>>().Value;

        if (options.ApplyMigrationsOnStartup)
        {
            await dbContext.Database.MigrateAsync(cancellationToken);
        }

        if (!options.SeedDemoData || await dbContext.WorkflowDefinitions.AnyAsync(cancellationToken))
        {
            return;
        }

        var definition = WorkflowDefinition.Create(
            tenantOptions.DefaultTenantId,
            "Lease Approval",
            "LEASE-APPROVAL");

        definition.ActivateVersion(1);

        dbContext.WorkflowDefinitions.Add(definition);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
