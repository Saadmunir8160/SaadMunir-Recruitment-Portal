import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatDialogRef, MatDialogModule } from '@angular/material/dialog';
import { DealerManagementService } from '../services/dealer-management.service';

@Component({
  selector: 'app-area-create-dialog',
  standalone: true,
  imports: [CommonModule, FormsModule, MatDialogModule],
  template: `
    <div class="dialog-container">
      <div class="dialog-header">
        <h2 mat-dialog-title>
          <i class="fas fa-plus-circle"></i>
          Create New Area
        </h2>
        <button class="btn-close" (click)="onClose()">
          <i class="fas fa-times"></i>
        </button>
      </div>

      <mat-dialog-content class="dialog-content">
        <div class="form-container">
          <div class="form-group">
            <label>Area Name *</label>
            <input 
              type="text" 
              class="form-control" 
              [(ngModel)]="areaName"
              placeholder="Enter area name"
              required
            />
          </div>

          <div class="form-group">
            <label>Area Code (LN ID) *</label>
            <input 
              type="text" 
              class="form-control" 
              [(ngModel)]="areaCode"
              placeholder="Enter area code / LN ID"
              required
            />
            <small class="form-text">This code is used in LN system integration</small>
          </div>
        </div>
      </mat-dialog-content>

      <mat-dialog-actions class="dialog-actions">
        <button class="btn btn-secondary" (click)="onClose()">Cancel</button>
        <button class="btn btn-primary" (click)="onSave()" [disabled]="!isValid()">
          <i class="fas fa-save"></i>
          Create Area
        </button>
      </mat-dialog-actions>
    </div>
  `,
  styles: [`
    .dialog-container {
      width: 100%;
      max-width: 500px;
    }

    .dialog-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding: 20px 24px;
      background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
      color: white;
      margin: -24px -24px 0 -24px;
      border-radius: 4px 4px 0 0;
    }

    .dialog-header h2 {
      margin: 0;
      display: flex;
      align-items: center;
      gap: 10px;
      font-size: 1.5rem;
      font-weight: 600;
    }

    .btn-close {
      background: rgba(255, 255, 255, 0.2);
      border: none;
      color: white;
      width: 32px;
      height: 32px;
      border-radius: 50%;
      display: flex;
      align-items: center;
      justify-content: center;
      cursor: pointer;
      transition: background 0.3s;
    }

    .btn-close:hover {
      background: rgba(255, 255, 255, 0.3);
    }

    .dialog-content {
      padding: 24px;
      max-height: 70vh;
      overflow-y: auto;
    }

    .form-container {
      display: flex;
      flex-direction: column;
      gap: 20px;
    }

    .form-group {
      display: flex;
      flex-direction: column;
      gap: 8px;
    }

    .form-group label {
      font-size: 0.875rem;
      font-weight: 600;
      color: #495057;
      text-transform: uppercase;
      letter-spacing: 0.5px;
    }

    .form-control {
      padding: 10px 12px;
      border: 1px solid #ced4da;
      border-radius: 4px;
      font-size: 1rem;
      transition: border-color 0.3s;
    }

    .form-control:focus {
      outline: none;
      border-color: #667eea;
      box-shadow: 0 0 0 3px rgba(102, 126, 234, 0.1);
    }

    .form-text {
      font-size: 0.75rem;
      color: #6c757d;
      margin-top: 4px;
    }

    .dialog-actions {
      padding: 16px 24px;
      border-top: 1px solid #e9ecef;
      display: flex;
      justify-content: flex-end;
      gap: 12px;
    }

    .btn {
      padding: 10px 24px;
      border-radius: 4px;
      font-weight: 500;
      cursor: pointer;
      transition: all 0.3s;
      border: none;
      display: flex;
      align-items: center;
      gap: 8px;
    }

    .btn-secondary {
      background-color: #6c757d;
      color: white;
    }

    .btn-secondary:hover {
      background-color: #5a6268;
    }

    .btn-primary {
      background-color: #667eea;
      color: white;
    }

    .btn-primary:hover {
      background-color: #5568d3;
    }

    .btn:disabled {
      opacity: 0.5;
      cursor: not-allowed;
    }
  `]
})
export class AreaCreateDialogComponent {
  areaName = '';
  areaCode = '';

  constructor(
    public dialogRef: MatDialogRef<AreaCreateDialogComponent>,
    private dealerService: DealerManagementService
  ) {}

  isValid(): boolean {
    return this.areaName.trim() !== '' && this.areaCode.trim() !== '';
  }

  onClose(): void {
    this.dialogRef.close();
  }

  onSave(): void {
    if (!this.isValid()) {
      return;
    }

    const payload = {
      areaName: this.areaName,
      areaCode: this.areaCode
    };

    this.dealerService.createDealerArea(payload).subscribe({
      next: (response) => {
        if (response.success) {
          alert('Area created successfully!');
          this.dialogRef.close(true);
        } else {
          alert(`Failed to create area: ${response.message}`);
        }
      },
      error: (error) => {
        console.error('Error creating area:', error);
        alert('An error occurred while creating the area.');
      }
    });
  }
}
