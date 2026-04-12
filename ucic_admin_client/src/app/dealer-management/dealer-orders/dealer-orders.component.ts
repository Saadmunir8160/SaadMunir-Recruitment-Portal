import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import {
  DealerManagementService,
  DealerOrder
} from '../services/dealer-management.service';
import { DealerTranslationService } from '../../dealer/services/translation.service';
import { PaginatedResponse } from '../../shared/interfaces/paginated-response.interface';
import { DealerOrderDetailsDialogComponent } from './dealer-order-details-dialog.component';
import { DeliveryNoteDialogComponent } from './delivery-note-dialog.component';

@Component({
  selector: 'app-dealer-orders',
  standalone: true,
  imports: [CommonModule, FormsModule, MatSnackBarModule],
  templateUrl: './dealer-orders.component.html',
  styleUrls: ['./dealer-orders.component.scss']
})
export class DealerOrdersComponent implements OnInit {
  orders: DealerOrder[] = [];
  loading = false;
  error: string | null = null;
  resendingOrderId: number | null = null;

  // Pagination
  currentPage = 1;
  pageSize = 10;
  totalPages = 0;
  totalRecords = 0;

  // Expose Math to template
  Math = Math;

  // Filtering
  searchTerm = '';
  selectedDealerId: number | null = null;
  selectedStatus = '';
  startDate = '';
  endDate = '';

  // Status options
  statusOptions = [
    { value: '', label: 'All Statuses' },
    { value: 'Pending', label: 'Pending' },
    { value: 'Approved', label: 'Approved' },
    { value: 'In Process', label: 'In Process' },
    { value: 'Modified', label: 'Modified' },
    { value: 'Canceled', label: 'Canceled' },
    { value: 'Blocked', label: 'Blocked' },
    { value: 'Loading', label: 'Loading' },
    { value: 'Delivered', label: 'Delivered' },
    { value: 'Invoiced', label: 'Invoiced' },
    { value: 'Closed', label: 'Closed' }
  ];

  constructor(
    private dealerService: DealerManagementService,
    private router: Router,
    private dialog: MatDialog,
    private snackBar: MatSnackBar,
    private translationService: DealerTranslationService
  ) { }

  ngOnInit(): void {
    this.loadOrders();
  }

  loadOrders(): void {
    this.loading = true;
    this.error = null;

    this.dealerService.getDealerOrders(
      this.currentPage,
      this.pageSize,
      this.searchTerm || undefined,
      this.selectedDealerId || undefined,
      this.selectedStatus || undefined,
      this.startDate || undefined,
      this.endDate || undefined
    ).subscribe({
      next: (response: PaginatedResponse<DealerOrder>) => {
        this.orders = response.data;
        this.totalRecords = response.metadata.totalCount;
        this.totalPages = response.metadata.totalPages;
        this.loading = false;
      },
      error: (error) => {
        this.error = 'Failed to load dealer orders';
        console.error('Error loading orders:', error);
        this.loading = false;
      }
    });
  }

  onSearch(): void {
    this.currentPage = 1;
    this.loadOrders();
  }

  onClearFilters(): void {
    this.searchTerm = '';
    this.selectedDealerId = null;
    this.selectedStatus = '';
    this.startDate = '';
    this.endDate = '';
    this.currentPage = 1;
    this.loadOrders();
  }

  onPageChange(page: number): void {
    this.currentPage = page;
    this.loadOrders();
  }

  viewOrderDetails(order: DealerOrder): void {
    // Open dialog with loading state
    const dialogRef = this.dialog.open(DealerOrderDetailsDialogComponent, {
      width: '90%',
      maxWidth: '1200px',
      maxHeight: '90vh',
      data: { orderDetails: {} as DealerOrder, loading: true },
      panelClass: 'dealer-order-details-dialog-container',
      direction: this.translationService.getCurrentDirection()
    });

    // Fetch full order details including order items
    this.dealerService.getDealerOrderById(order.dealerOrderID).subscribe({
      next: (orderDetails: DealerOrder) => {
        dialogRef.componentInstance.orderDetails = orderDetails;
        dialogRef.componentInstance.loading = false;
      },
      error: (error) => {
        console.error('Error loading order details:', error);
        dialogRef.close();
        alert('Failed to load order details. Please try again.');
      }
    });
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
        return 'badge-success';
      case 'cancelled':
        return 'badge-danger';
      default:
        return 'badge-secondary';
    }
  }

  formatDate(date: Date): string {
    return new Date(date).toLocaleDateString();
  }

  formatCurrency(amount: number): string {
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: 'USD'
    }).format(amount);
  }

  viewDeliveryNote(order: DealerOrder): void {
    // Open dialog with order data
    const dialogRef = this.dialog.open(DeliveryNoteDialogComponent, {
      width: '1200px',
      maxWidth: '95vw',
      maxHeight: '90vh',
      data: { order },
      panelClass: 'delivery-note-dialog-container',
      direction: this.translationService.getCurrentDirection()
    });
  }

  hasEmptyLnOrderNumber(order: DealerOrder): boolean {
    const ln = order?.ln_OrderNumber;
    return ln == null || (typeof ln === 'string' && ln.trim() === '');
  }

  resendOrder(order: DealerOrder): void {
    const customerOrderNumber = order?.customerOrderNumber?.trim();
    if (!customerOrderNumber) {
      this.snackBar.open('Customer order number is missing.', 'Close', { duration: 5000, panelClass: ['snackbar-error'] });
      return;
    }
    this.resendingOrderId = order.dealerOrderID;
    this.dealerService.resendOrder(customerOrderNumber).subscribe({
      next: (response) => {
        this.resendingOrderId = null;
        const message = response?.message ?? 'Order resent to external system successfully.';
        this.snackBar.open(message, 'Close', { duration: 6000, panelClass: ['snackbar-success'] });
        this.loadOrders();
      },
      error: (err) => {
        this.resendingOrderId = null;
        console.error('Resend order failed:', err);
        const message = err?.error?.message ?? err?.message ?? 'Failed to resend order. Please try again.';
        this.snackBar.open(message, 'Close', { duration: 7000, panelClass: ['snackbar-error'] });
      }
    });
  }
}