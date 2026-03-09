using Microsoft.AspNetCore.Mvc;
using StellarLeasing.Application.WorkflowDefinitions;

namespace StellarLeasing.Api.Controllers;

[ApiController]
[Route("api/workflow-definitions")]
public sealed class WorkflowDefinitionsController : ControllerBase
{
    private readonly IWorkflowDefinitionService _workflowDefinitionService;

    public WorkflowDefinitionsController(IWorkflowDefinitionService workflowDefinitionService)
    {
        _workflowDefinitionService = workflowDefinitionService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<WorkflowDefinitionSummary>>> List(CancellationToken cancellationToken)
    {
        var definitions = await _workflowDefinitionService.ListAsync(cancellationToken);
        return Ok(definitions);
    }

    [HttpPost]
    public async Task<ActionResult<WorkflowDefinitionSummary>> Create(
        [FromBody] CreateWorkflowDefinitionRequest request,
        CancellationToken cancellationToken)
    {
        var created = await _workflowDefinitionService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(List), new { id = created.Id }, created);
    }
}
