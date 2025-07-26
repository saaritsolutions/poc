import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';

import { MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatIconModule } from '@angular/material/icon';
import { MatCardModule } from '@angular/material/card';

@Component({
  selector: 'app-start-workflow-dialog',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatDialogModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatIconModule,
    MatCardModule
  ],
  template: `
    <div class="dialog-container">
      <h2 mat-dialog-title>
        <mat-icon>play_arrow</mat-icon>
        Start Workflow: {{ data.workflowName }}
      </h2>
      
      <mat-dialog-content>
        <div class="workflow-info">
          <mat-card class="info-card">
            <mat-card-content>
              <p><strong>Description:</strong> {{ data.description }}</p>
              <p><strong>Steps:</strong> {{ data.stepCount || 'Multiple steps' }}</p>
              <p><strong>Estimated Time:</strong> {{ data.estimatedTime || '2-3 business days' }}</p>
            </mat-card-content>
          </mat-card>
        </div>

        <div class="form-section">
          <h3>Instance Details</h3>
          
          <mat-form-field appearance="outline" class="full-width">
            <mat-label>Customer Name</mat-label>
            <input matInput placeholder="Enter customer full name" [(ngModel)]="customerName">
            <mat-icon matSuffix>person</mat-icon>
          </mat-form-field>

          <mat-form-field appearance="outline" class="full-width">
            <mat-label>Reference Number</mat-label>
            <input matInput placeholder="Auto-generated if empty" [(ngModel)]="referenceNumber">
            <mat-icon matSuffix>tag</mat-icon>
          </mat-form-field>

          @if (data.workflowType === 'loan' || data.workflowType === 'credit') {
            <mat-form-field appearance="outline" class="full-width">
              <mat-label>Amount</mat-label>
              <input matInput type="number" placeholder="Enter amount" [(ngModel)]="amount">
              <mat-icon matSuffix>attach_money</mat-icon>
            </mat-form-field>
          }

          <mat-form-field appearance="outline" class="full-width">
            <mat-label>Priority</mat-label>
            <mat-select [(ngModel)]="priority">
              <mat-option value="Low">Low Priority</mat-option>
              <mat-option value="Medium">Medium Priority</mat-option>
              <mat-option value="High">High Priority</mat-option>
              <mat-option value="Urgent">Urgent</mat-option>
            </mat-select>
          </mat-form-field>

          <mat-form-field appearance="outline" class="full-width">
            <mat-label>Notes (Optional)</mat-label>
            <textarea matInput rows="3" placeholder="Add any additional notes..." [(ngModel)]="notes"></textarea>
          </mat-form-field>
        </div>
      </mat-dialog-content>

      <mat-dialog-actions align="end">
        <button mat-button (click)="onCancel()">
          <mat-icon>close</mat-icon>
          Cancel
        </button>
        <button mat-raised-button color="primary" [disabled]="!isValid()" (click)="onStart()">
          <mat-icon>play_arrow</mat-icon>
          Start Workflow
        </button>
      </mat-dialog-actions>
    </div>
  `,
  styles: [`
    .dialog-container {
      min-width: 500px;
      max-width: 600px;
    }

    h2[mat-dialog-title] {
      display: flex;
      align-items: center;
      gap: 10px;
      margin-bottom: 20px;
      color: #1976d2;
    }

    .workflow-info {
      margin-bottom: 25px;
    }

    .info-card {
      background-color: #f5f5f5;
      
      p {
        margin: 8px 0;
        font-size: 14px;
      }
    }

    .form-section {
      h3 {
        margin: 20px 0 15px 0;
        color: #333;
        font-size: 16px;
      }
    }

    .full-width {
      width: 100%;
      margin-bottom: 15px;
    }

    mat-dialog-actions {
      padding: 20px 0 10px 0;
      
      button {
        margin-left: 10px;
      }
    }

    mat-icon[matSuffix] {
      color: #666;
    }
  `]
})
export class StartWorkflowDialogComponent {
  private dialogRef = inject(MatDialogRef<StartWorkflowDialogComponent>);
  data = inject(MAT_DIALOG_DATA);

  customerName = signal('');
  referenceNumber = signal('');
  amount = signal<number | null>(null);
  priority = signal('Medium');
  notes = signal('');

  isValid(): boolean {
    return this.customerName().trim().length > 0;
  }

  onCancel() {
    this.dialogRef.close();
  }

  onStart() {
    const instanceData = {
      workflowId: this.data.workflowId,
      workflowName: this.data.workflowName,
      customerName: this.customerName(),
      referenceNumber: this.referenceNumber() || this.generateReferenceNumber(),
      amount: this.amount(),
      priority: this.priority(),
      notes: this.notes(),
      startedAt: new Date().toISOString()
    };

    this.dialogRef.close(instanceData);
  }

  private generateReferenceNumber(): string {
    const prefix = this.data.workflowType === 'loan' ? 'LA' : 
                   this.data.workflowType === 'account' ? 'AO' : 
                   this.data.workflowType === 'credit' ? 'CC' : 'WF';
    const year = new Date().getFullYear();
    const random = Math.floor(Math.random() * 1000).toString().padStart(3, '0');
    return `${prefix}-${year}-${random}`;
  }
}
