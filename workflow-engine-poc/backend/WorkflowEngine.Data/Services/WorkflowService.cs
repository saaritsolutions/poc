using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WorkflowEngine.Core.DTOs;
using WorkflowEngine.Core.Entities;
using WorkflowEngine.Core.Interfaces;
using WorkflowEngine.Data;

namespace WorkflowEngine.Data.Services;

public class WorkflowService : IWorkflowService
{
    private readonly WorkflowDbContext _context;
    private readonly ILogger<WorkflowService> _logger;

    public WorkflowService(WorkflowDbContext context, ILogger<WorkflowService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<WorkflowDto> CreateWorkflowAsync(CreateWorkflowRequest request, string createdBy)
    {
        var workflow = new Workflow
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            WorkflowDefinition = request.WorkflowDefinition,
            Version = 1,
            IsActive = false,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = createdBy
        };

        _context.Workflows.Add(workflow);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Created workflow {WorkflowId} by user {CreatedBy}", workflow.Id, createdBy);

        return MapToDto(workflow);
    }

    public async Task<WorkflowDto?> GetWorkflowAsync(Guid id)
    {
        var workflow = await _context.Workflows
            .FirstOrDefaultAsync(w => w.Id == id);

        return workflow != null ? MapToDto(workflow) : null;
    }

    public async Task<IEnumerable<WorkflowDto>> GetActiveWorkflowsAsync()
    {
        var workflows = await _context.Workflows
            .Where(w => w.IsActive)
            .OrderBy(w => w.Name)
            .ToListAsync();

        return workflows.Select(MapToDto);
    }

    public async Task<WorkflowDto> UpdateWorkflowAsync(Guid id, CreateWorkflowRequest request, string updatedBy)
    {
        var workflow = await _context.Workflows.FirstOrDefaultAsync(w => w.Id == id);
        if (workflow == null)
            throw new ArgumentException($"Workflow with ID {id} not found");

        // Create new version
        var newWorkflow = new Workflow
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            WorkflowDefinition = request.WorkflowDefinition,
            Version = workflow.Version + 1,
            IsActive = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            CreatedBy = updatedBy
        };

        // Deactivate old version
        workflow.IsActive = false;
        workflow.UpdatedAt = DateTime.UtcNow;

        _context.Workflows.Add(newWorkflow);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Updated workflow {WorkflowId} to version {Version} by user {UpdatedBy}", 
            newWorkflow.Id, newWorkflow.Version, updatedBy);

        return MapToDto(newWorkflow);
    }

    public async Task<bool> DeleteWorkflowAsync(Guid id)
    {
        var workflow = await _context.Workflows
            .Include(w => w.Instances)
            .FirstOrDefaultAsync(w => w.Id == id);

        if (workflow == null)
            return false;

        // Check if there are active instances
        if (workflow.Instances.Any(i => i.Status == WorkflowStatus.Running))
        {
            throw new InvalidOperationException("Cannot delete workflow with active instances");
        }

        _context.Workflows.Remove(workflow);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Deleted workflow {WorkflowId}", id);
        return true;
    }

    public async Task<bool> ActivateWorkflowAsync(Guid id)
    {
        var workflow = await _context.Workflows.FirstOrDefaultAsync(w => w.Id == id);
        if (workflow == null)
            return false;

        workflow.IsActive = true;
        workflow.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Activated workflow {WorkflowId}", id);
        return true;
    }

    public async Task<bool> DeactivateWorkflowAsync(Guid id)
    {
        var workflow = await _context.Workflows.FirstOrDefaultAsync(w => w.Id == id);
        if (workflow == null)
            return false;

        workflow.IsActive = false;
        workflow.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Deactivated workflow {WorkflowId}", id);
        return true;
    }

    private static WorkflowDto MapToDto(Workflow workflow)
    {
        return new WorkflowDto
        {
            Id = workflow.Id,
            Name = workflow.Name,
            Description = workflow.Description,
            WorkflowDefinition = workflow.WorkflowDefinition,
            Version = workflow.Version,
            IsActive = workflow.IsActive,
            CreatedAt = workflow.CreatedAt,
            UpdatedAt = workflow.UpdatedAt,
            CreatedBy = workflow.CreatedBy
        };
    }
}
