# Tenant Plugin Architecture - .NET Core POC

## Overview
A complete proof-of-concept demonstrating a multi-tenant .NET Core application with dynamic plugin loading architecture.

## Features
- **Multi-tenant Plugin System**: Load tenant-specific business logic as separate DLLs
- **Forms Engine**: Process forms with custom validation per tenant
- **Workflow Engine**: Execute tenant-specific workflow steps
- **Web API**: RESTful endpoints for form submission, validation, and workflow execution
- **Plugin Isolation**: Each tenant's plugins are loaded in separate contexts
- **Hot Reload**: Plugins can be updated without restarting the application

## Architecture

### Core Components
1. **TenantPluginArchitecture.Core** - Plugin loader, forms engine, workflow engine
2. **TenantPluginArchitecture.Plugins.Contracts** - Interface definitions for plugins
3. **TenantPluginArchitecture.Api** - Web API with tenant-aware controllers
4. **TenantPluginArchitecture.Demo** - Console application demonstrating the system
5. **TenantPluginArchitecture.Plugins.TenantA** - Sample tenant-specific plugin

### Plugin Interfaces
- `ICustomValidator` - Custom validation logic per tenant
- `ICustomFormProcessor` - Custom form processing logic
- `ICustomWorkflowHandler` - Custom workflow step handling

## Quick Start

### 1. Build the Solution
```bash
dotnet build
```

### 2. Run the Demo Application
```bash
cd TenantPluginArchitecture.Demo
dotnet run
```

### 3. Start the Web API
```bash
cd TenantPluginArchitecture.Api
dotnet run
```

The API will be available at `https://localhost:5001` with Swagger documentation at the root URL.

## API Endpoints

### Forms
- `POST /api/forms/submit` - Submit a form for processing
- `POST /api/forms/validate` - Validate form data without submitting
- `GET /api/forms/configuration/{tenantId}/{formType}` - Get form configuration

### Workflows
- `POST /api/workflows/start` - Start a new workflow
- `POST /api/workflows/{workflowId}/next` - Execute next workflow step
- `GET /api/workflows/{workflowId}` - Get workflow instance details

### Plugins
- `GET /api/plugins/available/{tenantId}` - Check plugin availability
- `POST /api/plugins/reload/{tenantId}` - Reload tenant plugins

## Example Usage

### Submit a Form
```bash
curl -X POST https://localhost:5001/api/forms/submit \
  -H "Content-Type: application/json" \
  -d '{
    "formType": "UserRegistration",
    "tenantId": "TenantA",
    "fields": {
      "email": "user@company.com",
      "age": 25,
      "name": "John Doe"
    }
  }'
```

### Start a Workflow
```bash
curl -X POST https://localhost:5001/api/workflows/start \
  -H "Content-Type: application/json" \
  -d '{
    "tenantId": "TenantA",
    "workflowType": "OrderProcessing",
    "inputData": {
      "customerType": "premium",
      "orderValue": 1500.00
    }
  }'
```

## Tenant A Plugin Example

The included TenantA plugin demonstrates:
- **Email Validation**: Rejects @gmail.com addresses
- **Age Validation**: Requires users to be 18 or older
- **Phone Formatting**: Formats phone numbers
- **Premium Workflow**: Special handling for premium customers

## Extending with New Tenants

1. Create a new project: `TenantPluginArchitecture.Plugins.TenantB`
2. Reference the Contracts project
3. Implement the plugin interfaces
4. Build and copy the DLL to `plugins/TenantB/`
5. The system will automatically discover and load the plugin

## Plugin Directory Structure
```
plugins/
├── TenantA/
│   └── TenantPluginArchitecture.Plugins.TenantA.dll
├── TenantB/
│   └── TenantPluginArchitecture.Plugins.TenantB.dll
└── TenantC/
    └── TenantPluginArchitecture.Plugins.TenantC.dll
```

## Configuration

### appsettings.json
```json
{
  "PluginSettings": {
    "PluginDirectory": "plugins"
  }
}
```

## Technologies Used
- .NET 9.0
- ASP.NET Core Web API
- Microsoft.Extensions.Logging
- Swashbuckle (Swagger/OpenAPI)
- AssemblyLoadContext for plugin isolation
- Dependency Injection

## Demo Output

The demo application will show:
- Plugin availability for different tenants
- Form validation with tenant-specific rules
- Workflow execution with custom handlers
- Comparison between tenants with and without plugins

This demonstrates a real-world multi-tenant architecture where business logic can be customized per tenant without modifying the core application.
