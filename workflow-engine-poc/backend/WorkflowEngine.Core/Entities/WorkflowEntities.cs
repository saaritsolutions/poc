using System.ComponentModel.DataAnnotations;

namespace WorkflowEngine.Core.Entities;

public class Workflow
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string WorkflowDefinition { get; set; } = string.Empty; // JSON representation
    public int Version { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    
    // Navigation properties
    public ICollection<WorkflowInstance> Instances { get; set; } = new List<WorkflowInstance>();
}

public class WorkflowInstance
{
    public Guid Id { get; set; }
    public Guid WorkflowId { get; set; }
    public string InstanceData { get; set; } = string.Empty; // JSON data
    public WorkflowStatus Status { get; set; }
    public string? CurrentStepId { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string InitiatedBy { get; set; } = string.Empty;
    
    // Navigation properties
    public Workflow Workflow { get; set; } = null!;
    public ICollection<WorkflowStep> Steps { get; set; } = new List<WorkflowStep>();
    public ICollection<WorkflowAuditLog> AuditLogs { get; set; } = new List<WorkflowAuditLog>();
}

public class WorkflowStep
{
    public Guid Id { get; set; }
    public Guid WorkflowInstanceId { get; set; }
    public string StepId { get; set; } = string.Empty; // Reference to step in workflow definition
    public string StepName { get; set; } = string.Empty;
    public StepStatus Status { get; set; }
    public string? AssignedTo { get; set; }
    public string? AssignedRole { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? Comments { get; set; }
    public string? StepData { get; set; } // JSON data specific to this step
    
    // Navigation properties
    public WorkflowInstance WorkflowInstance { get; set; } = null!;
}

public class WorkflowAuditLog
{
    public Guid Id { get; set; }
    public Guid WorkflowInstanceId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? StepId { get; set; }
    public string? PreviousState { get; set; }
    public string? NewState { get; set; }
    public string PerformedBy { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string? Comments { get; set; }
    public string? AdditionalData { get; set; } // JSON data
    
    // Navigation properties
    public WorkflowInstance WorkflowInstance { get; set; } = null!;
}

public enum WorkflowStatus
{
    Draft = 0,
    Running = 1,
    Completed = 2,
    Cancelled = 3,
    Failed = 4,
    Suspended = 5
}

public enum StepStatus
{
    Pending = 0,
    InProgress = 1,
    Completed = 2,
    Rejected = 3,
    Skipped = 4,
    Failed = 5
}
