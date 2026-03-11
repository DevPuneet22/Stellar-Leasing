namespace StellarLeasing.Application.WorkflowDefinitions;

public sealed record WorkflowStepModel(
    string Key,
    string Name,
    string Type,
    string? AssigneeRule,
    int SortOrder,
    double PositionX,
    double PositionY);
