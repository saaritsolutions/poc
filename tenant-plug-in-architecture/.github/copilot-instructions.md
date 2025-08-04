# Copilot Instructions for Tenant Plugin Architecture POC

<!-- Use this file to provide workspace-specific custom instructions to Copilot. For more details, visit https://code.visualstudio.com/docs/copilot/copilot-customization#_use-a-githubcopilotinstructionsmd-file -->

## Project Overview

This is a .NET Core multi-tenant application with a plugin architecture POC that demonstrates:

- **Shared Core Engine**: Forms/workflow engine with plugin contracts
- **Plugin Loader**: Dynamic assembly loading by tenant ID
- **Tenant Isolation**: Each tenant can have custom validation logic and business rules
- **Web API**: RESTful endpoints that adapt behavior based on tenant plugins

## Architecture Principles

### Project Structure
- `TenantPluginArchitecture.Core`: Core forms/workflow engine and plugin loader
- `TenantPluginArchitecture.Plugins.Contracts`: Interface definitions for plugin contracts
- `TenantPluginArchitecture.Api`: Web API with tenant-aware endpoints
- `TenantPluginArchitecture.Plugins.TenantA`: Sample tenant-specific plugin implementation
- `TenantPluginArchitecture.Demo`: Console application for testing plugin system

### Key Design Patterns
- **Plugin Pattern**: Tenant-specific functionality through DLL loading
- **Factory Pattern**: Creating tenant-specific validators and workflow handlers
- **Dependency Injection**: Registering tenant plugins at runtime
- **Repository Pattern**: Abstracting data access for forms and workflows

### Coding Guidelines

1. **Plugin Contracts**: All plugin interfaces should be defined in the Contracts project
2. **Tenant Isolation**: Use tenant ID to load appropriate plugins and maintain data separation
3. **Error Handling**: Implement comprehensive error handling for plugin loading failures
4. **Logging**: Add structured logging for plugin discovery, loading, and execution
5. **Security**: Validate plugin assemblies and implement sandboxing where appropriate
6. **Performance**: Cache loaded plugins and use lazy loading where possible

### Common Patterns in This Codebase

```csharp
// Plugin contract definition
public interface ICustomValidator
{
    Task<ValidationResult> ValidateAsync(object data, ValidationContext context);
    string TenantId { get; }
}

// Plugin loader usage
var plugin = await pluginLoader.LoadPluginAsync<ICustomValidator>(tenantId);
var result = await plugin.ValidateAsync(formData, context);

// Tenant-aware service registration
services.AddSingleton<IPluginLoader, PluginLoader>();
services.AddScoped<ITenantResolver, TenantResolver>();
```

### Testing Approach
- Unit tests for core functionality and plugin contracts
- Integration tests for plugin loading and tenant isolation
- End-to-end tests for API endpoints with different tenant plugins

When working on this project, prioritize:
1. Type safety and clear interface definitions
2. Proper async/await patterns for plugin operations
3. Comprehensive error handling and logging
4. Clean separation between core functionality and tenant-specific logic
5. Performance considerations for plugin loading and caching
