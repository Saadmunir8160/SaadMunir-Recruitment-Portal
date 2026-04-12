import { Component, Inject, CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { DealerOrder, DealerOrderItem } from '../services/dealer-management.service';

@Component({
  selector: 'app-dealer-order-details-dialog',
  standalone: true,
  imports: [
    CommonModule,
    MatDialogModule,
    MatButtonModule,
    TranslateModule
  ],
  schemas: [CUSTOM_ELEMENTS_SCHEMA],
  templateUrl: './dealer-order-details-dialog.component.html',
  styleUrls: ['./dealer-order-details-dialog.component.scss']
})
export class DealerOrderDetailsDialogComponent {
  orderDetails: DealerOrder;
  loading: boolean = false;

  constructor(
    public dialogRef: MatDialogRef<DealerOrderDetailsDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: { orderDetails: DealerOrder; loading?: boolean },
    private translate: TranslateService
  ) {
    this.orderDetails = data.orderDetails;
    this.loading = data.loading || false;
  }

  closeDialog(): void {
    this.dialogRef.close();
  }

  getStatusClass(status: string): string {
    switch (status?.toLowerCase()) {
      case 'pending':
        return 'badge-warning';
      case 'processing':
        return 'badge-info';
      case 'shipped':
        return 'badge-primary';
      case 'delivered':
      case 'confirmed':
        return 'badge-success';
      case 'cancelled':
        return 'badge-danger';
      default:
        return 'badge-secondary';
    }
  }

  formatDate(date: Date | string): string {
    if (!date) return 'N/A';
    return new Date(date).toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
  }

  formatCurrency(amount: number): string {
    if (!amount && amount !== 0) return '$0.00';
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: 'USD'
    }).format(amount);
  }

  getTotalItemsQuantity(): number {
    if (!this.orderDetails?.orderItems) return 0;
    return this.orderDetails.orderItems.reduce((sum: number, item: DealerOrderItem) => {
      return sum + item.quantity;
    }, 0);
  }

  getTotalItemsAmount(): number {
    if (!this.orderDetails?.orderItems) return 0;
    return this.orderDetails.orderItems.reduce((sum: number, item: DealerOrderItem) => {
      return sum + item.totalPrice;
    }, 0);
  }
}
