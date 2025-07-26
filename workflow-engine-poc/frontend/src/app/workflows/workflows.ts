import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTableModule } from '@angular/material/table';
import { MatChipsModule } from '@angular/material/chips';
import { WorkflowService, WorkflowDto } from '../services/workflow.service';

@Component({
  selector: 'app-workflows',
  imports: [
    CommonModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatTableModule,
    MatChipsModule
  ],
  templateUrl: './workflows.html',
  styleUrl: './workflows.scss'
})
export class WorkflowsComponent implements OnInit {
  workflows: WorkflowDto[] = [];
  displayedColumns: string[] = ['name', 'description', 'version', 'status', 'createdAt', 'actions'];
  loading = true;

  constructor(private workflowService: WorkflowService) {}

  ngOnInit() {
    this.loadWorkflows();
  }

  loadWorkflows() {
    this.loading = true;
    this.workflowService.getActiveWorkflows().subscribe({
      next: (workflows) => {
        this.workflows = workflows;
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading workflows:', error);
        this.loading = false;
      }
    });
  }

  activateWorkflow(id: string) {
    this.workflowService.activateWorkflow(id).subscribe({
      next: () => {
        this.loadWorkflows();
      },
      error: (error) => {
        console.error('Error activating workflow:', error);
      }
    });
  }

  deactivateWorkflow(id: string) {
    this.workflowService.deactivateWorkflow(id).subscribe({
      next: () => {
        this.loadWorkflows();
      },
      error: (error) => {
        console.error('Error deactivating workflow:', error);
      }
    });
  }
}
