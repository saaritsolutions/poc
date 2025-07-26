import { Component, signal, inject } from '@angular/core';
import { RouterOutlet, RouterLink } from '@angular/router';

import { MatToolbarModule } from '@angular/material/toolbar';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatCardModule } from '@angular/material/card';
import { MatListModule } from '@angular/material/list';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';

import { StartWorkflowDialogComponent } from './start-workflow-dialog/start-workflow-dialog-simple';
import { WorkflowCreationDialogComponent } from './workflow-creation-dialog/workflow-creation-dialog';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [
    RouterOutlet,
    RouterLink,
    MatToolbarModule,
    MatButtonModule,
    MatIconModule,
    MatCardModule,
    MatListModule
  ],
  templateUrl: './app-simple.html',
  styleUrls: ['./app-simple.scss']
})
export class SimpleAppComponent {
  title = signal('Core Banking Workflow Engine');
  
  private dialog = inject(MatDialog);
  private snackBar = inject(MatSnackBar);

  quickStartWorkflow(workflowType: string) {
    let workflowData;
    
    switch(workflowType) {
      case 'loan':
        workflowData = {
          workflowId: '1',
          workflowName: 'Loan Application',
          description: 'Complete loan application workflow with credit check and approval',
          workflowType: 'loan'
        };
        break;
      case 'account':
        workflowData = {
          workflowId: '2', 
          workflowName: 'Account Opening',
          description: 'New customer account opening with KYC verification',
          workflowType: 'account'
        };
        break;
      case 'credit':
        workflowData = {
          workflowId: '3',
          workflowName: 'Credit Card Application', 
          description: 'Credit card application with credit score evaluation',
          workflowType: 'credit'
        };
        break;
      default:
        return;
    }

    const dialogRef = this.dialog.open(StartWorkflowDialogComponent, {
      width: '600px',
      data: workflowData
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.snackBar.open(
          `Workflow "${workflowData.workflowName}" started successfully for ${result.customerName}`,
          'Close',
          { duration: 3000 }
        );
        console.log('Quick start workflow:', result);
      }
    });
  }

  openWorkflowCreator() {
    this.dialog.open(WorkflowCreationDialogComponent, {
      width: '700px',
      disableClose: false
    });
  }
}
