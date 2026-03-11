namespace StellarLeasing.Application.WorkflowDefinitions;

public sealed record UpdateWorkflowDraftRequest(
    string Name,
    string? Code,
    int ExpectedRevision,
    IReadOnlyCollection<WorkflowStepInput> Steps,
    IReadOnlyCollection<WorkflowTransitionInput> Transitions);
