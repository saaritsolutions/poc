using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WorkflowEngine.Core.DTOs;
using WorkflowEngine.Core.Entities;
using WorkflowEngine.Core.Interfaces;
using WorkflowEngine.Data;

namespace WorkflowEngine.Data.Services;

public class WorkflowInstanceService : IWorkflowInstanceService
{
    private readonly WorkflowDbContext _context;
    private readonly IWorkflowEngine _workflowEngine;
    private readonly ILogger<WorkflowInstanceService> _logger;

    public WorkflowInstanceService(
        WorkflowDbContext context, 
        IWorkflowEngine workflowEngine,
        ILogger<WorkflowInstanceService> logger)
    {
        _context = context;
        _workflowEngine = workflowEngine;
        _logger = logger;
    }

    public async Task<WorkflowInstanceDto> StartWorkflowAsync(CreateWorkflowInstanceRequest request, string initiatedBy)
    {
        var workflow = await _context.Workflows
            .FirstOrDefaultAsync(w => w.Id == request.WorkflowId && w.IsActive);

        if (workflow == null)
            throw new ArgumentException("Workflow not found or not active");

        var instance = new WorkflowInstance
        {
            Id = Guid.NewGuid(),
            WorkflowId = request.WorkflowId,
            InstanceData = request.InstanceData,
            Status = WorkflowStatus.Running,
            StartedAt = DateTime.UtcNow,
            InitiatedBy = initiatedBy
        };

        _context.WorkflowInstances.Add(instance);

        // Add audit log
        var auditLog = new WorkflowAuditLog
        {
            Id = Guid.NewGuid(),
            WorkflowInstanceId = instance.Id,
            Action = "STARTED",
            NewState = WorkflowStatus.Running.ToString(),
            PerformedBy = initiatedBy,
            Timestamp = DateTime.UtcNow,
            Comments = "Workflow instance started"
        };

        _context.WorkflowAuditLogs.Add(auditLog);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Started workflow instance {InstanceId} for workflow {WorkflowId} by user {InitiatedBy}", 
            instance.Id, request.WorkflowId, initiatedBy);

        // Process the first step
        await _workflowEngine.ProcessNextStepAsync(instance.Id);

        return await GetWorkflowInstanceAsync(instance.Id) ?? throw new InvalidOperationException("Failed to retrieve created instance");
    }

    public async Task<WorkflowInstanceDto?> GetWorkflowInstanceAsync(Guid id)
    {
        var instance = await _context.WorkflowInstances
            .Include(i => i.Workflow)
            .Include(i => i.Steps)
            .FirstOrDefaultAsync(i => i.Id == id);

        return instance != null ? MapToDtoAsync(instance) : null;
    }

    public async Task<IEnumerable<WorkflowInstanceDto>> GetWorkflowInstancesAsync(Guid? workflowId = null, WorkflowStatus? status = null)
    {
        var query = _context.WorkflowInstances
            .Include(i => i.Workflow)
            .Include(i => i.Steps)
            .AsQueryable();

        if (workflowId.HasValue)
            query = query.Where(i => i.WorkflowId == workflowId.Value);

        if (status.HasValue)
            query = query.Where(i => i.Status == status.Value);

        var instances = await query
            .OrderByDescending(i => i.StartedAt)
            .ToListAsync();

        var result = new List<WorkflowInstanceDto>();
        foreach (var instance in instances)
        {
            result.Add(MapToDtoAsync(instance));
        }

        return result;
    }

    public async Task<IEnumerable<WorkflowInstanceDto>> GetUserTasksAsync(string userId)
    {
        var instances = await _context.WorkflowInstances
            .Include(i => i.Workflow)
            .Include(i => i.Steps)
            .Where(i => i.Status == WorkflowStatus.Running && 
                       i.Steps.Any(s => s.Status == StepStatus.Pending && 
                                       (s.AssignedTo == userId || s.AssignedRole != null)))
            .OrderBy(i => i.StartedAt)
            .ToListAsync();

        var result = new List<WorkflowInstanceDto>();
        foreach (var instance in instances)
        {
            result.Add(MapToDtoAsync(instance));
        }

        return result;
    }

    public async Task<WorkflowInstanceDto> CompleteStepAsync(Guid instanceId, string stepId, CompleteStepRequest request, string completedBy)
    {
        var instance = await _context.WorkflowInstances
            .Include(i => i.Steps)
            .FirstOrDefaultAsync(i => i.Id == instanceId);

        if (instance == null)
            throw new ArgumentException("Workflow instance not found");

        var step = instance.Steps.FirstOrDefault(s => s.StepId == stepId);
        if (step == null)
            throw new ArgumentException("Step not found");

        if (step.Status != StepStatus.Pending && step.Status != StepStatus.InProgress)
            throw new InvalidOperationException("Step is not in a completable state");

        // Update step
        step.Status = request.IsApproved ? StepStatus.Completed : StepStatus.Rejected;
        step.CompletedAt = DateTime.UtcNow;
        step.Comments = request.Comments;
        step.StepData = request.StepData;

        // Add audit log
        var auditLog = new WorkflowAuditLog
        {
            Id = Guid.NewGuid(),
            WorkflowInstanceId = instanceId,
            Action = request.IsApproved ? "APPROVED" : "REJECTED",
            StepId = stepId,
            PreviousState = StepStatus.Pending.ToString(),
            NewState = step.Status.ToString(),
            PerformedBy = completedBy,
            Timestamp = DateTime.UtcNow,
            Comments = request.Comments
        };

        _context.WorkflowAuditLogs.Add(auditLog);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Completed step {StepId} in instance {InstanceId} by user {CompletedBy} with result {Result}", 
            stepId, instanceId, completedBy, request.IsApproved ? "APPROVED" : "REJECTED");

        // Process next step
        await _workflowEngine.ProcessNextStepAsync(instanceId);

        return await GetWorkflowInstanceAsync(instanceId) ?? throw new InvalidOperationException("Failed to retrieve updated instance");
    }

    public async Task<WorkflowInstanceDto> CancelWorkflowAsync(Guid instanceId, string cancelledBy, string? reason = null)
    {
        var instance = await _context.WorkflowInstances.FirstOrDefaultAsync(i => i.Id == instanceId);
        if (instance == null)
            throw new ArgumentException("Workflow instance not found");

        instance.Status = WorkflowStatus.Cancelled;
        instance.CompletedAt = DateTime.UtcNow;

        // Add audit log
        var auditLog = new WorkflowAuditLog
        {
            Id = Guid.NewGuid(),
            WorkflowInstanceId = instanceId,
            Action = "CANCELLED",
            PreviousState = WorkflowStatus.Running.ToString(),
            NewState = WorkflowStatus.Cancelled.ToString(),
            PerformedBy = cancelledBy,
            Timestamp = DateTime.UtcNow,
            Comments = reason ?? "Workflow cancelled"
        };

        _context.WorkflowAuditLogs.Add(auditLog);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Cancelled workflow instance {InstanceId} by user {CancelledBy}", instanceId, cancelledBy);

        return await GetWorkflowInstanceAsync(instanceId) ?? throw new InvalidOperationException("Failed to retrieve cancelled instance");
    }

    public async Task<IEnumerable<WorkflowAuditLogDto>> GetAuditLogAsync(Guid instanceId)
    {
        var auditLogs = await _context.WorkflowAuditLogs
            .Where(a => a.WorkflowInstanceId == instanceId)
            .OrderByDescending(a => a.Timestamp)
            .ToListAsync();

        return auditLogs.Select(MapAuditLogToDto);
    }

    private WorkflowInstanceDto MapToDtoAsync(WorkflowInstance instance)
    {
        var currentStep = instance.Steps
            .Where(s => s.Status == StepStatus.Pending || s.Status == StepStatus.InProgress)
            .OrderBy(s => s.CreatedAt)
            .FirstOrDefault();

        return new WorkflowInstanceDto
        {
            Id = instance.Id,
            WorkflowId = instance.WorkflowId,
            WorkflowName = instance.Workflow?.Name ?? "Unknown",
            InstanceData = instance.InstanceData,
            Status = instance.Status.ToString(),
            CurrentStepId = currentStep?.StepId,
            CurrentStepName = currentStep?.StepName,
            StartedAt = instance.StartedAt,
            CompletedAt = instance.CompletedAt,
            InitiatedBy = instance.InitiatedBy,
            Steps = instance.Steps.Select(MapStepToDto).OrderBy(s => s.CreatedAt).ToList()
        };
    }

    private static WorkflowStepDto MapStepToDto(WorkflowStep step)
    {
        return new WorkflowStepDto
        {
            Id = step.Id,
            StepId = step.StepId,
            StepName = step.StepName,
            Status = step.Status.ToString(),
            AssignedTo = step.AssignedTo,
            AssignedRole = step.AssignedRole,
            CreatedAt = step.CreatedAt,
            StartedAt = step.StartedAt,
            CompletedAt = step.CompletedAt,
            Comments = step.Comments,
            StepData = step.StepData
        };
    }

    private static WorkflowAuditLogDto MapAuditLogToDto(WorkflowAuditLog auditLog)
    {
        return new WorkflowAuditLogDto
        {
            Id = auditLog.Id,
            Action = auditLog.Action,
            StepId = auditLog.StepId,
            PreviousState = auditLog.PreviousState,
            NewState = auditLog.NewState,
            PerformedBy = auditLog.PerformedBy,
            Timestamp = auditLog.Timestamp,
            Comments = auditLog.Comments,
            AdditionalData = auditLog.AdditionalData
        };
    }
}
