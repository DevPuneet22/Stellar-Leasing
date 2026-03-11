using StellarLeasing.Application;
using StellarLeasing.Application.Abstractions;
using StellarLeasing.Domain.WorkflowDefinitions;

namespace StellarLeasing.Infrastructure.Persistence;

public sealed class InMemoryWorkflowDefinitionRepository : IWorkflowDefinitionRepository
{
    private readonly Lock _sync = new();
    private readonly List<WorkflowDefinition> _definitions;

    public InMemoryWorkflowDefinitionRepository()
    {
        _definitions =
        [
            SeedLeaseWorkflow()
        ];
    }

    public Task<IReadOnlyCollection<WorkflowDefinition>> ListAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        lock (_sync)
        {
            return Task.FromResult<IReadOnlyCollection<WorkflowDefinition>>(
                _definitions.Where(definition => definition.TenantId == tenantId).ToArray());
        }
    }

    public Task<WorkflowDefinition?> GetByIdAsync(
        Guid tenantId,
        Guid id,
        CancellationToken cancellationToken = default)
    {
        lock (_sync)
        {
            return Task.FromResult(
                _definitions.SingleOrDefault(definition => definition.TenantId == tenantId && definition.Id == id));
        }
    }

    public Task<WorkflowDefinition?> GetForUpdateAsync(
        Guid tenantId,
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return GetByIdAsync(tenantId, id, cancellationToken);
    }

    public Task<bool> ExistsByCodeAsync(
        Guid tenantId,
        string code,
        Guid? excludingDefinitionId = null,
        CancellationToken cancellationToken = default)
    {
        lock (_sync)
        {
            var exists = _definitions.Any(definition =>
                definition.TenantId == tenantId &&
                (!excludingDefinitionId.HasValue || definition.Id != excludingDefinitionId.Value) &&
                string.Equals(definition.Code, code, StringComparison.OrdinalIgnoreCase));

            return Task.FromResult(exists);
        }
    }

    public Task AddAsync(WorkflowDefinition definition, CancellationToken cancellationToken = default)
    {
        lock (_sync)
        {
            _definitions.Add(definition);
        }

        return Task.CompletedTask;
    }

    public Task UpdateDraftAsync(
        WorkflowDefinition definition,
        IReadOnlyCollection<Guid> previousStepIds,
        IReadOnlyCollection<Guid> previousTransitionIds,
        CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task AddDraftVersionAsync(WorkflowDefinition definition, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task UpdateAsync(WorkflowDefinition definition, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    private static WorkflowDefinition SeedLeaseWorkflow()
    {
        var definition = WorkflowDefinition.Create(ApplicationDefaults.DemoTenantId, "Lease Approval", "LEASE-APPROVAL");
        definition.ActivateVersion(1);
        return definition;
    }
}
