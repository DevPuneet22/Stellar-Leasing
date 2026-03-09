using Microsoft.EntityFrameworkCore;
using StellarLeasing.Domain.WorkflowDefinitions;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace StellarLeasing.Infrastructure.Persistence.Configurations;

public sealed class WorkflowDefinitionConfiguration : IEntityTypeConfiguration<WorkflowDefinition>
{
    public void Configure(EntityTypeBuilder<WorkflowDefinition> builder)
    {
        builder.ToTable("WorkflowDefinitions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.TenantId)
            .IsRequired();

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Code)
            .IsRequired()
            .HasMaxLength(100);
        
        builder.HasIndex(x => new { x.TenantId, x.Code })
            .IsUnique();
        
        builder.HasMany(x => x.Versions)
            .WithOne()
            .HasForeignKey("WorkflowDefinitionId")
            .OnDelete(DeleteBehavior.Cascade);
    }
}