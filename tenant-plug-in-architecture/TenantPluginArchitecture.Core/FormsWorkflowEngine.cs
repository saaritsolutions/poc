using Microsoft.Extensions.Logging;
using TenantPluginArchitecture.Plugins.Contracts;

namespace TenantPluginArchitecture.Core;

/// <summary>
/// Form data model
/// </summary>
public class FormData
{
    public string FormId { get; set; } = string.Empty;
    public string FormType { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public Dictionary<string, object> Fields { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? SubmittedAt { get; set; }
    public string Status { get; set; } = "Draft";
}

/// <summary>
/// Workflow instance data model
/// </summary>
public class WorkflowInstance
{
    public string WorkflowId { get; set; } = string.Empty;
    public string WorkflowType { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string CurrentStep { get; set; } = string.Empty;
    public Dictionary<string, object> Variables { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    public string Status { get; set; } = "Active";
    public List<WorkflowStepExecution> StepExecutions { get; set; } = new();
}

/// <summary>
/// Workflow step execution record
/// </summary>
public class WorkflowStepExecution
{
    public string StepName { get; set; } = string.Empty;
    public DateTime ExecutedAt { get; set; } = DateTime.UtcNow;
    public bool IsSuccessful { get; set; }
    public Dictionary<string, object> InputData { get; set; } = new();
    public Dictionary<string, object> OutputData { get; set; } = new();
    public List<string> Messages { get; set; } = new();
    public List<string> Errors { get; set; } = new();
}

/// <summary>
/// Interface for the forms engine
/// </summary>
public interface IFormEngine
{
    /// <summary>
    /// Submit a form with tenant-specific validation and processing
    /// </summary>
    Task<FormProcessingResult> SubmitFormAsync(FormData formData);
    
    /// <summary>
    /// Validate form data using tenant-specific validators
    /// </summary>
    Task<ValidationResult> ValidateFormAsync(FormData formData);
    
    /// <summary>
    /// Get form configuration for a tenant and form type
    /// </summary>
    Task<FormConfiguration> GetFormConfigurationAsync(string tenantId, string formType);
    
    /// <summary>
    /// Process form data transformation
    /// </summary>
    Task<object> TransformFormDataAsync(FormData formData);
}

/// <summary>
/// Interface for the workflow engine
/// </summary>
public interface IWorkflowEngine
{
    /// <summary>
    /// Start a new workflow instance
    /// </summary>
    Task<WorkflowResult> StartWorkflowAsync(string tenantId, string workflowType, string userId, Dictionary<string, object> inputData);
    
    /// <summary>
    /// Execute the next step in a workflow
    /// </summary>
    Task<WorkflowResult> ExecuteNextStepAsync(string workflowId);
    
    /// <summary>
    /// Execute a specific step in a workflow
    /// </summary>
    Task<WorkflowResult> ExecuteStepAsync(string workflowId, string stepName, Dictionary<string, object> inputData);
    
    /// <summary>
    /// Get workflow instance by ID
    /// </summary>
    Task<WorkflowInstance?> GetWorkflowInstanceAsync(string workflowId);
    
    /// <summary>
    /// Get available workflow steps for a tenant and workflow type
    /// </summary>
    Task<IEnumerable<WorkflowStep>> GetWorkflowStepsAsync(string tenantId, string workflowType);
}

/// <summary>
/// Forms engine implementation
/// </summary>
public class FormEngine : IFormEngine
{
    private readonly IPluginLoader _pluginLoader;
    private readonly ILogger<FormEngine> _logger;

    public FormEngine(IPluginLoader pluginLoader, ILogger<FormEngine> logger)
    {
        _pluginLoader = pluginLoader;
        _logger = logger;
    }

    public async Task<FormProcessingResult> SubmitFormAsync(FormData formData)
    {
        try
        {
            _logger.LogInformation("Processing form submission for tenant {TenantId}, form {FormId}", 
                                 formData.TenantId, formData.FormId);

            // Validate form first
            var validationResult = await ValidateFormAsync(formData);
            if (!validationResult.IsValid)
            {
                return new FormProcessingResult
                {
                    IsSuccessful = false,
                    FormId = formData.FormId,
                    Errors = validationResult.Errors,
                    Messages = new List<string> { "Form validation failed" }
                };
            }

            // Transform form data
            var transformedData = await TransformFormDataAsync(formData);

            // Load tenant-specific form processor
            var processor = await _pluginLoader.LoadPluginAsync<ICustomFormProcessor>(formData.TenantId);
            if (processor != null)
            {
                var context = new ValidationContext
                {
                    TenantId = formData.TenantId,
                    UserId = formData.UserId,
                    FormId = formData.FormId
                };

                var result = await processor.ProcessFormAsync(formData.FormType, transformedData, context);
                
                _logger.LogInformation("Form {FormId} processed successfully for tenant {TenantId}", 
                                     formData.FormId, formData.TenantId);
                
                return result;
            }

            // Default processing if no tenant-specific processor
            return new FormProcessingResult
            {
                IsSuccessful = true,
                FormId = formData.FormId,
                ProcessedData = transformedData,
                Messages = new List<string> { "Form processed with default handler" }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing form {FormId} for tenant {TenantId}", 
                           formData.FormId, formData.TenantId);
            
            return new FormProcessingResult
            {
                IsSuccessful = false,
                FormId = formData.FormId,
                Errors = new List<ValidationError> 
                { 
                    new ValidationError 
                    { 
                        Message = "An error occurred processing the form", 
                        Code = "PROCESSING_ERROR" 
                    } 
                }
            };
        }
    }

    public async Task<ValidationResult> ValidateFormAsync(FormData formData)
    {
        try
        {
            var context = new ValidationContext
            {
                TenantId = formData.TenantId,
                UserId = formData.UserId,
                FormId = formData.FormId
            };

            // Load tenant-specific validator
            var validator = await _pluginLoader.LoadPluginAsync<ICustomValidator>(formData.TenantId);
            if (validator != null)
            {
                return await validator.ValidateAsync(formData.Fields, context);
            }

            // Default validation
            return new ValidationResult { IsValid = true };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating form {FormId} for tenant {TenantId}", 
                           formData.FormId, formData.TenantId);
            
            return new ValidationResult
            {
                IsValid = false,
                Errors = new List<ValidationError> 
                { 
                    new ValidationError 
                    { 
                        Message = "Validation error occurred", 
                        Code = "VALIDATION_ERROR" 
                    } 
                }
            };
        }
    }

    public async Task<FormConfiguration> GetFormConfigurationAsync(string tenantId, string formType)
    {
        try
        {
            var processor = await _pluginLoader.LoadPluginAsync<ICustomFormProcessor>(tenantId);
            if (processor != null)
            {
                return await processor.GetFormConfigurationAsync(formType);
            }

            // Return default configuration
            return new FormConfiguration
            {
                FormType = formType,
                DisplayName = formType,
                IsActive = true
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting form configuration for tenant {TenantId}, form type {FormType}", 
                           tenantId, formType);
            throw;
        }
    }

    public async Task<object> TransformFormDataAsync(FormData formData)
    {
        try
        {
            var processor = await _pluginLoader.LoadPluginAsync<ICustomFormProcessor>(formData.TenantId);
            if (processor != null)
            {
                return await processor.TransformFormDataAsync(formData.FormType, formData.Fields);
            }

            // Default transformation (no change)
            return formData.Fields;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error transforming form data for tenant {TenantId}, form {FormId}", 
                           formData.TenantId, formData.FormId);
            
            // Return original data on error
            return formData.Fields;
        }
    }
}

/// <summary>
/// Workflow engine implementation
/// </summary>
public class WorkflowEngine : IWorkflowEngine
{
    private readonly IPluginLoader _pluginLoader;
    private readonly ILogger<WorkflowEngine> _logger;
    private readonly Dictionary<string, WorkflowInstance> _workflowInstances;

    public WorkflowEngine(IPluginLoader pluginLoader, ILogger<WorkflowEngine> logger)
    {
        _pluginLoader = pluginLoader;
        _logger = logger;
        _workflowInstances = new Dictionary<string, WorkflowInstance>();
    }

    public async Task<WorkflowResult> StartWorkflowAsync(string tenantId, string workflowType, string userId, Dictionary<string, object> inputData)
    {
        try
        {
            var workflowId = Guid.NewGuid().ToString();
            
            var instance = new WorkflowInstance
            {
                WorkflowId = workflowId,
                WorkflowType = workflowType,
                TenantId = tenantId,
                UserId = userId,
                Variables = new Dictionary<string, object>(inputData),
                CurrentStep = "Start"
            };

            _workflowInstances[workflowId] = instance;

            _logger.LogInformation("Started workflow {WorkflowId} of type {WorkflowType} for tenant {TenantId}", 
                                 workflowId, workflowType, tenantId);

            // Execute first step
            return await ExecuteNextStepAsync(workflowId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting workflow of type {WorkflowType} for tenant {TenantId}", 
                           workflowType, tenantId);
            
            return new WorkflowResult
            {
                IsSuccessful = false,
                Errors = new List<WorkflowError> 
                { 
                    new WorkflowError 
                    { 
                        Message = "Failed to start workflow", 
                        Code = "WORKFLOW_START_ERROR", 
                        Exception = ex 
                    } 
                }
            };
        }
    }

    public async Task<WorkflowResult> ExecuteNextStepAsync(string workflowId)
    {
        if (!_workflowInstances.TryGetValue(workflowId, out var instance))
        {
            return new WorkflowResult
            {
                IsSuccessful = false,
                Errors = new List<WorkflowError> 
                { 
                    new WorkflowError 
                    { 
                        Message = "Workflow instance not found", 
                        Code = "WORKFLOW_NOT_FOUND" 
                    } 
                }
            };
        }

        try
        {
            var handler = await _pluginLoader.LoadPluginAsync<ICustomWorkflowHandler>(instance.TenantId);
            if (handler == null)
            {
                return new WorkflowResult
                {
                    IsSuccessful = false,
                    Errors = new List<WorkflowError> 
                    { 
                        new WorkflowError 
                        { 
                            Message = "No workflow handler found for tenant", 
                            Code = "HANDLER_NOT_FOUND" 
                        } 
                    }
                };
            }

            var nextStep = await handler.GetNextStepAsync(instance.CurrentStep, new WorkflowContext
            {
                TenantId = instance.TenantId,
                UserId = instance.UserId,
                WorkflowId = instance.WorkflowId,
                CurrentStep = instance.CurrentStep,
                Variables = instance.Variables
            });

            if (nextStep == null)
            {
                // Workflow completed
                instance.Status = "Completed";
                instance.CompletedAt = DateTime.UtcNow;
                
                return new WorkflowResult
                {
                    IsSuccessful = true,
                    Messages = new List<string> { "Workflow completed successfully" }
                };
            }

            return await ExecuteStepAsync(workflowId, nextStep, new Dictionary<string, object>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing next step for workflow {WorkflowId}", workflowId);
            
            return new WorkflowResult
            {
                IsSuccessful = false,
                Errors = new List<WorkflowError> 
                { 
                    new WorkflowError 
                    { 
                        Message = "Error executing workflow step", 
                        Code = "STEP_EXECUTION_ERROR", 
                        Exception = ex 
                    } 
                }
            };
        }
    }

    public async Task<WorkflowResult> ExecuteStepAsync(string workflowId, string stepName, Dictionary<string, object> inputData)
    {
        if (!_workflowInstances.TryGetValue(workflowId, out var instance))
        {
            return new WorkflowResult
            {
                IsSuccessful = false,
                Errors = new List<WorkflowError> 
                { 
                    new WorkflowError 
                    { 
                        Message = "Workflow instance not found", 
                        Code = "WORKFLOW_NOT_FOUND" 
                    } 
                }
            };
        }

        try
        {
            var handler = await _pluginLoader.LoadPluginAsync<ICustomWorkflowHandler>(instance.TenantId);
            if (handler == null)
            {
                return new WorkflowResult
                {
                    IsSuccessful = false,
                    Errors = new List<WorkflowError> 
                    { 
                        new WorkflowError 
                        { 
                            Message = "No workflow handler found for tenant", 
                            Code = "HANDLER_NOT_FOUND" 
                        } 
                    }
                };
            }

            var context = new WorkflowContext
            {
                TenantId = instance.TenantId,
                UserId = instance.UserId,
                WorkflowId = instance.WorkflowId,
                CurrentStep = stepName,
                Variables = instance.Variables,
                InputData = inputData
            };

            var result = await handler.ExecuteStepAsync(stepName, context);

            // Record step execution
            var stepExecution = new WorkflowStepExecution
            {
                StepName = stepName,
                IsSuccessful = result.IsSuccessful,
                InputData = inputData,
                OutputData = new Dictionary<string, object>(result.OutputData),
                Messages = result.Messages,
                Errors = result.Errors.Select(e => e.Message).ToList()
            };

            instance.StepExecutions.Add(stepExecution);

            if (result.IsSuccessful)
            {
                instance.CurrentStep = stepName;
                
                // Merge output data into workflow variables
                foreach (var kvp in result.OutputData)
                {
                    instance.Variables[kvp.Key] = kvp.Value;
                }
            }

            _logger.LogInformation("Executed step {StepName} for workflow {WorkflowId}. Success: {IsSuccessful}", 
                                 stepName, workflowId, result.IsSuccessful);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing step {StepName} for workflow {WorkflowId}", 
                           stepName, workflowId);
            
            return new WorkflowResult
            {
                IsSuccessful = false,
                Errors = new List<WorkflowError> 
                { 
                    new WorkflowError 
                    { 
                        Message = "Error executing workflow step", 
                        Code = "STEP_EXECUTION_ERROR", 
                        Step = stepName,
                        Exception = ex 
                    } 
                }
            };
        }
    }

    public async Task<WorkflowInstance?> GetWorkflowInstanceAsync(string workflowId)
    {
        _workflowInstances.TryGetValue(workflowId, out var instance);
        return await Task.FromResult(instance);
    }

    public async Task<IEnumerable<WorkflowStep>> GetWorkflowStepsAsync(string tenantId, string workflowType)
    {
        try
        {
            var handler = await _pluginLoader.LoadPluginAsync<ICustomWorkflowHandler>(tenantId);
            if (handler != null)
            {
                return await handler.GetWorkflowStepsAsync(workflowType);
            }

            return new List<WorkflowStep>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting workflow steps for tenant {TenantId}, type {WorkflowType}", 
                           tenantId, workflowType);
            return new List<WorkflowStep>();
        }
    }
}
