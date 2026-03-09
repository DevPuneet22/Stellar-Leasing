namespace StellarLeasing.Domain.WorkflowDefinitions;

public sealed class WorkflowTransition
{
    private WorkflowTransition() { } // EF core
    public WorkflowTransition(string fromStepKey, string toStepKey, string? conditionName = null)
    {
        Id = Guid.NewGuid();
        FromStepKey = fromStepKey;
        ToStepKey = toStepKey;
        ConditionName = conditionName;
    }

    public Guid Id { get; private set; }

    public string FromStepKey { get; private set; } = null!;

    public string ToStepKey { get; private set; } = null!;

    public string? ConditionName { get; private set; }
}
