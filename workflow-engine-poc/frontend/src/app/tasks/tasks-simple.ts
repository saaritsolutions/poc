import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';

import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';

@Component({
  selector: 'app-tasks',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatChipsModule
  ],
  template: `
    <div class="tasks-container">
      <div class="header">
        <h2>
          <mat-icon>task</mat-icon>
          My Tasks
        </h2>
        <div class="task-filters">
          <mat-chip-set>
            <mat-chip [class]="selectedFilter() === 'all' ? 'selected' : ''" (click)="setFilter('all')">
              All ({{ tasks().length }})
            </mat-chip>
            <mat-chip [class]="selectedFilter() === 'pending' ? 'selected' : ''" (click)="setFilter('pending')">
              Pending ({{ getPendingCount() }})
            </mat-chip>
            <mat-chip [class]="selectedFilter() === 'high' ? 'selected' : ''" (click)="setFilter('high')">
              High Priority ({{ getHighPriorityCount() }})
            </mat-chip>
          </mat-chip-set>
        </div>
      </div>

      <div class="tasks-list">
        @for (task of filteredTasks(); track task.id) {
          <mat-card class="task-card" [class]="getPriorityClass(task.priority)">
            <mat-card-header>
              <mat-card-title>{{ task.title }}</mat-card-title>
              <mat-card-subtitle>
                {{ task.workflowName }} • {{ task.instanceId }}
              </mat-card-subtitle>
            </mat-card-header>
            
            <mat-card-content>
              <div class="task-info">
                <mat-chip-set>
                  <mat-chip [class]="getPriorityChipClass(task.priority)">
                    {{ task.priority }} Priority
                  </mat-chip>
                  <mat-chip>{{ task.assignedAt }}</mat-chip>
                </mat-chip-set>
              </div>
              
              <p class="task-description">{{ task.description }}</p>
              
              <div class="task-details">
                <div class="detail-item">
                  <mat-icon>person</mat-icon>
                  <span>{{ task.customerName }}</span>
                </div>
                <div class="detail-item">
                  <mat-icon>schedule</mat-icon>
                  <span>Due: {{ task.dueDate }}</span>
                </div>
                @if (task.amount) {
                  <div class="detail-item">
                    <mat-icon>attach_money</mat-icon>
                    <span>{{ task.amount | currency }}</span>
                  </div>
                }
              </div>
            </mat-card-content>
            
            <mat-card-actions>
              <button mat-raised-button color="primary" (click)="completeTask(task.id, 'approve')">
                <mat-icon>check</mat-icon>
                Approve
              </button>
              <button mat-raised-button color="warn" (click)="completeTask(task.id, 'reject')">
                <mat-icon>close</mat-icon>
                Reject
              </button>
              <button mat-button (click)="viewDetails(task.id)">
                <mat-icon>visibility</mat-icon>
                Details
              </button>
            </mat-card-actions>
          </mat-card>
        }
        
        @if (filteredTasks().length === 0) {
          <mat-card class="no-tasks-card">
            <mat-card-content>
              <div class="no-tasks-content">
                <mat-icon>task_alt</mat-icon>
                <h3>No tasks found</h3>
                <p>{{ getNoTasksMessage() }}</p>
              </div>
            </mat-card-content>
          </mat-card>
        }
      </div>
    </div>
  `,
  styles: [`
    .tasks-container {
      padding: 20px;
    }

    .header {
      margin-bottom: 30px;
      
      h2 {
        display: flex;
        align-items: center;
        gap: 10px;
        margin: 0 0 15px 0;
      }
    }

    .task-filters {
      .selected {
        background-color: #1976d2 !important;
        color: white !important;
      }
    }

    .tasks-list {
      display: flex;
      flex-direction: column;
      gap: 15px;
    }

    .task-card {
      transition: transform 0.2s, box-shadow 0.2s;
      
      &:hover {
        transform: translateX(4px);
        box-shadow: 0 4px 12px rgba(0,0,0,0.15);
      }
      
      &.high-priority {
        border-left: 4px solid #f44336;
      }
      
      &.medium-priority {
        border-left: 4px solid #ff9800;
      }
      
      &.low-priority {
        border-left: 4px solid #4caf50;
      }
    }

    .task-info {
      margin: 15px 0;
    }

    .task-description {
      margin: 15px 0;
      color: #666;
      line-height: 1.5;
    }

    .task-details {
      display: flex;
      gap: 20px;
      margin: 15px 0;
      flex-wrap: wrap;
      
      .detail-item {
        display: flex;
        align-items: center;
        gap: 5px;
        font-size: 14px;
        color: #666;
      }
    }

    .high-priority-chip {
      background-color: #f44336 !important;
      color: white !important;
    }

    .medium-priority-chip {
      background-color: #ff9800 !important;
      color: white !important;
    }

    .low-priority-chip {
      background-color: #4caf50 !important;
      color: white !important;
    }

    .no-tasks-card {
      text-align: center;
      
      .no-tasks-content {
        padding: 40px;
        
        mat-icon {
          font-size: 64px;
          width: 64px;
          height: 64px;
          color: #ccc;
          margin-bottom: 20px;
        }
        
        h3 {
          color: #666;
          margin: 10px 0;
        }
        
        p {
          color: #999;
          margin: 0;
        }
      }
    }
  `]
})
export class SimpleTasksComponent {
  selectedFilter = signal('all');
  
  tasks = signal([
    {
      id: 'T-001',
      title: 'Review Loan Application',
      description: 'Review and approve loan application for $50,000. Customer has good credit history and sufficient income.',
      workflowName: 'Loan Application',
      instanceId: 'LA-2023-001',
      priority: 'High',
      customerName: 'John Smith',
      assignedAt: '2 hours ago',
      dueDate: 'Today 5:00 PM',
      amount: 50000
    },
    {
      id: 'T-002',
      title: 'KYC Document Verification',
      description: 'Verify customer identity documents and complete KYC process for new account opening.',
      workflowName: 'Account Opening',
      instanceId: 'AO-2023-045',
      priority: 'Medium',
      customerName: 'Sarah Johnson',
      assignedAt: '4 hours ago',
      dueDate: 'Tomorrow 12:00 PM',
      amount: null
    },
    {
      id: 'T-003',
      title: 'Credit Limit Approval',
      description: 'Approve credit limit for credit card application based on credit score and income verification.',
      workflowName: 'Credit Card Application',
      instanceId: 'CC-2023-112',
      priority: 'Low',
      customerName: 'Mike Wilson',
      assignedAt: '1 day ago',
      dueDate: 'Next Week',
      amount: 5000
    },
    {
      id: 'T-004',
      title: 'Loan Documentation Review',
      description: 'Final review of loan documentation before disbursement. Ensure all paperwork is complete.',
      workflowName: 'Loan Application',
      instanceId: 'LA-2023-003',
      priority: 'High',
      customerName: 'Emma Davis',
      assignedAt: '30 minutes ago',
      dueDate: 'Today 3:00 PM',
      amount: 75000
    }
  ]);

  filteredTasks = signal(this.tasks());

  setFilter(filter: string) {
    this.selectedFilter.set(filter);
    
    if (filter === 'all') {
      this.filteredTasks.set(this.tasks());
    } else if (filter === 'pending') {
      this.filteredTasks.set(this.tasks());
    } else if (filter === 'high') {
      this.filteredTasks.set(this.tasks().filter(task => task.priority === 'High'));
    }
  }

  getPendingCount(): number {
    return this.tasks().length;
  }

  getHighPriorityCount(): number {
    return this.tasks().filter(task => task.priority === 'High').length;
  }

  getPriorityClass(priority: string): string {
    return priority.toLowerCase() + '-priority';
  }

  getPriorityChipClass(priority: string): string {
    return priority.toLowerCase() + '-priority-chip';
  }

  getNoTasksMessage(): string {
    const filter = this.selectedFilter();
    if (filter === 'pending') return 'You have no pending tasks.';
    if (filter === 'high') return 'You have no high priority tasks.';
    return 'You have no tasks assigned.';
  }

  completeTask(taskId: string, action: string) {
    console.log(`Task ${taskId} ${action}ed`);
    // Remove task from list
    const currentTasks = this.tasks();
    const updatedTasks = currentTasks.filter(task => task.id !== taskId);
    this.tasks.set(updatedTasks);
    this.setFilter(this.selectedFilter()); // Refresh filtered tasks
  }

  viewDetails(taskId: string) {
    console.log('View details for task:', taskId);
  }
}
