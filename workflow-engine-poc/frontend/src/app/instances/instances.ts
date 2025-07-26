import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTableModule } from '@angular/material/table';
import { MatChipsModule } from '@angular/material/chips';
import { MatSelectModule } from '@angular/material/select';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatDialogModule, MatDialog } from '@angular/material/dialog';
import { FormsModule } from '@angular/forms';
import { WorkflowService, WorkflowInstanceDto, WorkflowDto } from '../services/workflow.service';
import { StartWorkflowDialogComponent } from '../start-workflow-dialog/start-workflow-dialog';

@Component({
  selector: 'app-instances',
  imports: [
    CommonModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatTableModule,
    MatChipsModule,
    MatSelectModule,
    MatFormFieldModule,
    MatProgressBarModule,
    MatDialogModule,
    FormsModule
  ],
  templateUrl: './instances.html',
  styleUrl: './instances.scss'
})
export class InstancesComponent implements OnInit {
  instances: WorkflowInstanceDto[] = [];
  workflows: WorkflowDto[] = [];
  displayedColumns: string[] = ['workflowName', 'status', 'currentStep', 'startedAt', 'initiatedBy', 'progress', 'actions'];
  loading = true;
  selectedWorkflowId = '';
  selectedStatus = '';

  statusOptions = [
    { value: '', label: 'All Statuses' },
    { value: 'Running', label: 'Running' },
    { value: 'Completed', label: 'Completed' },
    { value: 'Cancelled', label: 'Cancelled' },
    { value: 'Failed', label: 'Failed' }
  ];

  constructor(
    private workflowService: WorkflowService,
    private dialog: MatDialog
  ) {}

  ngOnInit() {
    this.loadWorkflows();
    this.loadInstances();
  }

  loadWorkflows() {
    this.workflowService.getActiveWorkflows().subscribe({
      next: (workflows) => {
        this.workflows = workflows;
      },
      error: (error) => {
        console.error('Error loading workflows:', error);
      }
    });
  }

  loadInstances() {
    this.loading = true;
    const workflowId = this.selectedWorkflowId || undefined;
    const status = this.selectedStatus || undefined;
    
    this.workflowService.getWorkflowInstances(workflowId, status).subscribe({
      next: (instances) => {
        this.instances = instances;
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading instances:', error);
        this.loading = false;
      }
    });
  }

  onFilterChange() {
    this.loadInstances();
  }

  getStatusColor(status: string): string {
    switch (status.toLowerCase()) {
      case 'running': return 'primary';
      case 'completed': return 'accent';
      case 'cancelled': return 'warn';
      case 'failed': return 'warn';
      default: return '';
    }
  }

  getProgressPercentage(instance: WorkflowInstanceDto): number {
    if (instance.status === 'Completed') return 100;
    if (instance.status === 'Cancelled' || instance.status === 'Failed') return 0;
    
    const totalSteps = instance.steps.length;
    const completedSteps = instance.steps.filter(s => s.status === 'Completed').length;
    
    return totalSteps > 0 ? Math.round((completedSteps / totalSteps) * 100) : 0;
  }

  viewInstanceDetails(instance: WorkflowInstanceDto) {
    // TODO: Open instance details dialog
    console.log('View details for instance:', instance.id);
  }

  cancelInstance(instance: WorkflowInstanceDto) {
    if (confirm(`Are you sure you want to cancel the workflow instance "${instance.workflowName}"?`)) {
      this.workflowService.cancelWorkflow(instance.id, 'Cancelled by user').subscribe({
        next: () => {
          this.loadInstances();
        },
        error: (error) => {
          console.error('Error cancelling instance:', error);
        }
      });
    }
  }

  startNewWorkflow() {
    const dialogRef = this.dialog.open(StartWorkflowDialogComponent, {
      width: '600px',
      data: {}
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        console.log('Workflow started:', result);
        this.loadInstances(); // Refresh the instances list
      }
    });
  }
}
