namespace StellarLeasing.Application.WorkflowDefinitions;

public sealed record WorkflowVersionSummary(
    int VersionNumber,
    string Status,
    int StepCount,
    int TransitionCount);
