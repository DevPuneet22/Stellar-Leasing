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

    public Task<IReadOnlyCollection<WorkflowDefinition>> ListAsync(CancellationToken cancellationToken = default)
    {
        lock (_sync)
        {
            return Task.FromResult<IReadOnlyCollection<WorkflowDefinition>>(_definitions.ToArray());
        }
    }

    public Task<bool> ExistsByCodeAsync(Guid tenantId, string code, CancellationToken cancellationToken = default)
    {
        lock (_sync)
        {
            var exists = _definitions.Any(definition =>
                definition.TenantId == tenantId &&
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

    private static WorkflowDefinition SeedLeaseWorkflow()
    {
        var definition = WorkflowDefinition.Create(ApplicationDefaults.DemoTenantId, "Lease Approval", "LEASE-APPROVAL");
        definition.ActivateVersion(1);
        return definition;
    }
}
