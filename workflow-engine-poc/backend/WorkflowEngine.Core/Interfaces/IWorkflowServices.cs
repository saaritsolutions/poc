using WorkflowEngine.Core.DTOs;
using WorkflowEngine.Core.Entities;

namespace WorkflowEngine.Core.Interfaces;

public interface IWorkflowService
{
    Task<WorkflowDto> CreateWorkflowAsync(CreateWorkflowRequest request, string createdBy);
    Task<WorkflowDto?> GetWorkflowAsync(Guid id);
    Task<IEnumerable<WorkflowDto>> GetActiveWorkflowsAsync();
    Task<WorkflowDto> UpdateWorkflowAsync(Guid id, CreateWorkflowRequest request, string updatedBy);
    Task<bool> DeleteWorkflowAsync(Guid id);
    Task<bool> ActivateWorkflowAsync(Guid id);
    Task<bool> DeactivateWorkflowAsync(Guid id);
}

public interface IWorkflowInstanceService
{
    Task<WorkflowInstanceDto> StartWorkflowAsync(CreateWorkflowInstanceRequest request, string initiatedBy);
    Task<WorkflowInstanceDto?> GetWorkflowInstanceAsync(Guid id);
    Task<IEnumerable<WorkflowInstanceDto>> GetWorkflowInstancesAsync(Guid? workflowId = null, WorkflowStatus? status = null);
    Task<IEnumerable<WorkflowInstanceDto>> GetUserTasksAsync(string userId);
    Task<WorkflowInstanceDto> CompleteStepAsync(Guid instanceId, string stepId, CompleteStepRequest request, string completedBy);
    Task<WorkflowInstanceDto> CancelWorkflowAsync(Guid instanceId, string cancelledBy, string? reason = null);
    Task<IEnumerable<WorkflowAuditLogDto>> GetAuditLogAsync(Guid instanceId);
}

public interface IWorkflowEngine
{
    Task<WorkflowInstance> ProcessNextStepAsync(Guid instanceId);
    Task<bool> CanProgressToNextStepAsync(Guid instanceId);
    Task<IEnumerable<string>> GetAvailableActionsAsync(Guid instanceId, string userId);
}

public interface IWorkflowDefinitionParser
{
    WorkflowDefinition ParseDefinition(string workflowJson);
    bool ValidateDefinition(string workflowJson);
    string SerializeDefinition(WorkflowDefinition definition);
}

public interface INotificationService
{
    Task SendWorkflowNotificationAsync(Guid instanceId, string stepId, string userId, string message);
    Task SendWorkflowCompletedNotificationAsync(Guid instanceId, string initiatedBy);
}

// Workflow Definition Models
public class WorkflowDefinition
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<WorkflowStepDefinition> Steps { get; set; } = new();
    public List<WorkflowTransition> Transitions { get; set; } = new();
    public string StartStepId { get; set; } = string.Empty;
}

public class WorkflowStepDefinition
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // manual, automatic, approval, etc.
    public string? AssignedRole { get; set; }
    public string? AssignedUser { get; set; }
    public Dictionary<string, object> Properties { get; set; } = new();
    public bool IsRequired { get; set; } = true;
}

public class WorkflowTransition
{
    public string Id { get; set; } = string.Empty;
    public string FromStepId { get; set; } = string.Empty;
    public string ToStepId { get; set; } = string.Empty;
    public string? Condition { get; set; } // JSON condition
    public string? Action { get; set; } // approve, reject, etc.
}
