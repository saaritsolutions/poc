import { Component, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommonModule, TitleCasePipe } from '@angular/common';

import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatStepperModule } from '@angular/material/stepper';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatCheckboxModule } from '@angular/material/checkbox';

@Component({
  selector: 'app-create-workflow',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    TitleCasePipe,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatStepperModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatCheckboxModule
  ],
  template: `
    <div class="create-workflow-container">
      <div class="header">
        <h2>
          <mat-icon>add_circle</mat-icon>
          Create New Workflow
        </h2>
      </div>

      <mat-card class="workflow-designer">
        <mat-card-content>
          <mat-stepper #stepper linear>
            <!-- Step 1: Basic Information -->
            <mat-step label="Basic Information">
              <div class="step-content">
                <mat-form-field appearance="outline" class="full-width">
                  <mat-label>Workflow Name</mat-label>
                  <input matInput placeholder="e.g., Personal Loan Application" [(ngModel)]="workflowName">
                </mat-form-field>

                <mat-form-field appearance="outline" class="full-width">
                  <mat-label>Description</mat-label>
                  <textarea matInput rows="3" placeholder="Describe the purpose of this workflow" [(ngModel)]="workflowDescription"></textarea>
                </mat-form-field>

                <mat-form-field appearance="outline">
                  <mat-label>Category</mat-label>
                  <mat-select [(ngModel)]="workflowCategory">
                    <mat-option value="lending">Lending</mat-option>
                    <mat-option value="accounts">Account Management</mat-option>
                    <mat-option value="credit">Credit Services</mat-option>
                    <mat-option value="compliance">Compliance</mat-option>
                  </mat-select>
                </mat-form-field>

                <div class="step-buttons">
                  <button mat-raised-button color="primary" matStepperNext>Next</button>
                </div>
              </div>
            </mat-step>

            <!-- Step 2: Define Steps -->
            <mat-step label="Define Steps">
              <div class="step-content">
                <div class="steps-header">
                  <h3>Workflow Steps</h3>
                  <button mat-button (click)="addStep()">
                    <mat-icon>add</mat-icon>
                    Add Step
                  </button>
                </div>

                <div class="workflow-steps">
                  @for (step of workflowSteps(); track step.id; let i = $index) {
                    <mat-card class="step-card">
                      <mat-card-content>
                        <div class="step-header">
                          <span class="step-number">{{ i + 1 }}</span>
                          <mat-form-field appearance="outline" class="step-name-field">
                            <mat-label>Step Name</mat-label>
                            <input matInput [(ngModel)]="step.name">
                          </mat-form-field>
                          <button mat-icon-button color="warn" (click)="removeStep(i)">
                            <mat-icon>delete</mat-icon>
                          </button>
                        </div>

                        <mat-form-field appearance="outline" class="full-width">
                          <mat-label>Step Type</mat-label>
                          <mat-select [(ngModel)]="step.type">
                            <mat-option value="manual">Manual Task</mat-option>
                            <mat-option value="automatic">Automatic</mat-option>
                            <mat-option value="approval">Approval Required</mat-option>
                            <mat-option value="condition">Conditional</mat-option>
                          </mat-select>
                        </mat-form-field>

                        <mat-form-field appearance="outline" class="full-width">
                          <mat-label>Description</mat-label>
                          <textarea matInput rows="2" [(ngModel)]="step.description"></textarea>
                        </mat-form-field>

                        @if (step.type === 'approval') {
                          <mat-form-field appearance="outline">
                            <mat-label>Approver Role</mat-label>
                            <mat-select [(ngModel)]="step.approverRole">
                              <mat-option value="manager">Manager</mat-option>
                              <mat-option value="senior-manager">Senior Manager</mat-option>
                              <mat-option value="risk-officer">Risk Officer</mat-option>
                              <mat-option value="compliance-officer">Compliance Officer</mat-option>
                            </mat-select>
                          </mat-form-field>
                        }
                      </mat-card-content>
                    </mat-card>
                  }
                </div>

                <div class="step-buttons">
                  <button mat-button matStepperPrevious>Previous</button>
                  <button mat-raised-button color="primary" matStepperNext>Next</button>
                </div>
              </div>
            </mat-step>

            <!-- Step 3: Configuration -->
            <mat-step label="Configuration">
              <div class="step-content">
                <h3>Workflow Configuration</h3>

                <div class="config-section">
                  <h4>Timeout Settings</h4>
                  <mat-form-field appearance="outline">
                    <mat-label>Default Step Timeout (hours)</mat-label>
                    <input matInput type="number" [(ngModel)]="defaultTimeout">
                  </mat-form-field>
                </div>

                <div class="config-section">
                  <h4>Notifications</h4>
                  <mat-checkbox [(ngModel)]="emailNotifications">Email Notifications</mat-checkbox>
                  <mat-checkbox [(ngModel)]="smsNotifications">SMS Notifications</mat-checkbox>
                </div>

                <div class="config-section">
                  <h4>Auto-Assignment</h4>
                  <mat-checkbox [(ngModel)]="autoAssignment">Enable Auto-Assignment</mat-checkbox>
                </div>

                <div class="step-buttons">
                  <button mat-button matStepperPrevious>Previous</button>
                  <button mat-raised-button color="primary" matStepperNext>Next</button>
                </div>
              </div>
            </mat-step>

            <!-- Step 4: Review -->
            <mat-step label="Review & Create">
              <div class="step-content">
                <h3>Review Your Workflow</h3>

                <mat-card class="review-card">
                  <mat-card-header>
                    <mat-card-title>{{ workflowName() || 'Untitled Workflow' }}</mat-card-title>
                    <mat-card-subtitle>{{ workflowCategory() }} Workflow</mat-card-subtitle>
                  </mat-card-header>
                  <mat-card-content>
                    <p><strong>Description:</strong> {{ workflowDescription() || 'No description provided' }}</p>
                    <p><strong>Number of Steps:</strong> {{ workflowSteps().length }}</p>
                    <p><strong>Default Timeout:</strong> {{ defaultTimeout() }} hours</p>
                  </mat-card-content>
                </mat-card>

                <div class="workflow-preview">
                  <h4>Workflow Steps Preview</h4>
                  @for (step of workflowSteps(); track step.id; let i = $index) {
                    <div class="preview-step">
                      <div class="step-indicator">{{ i + 1 }}</div>
                      <div class="step-info">
                        <h5>{{ step.name }}</h5>
                        <p>{{ step.description }}</p>
                        <span class="step-type">{{ step.type }}</span>
                      </div>
                    </div>
                  }
                </div>

                <div class="step-buttons">
                  <button mat-button matStepperPrevious>Previous</button>
                  <button mat-raised-button color="primary" (click)="createWorkflow()">
                    <mat-icon>save</mat-icon>
                    Create Workflow
                  </button>
                </div>
              </div>
            </mat-step>
          </mat-stepper>
        </mat-card-content>
      </mat-card>
    </div>
  `,
  styles: [`
    .create-workflow-container {
      padding: 20px;
      max-width: 1000px;
      margin: 0 auto;
    }

    .header {
      margin-bottom: 30px;
      
      h2 {
        display: flex;
        align-items: center;
        gap: 10px;
        margin: 0;
      }
    }

    .workflow-designer {
      min-height: 600px;
    }

    .step-content {
      padding: 20px 0;
      min-height: 400px;
    }

    .full-width {
      width: 100%;
      margin-bottom: 15px;
    }

    .step-buttons {
      margin-top: 30px;
      display: flex;
      gap: 10px;
    }

    .steps-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 20px;
    }

    .workflow-steps {
      display: flex;
      flex-direction: column;
      gap: 15px;
    }

    .step-card {
      border-left: 4px solid #1976d2;
    }

    .step-header {
      display: flex;
      align-items: center;
      gap: 15px;
      margin-bottom: 15px;
    }

    .step-number {
      background: #1976d2;
      color: white;
      border-radius: 50%;
      width: 30px;
      height: 30px;
      display: flex;
      align-items: center;
      justify-content: center;
      font-weight: bold;
      flex-shrink: 0;
    }

    .step-name-field {
      flex-grow: 1;
    }

    .config-section {
      margin: 20px 0;
      
      h4 {
        margin-bottom: 15px;
        color: #333;
      }
      
      mat-checkbox {
        display: block;
        margin: 10px 0;
      }
    }

    .review-card {
      margin-bottom: 30px;
    }

    .workflow-preview {
      h4 {
        margin-bottom: 20px;
        color: #333;
      }
    }

    .preview-step {
      display: flex;
      align-items: flex-start;
      gap: 15px;
      margin-bottom: 20px;
      padding: 15px;
      background: #f5f5f5;
      border-radius: 8px;
    }

    .step-indicator {
      background: #1976d2;
      color: white;
      border-radius: 50%;
      width: 30px;
      height: 30px;
      display: flex;
      align-items: center;
      justify-content: center;
      font-weight: bold;
      flex-shrink: 0;
    }

    .step-info {
      flex-grow: 1;
      
      h5 {
        margin: 0 0 5px 0;
        color: #333;
      }
      
      p {
        margin: 0 0 10px 0;
        color: #666;
        font-size: 14px;
      }
    }

    .step-type {
      background: #e3f2fd;
      color: #1976d2;
      padding: 4px 8px;
      border-radius: 12px;
      font-size: 12px;
      font-weight: 500;
    }
  `]
})
export class SimpleCreateWorkflowComponent {
  workflowName = signal('');
  workflowDescription = signal('');
  workflowCategory = signal('lending');
  defaultTimeout = signal(24);
  emailNotifications = signal(true);
  smsNotifications = signal(false);
  autoAssignment = signal(true);

  workflowSteps = signal([
    {
      id: '1',
      name: 'Initial Review',
      type: 'manual',
      description: 'Initial review of the application',
      approverRole: ''
    }
  ]);

  addStep() {
    const currentSteps = this.workflowSteps();
    const newStep = {
      id: Date.now().toString(),
      name: '',
      type: 'manual',
      description: '',
      approverRole: ''
    };
    this.workflowSteps.set([...currentSteps, newStep]);
  }

  removeStep(index: number) {
    const currentSteps = this.workflowSteps();
    currentSteps.splice(index, 1);
    this.workflowSteps.set([...currentSteps]);
  }

  createWorkflow() {
    const workflow = {
      name: this.workflowName(),
      description: this.workflowDescription(),
      category: this.workflowCategory(),
      steps: this.workflowSteps(),
      config: {
        defaultTimeout: this.defaultTimeout(),
        emailNotifications: this.emailNotifications(),
        smsNotifications: this.smsNotifications(),
        autoAssignment: this.autoAssignment()
      }
    };
    
    console.log('Creating workflow:', workflow);
    alert('Workflow created successfully!');
  }
}
