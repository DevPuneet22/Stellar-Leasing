using Microsoft.EntityFrameworkCore;
using StellarLeasing.Application.Abstractions;
using StellarLeasing.Domain.WorkflowDefinitions;

namespace StellarLeasing.Infrastructure.Persistence;

public sealed class PostgresWorkflowDefinitionRepository : IWorkflowDefinitionRepository
{
    private readonly StellarLeasingDbContext _dbContext;

    public PostgresWorkflowDefinitionRepository(StellarLeasingDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyCollection<WorkflowDefinition>> ListAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.WorkflowDefinitions
            .AsNoTracking()
            .Where(definition => definition.TenantId == tenantId)
            .Include(definition => definition.Versions)
            .OrderBy(definition => definition.Name)
            .ToArrayAsync(cancellationToken);
    }

    public Task<WorkflowDefinition?> GetByIdAsync(
        Guid tenantId,
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.WorkflowDefinitions
            .AsNoTracking()
            .AsSplitQuery()
            .Include(definition => definition.Versions)
                .ThenInclude(version => version.Steps)
            .Include(definition => definition.Versions)
                .ThenInclude(version => version.Transitions)
            .SingleOrDefaultAsync(
                definition => definition.TenantId == tenantId && definition.Id == id,
                cancellationToken);
    }

    public Task<WorkflowDefinition?> GetForUpdateAsync(
        Guid tenantId,
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.WorkflowDefinitions
            .AsSplitQuery()
            .Include(definition => definition.Versions)
                .ThenInclude(version => version.Steps)
            .Include(definition => definition.Versions)
                .ThenInclude(version => version.Transitions)
            .SingleOrDefaultAsync(
                definition => definition.TenantId == tenantId && definition.Id == id,
                cancellationToken);
    }

    public Task<bool> ExistsByCodeAsync(
        Guid tenantId,
        string code,
        Guid? excludingDefinitionId = null,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.WorkflowDefinitions
            .AsNoTracking()
            .AnyAsync(
                definition =>
                    definition.TenantId == tenantId &&
                    definition.Code == code &&
                    (!excludingDefinitionId.HasValue || definition.Id != excludingDefinitionId.Value),
                cancellationToken);
    }

    public async Task AddAsync(WorkflowDefinition definition, CancellationToken cancellationToken = default)
    {
        _dbContext.WorkflowDefinitions.Add(definition);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateDraftAsync(
        WorkflowDefinition definition,
        IReadOnlyCollection<Guid> previousStepIds,
        IReadOnlyCollection<Guid> previousTransitionIds,
        CancellationToken cancellationToken = default)
    {
        var previousStepIdSet = previousStepIds.ToHashSet();
        var previousTransitionIdSet = previousTransitionIds.ToHashSet();
        var draftVersion = definition.DraftVersion
            ?? throw new InvalidOperationException("A draft version is required to update workflow steps.");

        foreach (var step in draftVersion.Steps.Where(step => !previousStepIdSet.Contains(step.Id)))
        {
            _dbContext.Entry(step).State = EntityState.Added;
        }

        foreach (var transition in draftVersion.Transitions.Where(transition => !previousTransitionIdSet.Contains(transition.Id)))
        {
            _dbContext.Entry(transition).State = EntityState.Added;
        }

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException exception)
        {
            throw new InvalidOperationException(
                "This workflow changed while you were editing it. Refresh and try again.",
                exception);
        }
    }

    public async Task AddDraftVersionAsync(WorkflowDefinition definition, CancellationToken cancellationToken = default)
    {
        var draftVersion = definition.DraftVersion
            ?? throw new InvalidOperationException("A draft version is required to persist a new workflow version.");

        _dbContext.Entry(draftVersion).State = EntityState.Added;

        foreach (var step in draftVersion.Steps)
        {
            _dbContext.Entry(step).State = EntityState.Added;
        }

        foreach (var transition in draftVersion.Transitions)
        {
            _dbContext.Entry(transition).State = EntityState.Added;
        }

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException exception)
        {
            throw new InvalidOperationException(
                "This workflow changed while you were editing it. Refresh and try again.",
                exception);
        }
    }

    public async Task UpdateAsync(WorkflowDefinition definition, CancellationToken cancellationToken = default)
    {
        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException exception)
        {
            throw new InvalidOperationException(
                "This workflow changed while you were editing it. Refresh and try again.",
                exception);
        }
    }
}
