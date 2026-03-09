namespace StellarLeasing.Domain.WorkflowDefinitions;

public sealed class WorkflowVersion
{
    private readonly List<WorkflowStep> _steps= [];
    private readonly List<WorkflowTransition> _transitions = [];

    private WorkflowVersion() { } // EF core
    private WorkflowVersion(
        int versionNumber,
        WorkflowVersionStatus status,
        IEnumerable<WorkflowStep> steps,
        IEnumerable<WorkflowTransition> transitions)
    {
        Id = Guid.NewGuid();
        VersionNumber = versionNumber;
        Status = status;
        _steps.AddRange(steps);
        _transitions.AddRange(transitions);
    }

    public Guid Id { get; private set; }
    public int VersionNumber { get; private set; }

    public WorkflowVersionStatus Status { get; private set; }

    public IReadOnlyCollection<WorkflowStep> Steps => _steps.AsReadOnly();

    public IReadOnlyCollection<WorkflowTransition> Transitions => _transitions.AsReadOnly();

    public static WorkflowVersion CreateDraft(
        int versionNumber,
        IEnumerable<WorkflowStep> steps,
        IEnumerable<WorkflowTransition> transitions)
    {
        return new WorkflowVersion(versionNumber, WorkflowVersionStatus.Draft, steps, transitions);
    }

    public void Activate()
    {
        Status = WorkflowVersionStatus.Active;
    }

    public void Retire()
    {
        Status = WorkflowVersionStatus.Retired;
    }
}
