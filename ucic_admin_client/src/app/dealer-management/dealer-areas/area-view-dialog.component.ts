import { Component, Inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MAT_DIALOG_DATA, MatDialogRef, MatDialogModule } from '@angular/material/dialog';
import { DealerArea } from '../services/dealer-management.service';

@Component({
  selector: 'app-area-view-dialog',
  standalone: true,
  imports: [CommonModule, MatDialogModule],
  template: `
    <div class="dialog-container">
      <div class="dialog-header">
        <h2 mat-dialog-title>
          <i class="fas fa-map-marked-alt"></i>
          Area Details
        </h2>
        <button class="btn-close" (click)="onClose()">
          <i class="fas fa-times"></i>
        </button>
      </div>

      <mat-dialog-content class="dialog-content">
        <div class="details-grid">
          <div class="detail-item">
            <label>Area ID</label>
            <span>#{{ area.areaID }}</span>
          </div>

          <div class="detail-item">
            <label>Area Name</label>
            <span>{{ area.areaName }}</span>
          </div>

          <div class="detail-item">
            <label>Area Code (LN ID)</label>
            <span class="code-badge">{{ area.areaCode }}</span>
          </div>

          <div class="detail-item">
            <label>Status</label>
            <span class="badge" [class.badge-success]="area.isActive" [class.badge-danger]="!area.isActive">
              {{ area.isActive ? 'Active' : 'Inactive' }}
            </span>
          </div>

          <div class="detail-item">
            <label>Created Date</label>
            <span>{{ area.createdDate | date:'medium' }}</span>
          </div>

          <div class="detail-item">
            <label>Last Modified</label>
            <span>{{ area.modifiedDate | date:'medium' }}</span>
          </div>
        </div>
      </mat-dialog-content>

      <mat-dialog-actions class="dialog-actions">
        <button class="btn btn-secondary" (click)="onClose()">Close</button>
      </mat-dialog-actions>
    </div>
  `,
  styles: [`
    .dialog-container {
      width: 100%;
      max-width: 600px;
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
    }

    .details-grid {
      display: grid;
      grid-template-columns: repeat(2, 1fr);
      gap: 20px;
    }

    .detail-item {
      display: flex;
      flex-direction: column;
      gap: 8px;
    }

    .detail-item label {
      font-size: 0.875rem;
      font-weight: 600;
      color: #495057;
      text-transform: uppercase;
      letter-spacing: 0.5px;
    }

    .detail-item span {
      font-size: 1rem;
      color: #212529;
    }

    .code-badge {
      background: #e3f2fd;
      color: #1976d2;
      padding: 4px 12px;
      border-radius: 12px;
      font-size: 0.875rem;
      font-family: monospace;
      display: inline-block;
      width: fit-content;
    }

    .badge {
      padding: 4px 12px;
      border-radius: 12px;
      font-size: 0.875rem;
      font-weight: 600;
      display: inline-block;
      width: fit-content;
    }

    .badge-success {
      background: #d4edda;
      color: #155724;
    }

    .badge-danger {
      background: #f8d7da;
      color: #721c24;
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
    }

    .btn-secondary {
      background-color: #6c757d;
      color: white;
    }

    .btn-secondary:hover {
      background-color: #5a6268;
    }

    @media (max-width: 768px) {
      .details-grid {
        grid-template-columns: 1fr;
      }
    }
  `]
})
export class AreaViewDialogComponent {
  constructor(
    public dialogRef: MatDialogRef<AreaViewDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public area: DealerArea
  ) {}

  onClose(): void {
    this.dialogRef.close();
  }
}
