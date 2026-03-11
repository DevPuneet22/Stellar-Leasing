using StellarLeasing.Application.Abstractions;
using StellarLeasing.Domain.WorkflowDefinitions;

namespace StellarLeasing.Application.WorkflowDefinitions;

public sealed class WorkflowDefinitionService : IWorkflowDefinitionService
{
    private readonly IWorkflowDefinitionRepository _repository;
    private readonly ICurrentTenantAccessor _currentTenantAccessor;

    public WorkflowDefinitionService(
        IWorkflowDefinitionRepository repository,
        ICurrentTenantAccessor currentTenantAccessor)
    {
        _repository = repository;
        _currentTenantAccessor = currentTenantAccessor;
    }

    public async Task<IReadOnlyCollection<WorkflowDefinitionSummary>> ListAsync(CancellationToken cancellationToken = default)
    {
        var definitions = await _repository.ListAsync(_currentTenantAccessor.GetCurrentTenantId(), cancellationToken);
        return definitions
            .OrderBy(definition => definition.Name)
            .Select(MapSummary)
            .ToArray();
    }

    public async Task<WorkflowDefinitionDetail?> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var definition = await _repository.GetByIdAsync(_currentTenantAccessor.GetCurrentTenantId(), id, cancellationToken);
        return definition is null ? null : MapDetail(definition);
    }

    public async Task<WorkflowDefinitionDetail> CreateAsync(
        CreateWorkflowDefinitionRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new ArgumentException("Workflow name is required.", nameof(request));
        }

        var tenantId = _currentTenantAccessor.GetCurrentTenantId();
        var definition = WorkflowDefinition.Create(tenantId, request.Name, request.Code);
        if (await _repository.ExistsByCodeAsync(definition.TenantId, definition.Code, cancellationToken: cancellationToken))
        {
            throw new InvalidOperationException($"Workflow code '{definition.Code}' already exists.");
        }

        await _repository.AddAsync(definition, cancellationToken);
        return MapDetail(definition);
    }

    public async Task<WorkflowDefinitionDetail> UpdateDraftAsync(
        Guid id,
        UpdateWorkflowDraftRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.Steps is null || request.Transitions is null)
        {
            throw new ArgumentException("Workflow draft steps and transitions are required.", nameof(request));
        }

        var definition = await GetDefinitionForUpdateAsync(id, cancellationToken);
        EnsureExpectedRevision(definition, request.ExpectedRevision);
        var previousDraftVersion = definition.DraftVersion
            ?? throw new InvalidOperationException("Create a draft version before editing this workflow.");
        var previousStepIds = previousDraftVersion.Steps.Select(step => step.Id).ToArray();
        var previousTransitionIds = previousDraftVersion.Transitions.Select(transition => transition.Id).ToArray();

        definition.Rename(request.Name, request.Code);

        if (await _repository.ExistsByCodeAsync(
                definition.TenantId,
                definition.Code,
                definition.Id,
                cancellationToken))
        {
            throw new InvalidOperationException($"Workflow code '{definition.Code}' already exists.");
        }

        var steps = request.Steps
            .Select((step, index) => new WorkflowStep(
                step.Key,
                step.Name,
                ParseStepType(step.Type),
                step.AssigneeRule,
                index,
                step.PositionX,
                step.PositionY))
            .ToArray();

        var transitions = request.Transitions
            .Select((transition, index) => new WorkflowTransition(
                transition.From,
                transition.To,
                transition.Condition,
                index))
            .ToArray();

        definition.UpdateDraftVersion(steps, transitions);
        await _repository.UpdateDraftAsync(
            definition,
            previousStepIds,
            previousTransitionIds,
            cancellationToken);

        return MapDetail(definition);
    }

    public async Task<WorkflowDefinitionDetail> CreateNextDraftVersionAsync(
        Guid id,
        CreateNextWorkflowVersionRequest request,
        CancellationToken cancellationToken = default)
    {
        var definition = await GetDefinitionForUpdateAsync(id, cancellationToken);
        EnsureExpectedRevision(definition, request.ExpectedRevision);
        definition.AddDraftVersion();
        await _repository.AddDraftVersionAsync(definition, cancellationToken);
        return MapDetail(definition);
    }

    public async Task<WorkflowDefinitionDetail> ActivateVersionAsync(
        Guid id,
        ActivateWorkflowVersionRequest request,
        CancellationToken cancellationToken = default)
    {
        var definition = await GetDefinitionForUpdateAsync(id, cancellationToken);
        EnsureExpectedRevision(definition, request.ExpectedRevision);
        definition.ActivateVersion(request.VersionNumber);
        await _repository.UpdateAsync(definition, cancellationToken);
        return MapDetail(definition);
    }

    private async Task<WorkflowDefinition> GetDefinitionForUpdateAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _repository.GetForUpdateAsync(_currentTenantAccessor.GetCurrentTenantId(), id, cancellationToken)
            ?? throw new KeyNotFoundException($"Workflow definition '{id}' was not found.");
    }

    private static void EnsureExpectedRevision(WorkflowDefinition definition, int expectedRevision)
    {
        if (definition.Revision != expectedRevision)
        {
            throw new InvalidOperationException("This workflow changed while you were editing it. Refresh and try again.");
        }
    }

    private static WorkflowDefinitionSummary MapSummary(WorkflowDefinition definition)
    {
        return new WorkflowDefinitionSummary(
            definition.Id,
            definition.Name,
            definition.Code,
            definition.DraftVersion?.VersionNumber ?? 0,
            definition.ActiveVersion?.VersionNumber);
    }

    private static WorkflowDefinitionDetail MapDetail(WorkflowDefinition definition)
    {
        return new WorkflowDefinitionDetail(
            definition.Id,
            definition.Name,
            definition.Code,
            definition.Revision,
            definition.DraftVersion is null ? null : MapVersion(definition.DraftVersion),
            definition.ActiveVersion is null ? null : MapVersion(definition.ActiveVersion),
            definition.Versions
                .OrderByDescending(version => version.VersionNumber)
                .Select(version => new WorkflowVersionSummary(
                    version.VersionNumber,
                    version.Status.ToString().ToLowerInvariant(),
                    version.Steps.Count,
                    version.Transitions.Count))
                .ToArray());
    }

    private static WorkflowVersionDetail MapVersion(WorkflowVersion version)
    {
        return new WorkflowVersionDetail(
            version.VersionNumber,
            version.Status.ToString().ToLowerInvariant(),
            version.Steps
                .OrderBy(step => step.SortOrder)
                .Select(step => new WorkflowStepModel(
                    step.Key,
                    step.Name,
                    step.StepType.ToString().ToLowerInvariant(),
                    step.AssigneeRule,
                    step.SortOrder,
                    step.PositionX,
                    step.PositionY))
                .ToArray(),
            version.Transitions
                .OrderBy(transition => transition.SortOrder)
                .Select(transition => new WorkflowTransitionModel(
                    transition.FromStepKey,
                    transition.ToStepKey,
                    transition.ConditionName,
                    transition.SortOrder))
                .ToArray());
    }

    private static WorkflowStepType ParseStepType(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Workflow step type is required.", nameof(value));
        }

        return value.Trim().ToLowerInvariant() switch
        {
            "start" => WorkflowStepType.Start,
            "task" => WorkflowStepType.Task,
            "approval" => WorkflowStepType.Approval,
            "condition" => WorkflowStepType.Condition,
            "end" => WorkflowStepType.End,
            _ => throw new ArgumentException($"Workflow step type '{value}' is not supported.", nameof(value))
        };
    }
}
