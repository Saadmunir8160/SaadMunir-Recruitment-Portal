import { Component, Inject, CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';

@Component({
  selector: 'app-confirmation-dialog',
  standalone: true,
  imports: [CommonModule, MatDialogModule, MatButtonModule],
  schemas: [CUSTOM_ELEMENTS_SCHEMA],
  template: `
    <div class="confirmation-dialog">
      <!-- Header -->
      <div class="card-header bg-body border-0 rounded-top-4">
        <h5 class="card-title mb-0 fw-medium text-dark">
          <iconify-icon icon="solar:shield-check-broken" class="text-primary me-2"></iconify-icon>
          Confirm Payment
        </h5>
      </div>
      
      <!-- Content -->
      <div class="card-body">
        <div class="text-center py-2">
          <div class="mb-2">
            <iconify-icon icon="solar:question-circle-broken" class="text-warning" style="font-size: 2.5rem;"></iconify-icon>
          </div>
          <h6 class="fw-medium text-dark mb-1">Payment Confirmation Required</h6>
          <p class="text-muted mb-0">Are you sure you want to confirm this payment?</p>
          <p class="text-muted small mb-0">This action cannot be undone.</p>
        </div>
      </div>
      
      <!-- Actions -->
      <div class="card-footer d-flex justify-content-end gap-3 bg-light-subtle border-0 rounded-bottom-4">
        <button mat-button mat-dialog-close class="btn btn-outline-secondary">
          <iconify-icon icon="solar:close-circle-broken" class="me-2"></iconify-icon>
          Cancel
        </button>
        <button mat-button [mat-dialog-close]="true" class="btn btn-primary">
          <iconify-icon icon="solar:check-circle-broken" class="me-2"></iconify-icon>
          Confirm Payment
        </button>
      </div>
    </div>
  `,
  styles: [`
    .confirmation-dialog {
      min-width: 450px;
      border-radius: 16px;
      overflow: hidden;
      box-shadow: 0 10px 30px rgba(0, 0, 0, 0.1);
    }
    
    .card-header {
      padding: 1rem 1.5rem;
      border-bottom: 1px solid #e9ecef;
    }
    
    .card-body {
      padding: 1.25rem;
    }
    
    .card-footer {
      padding: 1rem 1.5rem;
      border-top: 1px solid #e9ecef;
    }
    
    .btn {
      padding: 0.5rem 1.5rem;
      border-radius: 8px;
      font-weight: 500;
      font-size: 0.875rem;
      border: 2px solid transparent;
      transition: all 0.3s ease;
      display: inline-flex;
      align-items: center;
      justify-content: center;
      min-height: 40px;
      text-decoration: none;
      cursor: pointer;
    }
    
    .btn-primary {
      background-color: #ff6c2f !important;
      border-color: #ff6c2f !important;
      color: white !important;
      box-shadow: 0 2px 4px rgba(255, 108, 47, 0.2);
    }
    
    .btn-primary:hover {
      background-color: #e65c1a !important;
      border-color: #e65c1a !important;
      color: white !important;
      transform: translateY(-2px);
      box-shadow: 0 4px 12px rgba(255, 108, 47, 0.3);
    }
    
    .btn-primary:active {
      transform: translateY(0);
      box-shadow: 0 2px 4px rgba(255, 108, 47, 0.2);
      color: white !important;
    }
    
    /* Ensure text is always white for primary button */
    .btn.btn-primary,
    .btn.btn-primary:hover,
    .btn.btn-primary:active,
    .btn.btn-primary:focus {
      color: white !important;
    }
    
    .btn-outline-secondary {
      background-color: transparent;
      border-color: #6c757d;
      color: #6c757d;
    }
    
    .btn-outline-secondary:hover {
      background-color: #6c757d;
      border-color: #6c757d;
      color: white;
      transform: translateY(-2px);
      box-shadow: 0 4px 12px rgba(108, 117, 125, 0.3);
    }
    
    .btn-outline-secondary:active {
      transform: translateY(0);
    }
    
    .text-primary {
      color: #ff6c2f !important;
    }
    
    .text-warning {
      color: #ffc107 !important;
    }
    
    .text-dark {
      color: #212529 !important;
    }
    
    .text-muted {
      color: #6c757d !important;
    }
    
    .bg-light-subtle {
      background-color: #f8f9fa !important;
    }
    
    .fw-medium {
      font-weight: 500 !important;
    }
    
    .me-2 {
      margin-right: 0.5rem !important;
    }
    
    .mb-0 {
      margin-bottom: 0 !important;
    }
    
    .mb-1 {
      margin-bottom: 0.25rem !important;
    }
    
    .mb-2 {
      margin-bottom: 0.5rem !important;
    }
    
    .py-2 {
      padding-top: 0.5rem !important;
      padding-bottom: 0.5rem !important;
    }
    
    .gap-3 {
      gap: 1rem !important;
    }
    
    .small {
      font-size: 0.875em !important;
    }
    
    .text-center {
      text-align: center !important;
    }
    
    .d-flex {
      display: flex !important;
    }
    
    .justify-content-end {
      justify-content: flex-end !important;
    }
    
    .align-items-center {
      align-items: center !important;
    }
    
    .rounded-top-4 {
      border-top-left-radius: 1rem !important;
      border-top-right-radius: 1rem !important;
    }
    
    .rounded-bottom-4 {
      border-bottom-left-radius: 1rem !important;
      border-bottom-right-radius: 1rem !important;
    }
    
    .border-0 {
      border: 0 !important;
    }
  `]
})
export class ConfirmationDialogComponent {
  constructor(
    public dialogRef: MatDialogRef<ConfirmationDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: any
  ) {}
} 