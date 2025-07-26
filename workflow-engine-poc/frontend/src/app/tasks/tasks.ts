import { Component, OnInit, Inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTableModule } from '@angular/material/table';
import { MatChipsModule } from '@angular/material/chips';
import { MatDialogModule, MatDialog, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { WorkflowService, WorkflowInstanceDto, WorkflowStepDto, CompleteStepRequest } from '../services/workflow.service';

@Component({
  selector: 'app-task-approval-dialog',
  template: `
    <h2 mat-dialog-title>{{ data.action }} Task</h2>
    <mat-dialog-content>
      <form [formGroup]="approvalForm">
        <div class="task-details">
          <h3>Task Information</h3>
          <p><strong>Workflow:</strong> {{ data.instance.workflowName }}</p>
          <p><strong>Step:</strong> {{ data.step.stepName }}</p>
          <p><strong>Started:</strong> {{ data.step.createdAt | date:'short' }}</p>
        </div>

        <mat-form-field appearance="outline" style="width: 100%; margin-top: 16px;">
          <mat-label>Comments</mat-label>
          <textarea matInput formControlName="comments" rows="4" 
                   placeholder="Add your comments about this {{ data.action.toLowerCase() }}..."></textarea>
        </mat-form-field>

        <mat-form-field appearance="outline" style="width: 100%;" *ngIf="data.action === 'Approve'">
          <mat-label>Additional Data (JSON)</mat-label>
          <textarea matInput formControlName="stepData" rows="3" 
                   placeholder='{"key": "value"}'></textarea>
        </mat-form-field>
      </form>
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-button mat-dialog-close>Cancel</button>
      <button mat-raised-button 
              [color]="data.action === 'Approve' ? 'primary' : 'warn'"
              (click)="submit()"
              [disabled]="!approvalForm.valid">
        {{ data.action }}
      </button>
    </mat-dialog-actions>
  `,
  imports: [
    CommonModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    ReactiveFormsModule
  ]
})
export class TaskApprovalDialogComponent {
  approvalForm: FormGroup;

  constructor(
    private fb: FormBuilder,
    public dialogRef: MatDialogRef<TaskApprovalDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: any
  ) {
    this.approvalForm = this.fb.group({
      comments: ['', Validators.required],
      stepData: ['']
    });
  }

  submit() {
    if (this.approvalForm.valid) {
      const result: CompleteStepRequest = {
        comments: this.approvalForm.value.comments,
        stepData: this.approvalForm.value.stepData || undefined,
        isApproved: this.data.action === 'Approve'
      };
      this.dialogRef.close(result);
    }
  }
}

@Component({
  selector: 'app-tasks',
  imports: [
    CommonModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatTableModule,
    MatChipsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatSelectModule,
    FormsModule
  ],
  templateUrl: './tasks.html',
  styleUrl: './tasks.scss'
})
export class TasksComponent implements OnInit {
  userTasks: WorkflowInstanceDto[] = [];
  displayedColumns: string[] = ['workflowName', 'stepName', 'priority', 'startedAt', 'assignedRole', 'actions'];
  loading = true;
  selectedPriority = '';

  priorityOptions = [
    { value: '', label: 'All Priorities' },
    { value: 'high', label: 'High Priority' },
    { value: 'medium', label: 'Medium Priority' },
    { value: 'low', label: 'Low Priority' }
  ];

  constructor(
    private workflowService: WorkflowService,
    private dialog: MatDialog
  ) {}

  ngOnInit() {
    this.loadUserTasks();
  }

  loadUserTasks() {
    this.loading = true;
    this.workflowService.getUserTasks().subscribe({
      next: (tasks) => {
        this.userTasks = tasks;
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading user tasks:', error);
        this.loading = false;
      }
    });
  }

  getCurrentStep(instance: WorkflowInstanceDto): WorkflowStepDto | null {
    return instance.steps.find(step => 
      step.status === 'Pending' || step.status === 'InProgress'
    ) || null;
  }

  getPriorityLevel(instance: WorkflowInstanceDto): string {
    // Simple priority logic based on how long the task has been pending
    const currentStep = this.getCurrentStep(instance);
    if (!currentStep) return 'low';

    const hoursSinceCreated = (new Date().getTime() - new Date(currentStep.createdAt).getTime()) / (1000 * 60 * 60);
    
    if (hoursSinceCreated > 24) return 'high';
    if (hoursSinceCreated > 8) return 'medium';
    return 'low';
  }

  getPriorityColor(priority: string): string {
    switch (priority) {
      case 'high': return 'warn';
      case 'medium': return 'accent';
      case 'low': return 'primary';
      default: return '';
    }
  }

  getHighPriorityCount(): number {
    return this.userTasks.filter(t => this.getPriorityLevel(t) === 'high').length;
  }

  getMediumPriorityCount(): number {
    return this.userTasks.filter(t => this.getPriorityLevel(t) === 'medium').length;
  }

  getLowPriorityCount(): number {
    return this.userTasks.filter(t => this.getPriorityLevel(t) === 'low').length;
  }

  approveTask(instance: WorkflowInstanceDto) {
    const currentStep = this.getCurrentStep(instance);
    if (!currentStep) return;

    const dialogRef = this.dialog.open(TaskApprovalDialogComponent, {
      width: '500px',
      data: {
        action: 'Approve',
        instance: instance,
        step: currentStep
      }
    });

    dialogRef.afterClosed().subscribe((result: CompleteStepRequest) => {
      if (result) {
        this.completeStep(instance.id, currentStep.stepId, result);
      }
    });
  }

  rejectTask(instance: WorkflowInstanceDto) {
    const currentStep = this.getCurrentStep(instance);
    if (!currentStep) return;

    const dialogRef = this.dialog.open(TaskApprovalDialogComponent, {
      width: '500px',
      data: {
        action: 'Reject',
        instance: instance,
        step: currentStep
      }
    });

    dialogRef.afterClosed().subscribe((result: CompleteStepRequest) => {
      if (result) {
        this.completeStep(instance.id, currentStep.stepId, result);
      }
    });
  }

  private completeStep(instanceId: string, stepId: string, request: CompleteStepRequest) {
    this.workflowService.completeStep(instanceId, stepId, request).subscribe({
      next: () => {
        this.loadUserTasks();
      },
      error: (error) => {
        console.error('Error completing step:', error);
      }
    });
  }

  viewInstanceDetails(instance: WorkflowInstanceDto) {
    // TODO: Navigate to instance details or open dialog
    console.log('View instance details:', instance.id);
  }

  getFilteredTasks() {
    if (!this.selectedPriority) return this.userTasks;
    
    return this.userTasks.filter(task => 
      this.getPriorityLevel(task) === this.selectedPriority
    );
  }
}
