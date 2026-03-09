namespace StellarLeasing.Application.WorkflowDefinitions;

public sealed record WorkflowDefinitionSummary(
    Guid Id,
    string Name,
    string Code,
    int DraftVersionNumber,
    int? ActiveVersionNumber);
