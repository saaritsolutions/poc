import { Component, inject, signal } from '@angular/core';
import { RouterLink, Router } from '@angular/router';

import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { MatBadgeModule } from '@angular/material/badge';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';

import { WorkflowService } from '../services/workflow.service';
import { StartWorkflowDialogComponent } from '../start-workflow-dialog/start-workflow-dialog-simple';
import { WorkflowCreationDialogComponent } from '../workflow-creation-dialog/workflow-creation-dialog';

@Component({
  selector: 'app-workflows',
  standalone: true,
  imports: [
    RouterLink,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatChipsModule,
    MatBadgeModule
  ],
  template: `
    <div class="workflows-container">
      <div class="header">
        <h2>
          <mat-icon>schema</mat-icon>
          Workflow Definitions
        </h2>
        <div class="header-buttons">
          <button mat-button routerLink="/workflow-canvas">
            <mat-icon>edit</mat-icon>
            Canvas Designer
          </button>
          <button mat-raised-button color="primary" (click)="createWorkflow()">
            <mat-icon>add</mat-icon>
            Create Workflow
          </button>
        </div>
      </div>

      <div class="workflows-grid">
        @for (workflow of workflows(); track workflow.id) {
          <mat-card class="workflow-card">
            <mat-card-header>
              <mat-card-title>{{ workflow.name }}</mat-card-title>
              <mat-card-subtitle>{{ workflow.description }}</mat-card-subtitle>
            </mat-card-header>
            
            <mat-card-content>
              <div class="workflow-stats">
                <mat-chip-set>
                  <mat-chip>{{ workflow.version }}</mat-chip>
                  <mat-chip [class]="workflow.isActive ? 'active' : 'inactive'">
                    {{ workflow.isActive ? 'Active' : 'Inactive' }}
                  </mat-chip>
                </mat-chip-set>
              </div>
              
              <div class="workflow-metrics">
                <div class="metric">
                  <mat-icon matBadge="{{workflow.totalInstances}}" matBadgeColor="accent">
                    play_circle
                  </mat-icon>
                  <span>Instances</span>
                </div>
                <div class="metric">
                  <mat-icon>schedule</mat-icon>
                  <span>{{ workflow.averageCompletionTime }}</span>
                </div>
              </div>
            </mat-card-content>
            
            <mat-card-actions>
              <button mat-button (click)="startWorkflow(workflow.id)">
                <mat-icon>play_arrow</mat-icon>
                Start
              </button>
              <button mat-button (click)="editWorkflow(workflow.id)">
                <mat-icon>edit</mat-icon>
                Edit
              </button>
              <button mat-button (click)="viewInstances(workflow.id)">
                <mat-icon>list</mat-icon>
                Instances
              </button>
            </mat-card-actions>
          </mat-card>
        }
      </div>
    </div>
  `,
  styles: [`
    .workflows-container {
      padding: 20px;
    }

    .header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 30px;
      
      h2 {
        display: flex;
        align-items: center;
        gap: 10px;
        margin: 0;
      }
      
      .header-buttons {
        display: flex;
        gap: 10px;
      }
    }

    .workflows-grid {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(350px, 1fr));
      gap: 20px;
    }

    .workflow-card {
      transition: transform 0.2s, box-shadow 0.2s;
      
      &:hover {
        transform: translateY(-2px);
        box-shadow: 0 4px 12px rgba(0,0,0,0.15);
      }
    }

    .workflow-stats {
      margin: 15px 0;
    }

    .workflow-metrics {
      display: flex;
      gap: 20px;
      margin: 15px 0;
      
      .metric {
        display: flex;
        align-items: center;
        gap: 5px;
        font-size: 14px;
        color: #666;
      }
    }

    .active {
      background-color: #4caf50 !important;
      color: white !important;
    }

    .inactive {
      background-color: #9e9e9e !important;
      color: white !important;
    }
  `]
})
export class SimpleWorkflowsComponent {
  private workflowService = inject(WorkflowService);
  private dialog = inject(MatDialog);
  private snackBar = inject(MatSnackBar);
  private router = inject(Router);
  
  workflows = signal([
    {
      id: '1',
      name: 'Loan Application',
      description: 'Complete loan application workflow with credit check and approval',
      version: 'v1.2',
      isActive: true,
      totalInstances: 25,
      averageCompletionTime: '2.5 days',
      type: 'loan'
    },
    {
      id: '2',
      name: 'Account Opening',
      description: 'New customer account opening with KYC verification',
      version: 'v1.0',
      isActive: true,
      totalInstances: 18,
      averageCompletionTime: '1.2 days',
      type: 'account'
    },
    {
      id: '3',
      name: 'Credit Card Application',
      description: 'Credit card application with credit score evaluation',
      version: 'v2.1',
      isActive: false,
      totalInstances: 8,
      averageCompletionTime: '3.1 days',
      type: 'credit'
    }
  ]);

  createWorkflow() {
    this.dialog.open(WorkflowCreationDialogComponent, {
      width: '700px',
      disableClose: false
    });
  }

  startWorkflow(workflowId: string) {
    const workflow = this.workflows().find(w => w.id === workflowId);
    if (!workflow) return;

    const dialogRef = this.dialog.open(StartWorkflowDialogComponent, {
      width: '600px',
      data: {
        workflowId: workflow.id,
        workflowName: workflow.name,
        description: workflow.description,
        workflowType: workflow.type,
        stepCount: 5,
        estimatedTime: workflow.averageCompletionTime
      }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        // Simulate starting the workflow
        this.snackBar.open(
          `Workflow "${workflow.name}" started successfully for ${result.customerName}`,
          'Close',
          { duration: 3000 }
        );
        
        // Update instance count
        const currentWorkflows = this.workflows();
        const updatedWorkflows = currentWorkflows.map(w => 
          w.id === workflowId ? { ...w, totalInstances: w.totalInstances + 1 } : w
        );
        this.workflows.set(updatedWorkflows);
        
        console.log('Workflow instance created:', result);
      }
    });
  }

  editWorkflow(workflowId: string) {
    console.log('Edit workflow:', workflowId);
  }

  viewInstances(workflowId: string) {
    console.log('View instances for workflow:', workflowId);
  }
}
