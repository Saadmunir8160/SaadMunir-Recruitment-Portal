import { Component, Inject, CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MAT_SNACK_BAR_DATA, MatSnackBarRef } from '@angular/material/snack-bar';

@Component({
  selector: 'app-success-snackbar',
  standalone: true,
  imports: [CommonModule],
  schemas: [CUSTOM_ELEMENTS_SCHEMA],
  template: `
    <div class="success-snackbar">
      <div class="content">
        <div class="icon">
          <iconify-icon icon="solar:check-circle-bold"></iconify-icon>
        </div>
        <div class="message">
          <h4>Success!</h4>
          <p>{{ data.message }}</p>
        </div>
      </div>
      <button class="close-button" (click)="snackBarRef.dismiss()">
        <iconify-icon icon="solar:close-circle-bold"></iconify-icon>
      </button>
    </div>
  `,
  styles: [`
    .success-snackbar {
      display: flex;
      align-items: center;
      justify-content: space-between;
      background: white;
      color: #1a365d;
      padding: 16px 24px;
      border-radius: 12px;
      box-shadow: 0 8px 32px rgba(0, 0, 0, 0.12);
      border-left: 4px solid #1c6e33;
      min-width: 400px;
    }

    .content {
      display: flex;
      align-items: center;
      gap: 16px;
    }

    .icon {
      display: flex;
      align-items: center;
      justify-content: center;
      width: 40px;
      height: 40px;
      background: #dbeafe;
      border-radius: 50%;
      color:#2ceb25;
    }

    .icon iconify-icon {
      font-size: 24px;
    }

    .message {
      display: flex;
      flex-direction: column;
      gap: 4px;
    }

    .message h4 {
      margin: 0;
      font-size: 16px;
      font-weight: 600;
      color: #1eaf36;
    }

    .message p {
      margin: 0;
      font-size: 14px;
      color: #475569;
    }

    .close-button {
      background: none;
      border: none;
      padding: 4px;
      cursor: pointer;
      color: #64748b;
      display: flex;
      align-items: center;
      justify-content: center;
      border-radius: 50%;
      transition: all 0.2s ease;
    }

    .close-button:hover {
      background: #f1f5f9;
      color: #475569;
    }

    .close-button iconify-icon {
      font-size: 20px;
    }
  `]
})
export class SuccessSnackbarComponent {
  constructor(
    @Inject(MAT_SNACK_BAR_DATA) public data: { message: string },
    public snackBarRef: MatSnackBarRef<SuccessSnackbarComponent>
  ) {}
} 