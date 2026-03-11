using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StellarLeasing.Application.WorkflowDefinitions;

namespace StellarLeasing.Worker.Services;

public sealed class WorkflowHeartbeatService : BackgroundService
{
    private readonly ILogger<WorkflowHeartbeatService> _logger;
    private readonly IWorkflowDefinitionService _workflowDefinitionService;

    public WorkflowHeartbeatService(
        ILogger<WorkflowHeartbeatService> logger,
        IWorkflowDefinitionService workflowDefinitionService)
    {
        _logger = logger;
        _workflowDefinitionService = workflowDefinitionService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(5));

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var definitions = await _workflowDefinitionService.ListAsync(stoppingToken);
                _logger.LogInformation(
                    "Workflow worker heartbeat at {UtcNow}. Registered workflow definitions: {Count}.",
                    DateTimeOffset.UtcNow,
                    definitions.Count);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                _logger.LogWarning(
                    exception,
                    "Workflow worker heartbeat skipped at {UtcNow} because the workflow catalog could not be queried.",
                    DateTimeOffset.UtcNow);
            }

            await timer.WaitForNextTickAsync(stoppingToken);
        }
    }
}
