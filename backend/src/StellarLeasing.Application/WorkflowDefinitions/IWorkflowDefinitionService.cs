namespace StellarLeasing.Application.WorkflowDefinitions;

public interface IWorkflowDefinitionService
{
    Task<IReadOnlyCollection<WorkflowDefinitionSummary>> ListAsync(CancellationToken cancellationToken = default);

    Task<WorkflowDefinitionDetail?> GetAsync(Guid id, CancellationToken cancellationToken = default);

    Task<WorkflowDefinitionDetail> CreateAsync(
        CreateWorkflowDefinitionRequest request,
        CancellationToken cancellationToken = default);

    Task<WorkflowDefinitionDetail> UpdateDraftAsync(
        Guid id,
        UpdateWorkflowDraftRequest request,
        CancellationToken cancellationToken = default);

    Task<WorkflowDefinitionDetail> CreateNextDraftVersionAsync(
        Guid id,
        CreateNextWorkflowVersionRequest request,
        CancellationToken cancellationToken = default);

    Task<WorkflowDefinitionDetail> ActivateVersionAsync(
        Guid id,
        ActivateWorkflowVersionRequest request,
        CancellationToken cancellationToken = default);
}
