using StellarLeasing.Domain.WorkflowDefinitions;

namespace StellarLeasing.Application.Abstractions;

public interface IWorkflowDefinitionRepository
{
    Task<IReadOnlyCollection<WorkflowDefinition>> ListAsync(CancellationToken cancellationToken = default);

    Task<bool> ExistsByCodeAsync(Guid tenantId, string code, CancellationToken cancellationToken = default);

    Task AddAsync(WorkflowDefinition definition, CancellationToken cancellationToken = default);
}
