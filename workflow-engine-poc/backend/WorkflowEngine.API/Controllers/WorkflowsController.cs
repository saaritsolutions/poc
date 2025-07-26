using Microsoft.AspNetCore.Mvc;
using WorkflowEngine.Core.DTOs;
using WorkflowEngine.Core.Interfaces;

namespace WorkflowEngine.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WorkflowsController : ControllerBase
{
    private readonly IWorkflowService _workflowService;
    private readonly ILogger<WorkflowsController> _logger;

    public WorkflowsController(IWorkflowService workflowService, ILogger<WorkflowsController> logger)
    {
        _workflowService = workflowService;
        _logger = logger;
    }

    /// <summary>
    /// Get all active workflows
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<WorkflowDto>>> GetActiveWorkflows()
    {
        try
        {
            var workflows = await _workflowService.GetActiveWorkflowsAsync();
            return Ok(workflows);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving active workflows");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get workflow by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<WorkflowDto>> GetWorkflow(Guid id)
    {
        try
        {
            var workflow = await _workflowService.GetWorkflowAsync(id);
            if (workflow == null)
                return NotFound();

            return Ok(workflow);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving workflow {WorkflowId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Create a new workflow
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<WorkflowDto>> CreateWorkflow([FromBody] CreateWorkflowRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // TODO: Get user from authentication context
            var createdBy = "system"; // This should come from JWT token

            var workflow = await _workflowService.CreateWorkflowAsync(request, createdBy);
            return CreatedAtAction(nameof(GetWorkflow), new { id = workflow.Id }, workflow);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating workflow");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Update workflow
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<WorkflowDto>> UpdateWorkflow(Guid id, [FromBody] CreateWorkflowRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // TODO: Get user from authentication context
            var updatedBy = "system";

            var workflow = await _workflowService.UpdateWorkflowAsync(id, request, updatedBy);
            return Ok(workflow);
        }
        catch (ArgumentException)
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating workflow {WorkflowId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Activate workflow
    /// </summary>
    [HttpPost("{id}/activate")]
    public async Task<ActionResult> ActivateWorkflow(Guid id)
    {
        try
        {
            var result = await _workflowService.ActivateWorkflowAsync(id);
            if (!result)
                return NotFound();

            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error activating workflow {WorkflowId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Deactivate workflow
    /// </summary>
    [HttpPost("{id}/deactivate")]
    public async Task<ActionResult> DeactivateWorkflow(Guid id)
    {
        try
        {
            var result = await _workflowService.DeactivateWorkflowAsync(id);
            if (!result)
                return NotFound();

            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deactivating workflow {WorkflowId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Delete workflow
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteWorkflow(Guid id)
    {
        try
        {
            var result = await _workflowService.DeleteWorkflowAsync(id);
            if (!result)
                return NotFound();

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting workflow {WorkflowId}", id);
            return StatusCode(500, "Internal server error");
        }
    }
}
