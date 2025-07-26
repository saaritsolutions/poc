import { Component, Inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatDialogModule, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatIconModule } from '@angular/material/icon';
import { WorkflowService, WorkflowDto, StartWorkflowRequest } from '../services/workflow.service';

@Component({
  selector: 'app-start-workflow-dialog',
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatDialogModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatIconModule
  ],
  templateUrl: './start-workflow-dialog.html',
  styleUrl: './start-workflow-dialog.scss'
})
export class StartWorkflowDialogComponent implements OnInit {
  startForm: FormGroup;
  workflows: WorkflowDto[] = [];
  loading = false;

  constructor(
    private fb: FormBuilder,
    private workflowService: WorkflowService,
    private dialogRef: MatDialogRef<StartWorkflowDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: any
  ) {
    this.startForm = this.fb.group({
      workflowId: ['', Validators.required],
      initiatedBy: ['system@bank.com', Validators.required],
      data: ['{}', Validators.required]
    });
  }

  ngOnInit() {
    this.loadWorkflows();
  }

  loadWorkflows() {
    this.workflowService.getWorkflows().subscribe({
      next: (workflows: any) => {
        this.workflows = workflows;
      },
      error: (error: any) => {
        console.error('Error loading workflows:', error);
      }
    });
  }

  onWorkflowChange() {
    const selectedWorkflow = this.workflows.find(w => w.id === this.startForm.value.workflowId);
    if (selectedWorkflow) {
      // Pre-populate with sample data based on workflow type
      let sampleData = {};
      
      if (selectedWorkflow.name.toLowerCase().includes('loan')) {
        sampleData = {
          applicantName: 'John Doe',
          loanAmount: 50000,
          loanType: 'Personal Loan',
          creditScore: 750,
          employmentStatus: 'Employed',
          annualIncome: 75000
        };
      } else if (selectedWorkflow.name.toLowerCase().includes('account')) {
        sampleData = {
          customerName: 'Jane Smith',
          accountType: 'Savings',
          initialDeposit: 1000,
          customerType: 'Individual',
          branch: 'Main Branch'
        };
      } else {
        sampleData = {
          requestType: 'General',
          priority: 'Normal',
          description: 'New workflow instance'
        };
      }
      
      this.startForm.patchValue({
        data: JSON.stringify(sampleData, null, 2)
      });
    }
  }

  onSubmit() {
    if (this.startForm.valid) {
      this.loading = true;
      
      try {
        const data = JSON.parse(this.startForm.value.data);
        const request: StartWorkflowRequest = {
          workflowId: this.startForm.value.workflowId,
          initiatedBy: this.startForm.value.initiatedBy,
          instanceData: JSON.stringify(data)
        };

        this.workflowService.startWorkflow(request).subscribe({
          next: (instance) => {
            this.loading = false;
            this.dialogRef.close(instance);
          },
          error: (error) => {
            this.loading = false;
            console.error('Error starting workflow:', error);
          }
        });
      } catch (error) {
        console.error('Invalid JSON data:', error);
        this.loading = false;
      }
    }
  }

  onCancel() {
    this.dialogRef.close();
  }
}
