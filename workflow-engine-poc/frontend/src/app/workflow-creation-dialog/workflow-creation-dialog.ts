import { Component, inject } from '@angular/core';
import { Router } from '@angular/router';
import { MatDialogRef } from '@angular/material/dialog';

import { MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatCardModule } from '@angular/material/card';

@Component({
  selector: 'app-workflow-creation-dialog',
  standalone: true,
  imports: [
    MatDialogModule,
    MatButtonModule,
    MatIconModule,
    MatCardModule
  ],
  template: `
    <div class="creation-dialog">
      <h2 mat-dialog-title>
        <mat-icon>add_circle</mat-icon>
        Choose Workflow Creation Method
      </h2>
      
      <mat-dialog-content>
        <p>How would you like to create your new workflow?</p>
        
        <div class="creation-options">
          <mat-card class="option-card" (click)="selectFormCreator()">
            <mat-card-content>
              <div class="option-icon">
                <mat-icon>list_alt</mat-icon>
              </div>
              <h3>Form-Based Creator</h3>
              <p>Step-by-step wizard with forms and configuration options. Perfect for structured workflow creation.</p>
              <div class="features">
                <span class="feature">• Step-by-step wizard</span>
                <span class="feature">• Detailed configuration</span>
                <span class="feature">• Role-based assignments</span>
              </div>
            </mat-card-content>
          </mat-card>

          <mat-card class="option-card" (click)="selectVisualCanvas()">
            <mat-card-content>
              <div class="option-icon">
                <mat-icon>schema</mat-icon>
              </div>
              <h3>Visual Canvas Designer</h3>
              <p>Drag-and-drop visual workflow builder with flowchart-style design. Great for complex workflows.</p>
              <div class="features">
                <span class="feature">• Drag & drop interface</span>
                <span class="feature">• Visual flow connections</span>
                <span class="feature">• Real-time preview</span>
              </div>
            </mat-card-content>
          </mat-card>
        </div>
      </mat-dialog-content>

      <mat-dialog-actions align="end">
        <button mat-button (click)="cancel()">
          <mat-icon>close</mat-icon>
          Cancel
        </button>
      </mat-dialog-actions>
    </div>
  `,
  styles: [`
    .creation-dialog {
      min-width: 600px;
    }

    h2[mat-dialog-title] {
      display: flex;
      align-items: center;
      gap: 10px;
      margin-bottom: 10px;
      color: #1976d2;
    }

    mat-dialog-content p {
      margin-bottom: 25px;
      color: #666;
    }

    .creation-options {
      display: flex;
      gap: 20px;
      margin-bottom: 20px;
    }

    .option-card {
      flex: 1;
      cursor: pointer;
      transition: all 0.3s ease;
      border: 2px solid transparent;
      
      &:hover {
        transform: translateY(-4px);
        box-shadow: 0 8px 25px rgba(0,0,0,0.15);
        border-color: #1976d2;
      }
    }

    .option-icon {
      text-align: center;
      margin-bottom: 15px;
      
      mat-icon {
        font-size: 48px;
        width: 48px;
        height: 48px;
        color: #1976d2;
      }
    }

    h3 {
      text-align: center;
      margin: 0 0 15px 0;
      color: #333;
      font-size: 18px;
    }

    p {
      text-align: center;
      color: #666;
      font-size: 14px;
      line-height: 1.4;
      margin-bottom: 20px;
    }

    .features {
      display: flex;
      flex-direction: column;
      gap: 8px;
      
      .feature {
        font-size: 12px;
        color: #888;
        display: flex;
        align-items: center;
      }
    }

    mat-dialog-actions {
      padding: 20px 0 10px 0;
    }
  `]
})
export class WorkflowCreationDialogComponent {
  private dialogRef = inject(MatDialogRef<WorkflowCreationDialogComponent>);
  private router = inject(Router);

  selectFormCreator() {
    this.dialogRef.close();
    this.router.navigate(['/create-workflow']);
  }

  selectVisualCanvas() {
    this.dialogRef.close();
    this.router.navigate(['/workflow-canvas']);
  }

  cancel() {
    this.dialogRef.close();
  }
}
