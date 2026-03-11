namespace StellarLeasing.Application.WorkflowDefinitions;

public sealed record WorkflowDefinitionDetail(
    Guid Id,
    string Name,
    string Code,
    int Revision,
    WorkflowVersionDetail? DraftVersion,
    WorkflowVersionDetail? ActiveVersion,
    IReadOnlyCollection<WorkflowVersionSummary> Versions);
