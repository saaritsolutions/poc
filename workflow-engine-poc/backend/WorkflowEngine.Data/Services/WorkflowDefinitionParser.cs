using System.Text.Json;
using Microsoft.Extensions.Logging;
using WorkflowEngine.Core.Interfaces;

namespace WorkflowEngine.Data.Services;

public class WorkflowDefinitionParser : IWorkflowDefinitionParser
{
    private readonly ILogger<WorkflowDefinitionParser> _logger;

    public WorkflowDefinitionParser(ILogger<WorkflowDefinitionParser> logger)
    {
        _logger = logger;
    }

    public WorkflowDefinition ParseDefinition(string workflowJson)
    {
        try
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var definition = JsonSerializer.Deserialize<WorkflowDefinition>(workflowJson, options);
            return definition ?? throw new ArgumentException("Failed to deserialize workflow definition");
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse workflow definition JSON");
            throw new ArgumentException("Invalid workflow definition JSON", ex);
        }
    }

    public bool ValidateDefinition(string workflowJson)
    {
        try
        {
            var definition = ParseDefinition(workflowJson);
            
            // Basic validation rules
            if (string.IsNullOrEmpty(definition.Name))
                return false;

            if (!definition.Steps.Any())
                return false;

            if (string.IsNullOrEmpty(definition.StartStepId))
                return false;

            // Check if start step exists
            if (!definition.Steps.Any(s => s.Id == definition.StartStepId))
                return false;

            // Check if all steps have unique IDs
            if (definition.Steps.GroupBy(s => s.Id).Any(g => g.Count() > 1))
                return false;

            // Check if all transitions reference valid steps
            foreach (var transition in definition.Transitions)
            {
                if (!definition.Steps.Any(s => s.Id == transition.FromStepId) ||
                    !definition.Steps.Any(s => s.Id == transition.ToStepId))
                    return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Workflow definition validation failed");
            return false;
        }
    }

    public string SerializeDefinition(WorkflowDefinition definition)
    {
        try
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            return JsonSerializer.Serialize(definition, options);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to serialize workflow definition");
            throw new ArgumentException("Failed to serialize workflow definition", ex);
        }
    }
}
