import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { TranslateModule } from '@ngx-translate/core';

@Component({
  selector: 'app-clear-cart-dialog',
  standalone: true,
  imports: [CommonModule, MatDialogModule, TranslateModule],
  template: `
    <div class="clear-cart-dialog">
      <div class="dialog-header">
        <div class="icon-wrapper">
          <i class="material-icons">warning</i>
        </div>
        <h2 mat-dialog-title>{{ 'DEALER.SHOP.CART.CLEAR_CONFIRMATION.TITLE' | translate }}</h2>
      </div>
      
        <p class="dialog-message mb-3">
          {{ 'DEALER.SHOP.CART.CLEAR_CONFIRMATION.MESSAGE' | translate }}
        </p>
      
      <mat-dialog-actions align="end">
        <button class="btn btn-outline-secondary" (click)="onCancel()">
          {{ 'DEALER.SHOP.CART.CLEAR_CONFIRMATION.CANCEL' | translate }}
        </button>
        <button class="btn btn-danger" (click)="onConfirm()">
          <i class="material-icons">delete</i>
          {{ 'DEALER.SHOP.CART.CLEAR_CONFIRMATION.CONFIRM' | translate }}
        </button>
      </mat-dialog-actions>
    </div>
  `,
  styles: [`
    .clear-cart-dialog {
      padding: 8px;
    }

    .dialog-header {
      display: flex;
      flex-direction: column;
      align-items: center;
      margin-bottom: 24px;
    }

    .icon-wrapper {
      width: 64px;
      height: 64px;
      border-radius: 50%;
      background: linear-gradient(135deg, #fee2e2 0%, #fecaca 100%);
      display: flex;
      align-items: center;
      justify-content: center;
      margin-bottom: 16px;
    }

    .icon-wrapper .material-icons {
      font-size: 36px;
      color: #dc2626;
    }

    h2 {
      margin: 0;
      font-size: 24px;
      font-weight: 600;
      color: #1f2937;
      text-align: center;
    }

    mat-dialog-content {
      padding: 0 24px;
      margin-bottom: 24px;
    }

    .dialog-message {
      font-size: 16px;
      color: #6b7280;
      text-align: center;
      line-height: 1.6;
      margin: 0;
    }

    mat-dialog-actions {
      padding: 0 24px 16px;
      gap: 12px;
      display: flex;
      justify-content: flex-end;
    }

    .btn {
      padding: 10px 24px;
      border-radius: 8px;
      font-size: 14px;
      font-weight: 500;
      display: flex;
      align-items: center;
      gap: 8px;
      cursor: pointer;
      transition: all 0.2s;
      border: none;
    }

    .btn-outline-secondary {
      background: white;
      color: #6b7280;
      border: 1px solid #d1d5db;
    }

    .btn-outline-secondary:hover {
      background: #f9fafb;
      border-color: #9ca3af;
    }

    .btn-danger {
      background: #dc2626;
      color: white;
    }

    .btn-danger:hover {
      background: #b91c1c;
    }

    .btn .material-icons {
      font-size: 18px;
    }

    :host ::ng-deep .mat-mdc-dialog-container {
      border-radius: 16px !important;
      padding: 0 !important;
    }

    :host ::ng-deep .mat-mdc-dialog-surface {
      border-radius: 16px !important;
    }
  `]
})
export class ClearCartDialogComponent {
  constructor(private dialogRef: MatDialogRef<ClearCartDialogComponent>) {}

  onCancel(): void {
    this.dialogRef.close(false);
  }

  onConfirm(): void {
    this.dialogRef.close(true);
  }
}
