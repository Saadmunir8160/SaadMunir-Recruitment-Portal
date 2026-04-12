import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { environment } from '../../../environments/environment';
import { CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatSnackBar } from '@angular/material/snack-bar';
import { SuccessSnackbarComponent } from '../../shared/components/success-snackbar/success-snackbar.component';
import { OrdersService } from '../services/orders.service';
import { Router, ActivatedRoute } from '@angular/router'; 
import { Location } from '@angular/common';
import { DomSanitizer, SafeResourceUrl } from '@angular/platform-browser';
import { OrderConfirmationDialogComponent } from './order-confirmation-dialog.component';
import { ConfirmationDialogComponent } from './confirmation-dialog.component';

@Component({
  selector: 'app-orders-list',
  standalone: true,
  imports: [
    CommonModule,
    MatDialogModule,
    MatButtonModule,
    FormsModule
  ],
  schemas: [CUSTOM_ELEMENTS_SCHEMA],
  templateUrl: './order-details.component.html',
  styleUrl: './order-details.component.scss'
})
export class OrderDetailsComponent implements OnInit {
  ordersList: any[] = [];
  apiError: string | null = null;
  order: any;
  dealers: any[] = [];
  selectedDealerId: number | null = null;
  timelineEvents: any[] = [];
  mapUrl: SafeResourceUrl | null = null;

  constructor(
    private location: Location, 
    private router: Router,
    private route: ActivatedRoute,
    private ordersService: OrdersService,  
    private snackBar: MatSnackBar,
    private sanitizer: DomSanitizer,
    private dialog: MatDialog
  ) {}

  ngOnInit(): void {
    this.loadOrder();
    this.loadDealers();
  }

  loadOrder(): void {
    this.route.params.subscribe(params => {
      const orderId = params['id'];
      if (orderId) {
        this.ordersService.getOrderById(orderId).subscribe({
          next: (data) => {
            this.order = data.data;
            this.timelineEvents = this.getTimelineEvents();
            this.mapUrl = this.getMapUrl();
          },
          error: (error) => {
            this.apiError = 'Error loading order: ' + (error?.error?.message || 'Unknown error');
          }
        });
      }
    });
  }

  calculateSubtotal(): number {
    if (!this.order?.orderItems) return 0;
    return this.order.orderItems.reduce((total: number, item: any) => total + (item.price || 0), 0);
  }

  isPaymentUnverified(): boolean {
    return !this.order?.paymentConfirmedBy && !this.order?.paymentConfirmedDate && this.order?.paymentSubmittedDate;
  }

  
  isOrderUnverified(): boolean {
    return !this.order?.orderConfirmedBy && !this.order?.orderConfirmedDate &&
    this.order?.paymentConfirmedBy && this.order?.paymentConfirmedDate;
  }

  loadDealers() {
    this.ordersService.getDealers().subscribe({
      next: (data) => {
        this.dealers = data;
      },
      error: (err) => {
      }
    });
  }
  verifyPayment(): void {
    // Show confirmation dialog first
    const dialogRef = this.dialog.open(ConfirmationDialogComponent, {
      width: '500px',
      disableClose: false,
      panelClass: 'custom-dialog',
      data: { message: 'Are you sure you want to Confirm Payment?' }
    });

    dialogRef.afterClosed().subscribe((result: boolean | undefined) => {
      if (result === true) {
        // User confirmed, proceed with payment verification
        const formData = new FormData();
        formData.append('OrderId', this.order?.orderId);
        
        this.ordersService.VerifyOrderPayment(this.order?.orderId, formData).subscribe({
          next: (success) => {
            if (success) {
              // Show success message
              this.snackBar.openFromComponent(SuccessSnackbarComponent, {
                data: {
                  message: 'Payment confirmed successfully!'
                },
                duration: 5000,
                horizontalPosition: 'center',
                verticalPosition: 'top',
                panelClass: ['custom-snackbar']
              });
              
              this.location.back();
            }
          },
          error: (error) => {
            this.apiError = 'Error updating order: ' + (error?.error?.message || 'Unknown error');
            this.location.back();
          }
        });
      }
      // If result is false or undefined, user cancelled - do nothing
    });
  }

  verifyOrder(): void {
    // Check if dealer is selected first
    if (!this.selectedDealerId) {
      alert('Please select a dealer before confirming the order.');
      return;
    }

    // Show confirmation dialog first
    const dialogRef = this.dialog.open(OrderConfirmationDialogComponent, {
      width: '500px',
      disableClose: false,
      panelClass: 'custom-dialog',
      data: { message: 'Are you sure you want to confirm this order?' }
    });

    dialogRef.afterClosed().subscribe((result: boolean | undefined) => {
      // If result is true, user confirmed - proceed with order confirmation
      if (result === true) {
        const formData = new FormData();
        formData.append('OrderId', this.order?.orderId);
        formData.append('DealerId', this.selectedDealerId!.toString());
        
        this.ordersService.OrderConfirm(this.order?.orderId, formData).subscribe({
          next: (success) => {
            if (success) {
              // Show success message
              this.snackBar.openFromComponent(SuccessSnackbarComponent, {
                data: {
                  message: 'Order Confirmed successfully!'
                },
                duration: 5000,
                horizontalPosition: 'center',
                verticalPosition: 'top',
                panelClass: ['custom-snackbar']
              });
              
              this.location.back();
            }
          },
          error: (error) => {
            this.apiError = 'Error updating order: ' + (error?.error?.message || 'Unknown error');
            this.location.back();
          }
        });
      }
      // If result is false or undefined, user cancelled - do nothing
    });
  }
  goBack(): void {
    this.location.back();
  }

    getImageUrl(relativePath: string): string {
    
    if (!relativePath) return '';
    
    // Extract just the relative path part after 'wwwroot'
    const wwwrootIndex = relativePath.toLowerCase().indexOf('wwwroot');
    let cleanPath = '';
    
    if (wwwrootIndex !== -1) {
      // If path contains 'wwwroot', take everything after it
      cleanPath = relativePath.substring(wwwrootIndex + 'wwwroot'.length);
    } else {
      // If no 'wwwroot' in path, use the path as is
      cleanPath = relativePath;
    }
    
    // Remove any leading slashes or backslashes
    cleanPath = cleanPath.replace(/^[/\\]+/, '');
    
    // Replace any backslashes with forward slashes
    cleanPath = cleanPath.replace(/\\/g, '/');
    
    // Construct the final URL
   return `${environment.apiUrl.replace('/api', '')}/static/Product/${cleanPath}`
  }

  getPaymentImageUrl(relativePath: string): string {
    if (!relativePath) return '';
    
    // Extract just the relative path part after 'wwwroot'
    const wwwrootIndex = relativePath.toLowerCase().indexOf('wwwroot');
    let cleanPath = '';
    
    if (wwwrootIndex !== -1) {
      // If path contains 'wwwroot', take everything after it
      cleanPath = relativePath.substring(wwwrootIndex + 'wwwroot'.length);
    } else {
      // If no 'wwwroot' in path, use the path as is
      cleanPath = relativePath;
    }
    
    // Remove any leading slashes or backslashes
    cleanPath = cleanPath.replace(/^[/\\]+/, '');
    
    // Replace any backslashes with forward slashes
    cleanPath = cleanPath.replace(/\\/g, '/');
    
    // Construct the final URL
    return `${environment.apiUrl.replace('/api', '')}/static/Payment/${cleanPath}`;
  }

  getMapUrl(): SafeResourceUrl | null {
    if (!this.order?.latitude || !this.order?.longitude) {
      return null;
    }
    const lat = encodeURIComponent(this.order.latitude);
    const lng = encodeURIComponent(this.order.longitude);
    const url = `https://maps.google.com/maps?q=${lat},${lng}&z=14&output=embed`;
    return this.sanitizer.bypassSecurityTrustResourceUrl(url);
  }

  getTimelineEvents(): any[] {
    const events = [];
    
    // Order Created
    events.push({
      title: 'Order Created',
      description: `Order #${this.order?.trackingID} has been created`,
      date: this.order?.createdDate,
      icon: 'solar:clipboard-text-broken',
      status: 'completed'
    });

    // Payment Submitted
    if (this.order?.paymentSubmittedDate) {
      events.push({
        title: 'Payment Submitted',
        description: `Payment has been submitted with transaction ID: ${this.order?.paymentTransactionId}`,
        date: this.order?.paymentSubmittedDate,
        icon: 'solar:card-transfer-broken',
        status: 'completed'
      });
    }

    // Payment Confirmed
    if (this.order?.paymentConfirmedDate) {
      events.push({
        title: 'Payment Confirmed',
        description: `Payment confirmed by ${this.order?.paymentConfirmedBy || 'Admin'}`,
        date: this.order?.paymentConfirmedDate,
        icon: 'solar:check-circle-broken',
        status: 'completed'
      });
    }

    // Order Confirmed
    if (this.order?.orderConfirmedDate) {
      events.push({
        title: 'Order Confirmed',
        description: `Order confirmed by ${this.order?.orderConfirmedBy || 'Admin'}`,
        date: this.order?.orderConfirmedDate,
        icon: 'solar:check-square-broken',
        status: 'completed'
      });
    }

    // Current Status
    const currentStatus = {
      title: 'Current Status',
      description: `Order is currently ${this.order?.status}`,
      date: new Date(),
      icon: this.getStatusIcon(this.order?.status),
      status: 'current'
    };
    events.push(currentStatus);

    return events;
  }

  private getStatusIcon(status: string): string {
    switch (status) {
      case 'Pending':
        return 'solar:clock-circle-broken';
      case 'PaymentSubmitted':
        return 'solar:card-transfer-broken';
      case 'PaymentConfirmed':
        return 'solar:check-circle-broken';
      case 'Confirmed':
        return 'solar:check-square-broken';
      case 'Shipped':
        return 'solar:box-minimalistic-broken';
      default:
        return 'solar:info-circle-broken';
    }
  }
}
