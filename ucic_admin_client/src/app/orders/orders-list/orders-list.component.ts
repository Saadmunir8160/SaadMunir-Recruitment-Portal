import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { CancelOrderDialogComponent } from '../cancel-order-dialog/cancel-order-dialog.component';
import { MatButtonModule } from '@angular/material/button';
import { environment } from '../../../environments/environment';
import { CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatSnackBar } from '@angular/material/snack-bar';
import { SuccessSnackbarComponent } from '../../shared/components/success-snackbar/success-snackbar.component';
import { OrdersService } from '../services/orders.service';
import { Router } from '@angular/router'; 
import { PaginationComponent } from '../../shared/components/pagination/pagination.component';
import { PaginatedResponse } from '../../shared/interfaces/paginated-response.interface';

@Component({
  selector: 'app-orders-list',
  standalone: true,
  imports: [
    CommonModule,
    MatDialogModule,
    CancelOrderDialogComponent,
    MatButtonModule,
    FormsModule,
    PaginationComponent
  ],
  schemas: [CUSTOM_ELEMENTS_SCHEMA],
  templateUrl: './orders-list.component.html',
  styleUrl: './orders-list.component.scss'
})
export class OrdersListComponent implements OnInit {
  ordersList: any[] = [];
  filteredOrders: any[] = [];
  searchText: string = '';
  
  // Pagination properties
  currentPage: number = 1;
  pageSize: number = 10;
  totalCount: number = 0;
  totalPages: number = 0;
  hasPreviousPage: boolean = false;
  hasNextPage: boolean = false;
  pageSizeOptions: (number | string)[] = [5, 10, 15, 20, 30, 50, 'All'];
  loading: boolean = false;
  // Set to true for local testing with dummy rows
  useDummyData: boolean = true;

  constructor(
    private ordersService: OrdersService,
    private router: Router,
    private dialog: MatDialog
  ) {}

  cancelOptions: string[] = ['Out of Stock', 'Price difference', 'None'];

  ngOnInit(): void {
      this.getAllOrders();
  }

  getAllOrders(): void {
    this.loading = true;
    let pageSizeToSend = this.pageSize === 0 ? 1000000 : this.pageSize;
    this.ordersService.getOrders(this.currentPage, pageSizeToSend).subscribe({
      next: (res: PaginatedResponse<any>) => {
        this.ordersList = res.data;
        this.totalCount = res.metadata.totalCount;
        this.totalPages = res.metadata.totalPages;
        this.hasPreviousPage = res.metadata.hasPrevious;
        this.hasNextPage = res.metadata.hasNext;
        this.currentPage = res.metadata.currentPage;
        this.pageSize = res.metadata.pageSize;
        this.filterOrders();
        this.loading = false;
      },
      error: (err) => {
        this.loading = false;
      }
    });
  }

  filterOrders(): void {
    const search = this.searchText.toLowerCase();
    this.filteredOrders = this.ordersList.filter(order =>
      order.trackingID?.toLowerCase().includes(search) ||
      order.customerName?.toLowerCase().includes(search) ||
      order.status?.toLowerCase().includes(search)
    );
  }

  onSearchChange(): void {
    // Reset to first page when searching
    this.currentPage = 1;
    this.getAllOrders();
  }

  onPageSizeChange(size: number): void {
    this.pageSize = size;
    this.currentPage = 1; // Reset to first page when changing page size
    this.getAllOrders();
  }

  goToPage(page: number): void {
    if (page >= 1 && page <= this.totalPages) {
      this.currentPage = page;
      this.getAllOrders();
    }
  }

  goToPreviousPage(): void {
    if (this.hasPreviousPage) {
      this.goToPage(this.currentPage - 1);
    }
  }

  goToNextPage(): void {
    if (this.hasNextPage) {
      this.goToPage(this.currentPage + 1);
    }
  }

  getPageNumbers(): number[] {
    const pages: number[] = [];
    const maxVisiblePages = 5;
    
    if (this.totalPages <= maxVisiblePages) {
      // Show all pages if total is less than max visible
      for (let i = 1; i <= this.totalPages; i++) {
        pages.push(i);
      }
    } else {
      // Show pages around current page
      let start = Math.max(1, this.currentPage - Math.floor(maxVisiblePages / 2));
      let end = Math.min(this.totalPages, start + maxVisiblePages - 1);
      
      // Adjust start if we're near the end
      if (end - start + 1 < maxVisiblePages) {
        start = Math.max(1, end - maxVisiblePages + 1);
      }
      
      for (let i = start; i <= end; i++) {
        pages.push(i);
      }
    }
    
    return pages;
  }

  getShowingText(): string {
    const start = (this.currentPage - 1) * this.pageSize + 1;
    const end = Math.min(this.currentPage * this.pageSize, this.totalCount);
    return `Showing ${start} to ${end} of ${this.totalCount} results`;
  }

  showVendorDetails(order: any): void {
    this.router.navigate(['/admin/orders/details', order.orderId]);
  }
  openCancelModal(order: any): void {
    const dialogRef = this.dialog.open(CancelOrderDialogComponent, {
      data: { order, options: this.cancelOptions }
    });

    dialogRef.afterClosed().subscribe((reason?: string) => {
      if (!reason) return;
      const id = order.orderId;
      this.ordersService.cancelOrder(id, reason).subscribe({
        next: () => {
          const idx = this.ordersList.findIndex(o => o.orderId === id);
          if (idx > -1) {
            this.ordersList[idx].status = 'Canceled';
            this.ordersList[idx].cancelReason = reason;
          }
          this.filterOrders();
        },
        error: () => {
          // optionally handle error (snackbar etc.)
        }
      });
    });
  }
  
}
