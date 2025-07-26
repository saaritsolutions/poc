import { Routes } from '@angular/router';

export const routes: Routes = [
  { path: '', redirectTo: '/workflows', pathMatch: 'full' },
  { 
    path: 'workflows', 
    loadComponent: () => import('./workflows/workflows-simple').then(m => m.SimpleWorkflowsComponent) 
  },
  { 
    path: 'instances', 
    loadComponent: () => import('./instances/instances-simple').then(m => m.SimpleInstancesComponent) 
  },
  { 
    path: 'tasks', 
    loadComponent: () => import('./tasks/tasks-simple').then(m => m.SimpleTasksComponent) 
  },
  { 
    path: 'create-workflow', 
    loadComponent: () => import('./workflow-creator/workflow-creator-simple').then(m => m.SimpleCreateWorkflowComponent) 
  },
  { 
    path: 'workflow-canvas', 
    loadComponent: () => import('./workflow-canvas/workflow-canvas').then(m => m.WorkflowCanvasComponent) 
  }
];
