namespace StellarLeasing.Domain.WorkflowDefinitions;

public sealed class WorkflowStep
{
    public Guid Id {get; private set;}
    public WorkflowStep(string key, string name, WorkflowStepType stepType, string? assigneeRule = null)
    {
        Id = Guid.NewGuid();
        Key = key;
        Name = name;
        StepType = stepType;
        AssigneeRule = assigneeRule;
    }

    private WorkflowStep() { } // EF core
    public string Key { get; private set; } = null!;

    public string Name { get; private set; } = null!;

    public WorkflowStepType StepType { get; private set; }

    public string? AssigneeRule { get; private set; }
}
