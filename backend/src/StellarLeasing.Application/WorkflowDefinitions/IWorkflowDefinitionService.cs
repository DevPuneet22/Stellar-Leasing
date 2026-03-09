namespace StellarLeasing.Application.WorkflowDefinitions;

public interface IWorkflowDefinitionService
{
    Task<IReadOnlyCollection<WorkflowDefinitionSummary>> ListAsync(CancellationToken cancellationToken = default);

    Task<WorkflowDefinitionSummary> CreateAsync(CreateWorkflowDefinitionRequest request, CancellationToken cancellationToken = default);
}
