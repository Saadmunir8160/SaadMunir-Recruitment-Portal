import { Component, Inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MAT_DIALOG_DATA, MatDialogRef, MatDialogModule } from '@angular/material/dialog';
import { DealerVehicle } from '../services/dealer-management.service';

@Component({
  selector: 'app-vehicle-view-dialog',
  standalone: true,
  imports: [CommonModule, MatDialogModule],
  template: `
    <div class="dialog-container">
      <div class="dialog-header">
        <h2 mat-dialog-title>
          <i class="material-icons">local_shipping</i>
          Vehicle Details
        </h2>
        <button class="btn-close" (click)="onClose()">
          <i class="material-icons">close</i>
        </button>
      </div>

      <mat-dialog-content class="dialog-content">
        <div class="vehicle-details">
          <!-- Vehicle Information Card -->
          <div class="info-card">
            <h3 class="card-title">
              <i class="material-icons">directions_car</i>
              Vehicle Information
            </h3>
            <div class="info-grid">
              <div class="info-item">
                <label>Vehicle ID:</label>
                <span class="value">#{{ vehicle.vehicleID }}</span>
              </div>
              <div class="info-item">
                <label>Vehicle Name:</label>
                <span class="value">{{ vehicle.vehicleName }}</span>
              </div>
              <div class="info-item">
                <label>License Plate:</label>
                <span class="value">{{ vehicle.licensePlate }}</span>
              </div>
              <div class="info-item">
                <label>Status:</label>
                <span class="badge" [ngClass]="vehicle.isActive ? 'badge-success' : 'badge-danger'">
                  {{ vehicle.isActive ? 'Active' : 'Inactive' }}
                </span>
              </div>
            </div>
          </div>

          <!-- Vehicle Specifications Card -->
          <div class="info-card">
            <h3 class="card-title">
              <i class="material-icons">settings</i>
              Specifications
            </h3>
            <div class="info-grid">
              <div class="info-item">
                <label>Vehicle Type:</label>
                <span class="badge" [ngClass]="getVehicleTypeClass()">
                  {{ vehicle.vehicleType }}
                </span>
              </div>
              <div class="info-item">
                <label>Capacity:</label>
                <span class="value">
                  <i class="material-icons">inventory</i>
                  {{ vehicle.capacity }} units
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
                <span class="value">{{ vehicle.dealerName || 'N/A' }}</span>
              </div>
              <div class="info-item">
                <label>Dealer ID:</label>
                <span class="value">#{{ vehicle.dealerID }}</span>
              </div>
            </div>
          </div>

          <!-- Driver Assignment Card -->
          <div class="info-card">
            <h3 class="card-title">
              <i class="material-icons">person</i>
              Driver Assignment
            </h3>
            <div class="info-grid">
              <div class="info-item">
                <label>Assigned Driver:</label>
                <span class="value" *ngIf="vehicle.driverID && vehicle.driverName; else noDriver">
                  {{ vehicle.driverName }}
                </span>
                <ng-template #noDriver>
                  <span class="text-muted">
                    <i class="material-icons">person_off</i>
                    No Driver Assigned
                  </span>
                </ng-template>
              </div>
              <div class="info-item" *ngIf="vehicle.driverID">
                <label>Driver ID:</label>
                <span class="value">#{{ vehicle.driverID }}</span>
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
                <span class="value">{{ formatDate(vehicle.createdDate) }}</span>
              </div>
              <div class="info-item">
                <label>Modified Date:</label>
                <span class="value">{{ formatDate(vehicle.modifiedDate) }}</span>
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

    .vehicle-details {
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

    .text-muted {
      color: #6c757d;
      display: flex;
      align-items: center;
      gap: 6px;
    }

    .text-muted .material-icons {
      font-size: 18px;
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

    .badge-primary {
      background-color: #cfe2ff;
      color: #084298;
    }

    .badge-info {
      background-color: #cff4fc;
      color: #055160;
    }

    .badge-warning {
      background-color: #fff3cd;
      color: #664d03;
    }

    .badge-secondary {
      background-color: #e2e3e5;
      color: #41464b;
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
export class VehicleViewDialogComponent {
  constructor(
    public dialogRef: MatDialogRef<VehicleViewDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public vehicle: DealerVehicle
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

  getVehicleTypeClass(): string {
    switch (this.vehicle.vehicleType?.toLowerCase()) {
      case 'truck':
        return 'badge-primary';
      case 'van':
        return 'badge-info';
      case 'car':
        return 'badge-success';
      case 'motorcycle':
        return 'badge-warning';
      default:
        return 'badge-secondary';
    }
  }
}
