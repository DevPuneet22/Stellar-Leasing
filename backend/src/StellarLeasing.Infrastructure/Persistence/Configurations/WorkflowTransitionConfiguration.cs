using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StellarLeasing.Domain.WorkflowDefinitions;

namespace StellarLeasing.Infrastructure.Persistence.Configurations;

public sealed class WorkflowTransitionConfiguration : IEntityTypeConfiguration<WorkflowTransition>
{
    public void Configure(EntityTypeBuilder<WorkflowTransition> builder)
    {
        builder.ToTable("workflow_transitions");

        builder.HasKey(x => x.Id);

        builder.Property<Guid>("WorkflowVersionId")
            .IsRequired();

        builder.Property(x => x.FromStepKey)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.ToStepKey)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.ConditionName)
            .HasMaxLength(100);
    }
}