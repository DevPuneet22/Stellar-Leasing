namespace StellarLeasing.Domain.WorkflowDefinitions;

public sealed class WorkflowTransition
{
    private WorkflowTransition() { } // EF core
    public WorkflowTransition(string fromStepKey, string toStepKey, string? conditionName = null, int sortOrder = 0)
    {
        Id = Guid.NewGuid();
        FromStepKey = string.IsNullOrWhiteSpace(fromStepKey)
            ? throw new ArgumentException("Transition source step is required.", nameof(fromStepKey))
            : fromStepKey.Trim();
        ToStepKey = string.IsNullOrWhiteSpace(toStepKey)
            ? throw new ArgumentException("Transition target step is required.", nameof(toStepKey))
            : toStepKey.Trim();
        ConditionName = string.IsNullOrWhiteSpace(conditionName) ? null : conditionName.Trim();
        SortOrder = sortOrder;
    }

    public Guid Id { get; private set; }

    public string FromStepKey { get; private set; } = null!;

    public string ToStepKey { get; private set; } = null!;

    public string? ConditionName { get; private set; }

    public int SortOrder { get; private set; }

    public WorkflowTransition CopyForDraft(int sortOrder)
    {
        return new WorkflowTransition(FromStepKey, ToStepKey, ConditionName, sortOrder);
    }
}
