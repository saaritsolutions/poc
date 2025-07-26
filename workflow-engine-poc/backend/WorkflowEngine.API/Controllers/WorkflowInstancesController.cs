using Microsoft.AspNetCore.Mvc;
using WorkflowEngine.Core.DTOs;
using WorkflowEngine.Core.Entities;
using WorkflowEngine.Core.Interfaces;

namespace WorkflowEngine.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WorkflowInstancesController : ControllerBase
{
    private readonly IWorkflowInstanceService _workflowInstanceService;
    private readonly ILogger<WorkflowInstancesController> _logger;

    public WorkflowInstancesController(
        IWorkflowInstanceService workflowInstanceService,
        ILogger<WorkflowInstancesController> logger)
    {
        _workflowInstanceService = workflowInstanceService;
        _logger = logger;
    }

    /// <summary>
    /// Get workflow instances with optional filtering
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<WorkflowInstanceDto>>> GetWorkflowInstances(
        [FromQuery] Guid? workflowId = null,
        [FromQuery] WorkflowStatus? status = null)
    {
        try
        {
            var instances = await _workflowInstanceService.GetWorkflowInstancesAsync(workflowId, status);
            return Ok(instances);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving workflow instances");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get workflow instance by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<WorkflowInstanceDto>> GetWorkflowInstance(Guid id)
    {
        try
        {
            var instance = await _workflowInstanceService.GetWorkflowInstanceAsync(id);
            if (instance == null)
                return NotFound();

            return Ok(instance);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving workflow instance {InstanceId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Start a new workflow instance
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<WorkflowInstanceDto>> StartWorkflow([FromBody] CreateWorkflowInstanceRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // TODO: Get user from authentication context
            var initiatedBy = "system";

            var instance = await _workflowInstanceService.StartWorkflowAsync(request, initiatedBy);
            return CreatedAtAction(nameof(GetWorkflowInstance), new { id = instance.Id }, instance);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting workflow instance");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Complete a workflow step
    /// </summary>
    [HttpPost("{instanceId}/steps/{stepId}/complete")]
    public async Task<ActionResult<WorkflowInstanceDto>> CompleteStep(
        Guid instanceId,
        string stepId,
        [FromBody] CompleteStepRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // TODO: Get user from authentication context
            var completedBy = "system";

            var instance = await _workflowInstanceService.CompleteStepAsync(instanceId, stepId, request, completedBy);
            return Ok(instance);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing step {StepId} for instance {InstanceId}", stepId, instanceId);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Cancel a workflow instance
    /// </summary>
    [HttpPost("{instanceId}/cancel")]
    public async Task<ActionResult<WorkflowInstanceDto>> CancelWorkflow(
        Guid instanceId,
        [FromBody] CancelWorkflowRequest request)
    {
        try
        {
            // TODO: Get user from authentication context
            var cancelledBy = "system";

            var instance = await _workflowInstanceService.CancelWorkflowAsync(instanceId, cancelledBy, request.Reason);
            return Ok(instance);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling workflow instance {InstanceId}", instanceId);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get user's assigned tasks
    /// </summary>
    [HttpGet("my-tasks")]
    public async Task<ActionResult<IEnumerable<WorkflowInstanceDto>>> GetMyTasks()
    {
        try
        {
            // TODO: Get user from authentication context
            var userId = "system";

            var tasks = await _workflowInstanceService.GetUserTasksAsync(userId);
            return Ok(tasks);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user tasks");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get audit log for workflow instance
    /// </summary>
    [HttpGet("{instanceId}/audit")]
    public async Task<ActionResult<IEnumerable<WorkflowAuditLogDto>>> GetAuditLog(Guid instanceId)
    {
        try
        {
            var auditLogs = await _workflowInstanceService.GetAuditLogAsync(instanceId);
            return Ok(auditLogs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving audit log for instance {InstanceId}", instanceId);
            return StatusCode(500, "Internal server error");
        }
    }
}

public class CancelWorkflowRequest
{
    public string? Reason { get; set; }
}
