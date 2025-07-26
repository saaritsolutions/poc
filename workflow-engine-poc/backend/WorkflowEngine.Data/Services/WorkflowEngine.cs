using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using WorkflowEngine.Core.Entities;
using WorkflowEngine.Core.Interfaces;
using WorkflowEngine.Data;

namespace WorkflowEngine.Data.Services;

public class WorkflowEngine : IWorkflowEngine
{
    private readonly WorkflowDbContext _context;
    private readonly IWorkflowDefinitionParser _definitionParser;
    private readonly ILogger<WorkflowEngine> _logger;

    public WorkflowEngine(
        WorkflowDbContext context,
        IWorkflowDefinitionParser definitionParser,
        ILogger<WorkflowEngine> logger)
    {
        _context = context;
        _definitionParser = definitionParser;
        _logger = logger;
    }

    public async Task<WorkflowInstance> ProcessNextStepAsync(Guid instanceId)
    {
        var instance = await _context.WorkflowInstances
            .Include(i => i.Workflow)
            .Include(i => i.Steps)
            .FirstOrDefaultAsync(i => i.Id == instanceId);

        if (instance == null)
            throw new ArgumentException("Workflow instance not found");

        if (instance.Status != WorkflowStatus.Running)
            return instance;

        var workflowDefinition = _definitionParser.ParseDefinition(instance.Workflow.WorkflowDefinition);

        // If no steps exist, create the first step
        if (!instance.Steps.Any())
        {
            CreateFirstStepAsync(instance, workflowDefinition);
        }
        else
        {
            // Check if current step is completed and move to next
            ProcessCurrentStepAsync(instance, workflowDefinition);
        }

        await _context.SaveChangesAsync();
        return instance;
    }

    public async Task<bool> CanProgressToNextStepAsync(Guid instanceId)
    {
        var instance = await _context.WorkflowInstances
            .Include(i => i.Steps)
            .FirstOrDefaultAsync(i => i.Id == instanceId);

        if (instance == null || instance.Status != WorkflowStatus.Running)
            return false;

        // Check if all pending/in-progress steps are completed
        return !instance.Steps.Any(s => s.Status == StepStatus.Pending || s.Status == StepStatus.InProgress);
    }

    public async Task<IEnumerable<string>> GetAvailableActionsAsync(Guid instanceId, string userId)
    {
        var instance = await _context.WorkflowInstances
            .Include(i => i.Steps)
            .FirstOrDefaultAsync(i => i.Id == instanceId);

        if (instance == null || instance.Status != WorkflowStatus.Running)
            return new List<string>();

        var availableActions = new List<string>();

        // Check if user has pending tasks
        var userTasks = instance.Steps.Where(s => 
            s.Status == StepStatus.Pending && 
            (s.AssignedTo == userId || s.AssignedRole != null))
            .ToList();

        if (userTasks.Any())
        {
            availableActions.Add("approve");
            availableActions.Add("reject");
        }

        // Check if user can cancel (e.g., if they initiated the workflow)
        if (instance.InitiatedBy == userId)
        {
            availableActions.Add("cancel");
        }

        return availableActions;
    }

    private void CreateFirstStepAsync(WorkflowInstance instance, WorkflowDefinition workflowDefinition)
    {
        var firstStepDef = workflowDefinition.Steps.FirstOrDefault(s => s.Id == workflowDefinition.StartStepId);
        if (firstStepDef == null)
        {
            _logger.LogError("Start step not found in workflow definition for instance {InstanceId}", instance.Id);
            instance.Status = WorkflowStatus.Failed;
            return;
        }

        var step = new WorkflowStep
        {
            Id = Guid.NewGuid(),
            WorkflowInstanceId = instance.Id,
            StepId = firstStepDef.Id,
            StepName = firstStepDef.Name,
            Status = StepStatus.Pending,
            AssignedTo = firstStepDef.AssignedUser,
            AssignedRole = firstStepDef.AssignedRole,
            CreatedAt = DateTime.UtcNow
        };

        _context.WorkflowSteps.Add(step);
        instance.CurrentStepId = firstStepDef.Id;

        _logger.LogInformation("Created first step {StepId} for workflow instance {InstanceId}", 
            firstStepDef.Id, instance.Id);
    }

    private void ProcessCurrentStepAsync(WorkflowInstance instance, WorkflowDefinition workflowDefinition)
    {
        var completedSteps = instance.Steps.Where(s => s.Status == StepStatus.Completed).ToList();
        var currentStep = instance.Steps.FirstOrDefault(s => s.Status == StepStatus.Pending || s.Status == StepStatus.InProgress);

        // If there's still a pending/in-progress step, nothing to do
        if (currentStep != null)
            return;

        // If no pending steps, find next step(s) to create
        var lastCompletedStep = completedSteps.LastOrDefault();
        if (lastCompletedStep == null)
            return;

        var transitions = workflowDefinition.Transitions
            .Where(t => t.FromStepId == lastCompletedStep.StepId)
            .ToList();

        if (!transitions.Any())
        {
            // No more transitions, workflow is complete
            instance.Status = WorkflowStatus.Completed;
            instance.CompletedAt = DateTime.UtcNow;
            instance.CurrentStepId = null;

            _logger.LogInformation("Workflow instance {InstanceId} completed", instance.Id);
            return;
        }

        // Create next step(s)
        foreach (var transition in transitions)
        {
            if (ShouldTransition(transition, instance))
            {
                CreateNextStepAsync(instance, workflowDefinition, transition.ToStepId);
            }
        }
    }

    private void CreateNextStepAsync(WorkflowInstance instance, WorkflowDefinition workflowDefinition, string stepId)
    {
        var stepDef = workflowDefinition.Steps.FirstOrDefault(s => s.Id == stepId);
        if (stepDef == null)
        {
            _logger.LogError("Step definition not found for step {StepId} in workflow instance {InstanceId}", 
                stepId, instance.Id);
            return;
        }

        var step = new WorkflowStep
        {
            Id = Guid.NewGuid(),
            WorkflowInstanceId = instance.Id,
            StepId = stepDef.Id,
            StepName = stepDef.Name,
            Status = StepStatus.Pending,
            AssignedTo = stepDef.AssignedUser,
            AssignedRole = stepDef.AssignedRole,
            CreatedAt = DateTime.UtcNow
        };

        _context.WorkflowSteps.Add(step);
        instance.CurrentStepId = stepDef.Id;

        _logger.LogInformation("Created next step {StepId} for workflow instance {InstanceId}", 
            stepDef.Id, instance.Id);
    }

    private bool ShouldTransition(WorkflowTransition transition, WorkflowInstance instance)
    {
        // If no condition, always transition
        if (string.IsNullOrEmpty(transition.Condition))
            return true;

        // TODO: Implement condition evaluation logic
        // For now, return true to allow progression
        return true;
    }
}
