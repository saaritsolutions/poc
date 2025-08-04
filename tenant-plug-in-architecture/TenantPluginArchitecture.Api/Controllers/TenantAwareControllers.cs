using Microsoft.AspNetCore.Mvc;
using TenantPluginArchitecture.Core;
using TenantPluginArchitecture.Plugins.Contracts;

namespace TenantPluginArchitecture.Api.Controllers;

/// <summary>
/// Controller for form-related operations with tenant-specific processing
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class FormsController : ControllerBase
{
    private readonly IFormEngine _formEngine;
    private readonly ILogger<FormsController> _logger;

    public FormsController(IFormEngine formEngine, ILogger<FormsController> logger)
    {
        _formEngine = formEngine;
        _logger = logger;
    }

    /// <summary>
    /// Submit a form for processing
    /// </summary>
    [HttpPost("submit")]
    public async Task<ActionResult<FormProcessingResult>> SubmitForm([FromBody] SubmitFormRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(request.TenantId))
            {
                return BadRequest("Tenant ID is required");
            }

            var formData = new TenantPluginArchitecture.Core.FormData
            {
                FormId = request.FormId ?? Guid.NewGuid().ToString(),
                FormType = request.FormType,
                TenantId = request.TenantId,
                UserId = request.UserId ?? "anonymous",
                Fields = request.Fields,
                SubmittedAt = DateTime.UtcNow,
                Status = "Submitted"
            };

            var result = await _formEngine.SubmitFormAsync(formData);
            
            if (result.IsSuccessful)
            {
                return Ok(result);
            }
            
            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting form for tenant {TenantId}", request.TenantId);
            return StatusCode(500, "An error occurred processing the form");
        }
    }

    /// <summary>
    /// Validate form data without submitting
    /// </summary>
    [HttpPost("validate")]
    public async Task<ActionResult<ValidationResult>> ValidateForm([FromBody] ValidateFormRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(request.TenantId))
            {
                return BadRequest("Tenant ID is required");
            }

            var formData = new TenantPluginArchitecture.Core.FormData
            {
                FormId = request.FormId ?? Guid.NewGuid().ToString(),
                FormType = request.FormType,
                TenantId = request.TenantId,
                UserId = request.UserId ?? "anonymous",
                Fields = request.Fields
            };

            var result = await _formEngine.ValidateFormAsync(formData);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating form for tenant {TenantId}", request.TenantId);
            return StatusCode(500, "An error occurred validating the form");
        }
    }

    /// <summary>
    /// Get form configuration for a specific tenant and form type
    /// </summary>
    [HttpGet("configuration/{tenantId}/{formType}")]
    public async Task<ActionResult<FormConfiguration>> GetFormConfiguration(string tenantId, string formType)
    {
        try
        {
            var configuration = await _formEngine.GetFormConfigurationAsync(tenantId, formType);
            return Ok(configuration);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting form configuration for tenant {TenantId}, form type {FormType}", 
                           tenantId, formType);
            return StatusCode(500, "An error occurred getting form configuration");
        }
    }
}

/// <summary>
/// Controller for workflow-related operations with tenant-specific processing
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class WorkflowsController : ControllerBase
{
    private readonly IWorkflowEngine _workflowEngine;
    private readonly ILogger<WorkflowsController> _logger;

    public WorkflowsController(IWorkflowEngine workflowEngine, ILogger<WorkflowsController> logger)
    {
        _workflowEngine = workflowEngine;
        _logger = logger;
    }

    /// <summary>
    /// Start a new workflow instance
    /// </summary>
    [HttpPost("start")]
    public async Task<ActionResult<WorkflowResult>> StartWorkflow([FromBody] StartWorkflowRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(request.TenantId))
            {
                return BadRequest("Tenant ID is required");
            }

            var result = await _workflowEngine.StartWorkflowAsync(
                request.TenantId, 
                request.WorkflowType, 
                request.UserId ?? "anonymous", 
                request.InputData);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting workflow for tenant {TenantId}", request.TenantId);
            return StatusCode(500, "An error occurred starting the workflow");
        }
    }

    /// <summary>
    /// Execute the next step in a workflow
    /// </summary>
    [HttpPost("{workflowId}/next")]
    public async Task<ActionResult<WorkflowResult>> ExecuteNextStep(string workflowId)
    {
        try
        {
            var result = await _workflowEngine.ExecuteNextStepAsync(workflowId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing next step for workflow {WorkflowId}", workflowId);
            return StatusCode(500, "An error occurred executing the workflow step");
        }
    }

    /// <summary>
    /// Execute a specific step in a workflow
    /// </summary>
    [HttpPost("{workflowId}/steps/{stepName}")]
    public async Task<ActionResult<WorkflowResult>> ExecuteStep(
        string workflowId, 
        string stepName, 
        [FromBody] ExecuteStepRequest request)
    {
        try
        {
            var result = await _workflowEngine.ExecuteStepAsync(workflowId, stepName, request.InputData);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing step {StepName} for workflow {WorkflowId}", 
                           stepName, workflowId);
            return StatusCode(500, "An error occurred executing the workflow step");
        }
    }

    /// <summary>
    /// Get workflow instance details
    /// </summary>
    [HttpGet("{workflowId}")]
    public async Task<ActionResult<TenantPluginArchitecture.Core.WorkflowInstance>> GetWorkflowInstance(string workflowId)
    {
        try
        {
            var instance = await _workflowEngine.GetWorkflowInstanceAsync(workflowId);
            
            if (instance == null)
            {
                return NotFound($"Workflow {workflowId} not found");
            }

            return Ok(instance);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting workflow instance {WorkflowId}", workflowId);
            return StatusCode(500, "An error occurred getting the workflow instance");
        }
    }

    /// <summary>
    /// Get available workflow steps for a tenant and workflow type
    /// </summary>
    [HttpGet("steps/{tenantId}/{workflowType}")]
    public async Task<ActionResult<IEnumerable<WorkflowStep>>> GetWorkflowSteps(string tenantId, string workflowType)
    {
        try
        {
            var steps = await _workflowEngine.GetWorkflowStepsAsync(tenantId, workflowType);
            return Ok(steps);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting workflow steps for tenant {TenantId}, type {WorkflowType}", 
                           tenantId, workflowType);
            return StatusCode(500, "An error occurred getting workflow steps");
        }
    }
}

/// <summary>
/// Controller for plugin management operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class PluginsController : ControllerBase
{
    private readonly IPluginLoader _pluginLoader;
    private readonly ILogger<PluginsController> _logger;

    public PluginsController(IPluginLoader pluginLoader, ILogger<PluginsController> logger)
    {
        _pluginLoader = pluginLoader;
        _logger = logger;
    }

    /// <summary>
    /// Check if plugins are available for a tenant
    /// </summary>
    [HttpGet("available/{tenantId}")]
    public async Task<ActionResult<PluginAvailabilityResponse>> CheckPluginAvailability(string tenantId)
    {
        try
        {
            var hasValidator = await _pluginLoader.IsPluginAvailableAsync<ICustomValidator>(tenantId);
            var hasFormProcessor = await _pluginLoader.IsPluginAvailableAsync<ICustomFormProcessor>(tenantId);
            var hasWorkflowHandler = await _pluginLoader.IsPluginAvailableAsync<ICustomWorkflowHandler>(tenantId);

            var response = new PluginAvailabilityResponse
            {
                TenantId = tenantId,
                HasValidator = hasValidator,
                HasFormProcessor = hasFormProcessor,
                HasWorkflowHandler = hasWorkflowHandler
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking plugin availability for tenant {TenantId}", tenantId);
            return StatusCode(500, "An error occurred checking plugin availability");
        }
    }

    /// <summary>
    /// Reload plugins for a specific tenant
    /// </summary>
    [HttpPost("reload/{tenantId}")]
    public async Task<ActionResult> ReloadPlugins(string tenantId)
    {
        try
        {
            await _pluginLoader.ReloadTenantPluginsAsync(tenantId);
            return Ok($"Plugins reloaded for tenant {tenantId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reloading plugins for tenant {TenantId}", tenantId);
            return StatusCode(500, "An error occurred reloading plugins");
        }
    }
}

// Request/Response models
public class SubmitFormRequest
{
    public string? FormId { get; set; }
    public string FormType { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public string? UserId { get; set; }
    public Dictionary<string, object> Fields { get; set; } = new();
}

public class ValidateFormRequest
{
    public string? FormId { get; set; }
    public string FormType { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public string? UserId { get; set; }
    public Dictionary<string, object> Fields { get; set; } = new();
}

public class StartWorkflowRequest
{
    public string TenantId { get; set; } = string.Empty;
    public string WorkflowType { get; set; } = string.Empty;
    public string? UserId { get; set; }
    public Dictionary<string, object> InputData { get; set; } = new();
}

public class ExecuteStepRequest
{
    public Dictionary<string, object> InputData { get; set; } = new();
}

public class PluginAvailabilityResponse
{
    public string TenantId { get; set; } = string.Empty;
    public bool HasValidator { get; set; }
    public bool HasFormProcessor { get; set; }
    public bool HasWorkflowHandler { get; set; }
}
