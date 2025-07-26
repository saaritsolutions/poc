import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, FormArray, Validators, FormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatStepperModule } from '@angular/material/stepper';
import { MatChipsModule } from '@angular/material/chips';
import { MatTabsModule } from '@angular/material/tabs';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { Router } from '@angular/router';
import { WorkflowService, CreateWorkflowRequest } from '../services/workflow.service';

interface WorkflowStep {
  id: string;
  name: string;
  type: string;
  assignedRole?: string;
  assignedUser?: string;
  isRequired: boolean;
}

interface WorkflowTransition {
  id: string;
  fromStepId: string;
  toStepId: string;
  condition?: string;
  action?: string;
}

@Component({
  selector: 'app-workflow-creator',
  imports: [
    CommonModule,
    ReactiveFormsModule,
    FormsModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatStepperModule,
    MatChipsModule,
    MatTabsModule,
    MatCheckboxModule
  ],
  templateUrl: './workflow-creator.html',
  styleUrl: './workflow-creator.scss'
})
export class WorkflowCreatorComponent implements OnInit {
  workflowForm: FormGroup;
  steps: WorkflowStep[] = [];
  transitions: WorkflowTransition[] = [];
  
  stepTypes = [
    { value: 'manual', label: 'Manual Task' },
    { value: 'approval', label: 'Approval Task' },
    { value: 'automatic', label: 'Automatic Task' },
    { value: 'notification', label: 'Notification' }
  ];

  bankingRoles = [
    'LoanOfficer',
    'LoanManager', 
    'RiskAnalyst',
    'ComplianceOfficer',
    'BranchManager',
    'SeniorManager',
    'AuditManager'
  ];

  bankingTemplates = [
    {
      name: 'Loan Approval Process',
      description: 'Standard loan approval workflow with credit check and risk assessment',
      steps: [
        { id: 'application', name: 'Application Review', type: 'manual', assignedRole: 'LoanOfficer', isRequired: true },
        { id: 'credit-check', name: 'Credit Check', type: 'automatic', isRequired: true },
        { id: 'risk-assessment', name: 'Risk Assessment', type: 'approval', assignedRole: 'RiskAnalyst', isRequired: true },
        { id: 'manager-approval', name: 'Manager Approval', type: 'approval', assignedRole: 'LoanManager', isRequired: true },
        { id: 'documentation', name: 'Documentation', type: 'manual', assignedRole: 'LoanOfficer', isRequired: true }
      ],
      transitions: [
        { id: 't1', fromStepId: 'application', toStepId: 'credit-check', action: 'approve' },
        { id: 't2', fromStepId: 'credit-check', toStepId: 'risk-assessment', action: 'approve' },
        { id: 't3', fromStepId: 'risk-assessment', toStepId: 'manager-approval', action: 'approve' },
        { id: 't4', fromStepId: 'manager-approval', toStepId: 'documentation', action: 'approve' }
      ]
    },
    {
      name: 'Account Opening Workflow',
      description: 'Customer account opening with KYC verification',
      steps: [
        { id: 'customer-info', name: 'Customer Information', type: 'manual', assignedRole: 'LoanOfficer', isRequired: true },
        { id: 'kyc-verification', name: 'KYC Verification', type: 'approval', assignedRole: 'ComplianceOfficer', isRequired: true },
        { id: 'document-validation', name: 'Document Validation', type: 'manual', assignedRole: 'LoanOfficer', isRequired: true },
        { id: 'manager-approval', name: 'Manager Approval', type: 'approval', assignedRole: 'BranchManager', isRequired: true },
        { id: 'account-creation', name: 'Account Creation', type: 'automatic', isRequired: true }
      ],
      transitions: [
        { id: 't1', fromStepId: 'customer-info', toStepId: 'kyc-verification', action: 'approve' },
        { id: 't2', fromStepId: 'kyc-verification', toStepId: 'document-validation', action: 'approve' },
        { id: 't3', fromStepId: 'document-validation', toStepId: 'manager-approval', action: 'approve' },
        { id: 't4', fromStepId: 'manager-approval', toStepId: 'account-creation', action: 'approve' }
      ]
    }
  ];

  constructor(
    private fb: FormBuilder,
    private workflowService: WorkflowService,
    private router: Router
  ) {
    this.workflowForm = this.fb.group({
      name: ['', Validators.required],
      description: ['', Validators.required]
    });
  }

  ngOnInit() {}

  loadTemplate(template: any) {
    this.workflowForm.patchValue({
      name: template.name,
      description: template.description
    });
    this.steps = [...template.steps];
    this.transitions = [...template.transitions];
  }

  addStep() {
    const newStep: WorkflowStep = {
      id: `step-${Date.now()}`,
      name: '',
      type: 'manual',
      isRequired: true
    };
    this.steps.push(newStep);
  }

  removeStep(index: number) {
    const stepId = this.steps[index].id;
    this.steps.splice(index, 1);
    
    // Remove related transitions
    this.transitions = this.transitions.filter(t => 
      t.fromStepId !== stepId && t.toStepId !== stepId
    );
  }

  addTransition() {
    if (this.steps.length < 2) return;
    
    const newTransition: WorkflowTransition = {
      id: `transition-${Date.now()}`,
      fromStepId: this.steps[0].id,
      toStepId: this.steps[1].id,
      action: 'approve'
    };
    this.transitions.push(newTransition);
  }

  removeTransition(index: number) {
    this.transitions.splice(index, 1);
  }

  generateWorkflowDefinition(): string {
    const definition = {
      name: this.workflowForm.value.name,
      description: this.workflowForm.value.description,
      startStepId: this.steps.length > 0 ? this.steps[0].id : '',
      steps: this.steps.map(step => ({
        id: step.id,
        name: step.name,
        type: step.type,
        assignedRole: step.assignedRole,
        assignedUser: step.assignedUser,
        isRequired: step.isRequired,
        properties: {}
      })),
      transitions: this.transitions
    };

    return JSON.stringify(definition, null, 2);
  }

  saveWorkflow() {
    if (this.workflowForm.valid && this.steps.length > 0) {
      const request: CreateWorkflowRequest = {
        name: this.workflowForm.value.name,
        description: this.workflowForm.value.description,
        workflowDefinition: this.generateWorkflowDefinition()
      };

      this.workflowService.createWorkflow(request).subscribe({
        next: (workflow) => {
          console.log('Workflow created successfully:', workflow);
          this.router.navigate(['/workflows']);
        },
        error: (error) => {
          console.error('Error creating workflow:', error);
        }
      });
    }
  }

  previewDefinition() {
    console.log('Workflow Definition:', this.generateWorkflowDefinition());
  }

  getUniqueRoles(): string[] {
    const roles = this.steps
      .filter(step => step.assignedRole)
      .map(step => step.assignedRole!);
    return [...new Set(roles)];
  }

  getApprovalStepsCount(): number {
    return this.steps.filter(s => s.type === 'approval').length;
  }

  trackByStepId(index: number, item: WorkflowStep): string {
    return item.id;
  }

  trackByTransitionId(index: number, item: WorkflowTransition): string {
    return item.id;
  }
}
