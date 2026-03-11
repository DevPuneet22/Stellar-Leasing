using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StellarLeasing.Domain.WorkflowDefinitions;

namespace StellarLeasing.Infrastructure.Persistence.Configurations;

public sealed class WorkflowStepConfiguration : IEntityTypeConfiguration<WorkflowStep>
{
    public void Configure(EntityTypeBuilder<WorkflowStep> builder)
    {
        builder.ToTable("workflow_steps");

        builder.HasKey(x => x.Id);

        builder.Property<Guid>("WorkflowVersionId")
            .IsRequired();

        builder.Property(x => x.Key)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.StepType)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.AssigneeRule)
            .HasMaxLength(200);

        builder.Property(x => x.SortOrder)
            .IsRequired();

        builder.Property(x => x.PositionX)
            .IsRequired();

        builder.Property(x => x.PositionY)
            .IsRequired();

        builder.HasIndex("WorkflowVersionId", nameof(WorkflowStep.Key))
            .IsUnique();
    }
}
