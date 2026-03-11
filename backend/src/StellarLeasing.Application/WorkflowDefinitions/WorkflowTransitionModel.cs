namespace StellarLeasing.Application.WorkflowDefinitions;

public sealed record WorkflowTransitionModel(
    string From,
    string To,
    string? Condition,
    int SortOrder);
