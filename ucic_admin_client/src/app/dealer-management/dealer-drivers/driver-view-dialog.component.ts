import { Component, Inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MAT_DIALOG_DATA, MatDialogRef, MatDialogModule } from '@angular/material/dialog';
import { DealerDriver } from '../services/dealer-management.service';

@Component({
  selector: 'app-driver-view-dialog',
  standalone: true,
  imports: [CommonModule, MatDialogModule],
  template: `
    <div class="dialog-container">
      <div class="dialog-header">
        <h2 mat-dialog-title>
          <i class="material-icons">person</i>
          Driver Details
        </h2>
        <button class="btn-close" (click)="onClose()">
          <i class="material-icons">close</i>
        </button>
      </div>

      <mat-dialog-content class="dialog-content">
        <div class="driver-details">
          <!-- Driver Information Card -->
          <div class="info-card">
            <h3 class="card-title">
              <i class="material-icons">badge</i>
              Driver Information
            </h3>
            <div class="info-grid">
              <div class="info-item">
                <label>ID:</label>
                <span class="value">#{{ driver.driverID }}</span>
              </div>
              <div class="info-item">
                <label>Driver Name:</label>
                <span class="value">{{ driver.driverName }}</span>
              </div>
              <div class="info-item">
                <label>LN ID:</label>
                <span class="value">{{ driver.ln_ID || 'N/A' }}</span>
              </div>
              <div class="info-item">
                <label>Driver ID:</label>
                <span class="value">{{ driver.iqamaNumber || 'N/A' }}</span>
              </div>
              <div class="info-item">
                <label>Status:</label>
                <span class="badge" [ngClass]="driver.isActive ? 'badge-success' : 'badge-danger'">
                  {{ driver.isActive ? 'Active' : 'Inactive' }}
                </span>
              </div>
            </div>
          </div>

          <!-- Contact Information Card -->
          <div class="info-card">
            <h3 class="card-title">
              <i class="material-icons">contact_phone</i>
              Contact Information
            </h3>
            <div class="info-grid">
              <div class="info-item">
                <label>Phone Number:</label>
                <span class="value">
                  <i class="material-icons">phone</i>
                  {{ driver.phoneNumber }}
                </span>
              </div>
              <div class="info-item" *ngIf="driver.email">
                <label>Email:</label>
                <span class="value">
                  <i class="material-icons">email</i>
                  <a [href]="'mailto:' + driver.email">{{ driver.email }}</a>
                </span>
              </div>
            </div>
          </div>

          <!-- Dealer Information Card -->
          <div class="info-card">
            <h3 class="card-title">
              <i class="material-icons">business</i>
              Dealer Information
            </h3>
            <div class="info-grid">
              <div class="info-item">
                <label>Dealer Name:</label>
                <span class="value">{{ driver.dealerName || 'N/A' }}</span>
              </div>
              <div class="info-item">
                <label>Dealer ID:</label>
                <span class="value">#{{ driver.dealerID }}</span>
              </div>
            </div>
          </div>

          <!-- Additional Details Card -->
          <div class="info-card">
            <h3 class="card-title">
              <i class="material-icons">info</i>
              Additional Details
            </h3>
            <div class="info-grid">
              <div class="info-item">
                <label>Created Date:</label>
                <span class="value">{{ formatDate(driver.createdDate) }}</span>
              </div>
              <div class="info-item">
                <label>Modified Date:</label>
                <span class="value">{{ formatDate(driver.modifiedDate) }}</span>
              </div>
            </div>
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
      max-width: 700px;
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

    .driver-details {
      display: flex;
      flex-direction: column;
      gap: 20px;
    }

    .info-card {
      background: #f8f9fa;
      border-radius: 8px;
      padding: 16px;
      border: 1px solid #e9ecef;
    }

    .card-title {
      display: flex;
      align-items: center;
      gap: 8px;
      font-size: 1rem;
      font-weight: 600;
      color: #495057;
      margin: 0 0 16px 0;
      padding-bottom: 12px;
      border-bottom: 2px solid #dee2e6;
    }

    .card-title .material-icons {
      font-size: 20px;
      color: #667eea;
    }

    .info-grid {
      display: grid;
      grid-template-columns: repeat(2, 1fr);
      gap: 16px;
    }

    .info-item {
      display: flex;
      flex-direction: column;
      gap: 4px;
    }

    .info-item label {
      font-size: 0.875rem;
      font-weight: 600;
      color: #6c757d;
      text-transform: uppercase;
      letter-spacing: 0.5px;
    }

    .info-item .value {
      font-size: 1rem;
      color: #212529;
      display: flex;
      align-items: center;
      gap: 6px;
    }

    .info-item .value .material-icons {
      font-size: 18px;
      color: #667eea;
    }

    .info-item .value a {
      color: #667eea;
      text-decoration: none;
    }

    .info-item .value a:hover {
      text-decoration: underline;
    }

    .badge {
      display: inline-block;
      padding: 4px 12px;
      border-radius: 12px;
      font-size: 0.875rem;
      font-weight: 600;
      text-align: center;
    }

    .badge-success {
      background-color: #d4edda;
      color: #155724;
    }

    .badge-danger {
      background-color: #f8d7da;
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
      padding: 8px 20px;
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
      .info-grid {
        grid-template-columns: 1fr;
      }
    }
  `]
})
export class DriverViewDialogComponent {
  constructor(
    public dialogRef: MatDialogRef<DriverViewDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public driver: DealerDriver
  ) {}

  onClose(): void {
    this.dialogRef.close();
  }

  formatDate(date: Date): string {
    return new Date(date).toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'long',
      day: 'numeric'
    });
  }
}
