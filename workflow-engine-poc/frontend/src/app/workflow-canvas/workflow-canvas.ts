import { Component, signal, ElementRef, ViewChild, AfterViewInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatListModule } from '@angular/material/list';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatDialogModule, MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';

interface WorkflowStep {
  id: string;
  type: 'start' | 'manual' | 'automatic' | 'approval' | 'condition' | 'end';
  name: string;
  description: string;
  x: number;
  y: number;
  width: number;
  height: number;
  inputs: string[];
  outputs: string[];
  config?: any;
}

interface Connection {
  id: string;
  fromStepId: string;
  toStepId: string;
  fromPort: string;
  toPort: string;
  condition?: string;
}

@Component({
  selector: 'app-workflow-canvas',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatToolbarModule,
    MatSidenavModule,
    MatListModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatDialogModule
  ],
  template: `
    <div class="workflow-canvas-container">
      <mat-toolbar color="primary" class="canvas-toolbar">
        <span>
          <mat-icon>schema</mat-icon>
          Workflow Designer
        </span>
        <span class="spacer"></span>
        <button mat-button (click)="saveWorkflow()">
          <mat-icon>save</mat-icon>
          Save
        </button>
        <button mat-button (click)="clearCanvas()">
          <mat-icon>clear</mat-icon>
          Clear
        </button>
        <button mat-raised-button color="accent" (click)="deployWorkflow()">
          <mat-icon>publish</mat-icon>
          Deploy
        </button>
      </mat-toolbar>

      <div class="canvas-content">
        <mat-sidenav-container class="sidenav-container">
          <mat-sidenav mode="side" opened class="toolbox">
            <div class="toolbox-header">
              <h3>Workflow Steps</h3>
            </div>
            
            <div class="step-category">
              <h4>Flow Control</h4>
              <div class="step-item" 
                   draggable="true" 
                   (dragstart)="onDragStart($event, 'start')"
                   [class.disabled]="hasStartStep()">
                <mat-icon class="step-icon start-step">play_circle</mat-icon>
                <span>Start</span>
              </div>
              <div class="step-item" 
                   draggable="true" 
                   (dragstart)="onDragStart($event, 'end')"
                   [class.disabled]="hasEndStep()">
                <mat-icon class="step-icon end-step">stop_circle</mat-icon>
                <span>End</span>
              </div>
            </div>

            <div class="step-category">
              <h4>Process Steps</h4>
              <div class="step-item" draggable="true" (dragstart)="onDragStart($event, 'manual')">
                <mat-icon class="step-icon manual-step">person</mat-icon>
                <span>Manual Task</span>
              </div>
              <div class="step-item" draggable="true" (dragstart)="onDragStart($event, 'automatic')">
                <mat-icon class="step-icon auto-step">settings</mat-icon>
                <span>Automatic</span>
              </div>
              <div class="step-item" draggable="true" (dragstart)="onDragStart($event, 'approval')">
                <mat-icon class="step-icon approval-step">check_circle</mat-icon>
                <span>Approval</span>
              </div>
              <div class="step-item" draggable="true" (dragstart)="onDragStart($event, 'condition')">
                <mat-icon class="step-icon condition-step">alt_route</mat-icon>
                <span>Condition</span>
              </div>
            </div>

            <div class="step-category">
              <h4>Banking Steps</h4>
              <div class="step-item" draggable="true" (dragstart)="onDragStart($event, 'credit-check')">
                <mat-icon class="step-icon banking-step">account_balance</mat-icon>
                <span>Credit Check</span>
              </div>
              <div class="step-item" draggable="true" (dragstart)="onDragStart($event, 'kyc')">
                <mat-icon class="step-icon banking-step">verified_user</mat-icon>
                <span>KYC Verification</span>
              </div>
              <div class="step-item" draggable="true" (dragstart)="onDragStart($event, 'risk-assessment')">
                <mat-icon class="step-icon banking-step">shield</mat-icon>
                <span>Risk Assessment</span>
              </div>
            </div>
          </mat-sidenav>

          <mat-sidenav-content class="canvas-main">
            @if (isConnecting) {
              <div class="connection-instructions">
                <mat-icon>info</mat-icon>
                <span>Click on a green input port to complete the connection, or click anywhere to cancel</span>
                <button mat-icon-button (click)="cancelConnection()">
                  <mat-icon>close</mat-icon>
                </button>
              </div>
            }
            
            <div class="canvas-wrapper">
              <svg #canvas 
                   class="workflow-canvas"
                   [class.connecting-mode]="isConnecting"
                   [class.dragging-mode]="draggingStep !== null"
                   (drop)="onDrop($event)" 
                   (dragover)="onDragOver($event)"
                   (click)="onCanvasClick($event)"
                   (mousemove)="onMouseMove($event)"
                   (mouseup)="onMouseUp($event)">
                
                <!-- Grid Pattern -->
                <defs>
                  <pattern id="grid" width="20" height="20" patternUnits="userSpaceOnUse">
                    <path d="M 20 0 L 0 0 0 20" fill="none" stroke="#e0e0e0" stroke-width="1"/>
                  </pattern>
                </defs>
                <rect width="100%" height="100%" fill="url(#grid)" />
                
                <!-- Connections -->
                @for (connection of connections(); track connection.id) {
                  <g class="connection">
                    <path [attr.d]="getConnectionPath(connection)" 
                          stroke="#1976d2" 
                          stroke-width="2" 
                          fill="none"
                          marker-end="url(#arrowhead)" />
                  </g>
                }
                
                <!-- Arrow marker -->
                <defs>
                  <marker id="arrowhead" markerWidth="10" markerHeight="7" 
                          refX="9" refY="3.5" orient="auto">
                    <polygon points="0 0, 10 3.5, 0 7" fill="#1976d2" />
                  </marker>
                </defs>
                
                <!-- Workflow Steps -->
                @for (step of steps(); track step.id) {
                  <g class="workflow-step" 
                     [attr.transform]="'translate(' + step.x + ',' + step.y + ')'"
                     (click)="selectStep(step, $event)"
                     (mousedown)="onStepMouseDown(step, $event)"
                     [class.selected]="selectedStep()?.id === step.id"
                     [class.dragging]="draggingStep?.id === step.id">
                    
                    <rect [attr.width]="step.width" 
                          [attr.height]="step.height"
                          [attr.class]="'step-rect ' + step.type + '-step'"
                          rx="8" />
                    
                    <!-- Step Icon -->
                    <foreignObject x="8" y="8" width="24" height="24">
                      <mat-icon [class]="'step-icon ' + step.type + '-step'">
                        {{ getStepIcon(step.type) }}
                      </mat-icon>
                    </foreignObject>
                    
                    <!-- Step Name -->
                    <text x="40" y="20" class="step-name">{{ step.name }}</text>
                    <text x="40" y="35" class="step-description">{{ step.description }}</text>
                    
                    <!-- Connection Points -->
                    <circle cx="0" cy="25" r="4" class="connection-point input" 
                            (click)="handleConnectionPoint(step.id, 'input', $event)"
                            [class.connecting]="connectingFrom?.stepId === step.id"/>
                    <circle [attr.cx]="step.width" cy="25" r="4" class="connection-point output"
                            (click)="handleConnectionPoint(step.id, 'output', $event)"
                            [class.connecting]="connectingFrom?.stepId === step.id"/>
                  </g>
                }
                
                <!-- Temporary connection line while dragging -->
                @if (tempConnection()) {
                  <line [attr.x1]="tempConnection()!.x1" 
                        [attr.y1]="tempConnection()!.y1"
                        [attr.x2]="tempConnection()!.x2" 
                        [attr.y2]="tempConnection()!.y2"
                        stroke="#ff9800" 
                        stroke-width="2" 
                        stroke-dasharray="5,5" />
                }
              </svg>
            </div>
          </mat-sidenav-content>
        </mat-sidenav-container>
      </div>

      <!-- Properties Panel -->
      @if (selectedStep()) {
        <div class="properties-panel">
          <mat-card>
            <mat-card-header>
              <mat-card-title>Step Properties</mat-card-title>
              <button mat-icon-button (click)="deleteStep()" color="warn">
                <mat-icon>delete</mat-icon>
              </button>
            </mat-card-header>
            <mat-card-content>
              <mat-form-field appearance="outline" class="full-width">
                <mat-label>Step Name</mat-label>
                <input matInput [(ngModel)]="selectedStep()!.name">
              </mat-form-field>
              
              <mat-form-field appearance="outline" class="full-width">
                <mat-label>Description</mat-label>
                <textarea matInput rows="2" [(ngModel)]="selectedStep()!.description"></textarea>
              </mat-form-field>
              
              @if (selectedStep()!.type === 'approval') {
                <mat-form-field appearance="outline" class="full-width">
                  <mat-label>Approver Role</mat-label>
                  <mat-select [(ngModel)]="selectedStep()!.config.approverRole">
                    <mat-option value="manager">Manager</mat-option>
                    <mat-option value="senior-manager">Senior Manager</mat-option>
                    <mat-option value="risk-officer">Risk Officer</mat-option>
                    <mat-option value="compliance-officer">Compliance Officer</mat-option>
                  </mat-select>
                </mat-form-field>
              }
              
              @if (selectedStep()!.type === 'condition') {
                <mat-form-field appearance="outline" class="full-width">
                  <mat-label>Condition Expression</mat-label>
                  <input matInput [(ngModel)]="selectedStep()!.config.condition" 
                         placeholder="e.g., amount > 50000">
                </mat-form-field>
              }
            </mat-card-content>
          </mat-card>
        </div>
      }
    </div>
  `,
  styles: [`
    .workflow-canvas-container {
      height: 100vh;
      display: flex;
      flex-direction: column;
    }

    .canvas-toolbar {
      flex-shrink: 0;
      
      .spacer {
        flex: 1 1 auto;
      }
    }

    .canvas-content {
      flex: 1;
      overflow: hidden;
    }

    .sidenav-container {
      height: 100%;
    }

    .toolbox {
      width: 250px;
      background: #f5f5f5;
      padding: 10px;
      overflow-y: auto;
    }

    .toolbox-header {
      text-align: center;
      margin-bottom: 20px;
      
      h3 {
        margin: 0;
        color: #333;
      }
    }

    .step-category {
      margin-bottom: 25px;
      
      h4 {
        margin: 0 0 10px 0;
        color: #666;
        font-size: 14px;
        text-transform: uppercase;
        letter-spacing: 0.5px;
      }
    }

    .step-item {
      display: flex;
      align-items: center;
      padding: 10px;
      margin: 5px 0;
      background: white;
      border-radius: 6px;
      cursor: grab;
      transition: all 0.2s;
      border: 1px solid #ddd;
      
      &:hover {
        background: #e3f2fd;
        transform: translateX(2px);
      }
      
      &.disabled {
        opacity: 0.5;
        cursor: not-allowed;
        background: #f0f0f0;
      }
      
      .step-icon {
        margin-right: 10px;
        font-size: 20px;
      }
      
      span {
        font-size: 14px;
        font-weight: 500;
      }
    }

    .canvas-main {
      position: relative;
    }

    .connection-instructions {
      position: absolute;
      top: 10px;
      left: 50%;
      transform: translateX(-50%);
      background: #ff9800;
      color: white;
      padding: 10px 15px;
      border-radius: 6px;
      display: flex;
      align-items: center;
      gap: 10px;
      z-index: 1000;
      box-shadow: 0 2px 8px rgba(0,0,0,0.2);
      
      mat-icon {
        font-size: 18px;
      }
      
      span {
        font-size: 14px;
        font-weight: 500;
      }
      
      button {
        color: white;
        
        mat-icon {
          font-size: 16px;
        }
      }
    }

    .canvas-wrapper {
      width: 100%;
      height: 100%;
      overflow: auto;
    }

    .workflow-canvas {
      width: 2000px;
      height: 1500px;
      background: white;
      cursor: default;
      
      &.connecting-mode {
        cursor: crosshair;
      }
      
      &.dragging-mode {
        cursor: grabbing;
      }
    }

    .workflow-step {
      cursor: grab;
      transition: filter 0.2s ease;
      
      &:hover {
        filter: drop-shadow(0 4px 8px rgba(0,0,0,0.1));
      }
      
      &.selected .step-rect {
        stroke: #ff9800;
        stroke-width: 2;
      }
      
      &.dragging {
        cursor: grabbing;
        filter: drop-shadow(0 6px 12px rgba(0,0,0,0.2));
        
        .step-rect {
          opacity: 0.8;
        }
      }
    }

    .step-rect {
      fill: white;
      stroke: #ddd;
      stroke-width: 1;
      
      &.start-step { fill: #e8f5e8; stroke: #4caf50; }
      &.end-step { fill: #fce4ec; stroke: #e91e63; }
      &.manual-step { fill: #fff3e0; stroke: #ff9800; }
      &.auto-step { fill: #e3f2fd; stroke: #2196f3; }
      &.approval-step { fill: #f3e5f5; stroke: #9c27b0; }
      &.condition-step { fill: #fff8e1; stroke: #ffc107; }
      &.banking-step { fill: #e0f2f1; stroke: #009688; }
    }

    .step-name {
      font-size: 12px;
      font-weight: bold;
      fill: #333;
    }

    .step-description {
      font-size: 10px;
      fill: #666;
    }

    .connection-point {
      fill: #1976d2;
      stroke: white;
      stroke-width: 2;
      cursor: pointer;
      transition: all 0.2s ease;
      
      &:hover {
        fill: #ff9800;
        r: 6;
        stroke-width: 3;
      }
      
      &.connecting {
        fill: #ff9800;
        r: 6;
        stroke: #fff;
        stroke-width: 3;
        animation: pulse 1s infinite;
      }
      
      &.input {
        fill: #4caf50;
        
        &:hover {
          fill: #66bb6a;
        }
      }
      
      &.output {
        fill: #f44336;
        
        &:hover {
          fill: #ef5350;
        }
      }
    }

    @keyframes pulse {
      0% { r: 4; }
      50% { r: 7; }
      100% { r: 4; }
    }

    .connection {
      cursor: pointer;
      
      path:hover {
        stroke: #ff9800;
        stroke-width: 3;
      }
    }

    .properties-panel {
      position: absolute;
      top: 20px;
      right: 20px;
      width: 300px;
      z-index: 100;
      
      .full-width {
        width: 100%;
        margin-bottom: 15px;
      }
    }

    /* Step Icon Colors */
    .start-step { color: #4caf50; }
    .end-step { color: #e91e63; }
    .manual-step { color: #ff9800; }
    .auto-step { color: #2196f3; }
    .approval-step { color: #9c27b0; }
    .condition-step { color: #ffc107; }
    .banking-step { color: #009688; }
  `]
})
export class WorkflowCanvasComponent implements AfterViewInit {
  @ViewChild('canvas', { static: true }) canvasRef!: ElementRef<SVGElement>;
  
  private dialog = inject(MatDialog);
  private snackBar = inject(MatSnackBar);

  steps = signal<WorkflowStep[]>([]);
  connections = signal<Connection[]>([]);
  selectedStep = signal<WorkflowStep | null>(null);
  tempConnection = signal<{x1: number, y1: number, x2: number, y2: number} | null>(null);
  
  private draggedStepType = '';
  connectingFrom: { stepId: string; port: string; x: number; y: number } | null = null;
  private stepIdCounter = 1;
  private connectionIdCounter = 1;
  isConnecting = false;
  
  // Drag and drop state for rearranging steps
  draggingStep: WorkflowStep | null = null;
  private dragOffset = { x: 0, y: 0 };

  ngAfterViewInit() {
    // Add some sample steps
    this.addSampleWorkflow();
  }

  addSampleWorkflow() {
    const startStep: WorkflowStep = {
      id: 'start-1',
      type: 'start',
      name: 'Start',
      description: 'Workflow Start',
      x: 100,
      y: 100,
      width: 150,
      height: 50,
      inputs: [],
      outputs: ['out'],
      config: {}
    };

    const manualStep: WorkflowStep = {
      id: 'manual-1',
      type: 'manual',
      name: 'Document Review',
      description: 'Review submitted documents',
      x: 300,
      y: 100,
      width: 150,
      height: 50,
      inputs: ['in'],
      outputs: ['out'],
      config: {}
    };

    const approvalStep: WorkflowStep = {
      id: 'approval-1',
      type: 'approval',
      name: 'Manager Approval',
      description: 'Manager reviews and approves',
      x: 500,
      y: 100,
      width: 150,
      height: 50,
      inputs: ['in'],
      outputs: ['out'],
      config: { approverRole: 'manager' }
    };

    this.steps.set([startStep, manualStep, approvalStep]);
    
    const connection1: Connection = {
      id: 'conn-1',
      fromStepId: 'start-1',
      toStepId: 'manual-1',
      fromPort: 'out',
      toPort: 'in'
    };

    const connection2: Connection = {
      id: 'conn-2',
      fromStepId: 'manual-1',
      toStepId: 'approval-1',
      fromPort: 'out',
      toPort: 'in'
    };

    this.connections.set([connection1, connection2]);
  }

  onDragStart(event: DragEvent, stepType: string) {
    if (stepType === 'start' && this.hasStartStep()) return;
    if (stepType === 'end' && this.hasEndStep()) return;
    
    this.draggedStepType = stepType;
    if (event.dataTransfer) {
      event.dataTransfer.effectAllowed = 'copy';
    }
  }

  onDragOver(event: DragEvent) {
    event.preventDefault();
    if (event.dataTransfer) {
      event.dataTransfer.dropEffect = 'copy';
    }
  }

  onDrop(event: DragEvent) {
    event.preventDefault();
    const rect = this.canvasRef.nativeElement.getBoundingClientRect();
    const x = event.clientX - rect.left;
    const y = event.clientY - rect.top;
    
    this.addStep(this.draggedStepType, x, y);
  }

  addStep(type: string, x: number, y: number) {
    const stepId = `${type}-${this.stepIdCounter++}`;
    const stepNames = {
      'start': 'Start',
      'end': 'End',
      'manual': 'Manual Task',
      'automatic': 'Automatic Step',
      'approval': 'Approval Step',
      'condition': 'Condition',
      'credit-check': 'Credit Check',
      'kyc': 'KYC Verification',
      'risk-assessment': 'Risk Assessment'
    };

    const newStep: WorkflowStep = {
      id: stepId,
      type: type as any,
      name: stepNames[type as keyof typeof stepNames] || 'New Step',
      description: 'Step description',
      x: x - 75,
      y: y - 25,
      width: 150,
      height: 50,
      inputs: type === 'start' ? [] : ['in'],
      outputs: type === 'end' ? [] : ['out'],
      config: {}
    };

    this.steps.update(steps => [...steps, newStep]);
  }

  selectStep(step: WorkflowStep, event: Event) {
    event.stopPropagation();
    this.selectedStep.set(step);
  }

  onCanvasClick(event: Event) {
    if (!this.isConnecting && !this.draggingStep) {
      this.selectedStep.set(null);
    }
    this.cancelConnection();
  }

  onMouseMove(event: MouseEvent) {
    if (this.connectingFrom && this.isConnecting) {
      const rect = this.canvasRef.nativeElement.getBoundingClientRect();
      const x2 = event.clientX - rect.left;
      const y2 = event.clientY - rect.top;
      
      this.tempConnection.set({
        x1: this.connectingFrom.x,
        y1: this.connectingFrom.y,
        x2: x2,
        y2: y2
      });
    }
    
    // Handle step dragging
    if (this.draggingStep) {
      const rect = this.canvasRef.nativeElement.getBoundingClientRect();
      const x = event.clientX - rect.left - this.dragOffset.x;
      const y = event.clientY - rect.top - this.dragOffset.y;
      
      // Update the step position
      this.steps.update(steps => 
        steps.map(step => 
          step.id === this.draggingStep!.id 
            ? { ...step, x: Math.max(0, x), y: Math.max(0, y) }
            : step
        )
      );
      
      // Update draggingStep reference
      const updatedStep = this.steps().find(s => s.id === this.draggingStep!.id);
      if (updatedStep) {
        this.draggingStep = updatedStep;
      }
    }
  }

  onStepMouseDown(step: WorkflowStep, event: MouseEvent) {
    // Prevent if clicking on connection points
    const target = event.target as Element;
    if (target.classList.contains('connection-point')) {
      return;
    }
    
    event.stopPropagation();
    event.preventDefault();
    
    this.draggingStep = step;
    this.selectedStep.set(step);
    
    // Calculate offset from mouse to step origin
    const rect = this.canvasRef.nativeElement.getBoundingClientRect();
    this.dragOffset.x = event.clientX - rect.left - step.x;
    this.dragOffset.y = event.clientY - rect.top - step.y;
  }

  onMouseUp(event: MouseEvent) {
    if (this.draggingStep) {
      this.draggingStep = null;
    }
  }

  handleConnectionPoint(stepId: string, port: string, event: Event) {
    event.stopPropagation();
    
    if (!this.connectingFrom) {
      // Start a new connection
      this.startConnection(stepId, port, event);
    } else {
      // Complete an existing connection
      this.completeConnection(stepId, port, event);
    }
  }

  startConnection(stepId: string, port: string, event: Event) {
    if (port === 'input') {
      // Can't start connection from input port
      return;
    }
    
    const step = this.steps().find(s => s.id === stepId);
    if (!step) return;
    
    const rect = this.canvasRef.nativeElement.getBoundingClientRect();
    const x = step.x + (port === 'output' ? step.width : 0);
    const y = step.y + 25;
    
    this.connectingFrom = { stepId, port, x, y };
    this.isConnecting = true;
    this.tempConnection.set({ x1: x, y1: y, x2: x, y2: y });
  }

  completeConnection(stepId: string, port: string, event: Event) {
    if (!this.connectingFrom || this.connectingFrom.stepId === stepId) {
      this.cancelConnection();
      return;
    }
    
    if (port === 'output') {
      // Can't connect to output port
      this.cancelConnection();
      return;
    }
    
    // Check if connection already exists
    const existingConnection = this.connections().find(conn => 
      conn.fromStepId === this.connectingFrom!.stepId && conn.toStepId === stepId
    );
    
    if (existingConnection) {
      this.cancelConnection();
      return;
    }

    const connectionId = `conn-${this.connectionIdCounter++}`;
    const newConnection: Connection = {
      id: connectionId,
      fromStepId: this.connectingFrom.stepId,
      toStepId: stepId,
      fromPort: this.connectingFrom.port,
      toPort: port
    };

    this.connections.update(connections => [...connections, newConnection]);
    this.cancelConnection();
  }

  cancelConnection() {
    this.connectingFrom = null;
    this.isConnecting = false;
    this.tempConnection.set(null);
  }

  deleteStep() {
    const step = this.selectedStep();
    if (!step) return;

    // Remove connections involving this step
    this.connections.update(connections => 
      connections.filter(conn => 
        conn.fromStepId !== step.id && conn.toStepId !== step.id
      )
    );

    // Remove the step
    this.steps.update(steps => steps.filter(s => s.id !== step.id));
    this.selectedStep.set(null);
  }

  getConnectionPath(connection: Connection): string {
    const fromStep = this.steps().find(s => s.id === connection.fromStepId);
    const toStep = this.steps().find(s => s.id === connection.toStepId);
    
    if (!fromStep || !toStep) return '';

    const x1 = fromStep.x + fromStep.width;
    const y1 = fromStep.y + 25;
    const x2 = toStep.x;
    const y2 = toStep.y + 25;

    const midX = (x1 + x2) / 2;
    
    return `M ${x1} ${y1} C ${midX} ${y1}, ${midX} ${y2}, ${x2} ${y2}`;
  }

  getStepIcon(type: string): string {
    const icons = {
      'start': 'play_circle',
      'end': 'stop_circle',
      'manual': 'person',
      'automatic': 'settings',
      'approval': 'check_circle',
      'condition': 'alt_route',
      'credit-check': 'account_balance',
      'kyc': 'verified_user',
      'risk-assessment': 'shield'
    };
    return icons[type as keyof typeof icons] || 'help';
  }

  hasStartStep(): boolean {
    return this.steps().some(step => step.type === 'start');
  }

  hasEndStep(): boolean {
    return this.steps().some(step => step.type === 'end');
  }

  saveWorkflow() {
    const workflowData = {
      steps: this.steps(),
      connections: this.connections()
    };
    
    console.log('Saving workflow:', workflowData);
    this.snackBar.open('Workflow saved successfully!', 'Close', { duration: 3000 });
  }

  clearCanvas() {
    this.steps.set([]);
    this.connections.set([]);
    this.selectedStep.set(null);
    this.stepIdCounter = 1;
    this.connectionIdCounter = 1;
  }

  deployWorkflow() {
    if (this.steps().length === 0) {
      this.snackBar.open('Cannot deploy empty workflow', 'Close', { duration: 3000 });
      return;
    }

    const workflowData = {
      name: 'Custom Workflow',
      steps: this.steps(),
      connections: this.connections(),
      createdAt: new Date().toISOString()
    };
    
    console.log('Deploying workflow:', workflowData);
    this.snackBar.open('Workflow deployed successfully!', 'Close', { duration: 3000 });
  }
}
