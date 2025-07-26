# Workflow Engine for Core Banking

A comprehensive workflow engine designed for core banking solutions, built with .NET 8 backend, Angular 20 frontend, and PostgreSQL database.

## Features

- **Dynamic Workflow Definition**: Create and manage complex multi-step workflows
- **Approval Chains**: Support for sequential and parallel approval processes
- **Role-based Task Assignment**: Assign tasks to specific users or roles
- **Audit Trail**: Complete history of all workflow actions and state changes
- **Real-time Notifications**: Stay informed about workflow progress
- **Banking-specific Templates**: Pre-built workflows for common banking processes

## Architecture

### Backend (.NET 8)
- **WorkflowEngine.API**: RESTful API with Swagger documentation
- **WorkflowEngine.Core**: Business logic and domain models
- **WorkflowEngine.Data**: Entity Framework Core with PostgreSQL

### Frontend (Angular 20)
- Modern Angular application with Material Design
- Zoneless change detection for better performance
- Reactive forms and observables
- Component-based architecture

### Database (PostgreSQL)
- Workflows and workflow definitions
- Workflow instances and steps
- Audit logs and user assignments
- Optimized indexes for performance

## Quick Start

### Prerequisites
- .NET 8 SDK
- Node.js 20+ (LTS recommended)
- PostgreSQL 15+
- Docker (optional)

### Using Docker Compose (Recommended)

1. Clone the repository:
```bash
git clone <repository-url>
cd workflow-engine-poc
```

2. Start all services:
```bash
docker-compose up -d
```

3. Access the application:
- Frontend: http://localhost:4200
- Backend API: http://localhost:5000
- API Documentation: http://localhost:5000/swagger

### Manual Setup

#### Backend Setup

1. Navigate to the backend directory:
```bash
cd backend
```

2. Restore dependencies:
```bash
dotnet restore
```

3. Update the connection string in `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=workflow_engine;Username=postgres;Password=postgres"
  }
}
```

4. Run database migrations:
```bash
dotnet ef database update --project WorkflowEngine.Data --startup-project WorkflowEngine.API
```

5. Start the API:
```bash
cd WorkflowEngine.API
dotnet run
```

#### Frontend Setup

1. Navigate to the frontend directory:
```bash
cd frontend
```

2. Install dependencies:
```bash
npm install
```

3. Start the development server:
```bash
npm start
```

## Core Banking Workflows

The system supports various banking workflows:

### Loan Approval Workflow
- Application submission
- Credit check
- Risk assessment
- Manager approval
- Documentation
- Disbursement

### Account Opening Workflow
- Customer registration
- KYC verification
- Document validation
- Compliance check
- Account creation
- Welcome package

### Transaction Processing
- Transaction validation
- Fraud detection
- Approval routing
- Settlement
- Reconciliation

## API Endpoints

### Workflows
- `GET /api/workflows` - Get all active workflows
- `POST /api/workflows` - Create new workflow
- `GET /api/workflows/{id}` - Get workflow by ID
- `PUT /api/workflows/{id}` - Update workflow
- `POST /api/workflows/{id}/activate` - Activate workflow
- `POST /api/workflows/{id}/deactivate` - Deactivate workflow

### Workflow Instances
- `GET /api/workflowinstances` - Get workflow instances
- `POST /api/workflowinstances` - Start new workflow instance
- `GET /api/workflowinstances/{id}` - Get instance details
- `POST /api/workflowinstances/{id}/steps/{stepId}/complete` - Complete step
- `POST /api/workflowinstances/{id}/cancel` - Cancel workflow

## Workflow Definition Format

```json
{
  "name": "Loan Approval",
  "description": "Standard loan approval process",
  "startStepId": "application",
  "steps": [
    {
      "id": "application",
      "name": "Application Review",
      "type": "manual",
      "assignedRole": "LoanOfficer",
      "isRequired": true
    },
    {
      "id": "approval",
      "name": "Manager Approval",
      "type": "approval",
      "assignedRole": "LoanManager",
      "isRequired": true
    }
  ],
  "transitions": [
    {
      "id": "app-to-approval",
      "fromStepId": "application",
      "toStepId": "approval",
      "action": "approve"
    }
  ]
}
```

## Development

### Project Structure

```
workflow-engine-poc/
├── backend/
│   ├── WorkflowEngine.API/          # Web API controllers
│   ├── WorkflowEngine.Core/         # Business logic
│   └── WorkflowEngine.Data/         # Data access layer
├── frontend/
│   ├── src/app/
│   │   ├── services/               # Angular services
│   │   ├── workflows/              # Workflow management
│   │   ├── instances/              # Instance management
│   │   └── tasks/                  # Task management
├── docker-compose.yml              # Docker configuration
└── README.md
```

### Adding New Features

1. **Backend**: Add new controllers in `WorkflowEngine.API`, business logic in `WorkflowEngine.Core`, and data access in `WorkflowEngine.Data`
2. **Frontend**: Create new components and services in the respective directories
3. **Database**: Add migrations using Entity Framework Core

### Testing

```bash
# Backend tests
cd backend
dotnet test

# Frontend tests
cd frontend
npm test
```

## Configuration

### Environment Variables

#### Backend
- `ConnectionStrings__DefaultConnection`: PostgreSQL connection string
- `ASPNETCORE_ENVIRONMENT`: Environment (Development/Production)

#### Frontend
- `API_URL`: Backend API URL (default: http://localhost:5000/api)

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests
5. Submit a pull request

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Support

For support and questions, please create an issue in the repository or contact the development team.
