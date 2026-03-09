using StellarLeasing.Application.Abstractions;
using StellarLeasing.Domain.WorkflowDefinitions;

namespace StellarLeasing.Application.WorkflowDefinitions;

public sealed class WorkflowDefinitionService : IWorkflowDefinitionService
{
    private readonly IWorkflowDefinitionRepository _repository;

    public WorkflowDefinitionService(IWorkflowDefinitionRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyCollection<WorkflowDefinitionSummary>> ListAsync(CancellationToken cancellationToken = default)
    {
        var definitions = await _repository.ListAsync(cancellationToken);
        return definitions
            .OrderBy(definition => definition.Name)
            .Select(Map)
            .ToArray();
    }

    public async Task<WorkflowDefinitionSummary> CreateAsync(
        CreateWorkflowDefinitionRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new ArgumentException("Workflow name is required.", nameof(request));
        }

        var definition = WorkflowDefinition.Create(ApplicationDefaults.DemoTenantId, request.Name, request.Code);
        if (await _repository.ExistsByCodeAsync(definition.TenantId, definition.Code, cancellationToken))
        {
            throw new InvalidOperationException($"Workflow code '{definition.Code}' already exists.");
        }

        await _repository.AddAsync(definition, cancellationToken);
        return Map(definition);
    }

    private static WorkflowDefinitionSummary Map(WorkflowDefinition definition)
    {
        var draftVersion = definition.Versions
            .Where(version => version.Status == WorkflowVersionStatus.Draft)
            .OrderByDescending(version => version.VersionNumber)
            .FirstOrDefault();

        return new WorkflowDefinitionSummary(
            definition.Id,
            definition.Name,
            definition.Code,
            draftVersion?.VersionNumber ?? 0,
            definition.ActiveVersion?.VersionNumber);
    }
}
