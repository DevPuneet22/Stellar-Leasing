using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StellarLeasing.Domain.WorkflowDefinitions;

namespace StellarLeasing.Infrastructure.Persistence.Configurations;

public sealed class WorkflowVersionConfiguration : IEntityTypeConfiguration<WorkflowVersion>
{
    public void Configure(EntityTypeBuilder<WorkflowVersion> builder)
    {
        builder.ToTable("workflow_versions");

        builder.HasKey(x => x.Id);

        builder.Property<Guid>("WorkflowDefinitionId")
            .IsRequired();

        builder.Property(x => x.VersionNumber)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.HasIndex("WorkflowDefinitionId", nameof(WorkflowVersion.VersionNumber))
            .IsUnique();

        builder.HasMany(x => x.Steps)
            .WithOne()
            .HasForeignKey("WorkflowVersionId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Transitions)
            .WithOne()
            .HasForeignKey("WorkflowVersionId")
            .OnDelete(DeleteBehavior.Cascade);
    }
}