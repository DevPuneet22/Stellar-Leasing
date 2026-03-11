using StellarLeasing.Domain.WorkflowDefinitions;

namespace StellarLeasing.Application.Abstractions;

public interface IWorkflowDefinitionRepository
{
    Task<IReadOnlyCollection<WorkflowDefinition>> ListAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default);

    Task<WorkflowDefinition?> GetByIdAsync(
        Guid tenantId,
        Guid id,
        CancellationToken cancellationToken = default);

    Task<WorkflowDefinition?> GetForUpdateAsync(
        Guid tenantId,
        Guid id,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsByCodeAsync(
        Guid tenantId,
        string code,
        Guid? excludingDefinitionId = null,
        CancellationToken cancellationToken = default);

    Task AddAsync(WorkflowDefinition definition, CancellationToken cancellationToken = default);

    Task UpdateDraftAsync(
        WorkflowDefinition definition,
        IReadOnlyCollection<Guid> previousStepIds,
        IReadOnlyCollection<Guid> previousTransitionIds,
        CancellationToken cancellationToken = default);

    Task AddDraftVersionAsync(WorkflowDefinition definition, CancellationToken cancellationToken = default);

    Task UpdateAsync(WorkflowDefinition definition, CancellationToken cancellationToken = default);
}
