namespace WorkflowEngine.Core.DTOs;

public class WorkflowDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string WorkflowDefinition { get; set; } = string.Empty;
    public int Version { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
}

public class CreateWorkflowRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string WorkflowDefinition { get; set; } = string.Empty;
}

public class WorkflowInstanceDto
{
    public Guid Id { get; set; }
    public Guid WorkflowId { get; set; }
    public string WorkflowName { get; set; } = string.Empty;
    public string InstanceData { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? CurrentStepId { get; set; }
    public string? CurrentStepName { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string InitiatedBy { get; set; } = string.Empty;
    public List<WorkflowStepDto> Steps { get; set; } = new();
}

public class CreateWorkflowInstanceRequest
{
    public Guid WorkflowId { get; set; }
    public string InstanceData { get; set; } = string.Empty;
}

public class WorkflowStepDto
{
    public Guid Id { get; set; }
    public string StepId { get; set; } = string.Empty;
    public string StepName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? AssignedTo { get; set; }
    public string? AssignedRole { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? Comments { get; set; }
    public string? StepData { get; set; }
}

public class CompleteStepRequest
{
    public string? Comments { get; set; }
    public string? StepData { get; set; }
    public bool IsApproved { get; set; } = true;
}

public class WorkflowAuditLogDto
{
    public Guid Id { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? StepId { get; set; }
    public string? PreviousState { get; set; }
    public string? NewState { get; set; }
    public string PerformedBy { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string? Comments { get; set; }
    public string? AdditionalData { get; set; }
}
