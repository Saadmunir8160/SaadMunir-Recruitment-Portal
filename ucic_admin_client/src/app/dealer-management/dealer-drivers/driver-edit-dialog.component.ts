import { Component, Inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogRef, MatDialogModule } from '@angular/material/dialog';
import { DealerDriver, DealerManagementService } from '../services/dealer-management.service';

@Component({
  selector: 'app-driver-edit-dialog',
  standalone: true,
  imports: [CommonModule, FormsModule, MatDialogModule],
  template: `
    <div class="dialog-container">
      <div class="dialog-header">
        <h2 mat-dialog-title>
          <i class="material-icons">edit</i>
          Edit Driver
        </h2>
        <button class="btn-close" (click)="onClose()">
          <i class="material-icons">close</i>
        </button>
      </div>

      <mat-dialog-content class="dialog-content">
        <div class="form-container">
          <div class="form-group">
            <label>Driver Name</label>
            <input 
              type="text" 
              class="form-control" 
              [(ngModel)]="editedDriver.driverName"
              placeholder="Enter driver name"
            />
          </div>

          <div class="form-group">
            <label>LN ID</label>
            <input 
              type="text" 
              class="form-control" 
              [(ngModel)]="editedDriver.ln_ID"
              placeholder="Enter LN ID"
            />
          </div>

          <div class="form-group">
            <label>Iqama Number</label>
            <input 
              type="text" 
              class="form-control" 
              [(ngModel)]="editedDriver.iqamaNumber"
              placeholder="Enter Iqama number"
            />
          </div>

          <div class="form-group">
            <label>Phone Number</label>
            <input 
              type="tel" 
              class="form-control" 
              [(ngModel)]="editedDriver.phoneNumber"
              placeholder="Enter phone number"
            />
          </div>

          <div class="form-group">
            <label>Email</label>
            <input 
              type="email" 
              class="form-control" 
              [(ngModel)]="editedDriver.email"
              placeholder="Enter email address"
            />
          </div>

          <div class="form-group">
            <label class="toggle-label">
              <span>Status</span>
              <div class="toggle-container">
                <label class="switch">
                  <input 
                    type="checkbox" 
                    [(ngModel)]="editedDriver.isActive"
                  />
                  <span class="slider"></span>
                </label>
                <span class="status-text" [class.active]="editedDriver.isActive">
                  {{ editedDriver.isActive ? 'Active' : 'Inactive' }}
                </span>
              </div>
            </label>
          </div>
        </div>
      </mat-dialog-content>

      <mat-dialog-actions class="dialog-actions">
        <button class="btn btn-secondary" (click)="onClose()">Cancel</button>
        <button class="btn btn-primary" (click)="onSave()">
          <i class="material-icons">save</i>
          Save Changes
        </button>
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

    .toggle-label {
      display: flex;
      justify-content: space-between;
      align-items: center;
    }

    .toggle-container {
      display: flex;
      align-items: center;
      gap: 12px;
    }

    .switch {
      position: relative;
      display: inline-block;
      width: 50px;
      height: 24px;
    }

    .switch input {
      opacity: 0;
      width: 0;
      height: 0;
    }

    .slider {
      position: absolute;
      cursor: pointer;
      top: 0;
      left: 0;
      right: 0;
      bottom: 0;
      background-color: #ccc;
      transition: .4s;
      border-radius: 24px;
    }

    .slider:before {
      position: absolute;
      content: "";
      height: 18px;
      width: 18px;
      left: 3px;
      bottom: 3px;
      background-color: white;
      transition: .4s;
      border-radius: 50%;
    }

    input:checked + .slider {
      background-color: #28a745;
    }

    input:checked + .slider:before {
      transform: translateX(26px);
    }

    .status-text {
      font-size: 0.875rem;
      font-weight: 600;
      color: #dc3545;
    }

    .status-text.active {
      color: #28a745;
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

    .btn .material-icons {
      font-size: 18px;
    }
  `]
})
export class DriverEditDialogComponent implements OnInit {
  editedDriver: DealerDriver;

  constructor(
    public dialogRef: MatDialogRef<DriverEditDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public driver: DealerDriver,
    private dealerService: DealerManagementService
  ) {
    this.editedDriver = { ...driver };
  }

  ngOnInit(): void {}

  onClose(): void {
    this.dialogRef.close();
  }

  onSave(): void {
    // Prepare the update payload matching the backend DTO
    const updatePayload = {
      driverName: this.editedDriver.driverName,
      ln_ID: this.editedDriver.ln_ID,
      iqamaNumber: this.editedDriver.iqamaNumber,
      phoneNumber: this.editedDriver.phoneNumber,
      email: this.editedDriver.email,
      isActive: this.editedDriver.isActive
    };

    // Call the update API
    this.dealerService.updateDealerDriver(this.editedDriver.driverID, updatePayload).subscribe({
      next: (response) => {
        if (response.success) {
          this.dialogRef.close(true); // Close and signal success
        } else {
          alert(`Failed to update driver: ${response.message}`);
        }
      },
      error: (error) => {
        console.error('Error updating driver:', error);
        alert('An error occurred while updating the driver.');
      }
    });
  }
}
