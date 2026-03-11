namespace StellarLeasing.Application.WorkflowDefinitions;

public sealed record WorkflowTransitionInput(
    string From,
    string To,
    string? Condition);
