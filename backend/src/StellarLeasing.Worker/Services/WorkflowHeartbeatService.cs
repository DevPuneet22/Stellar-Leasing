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
            var definitions = await _workflowDefinitionService.ListAsync(stoppingToken);
            _logger.LogInformation(
                "Workflow worker heartbeat at {UtcNow}. Registered workflow definitions: {Count}.",
                DateTimeOffset.UtcNow,
                definitions.Count);

            await timer.WaitForNextTickAsync(stoppingToken);
        }
    }
}
