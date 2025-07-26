import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface WorkflowDto {
  id: string;
  name: string;
  description: string;
  workflowDefinition: string;
  version: number;
  isActive: boolean;
  createdAt: string;
  updatedAt?: string;
  createdBy: string;
}

export interface CreateWorkflowRequest {
  name: string;
  description: string;
  workflowDefinition: string;
}

export interface WorkflowInstanceDto {
  id: string;
  workflowId: string;
  workflowName: string;
  instanceData: string;
  status: string;
  currentStepId?: string;
  currentStepName?: string;
  startedAt: string;
  completedAt?: string;
  initiatedBy: string;
  steps: WorkflowStepDto[];
}

export interface CreateWorkflowInstanceRequest {
  workflowId: string;
  instanceData: string;
  initiatedBy: string;
}

export type StartWorkflowRequest = CreateWorkflowInstanceRequest;

export interface WorkflowStepDto {
  id: string;
  stepId: string;
  stepName: string;
  status: string;
  assignedTo?: string;
  assignedRole?: string;
  createdAt: string;
  startedAt?: string;
  completedAt?: string;
  comments?: string;
  stepData?: string;
}

export interface CompleteStepRequest {
  comments?: string;
  stepData?: string;
  isApproved: boolean;
}

export interface WorkflowAuditLogDto {
  id: string;
  action: string;
  stepId?: string;
  previousState?: string;
  newState?: string;
  performedBy: string;
  timestamp: string;
  comments?: string;
  additionalData?: string;
}

@Injectable({
  providedIn: 'root'
})
export class WorkflowService {
  private readonly apiUrl = 'http://localhost:5000/api';

  constructor(private http: HttpClient) {}

  // Workflow Management
  getWorkflows(): Observable<WorkflowDto[]> {
    return this.http.get<WorkflowDto[]>(`${this.apiUrl}/workflows`);
  }

  getActiveWorkflows(): Observable<WorkflowDto[]> {
    return this.http.get<WorkflowDto[]>(`${this.apiUrl}/workflows`);
  }

  getWorkflow(id: string): Observable<WorkflowDto> {
    return this.http.get<WorkflowDto>(`${this.apiUrl}/workflows/${id}`);
  }

  createWorkflow(request: CreateWorkflowRequest): Observable<WorkflowDto> {
    return this.http.post<WorkflowDto>(`${this.apiUrl}/workflows`, request);
  }

  updateWorkflow(id: string, request: CreateWorkflowRequest): Observable<WorkflowDto> {
    return this.http.put<WorkflowDto>(`${this.apiUrl}/workflows/${id}`, request);
  }

  activateWorkflow(id: string): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/workflows/${id}/activate`, {});
  }

  deactivateWorkflow(id: string): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/workflows/${id}/deactivate`, {});
  }

  deleteWorkflow(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/workflows/${id}`);
  }

  // Workflow Instance Management
  getWorkflowInstances(workflowId?: string, status?: string): Observable<WorkflowInstanceDto[]> {
    let params = new HttpParams();
    if (workflowId) params = params.set('workflowId', workflowId);
    if (status) params = params.set('status', status);
    
    return this.http.get<WorkflowInstanceDto[]>(`${this.apiUrl}/workflowinstances`, { params });
  }

  getWorkflowInstance(id: string): Observable<WorkflowInstanceDto> {
    return this.http.get<WorkflowInstanceDto>(`${this.apiUrl}/workflowinstances/${id}`);
  }

  startWorkflow(request: CreateWorkflowInstanceRequest): Observable<WorkflowInstanceDto> {
    return this.http.post<WorkflowInstanceDto>(`${this.apiUrl}/workflowinstances`, request);
  }

  getUserTasks(): Observable<WorkflowInstanceDto[]> {
    return this.http.get<WorkflowInstanceDto[]>(`${this.apiUrl}/workflowinstances/tasks`);
  }

  completeStep(instanceId: string, stepId: string, request: CompleteStepRequest): Observable<WorkflowInstanceDto> {
    return this.http.post<WorkflowInstanceDto>(`${this.apiUrl}/workflowinstances/${instanceId}/steps/${stepId}/complete`, request);
  }

  cancelWorkflow(instanceId: string, reason?: string): Observable<WorkflowInstanceDto> {
    const request = { reason };
    return this.http.post<WorkflowInstanceDto>(`${this.apiUrl}/workflowinstances/${instanceId}/cancel`, request);
  }

  getAuditLog(instanceId: string): Observable<WorkflowAuditLogDto[]> {
    return this.http.get<WorkflowAuditLogDto[]>(`${this.apiUrl}/workflowinstances/${instanceId}/audit`);
  }
}
