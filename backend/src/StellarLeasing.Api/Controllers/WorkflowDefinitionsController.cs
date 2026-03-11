using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.AspNetCore.Mvc;
using StellarLeasing.Application.WorkflowDefinitions;

namespace StellarLeasing.Api.Controllers;

[ApiController]
[Authorize]
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
        try
        {
            var definitions = await _workflowDefinitionService.ListAsync(cancellationToken);
            return Ok(definitions);
        }
        catch (ArgumentException exception)
        {
            return CreateProblem(400, exception.Message);
        }
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<WorkflowDefinitionDetail>> GetById(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var definition = await _workflowDefinitionService.GetAsync(id, cancellationToken);
            return definition is null ? NotFound() : Ok(definition);
        }
        catch (ArgumentException exception)
        {
            return CreateProblem(400, exception.Message);
        }
    }

    [HttpPost]
    public async Task<ActionResult<WorkflowDefinitionDetail>> Create(
        [FromBody] CreateWorkflowDefinitionRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var created = await _workflowDefinitionService.CreateAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (ArgumentException exception)
        {
            return CreateProblem(400, exception.Message);
        }
        catch (InvalidOperationException exception)
        {
            return CreateProblem(409, exception.Message);
        }
    }

    [HttpPut("{id:guid}/draft")]
    public async Task<ActionResult<WorkflowDefinitionDetail>> UpdateDraft(
        Guid id,
        [FromBody] UpdateWorkflowDraftRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var updated = await _workflowDefinitionService.UpdateDraftAsync(id, request, cancellationToken);
            return Ok(updated);
        }
        catch (KeyNotFoundException exception)
        {
            return CreateProblem(404, exception.Message);
        }
        catch (ArgumentException exception)
        {
            return CreateProblem(400, exception.Message);
        }
        catch (InvalidOperationException exception)
        {
            return CreateProblem(409, exception.Message);
        }
    }

    [HttpPost("{id:guid}/versions")]
    public async Task<ActionResult<WorkflowDefinitionDetail>> CreateNextDraftVersion(
        Guid id,
        [FromBody] CreateNextWorkflowVersionRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var updated = await _workflowDefinitionService.CreateNextDraftVersionAsync(id, request, cancellationToken);
            return Ok(updated);
        }
        catch (KeyNotFoundException exception)
        {
            return CreateProblem(404, exception.Message);
        }
        catch (ArgumentException exception)
        {
            return CreateProblem(400, exception.Message);
        }
        catch (InvalidOperationException exception)
        {
            return CreateProblem(409, exception.Message);
        }
    }

    [HttpPost("{id:guid}/activate")]
    public async Task<ActionResult<WorkflowDefinitionDetail>> ActivateVersion(
        Guid id,
        [FromBody] ActivateWorkflowVersionRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var updated = await _workflowDefinitionService.ActivateVersionAsync(id, request, cancellationToken);
            return Ok(updated);
        }
        catch (KeyNotFoundException exception)
        {
            return CreateProblem(404, exception.Message);
        }
        catch (ArgumentException exception)
        {
            return CreateProblem(400, exception.Message);
        }
        catch (InvalidOperationException exception)
        {
            return CreateProblem(409, exception.Message);
        }
    }

    private ActionResult CreateProblem(int statusCode, string detail)
    {
        return Problem(
            statusCode: statusCode,
            title: ReasonPhrases.GetReasonPhrase(statusCode),
            detail: detail);
    }
}
