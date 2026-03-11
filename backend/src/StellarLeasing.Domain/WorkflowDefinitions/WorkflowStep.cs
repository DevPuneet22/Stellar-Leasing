namespace StellarLeasing.Domain.WorkflowDefinitions;

public sealed class WorkflowStep
{
    public Guid Id {get; private set;}
    public WorkflowStep(
        string key,
        string name,
        WorkflowStepType stepType,
        string? assigneeRule = null,
        int sortOrder = 0,
        double positionX = 0,
        double positionY = 0)
    {
        Id = Guid.NewGuid();
        Key = string.IsNullOrWhiteSpace(key)
            ? throw new ArgumentException("Step key is required.", nameof(key))
            : key.Trim();
        Name = string.IsNullOrWhiteSpace(name)
            ? throw new ArgumentException("Step name is required.", nameof(name))
            : name.Trim();
        StepType = stepType;
        AssigneeRule = string.IsNullOrWhiteSpace(assigneeRule) ? null : assigneeRule.Trim();
        SortOrder = sortOrder;
        PositionX = positionX;
        PositionY = positionY;
    }

    private WorkflowStep() { } // EF core
    public string Key { get; private set; } = null!;

    public string Name { get; private set; } = null!;

    public WorkflowStepType StepType { get; private set; }

    public string? AssigneeRule { get; private set; }

    public int SortOrder { get; private set; }

    public double PositionX { get; private set; }

    public double PositionY { get; private set; }

    public WorkflowStep CopyForDraft(int sortOrder)
    {
        return new WorkflowStep(Key, Name, StepType, AssigneeRule, sortOrder, PositionX, PositionY);
    }
}
