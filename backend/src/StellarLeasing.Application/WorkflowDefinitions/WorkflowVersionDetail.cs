namespace StellarLeasing.Application.WorkflowDefinitions;

public sealed record WorkflowVersionDetail(
    int VersionNumber,
    string Status,
    IReadOnlyCollection<WorkflowStepModel> Steps,
    IReadOnlyCollection<WorkflowTransitionModel> Transitions);
