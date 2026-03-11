namespace StellarLeasing.Application.WorkflowDefinitions;

public sealed record WorkflowStepInput(
    string Key,
    string Name,
    string Type,
    string? AssigneeRule,
    double PositionX,
    double PositionY);
