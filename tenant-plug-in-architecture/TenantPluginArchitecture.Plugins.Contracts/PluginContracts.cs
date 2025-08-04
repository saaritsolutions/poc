using System.ComponentModel.DataAnnotations;

namespace TenantPluginArchitecture.Plugins.Contracts;

/// <summary>
/// Base interface for all tenant plugins
/// </summary>
public interface ITenantPlugin
{
    /// <summary>
    /// The tenant ID this plugin serves
    /// </summary>
    string TenantId { get; }
    
    /// <summary>
    /// Plugin name for identification
    /// </summary>
    string Name { get; }
    
    /// <summary>
    /// Plugin version
    /// </summary>
    string Version { get; }
    
    /// <summary>
    /// Initialize the plugin with tenant-specific configuration
    /// </summary>
    Task InitializeAsync(IDictionary<string, object> configuration);
}

/// <summary>
/// Custom validation result
/// </summary>
public class ValidationResult
{
    public bool IsValid { get; set; }
    public List<ValidationError> Errors { get; set; } = new();
    public IDictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
}

/// <summary>
/// Validation error details
/// </summary>
public class ValidationError
{
    public string Field { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public object? Value { get; set; }
}

/// <summary>
/// Validation context for passing additional information
/// </summary>
public class ValidationContext
{
    public string TenantId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string FormId { get; set; } = string.Empty;
    public IDictionary<string, object> Properties { get; set; } = new Dictionary<string, object>();
}

/// <summary>
/// Interface for custom validators that tenants can implement
/// </summary>
public interface ICustomValidator : ITenantPlugin
{
    /// <summary>
    /// Validate form data using tenant-specific rules
    /// </summary>
    Task<ValidationResult> ValidateAsync(object data, ValidationContext context);
    
    /// <summary>
    /// Get validation rules for a specific form type
    /// </summary>
    Task<IEnumerable<ValidationRule>> GetValidationRulesAsync(string formType);
}

/// <summary>
/// Validation rule definition
/// </summary>
public class ValidationRule
{
    public string Field { get; set; } = string.Empty;
    public string RuleType { get; set; } = string.Empty;
    public object? RuleValue { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
    public int Priority { get; set; }
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// Workflow execution result
/// </summary>
public class WorkflowResult
{
    public bool IsSuccessful { get; set; }
    public string? NextStep { get; set; }
    public IDictionary<string, object> OutputData { get; set; } = new Dictionary<string, object>();
    public List<string> Messages { get; set; } = new();
    public List<WorkflowError> Errors { get; set; } = new();
}

/// <summary>
/// Workflow error details
/// </summary>
public class WorkflowError
{
    public string Code { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? Step { get; set; }
    public Exception? Exception { get; set; }
}

/// <summary>
/// Workflow execution context
/// </summary>
public class WorkflowContext
{
    public string TenantId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string WorkflowId { get; set; } = string.Empty;
    public string CurrentStep { get; set; } = string.Empty;
    public IDictionary<string, object> Variables { get; set; } = new Dictionary<string, object>();
    public IDictionary<string, object> InputData { get; set; } = new Dictionary<string, object>();
}

/// <summary>
/// Interface for custom workflow handlers that tenants can implement
/// </summary>
public interface ICustomWorkflowHandler : ITenantPlugin
{
    /// <summary>
    /// Execute a workflow step with tenant-specific logic
    /// </summary>
    Task<WorkflowResult> ExecuteStepAsync(string stepName, WorkflowContext context);
    
    /// <summary>
    /// Get available workflow steps for this tenant
    /// </summary>
    Task<IEnumerable<WorkflowStep>> GetWorkflowStepsAsync(string workflowType);
    
    /// <summary>
    /// Determine the next step in the workflow
    /// </summary>
    Task<string?> GetNextStepAsync(string currentStep, WorkflowContext context);
}

/// <summary>
/// Workflow step definition
/// </summary>
public class WorkflowStep
{
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Order { get; set; }
    public bool IsActive { get; set; } = true;
    public IDictionary<string, object> Configuration { get; set; } = new Dictionary<string, object>();
}

/// <summary>
/// Form processing result
/// </summary>
public class FormProcessingResult
{
    public bool IsSuccessful { get; set; }
    public string? FormId { get; set; }
    public object? ProcessedData { get; set; }
    public List<string> Messages { get; set; } = new();
    public List<ValidationError> Errors { get; set; } = new();
    public IDictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
}

/// <summary>
/// Interface for custom form processors that tenants can implement
/// </summary>
public interface ICustomFormProcessor : ITenantPlugin
{
    /// <summary>
    /// Process form submission with tenant-specific logic
    /// </summary>
    Task<FormProcessingResult> ProcessFormAsync(string formType, object formData, ValidationContext context);
    
    /// <summary>
    /// Transform form data before processing
    /// </summary>
    Task<object> TransformFormDataAsync(string formType, object rawData);
    
    /// <summary>
    /// Get form configuration for a specific form type
    /// </summary>
    Task<FormConfiguration> GetFormConfigurationAsync(string formType);
}

/// <summary>
/// Form configuration definition
/// </summary>
public class FormConfiguration
{
    public string FormType { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public List<FormField> Fields { get; set; } = new();
    public IDictionary<string, object> Settings { get; set; } = new Dictionary<string, object>();
}

/// <summary>
/// Form field definition
/// </summary>
public class FormField
{
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string FieldType { get; set; } = string.Empty;
    public bool IsRequired { get; set; }
    public object? DefaultValue { get; set; }
    public IDictionary<string, object> Properties { get; set; } = new Dictionary<string, object>();
}

/// <summary>
/// Form data submitted by users
/// </summary>
public class FormData
{
    public string FormId { get; set; } = string.Empty;
    public string FormType { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public Dictionary<string, object> Fields { get; set; } = new();
    public DateTime SubmittedAt { get; set; }
    public string Status { get; set; } = string.Empty;
}

/// <summary>
/// Workflow instance data
/// </summary>
public class WorkflowInstance
{
    public string Id { get; set; } = string.Empty;
    public string WorkflowType { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string CurrentStep { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public Dictionary<string, object> Data { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}
