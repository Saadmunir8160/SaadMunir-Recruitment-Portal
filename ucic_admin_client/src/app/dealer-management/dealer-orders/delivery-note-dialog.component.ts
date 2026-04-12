import { Component, Inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { DealerOrder, DealerOrderItem, DealerManagementService, DeliveryDTO } from '../services/dealer-management.service';

@Component({
  selector: 'app-delivery-note-dialog',
  standalone: true,
  imports: [
    CommonModule,
    MatDialogModule,
    MatButtonModule
  ],
  templateUrl: './delivery-note-dialog.component.html',
  styleUrls: ['./delivery-note-dialog.component.scss']
})
export class DeliveryNoteDialogComponent {
  order!: DealerOrder;
  deliveryData: DeliveryDTO | null = null;

  constructor(
    public dialogRef: MatDialogRef<DeliveryNoteDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: { order: DealerOrder },
    private dealerService: DealerManagementService
  ) {
    this.order = data.order;
  }

  ngOnInit() {
    if (this.order && this.order.ln_OrderNumber) {
      this.dealerService.getDeliveryByLnOrderNumber(this.order.ln_OrderNumber).subscribe({
        next: (response) => {
          if (response && response.data) {
            this.deliveryData = response.data;
          }
        },
        error: (err) => {
          console.error('Error fetching delivery note data:', err);
        }
      });
    }
  }

  closeDialog(): void {
    this.dialogRef.close();
  }

  // Get customer order number
  getCustomerOrder(): string {
    return this.deliveryData?.customerOrder || '';
  }

  // Get vehicle number/name
  getVehicleNo(): string {
    return this.deliveryData?.car || '';
  }

  // Get shipment number
  getShipmentNo(): string {
    return this.deliveryData?.shipment || '';
  }

  // Get date formatted
  getDate(): string {
    const dateVal = this.deliveryData?.dateOut;
    if (!dateVal) return '';
    const date = new Date(dateVal);
    if (isNaN(date.getTime())) return '';

    return date.toLocaleString('en-US', {
      year: 'numeric',
      month: '2-digit',
      day: '2-digit',
      hour: '2-digit',
      minute: '2-digit',
      hour12: false
    }).replace(',', '');
  }

  // Get customer name
  getCustomerName(): string {
    return this.deliveryData?.customerName || '';
  }

  // Get loading order number
  getLoadingOrderNo(): string {
    return this.deliveryData?.lnOrderNumber || '';
  }

  // Get transporter name
  getTransporterName(): string {
    return this.deliveryData?.transporterName || '';
  }

  // Get Iqama number
  getIqamaNo(): string {
    return this.deliveryData?.iqn || '';
  }

  // Get driver name
  getDriverName(): string {
    return this.deliveryData?.driverName || '';
  }

  // Get net weight
  getNetWeight(): string {
    return this.deliveryData?.quantityShipped ? this.deliveryData.quantityShipped.toFixed(4) : '';
  }

  // Get gross weight
  getGrossWeight(): string {
    return this.deliveryData?.weightOut ? this.deliveryData.weightOut.toFixed(4) : '';
  }

  // Get empty weight
  getEmptyWeight(): string {
    return this.deliveryData?.weightIN ? this.deliveryData.weightIN.toFixed(4) : '';
  }

  // Get delivery area
  getDeliveryArea(): string {
    return this.deliveryData?.areaDescription || '';
  }

  // Get type of cement
  getCementType(): string {
    return this.deliveryData?.itemDescription || '';
  }

  // Get warehouse
  getWarehouse(): string {
    return this.deliveryData?.warehouseDescription || '';
  }

  // Get entrance time
  getEntranceTime(): string {
    if (this.deliveryData?.dateIN) {
      const date = new Date(this.deliveryData.dateIN);
      if (isNaN(date.getTime())) return '';

      return date.toLocaleString('en-US', {
        year: 'numeric',
        month: '2-digit',
        day: '2-digit',
        hour: '2-digit',
        minute: '2-digit',
        hour12: false
      }).replace(',', '');
    }
    return '';
  }

  // Get exit time
  getExitTime(): string {
    // Usually same as Date Out
    return this.getDate();
  }

  // Get dispatch clerk
  getDispatchClerk(): string {
    return this.deliveryData?.createdBy || '';
  }

  // Print functionality
  printDeliveryNote(): void {
    window.print();
  }
}


