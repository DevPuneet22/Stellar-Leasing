using Microsoft.EntityFrameworkCore;
using StellarLeasing.Domain.WorkflowDefinitions;

namespace StellarLeasing.Infrastructure.Persistence;
public class StellarLeasingDbContext : DbContext
{
    public StellarLeasingDbContext(DbContextOptions<StellarLeasingDbContext> options) : base(options)
    {
    }

    public DbSet<WorkflowDefinition> WorkflowDefinitions => Set<WorkflowDefinition>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(StellarLeasingDbContext).Assembly);
    }
}