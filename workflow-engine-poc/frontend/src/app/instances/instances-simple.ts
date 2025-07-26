import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';

import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { MatProgressBarModule } from '@angular/material/progress-bar';

@Component({
  selector: 'app-instances',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatChipsModule,
    MatProgressBarModule
  ],
  template: `
    <div class="instances-container">
      <div class="header">
        <h2>
          <mat-icon>playlist_play</mat-icon>
          Workflow Instances
        </h2>
      </div>

      <div class="instances-list">
        @for (instance of instances(); track instance.id) {
          <mat-card class="instance-card">
            <mat-card-header>
              <mat-card-title>{{ instance.workflowName }} #{{ instance.id }}</mat-card-title>
              <mat-card-subtitle>Started {{ instance.startedAt }}</mat-card-subtitle>
            </mat-card-header>
            
            <mat-card-content>
              <div class="instance-info">
                <mat-chip-set>
                  <mat-chip [class]="getStatusClass(instance.status)">
                    {{ instance.status }}
                  </mat-chip>
                  <mat-chip>{{ instance.currentStep }}</mat-chip>
                </mat-chip-set>
              </div>
              
              <div class="progress-section">
                <div class="progress-header">
                  <span>Progress</span>
                  <span>{{ instance.completedSteps }}/{{ instance.totalSteps }} steps</span>
                </div>
                <mat-progress-bar 
                  mode="determinate" 
                  [value]="(instance.completedSteps / instance.totalSteps) * 100">
                </mat-progress-bar>
              </div>
              
              <div class="instance-details">
                <p><strong>Customer:</strong> {{ instance.customerName }}</p>
                <p><strong>Reference:</strong> {{ instance.referenceNumber }}</p>
                <p><strong>Amount:</strong> {{ instance.amount | currency }}</p>
              </div>
            </mat-card-content>
            
            <mat-card-actions>
              <button mat-button (click)="viewDetails(instance.id)">
                <mat-icon>visibility</mat-icon>
                View Details
              </button>
              @if (instance.status === 'Running') {
                <button mat-button color="warn" (click)="pauseInstance(instance.id)">
                  <mat-icon>pause</mat-icon>
                  Pause
                </button>
              }
              @if (instance.status === 'Paused') {
                <button mat-button color="primary" (click)="resumeInstance(instance.id)">
                  <mat-icon>play_arrow</mat-icon>
                  Resume
                </button>
              }
            </mat-card-actions>
          </mat-card>
        }
      </div>
    </div>
  `,
  styles: [`
    .instances-container {
      padding: 20px;
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

    .instances-list {
      display: flex;
      flex-direction: column;
      gap: 20px;
    }

    .instance-card {
      transition: transform 0.2s, box-shadow 0.2s;
      
      &:hover {
        transform: translateX(4px);
        box-shadow: 0 4px 12px rgba(0,0,0,0.15);
      }
    }

    .instance-info {
      margin: 15px 0;
    }

    .progress-section {
      margin: 20px 0;
      
      .progress-header {
        display: flex;
        justify-content: space-between;
        margin-bottom: 8px;
        font-size: 14px;
        color: #666;
      }
    }

    .instance-details {
      margin: 15px 0;
      
      p {
        margin: 5px 0;
        font-size: 14px;
      }
    }

    .running {
      background-color: #2196f3 !important;
      color: white !important;
    }

    .completed {
      background-color: #4caf50 !important;
      color: white !important;
    }

    .paused {
      background-color: #ff9800 !important;
      color: white !important;
    }

    .failed {
      background-color: #f44336 !important;
      color: white !important;
    }
  `]
})
export class SimpleInstancesComponent {
  instances = signal([
    {
      id: 'LA-2023-001',
      workflowName: 'Loan Application',
      status: 'Running',
      currentStep: 'Manager Approval',
      completedSteps: 3,
      totalSteps: 5,
      startedAt: '2 hours ago',
      customerName: 'John Smith',
      referenceNumber: 'REF-LA-001',
      amount: 50000
    },
    {
      id: 'AO-2023-045',
      workflowName: 'Account Opening',
      status: 'Paused',
      currentStep: 'KYC Verification',
      completedSteps: 2,
      totalSteps: 4,
      startedAt: '1 day ago',
      customerName: 'Sarah Johnson',
      referenceNumber: 'REF-AO-045',
      amount: 0
    },
    {
      id: 'CC-2023-112',
      workflowName: 'Credit Card Application',
      status: 'Failed',
      currentStep: 'Credit Check',
      completedSteps: 1,
      totalSteps: 6,
      startedAt: '3 days ago',
      customerName: 'Mike Wilson',
      referenceNumber: 'REF-CC-112',
      amount: 5000
    },
    {
      id: 'LA-2023-002',
      workflowName: 'Loan Application',
      status: 'Completed',
      currentStep: 'Approved',
      completedSteps: 5,
      totalSteps: 5,
      startedAt: '1 week ago',
      customerName: 'Emma Davis',
      referenceNumber: 'REF-LA-002',
      amount: 25000
    }
  ]);

  getStatusClass(status: string): string {
    return status.toLowerCase();
  }

  viewDetails(instanceId: string) {
    console.log('View details for instance:', instanceId);
  }

  pauseInstance(instanceId: string) {
    console.log('Pause instance:', instanceId);
  }

  resumeInstance(instanceId: string) {
    console.log('Resume instance:', instanceId);
  }
}
