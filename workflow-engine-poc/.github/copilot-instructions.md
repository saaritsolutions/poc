# Copilot Instructions

<!-- Use this file to provide workspace-specific custom instructions to Copilot. For more details, visit https://code.visualstudio.com/docs/copilot/copilot-customization#_use-a-githubcopilotinstructionsmd-file -->

## Project Overview
This is a Workflow Engine for Core Banking Solutions built with:
- **Backend**: .NET 8 Web API with Entity Framework Core and PostgreSQL
- **Frontend**: Angular 18+ with modern TypeScript
- **Database**: PostgreSQL for data persistence

## Architecture Guidelines
- Follow Clean Architecture principles with separation of concerns
- Use dependency injection and SOLID principles
- Implement proper error handling and logging
- Follow RESTful API conventions
- Use async/await patterns for database operations

## Core Banking Domain
When generating code, consider core banking concepts:
- Account management and transactions
- Loan processing and approvals
- Customer onboarding workflows
- Compliance and audit requirements
- Multi-step approval processes
- Risk assessment workflows

## Workflow Engine Features
The workflow engine should support:
- Dynamic workflow definition and execution
- Step-by-step approval chains
- Conditional branching and parallel processing
- Audit trail and history tracking
- Role-based task assignments
- Notification and alerting
- Workflow templates for common banking processes

## Code Style Preferences
- Use descriptive names for variables, methods, and classes
- Include XML documentation for public APIs
- Follow .NET naming conventions (PascalCase for public members)
- Use Angular best practices (reactive forms, observables, etc.)
- Implement proper TypeScript typing
- Include unit tests for business logic

## Technology Stack
- .NET 8 with C# 12
- Entity Framework Core 9.x
- PostgreSQL database
- Angular 18+ with TypeScript
- RxJS for reactive programming
- Angular Material for UI components
