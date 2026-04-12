import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';
import { MatDialog } from '@angular/material/dialog';
import { DealerOrdersService } from '../shop/services/dealer-orders.service';
import { DealerOrderDetailsDialogComponent } from '../../dealer-management/dealer-orders/dealer-order-details-dialog.component';
import { DeliveryNoteDialogComponent } from '../../dealer-management/dealer-orders/delivery-note-dialog.component';
import { ProductSearchPopupComponent } from '../shop/components/product-search-popup/product-search-popup.component';
import { DealerTranslationService } from '../services/translation.service';
import { CartItem } from '../shop/models/cart-item.model';
import { DealerManagementService, DeliveryDTO } from '../../dealer-management/services/dealer-management.service';
import { DriverService } from '../services/driver.service';
import jsPDF from 'jspdf';
import JsBarcode from 'jsbarcode';
import html2canvas from 'html2canvas';

export interface Order {
  id: string;
  orderNumber: string;
  customerOrderNumber?: string;
  ln_OrderNumber?: string;  // NEW: LN Order Number from database
  orderDate: string;
  items: Array<{
    name: string;
    quantity: number;
    unit: 'bags' | 'tons';
    price: number;
  }>;
  total: number;
  status: 'Pending' | 'Processing' | 'Shipped' | 'Delivered' | 'Cancelled';
  shippingAddress: string;
  trackingNumber?: string;
  specialInstructions?: string;  // NEW: Special delivery instructions
  deliveryAreaName?: string; // NEW: Delivery Area Name
  deliveryAreaCode?: string; // NEW: Delivery Area Code
  driverName?: string; // NEW: Driver Name
  vehicleName?: string; // NEW: Vehicle Name
}

@Component({
  selector: 'app-order-history',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule, TranslateModule, ProductSearchPopupComponent],
  templateUrl: './order-history.component.html',
  styleUrls: ['./order-history.component.scss']
})
export class OrderHistoryComponent implements OnInit {
  searchTerm: string = '';
  statusFilter: string = '';
  dateFilter: string = '';
  loading: boolean = false;
  error: string | null = null;

  orders: Order[] = [];
  filteredOrders: Order[] = [];

  // Pagination properties
  currentPage: number = 1;
  pageSize: number = 10;
  totalPages: number = 0;
  paginatedOrders: Order[] = [];


  // Track expanded order items (for table row expansion)
  private expandedOrderItems = new Set<string>();

  // Make Math available in template
  Math = Math;

  // Product search popup state
  showProductSearchPopup: boolean = false;

  constructor(
    private dealerOrdersService: DealerOrdersService,
    private dialog: MatDialog,
    private router: Router,
    private translationService: DealerTranslationService,
    private dealerManagementService: DealerManagementService,
    private driverService: DriverService
  ) { }

  ngOnInit() {
    this.loadOrderHistory();
  }

  loadOrderHistory() {
    this.loading = true;
    this.error = null;

    this.dealerOrdersService.getOrderHistory().subscribe({
      next: (response) => {

        // Handle different response formats - the actual API returns Response<List<DealerOrderDTO>>
        let orders = [];
        if (response) {
          if (Array.isArray(response)) {
            orders = response;
          } else if (response.success && response.data && Array.isArray(response.data)) {
            orders = response.data;  // Handle Response<List<DealerOrderDTO>> structure
          } else if (response.data && Array.isArray(response.data)) {
            orders = response.data;
          } else if (response.orders && Array.isArray(response.orders)) {
            orders = response.orders;
          }
        }

        this.orders = this.mapDealerOrdersToOrders(orders);
        this.filteredOrders = [...this.orders];
        this.updatePagination();
        this.loading = false;

      },
      error: (err) => {
        this.error = 'Failed to load order history. Please try again.';
        this.loading = false;
        this.orders = [];
        this.filteredOrders = [];
      }
    });
  }

  private mapDealerOrdersToOrders(dealerOrders: any[]): Order[] {
    // Handle null/undefined input
    if (!dealerOrders || !Array.isArray(dealerOrders)) {
      return [];
    }

    // Debug: Log the first order to see the structure
    if (dealerOrders.length > 0) {
      console.log('First order structure:', dealerOrders[0]);
    }

    return dealerOrders.map(dealerOrder => {

      const customerOrderNum = dealerOrder.customerOrderNumber;
      const orderNum = dealerOrder.orderNumber || `ORD-${dealerOrder.id}`;

      return {
        id: dealerOrder.dealerOrderID?.toString() || `order-${Math.random().toString(36).substr(2, 9)}`,
        orderNumber: orderNum,
        customerOrderNumber: customerOrderNum,
        ln_OrderNumber: dealerOrder.ln_OrderNumber || dealerOrder.Ln_OrderNumber || '-',  // Handle both camelCase and PascalCase
        orderDate: dealerOrder.createdDate || dealerOrder.orderDate || new Date().toISOString(),
        items: dealerOrder.dealerOrderItems?.map((item: any) => ({
          name: item.productDescription || 'Unknown Product',
          quantity: item.quantity || 1,
          unit: item.unit, // Get unit from DealerOrderItem with fallback
          price: 0 // Price not included in DealerOrderItemDTO
        })) || [],
        total: dealerOrder.totalAmount || 0,
        status: dealerOrder.status, // Keep original status from API for display
        // Prefer delivery area returned by the API (DeliveryAreaName/DeliveryAreaCode).
        // Support both camelCase and PascalCase DTO fields and fallback to full shipping address formatting if area is not set.
        shippingAddress: (dealerOrder.deliveryAreaName || dealerOrder.DeliveryAreaName)
          ? ((dealerOrder.deliveryAreaCode || dealerOrder.DeliveryAreaCode)
            ? `${dealerOrder.deliveryAreaName || dealerOrder.DeliveryAreaName} (${dealerOrder.deliveryAreaCode || dealerOrder.DeliveryAreaCode})`
            : (dealerOrder.deliveryAreaName || dealerOrder.DeliveryAreaName))
          : this.formatAddress(dealerOrder.shippingAddress),
        trackingNumber: dealerOrder.trackingNumber,
        driverName: dealerOrder.driverName || '-',
        vehicleName: dealerOrder.vehicleName || '-'
      };
    });
  }

  private formatAddress(shippingAddress: any): string {
    if (!shippingAddress) {
      return 'Address not available';
    }

    const addressParts: string[] = [];

    // Add city if available
    if (shippingAddress.city) {
      addressParts.push(shippingAddress.city);
    }

    // Add state if available and different from city
    if (shippingAddress.state && shippingAddress.state !== shippingAddress.city) {
      addressParts.push(shippingAddress.state);
    }

    // Add postal code if available
    if (shippingAddress.postalCode) {
      addressParts.push(shippingAddress.postalCode);
    }

    // If we have city/state/postal, use that combination
    if (addressParts.length > 0) {
      const combinedAddress = addressParts.join(', ');
      return combinedAddress.length > 30 ? combinedAddress.substring(0, 27) + '...' : combinedAddress;
    }

    // Fallback to address line 1 if no city/state info
    if (shippingAddress.addressLine1) {
      return shippingAddress.addressLine1.length > 30 ? shippingAddress.addressLine1.substring(0, 27) + '...' : shippingAddress.addressLine1;
    }

    return 'Address not available';
  }

  private mapOrderStatus(status: string): 'Pending' | 'Processing' | 'Shipped' | 'Delivered' | 'Cancelled' {
    if (!status) return 'Pending';

    const statusLower = status.toLowerCase();
    switch (statusLower) {
      case 'pending':
        return 'Pending';
      case 'processing':
      case 'confirmed':
        return 'Processing';
      case 'shipped':
      case 'dispatched':
        return 'Shipped';
      case 'delivered':
      case 'completed':
        return 'Delivered';
      case 'cancelled':
      case 'canceled':
        return 'Cancelled';
      default:
        return 'Pending';
    }
  }

  filterOrders() {
    this.filteredOrders = this.orders.filter(order => {
      const matchesSearch = !this.searchTerm ||
        order.orderNumber.toLowerCase().includes(this.searchTerm.toLowerCase()) ||
        (order.customerOrderNumber && order.customerOrderNumber.toLowerCase().includes(this.searchTerm.toLowerCase())) ||
        (order.ln_OrderNumber && order.ln_OrderNumber.toLowerCase().includes(this.searchTerm.toLowerCase())) ||
        order.items.some(item => item.name.toLowerCase().includes(this.searchTerm.toLowerCase()));

      // Case-insensitive status comparison to handle different status formats from API
      const matchesStatus = !this.statusFilter ||
        (order.status && order.status.toLowerCase() === this.statusFilter.toLowerCase());

      let matchesDate = true;
      if (this.dateFilter) {
        const orderDate = new Date(order.orderDate);
        const now = new Date();
        const daysAgo = parseInt(this.dateFilter);
        const cutoffDate = new Date(now.getTime() - (daysAgo * 24 * 60 * 60 * 1000));
        matchesDate = orderDate >= cutoffDate;
      }

      return matchesSearch && matchesStatus && matchesDate;
    });

    // Reset to first page when filtering
    this.currentPage = 1;
    this.updatePagination();
  }

  updatePagination() {
    this.totalPages = Math.ceil(this.filteredOrders.length / this.pageSize);
    const startIndex = (this.currentPage - 1) * this.pageSize;
    const endIndex = startIndex + this.pageSize;
    this.paginatedOrders = this.filteredOrders.slice(startIndex, endIndex);
    console.log(this.paginatedOrders)
  }

  goToPage(page: number) {
    if (page >= 1 && page <= this.totalPages) {
      this.currentPage = page;
      this.updatePagination();
    }
  }

  nextPage() {
    if (this.currentPage < this.totalPages) {
      this.currentPage++;
      this.updatePagination();
    }
  }

  previousPage() {
    if (this.currentPage > 1) {
      this.currentPage--;
      this.updatePagination();
    }
  }

  changePageSize(newSize: number) {
    this.pageSize = newSize;
    this.currentPage = 1;
    this.updatePagination();
  }

  getPageNumbers(): number[] {
    const pages = [];
    const maxVisiblePages = 5;
    let startPage = Math.max(1, this.currentPage - Math.floor(maxVisiblePages / 2));
    let endPage = Math.min(this.totalPages, startPage + maxVisiblePages - 1);

    // Adjust start page if we're near the end
    if (endPage - startPage < maxVisiblePages - 1) {
      startPage = Math.max(1, endPage - maxVisiblePages + 1);
    }

    for (let i = startPage; i <= endPage; i++) {
      pages.push(i);
    }
    return pages;
  }

  getDeliveredCount(): number {
    return this.orders.filter(order => order.status === 'Delivered').length;
  }

  // Order items expansion methods
  toggleOrderItems(orderId: string): void {
    if (this.expandedOrderItems.has(orderId)) {
      this.expandedOrderItems.delete(orderId);
    } else {
      this.expandedOrderItems.add(orderId);
    }
  }

  isOrderItemsExpanded(orderId: string): boolean {
    return this.expandedOrderItems.has(orderId);
  }

  viewOrderDetails(orderId: string): void {
    // Open dialog with loading state
    const dialogRef = this.dialog.open(DealerOrderDetailsDialogComponent, {
      width: '90%',
      maxWidth: '1200px',
      maxHeight: '90vh',
      data: { orderDetails: {} as any, loading: true },
      panelClass: 'dealer-order-details-dialog-container',
      direction: this.translationService.getCurrentDirection()
    });

    // Convert string id to number
    const orderIdNum = parseInt(orderId, 10);

    // Fetch full order details - now returns same structure as admin API
    this.dealerOrdersService.getOrderById(orderIdNum).subscribe({
      next: (response: any) => {
        // Extract data from response wrapper
        const orderDetails = response?.data || response;

        // API now returns the same DealerOrderForAdminDTO structure as admin side
        dialogRef.componentInstance.orderDetails = orderDetails;
        dialogRef.componentInstance.loading = false;
      },
      error: (err: any) => {
        console.error('Error loading order details:', err);
        dialogRef.close();
        alert('Failed to load order details. Please try again.');
      }
    });
  }

  // Product search popup methods
  openProductSearch(): void {
    this.showProductSearchPopup = true;
  }

  closeProductSearch(): void {
    this.showProductSearchPopup = false;
  }

  onProductAdded(cartItem: CartItem): void {
    // Product is added to cart, redirect to cart page
    console.log('Product added to cart:', cartItem.name);
    // Navigate to the shop cart page
    this.router.navigate(['/dealer/shop/cart']);
  }

  generateOrderPDF(orderId: string): void {
    // Convert string id to number
    const orderIdNum = parseInt(orderId, 10);

    // Fetch full order details
    this.dealerOrdersService.getOrderById(orderIdNum).subscribe({
      next: (response: any) => {
        // Extract data from response wrapper
        const orderDetails = response?.data || response;

        // Fetch driver data if driverID is available
        if (orderDetails.driverID) {
          this.driverService.getDriver(orderDetails.driverID).subscribe({
            next: (driver: any) => {
              // Add driver data to orderDetails
              orderDetails.driver = driver;
              this.proceedWithPDFGeneration(orderDetails);
            },
            error: (driverErr: any) => {
              console.error('Error loading driver data for PDF:', driverErr);
              // Continue without driver data
              this.proceedWithPDFGeneration(orderDetails);
            }
          });
        } else {
          // No driver ID, proceed without driver data
          this.proceedWithPDFGeneration(orderDetails);
        }
      },
      error: (err: any) => {
        console.error('Error loading order details for PDF:', err);
        alert('Failed to generate PDF. Please try again.');
      }
    });
  }

  private proceedWithPDFGeneration(orderDetails: any): void {
    // Try to fetch delivery data if ln_OrderNumber is available
    if (orderDetails.ln_OrderNumber) {
      this.dealerManagementService.getDeliveryByLnOrderNumber(orderDetails.ln_OrderNumber).subscribe({
        next: (deliveryResponse: any) => {
          const deliveryData = deliveryResponse?.data || null;
          // Load Arabic font and create PDF with both order and delivery data
          this.loadArabicFontAndCreatePDF(orderDetails, deliveryData);
        },
        error: (deliveryErr: any) => {
          console.error('Error loading delivery data for PDF:', deliveryErr);
          // Continue with just order details if delivery data fails
          this.loadArabicFontAndCreatePDF(orderDetails, null);
        }
      });
    } else {
      // Load Arabic font and create PDF with just order details
      this.loadArabicFontAndCreatePDF(orderDetails, null);
    }
  }

  viewDeliveryNote(order: Order): void {
    // Map Order to DealerOrder (partial match for dialog)
    const dealerOrder: any = {
      dealerOrderID: parseInt(order.id, 10) || 0,
      customerOrderNumber: order.customerOrderNumber,
      ln_OrderNumber: order.ln_OrderNumber,
      // Add other fields if needed for fallback, but dialog currently relies on API
    };

    this.dialog.open(DeliveryNoteDialogComponent, {
      width: '1200px',
      maxWidth: '95vw',
      maxHeight: '90vh',
      data: { order: dealerOrder },
      panelClass: 'delivery-note-dialog-container',
      direction: this.translationService.getCurrentDirection()
    });
  }

  private async loadArabicFontAndCreatePDF(orderDetails: any, deliveryData: DeliveryDTO | null): Promise<void> {
    try {
      // Load the Noto Naskh Arabic font from assets
      const fontUrl = 'assets/fonts/Noto_Naskh_Arabic/static/NotoNaskhArabic-Regular.ttf';
      const fontResponse = await fetch(fontUrl);
      const fontBlob = await fontResponse.blob();

      // Load the logo image
      const logoUrl = 'assets/images/logo-blue.jpg';
      const logoResponse = await fetch(logoUrl);
      const logoBlob = await logoResponse.blob();

      const fontReader = new FileReader();
      fontReader.onloadend = () => {
        const base64Font = (fontReader.result as string).split(',')[1];

        const logoReader = new FileReader();
        logoReader.onloadend = async () => {
          const base64Logo = logoReader.result as string;
          await this.createLoadingReportPDF(orderDetails, deliveryData, base64Font, base64Logo);
        };
        logoReader.readAsDataURL(logoBlob);
      };
      fontReader.readAsDataURL(fontBlob);
    } catch (error) {
      console.error('Error loading resources:', error);
      alert('Failed to load resources. PDF cannot be generated.');
    }
  }

  private async createLoadingReportPDF(orderDetails: any, deliveryData: DeliveryDTO | null, arabicFont: string, logoImage: string): Promise<void> {
    // Helper function to get data with fallback
    const getValue = (deliveryField: any, orderField: any, fallback: string = '') => {
      return deliveryField || orderField || fallback;
    };

    // Extract data with preference for delivery data
    const iqn = getValue(deliveryData?.iqn, orderDetails.driver?.iqamaNumber) || '';
    const driverName = getValue(deliveryData?.driverName, orderDetails.driverName) || '';
    const vehicleNo = getValue(deliveryData?.car, orderDetails.vehicleName) || '';
    const customerOrder = getValue(deliveryData?.customerOrder, orderDetails.customerOrderNumber) || '';
    const customerName = getValue(deliveryData?.customerName, orderDetails.dealerName) || '';
    const transporter = getValue(deliveryData?.transporterName, orderDetails.transporterName) || '';
    const loadingOrderNo = getValue(deliveryData?.lnOrderNumber, orderDetails.ln_OrderNumber, '') || '';
    const deliveryArea = deliveryData?.areaDescription
      ? `${deliveryData.areaDescription}${deliveryData.area ? ` (${deliveryData.area})` : ''}`
      : (orderDetails.areaName
        ? `${orderDetails.areaName}${orderDetails.areaCode ? ` (${orderDetails.areaCode})` : ''}`
        : '');
    const warehouse = getValue(deliveryData?.warehouseDescription, null) || '';

    // Get first product as cement type (from order items)
    const firstItem = orderDetails.orderItems?.[0];
    const cementType = firstItem
      ? `${firstItem.productDescription || firstItem.productName || ''}${firstItem.productCode ? ` (${firstItem.productCode})` : ''}`
      : (getValue(deliveryData?.itemDescription, null) || '');
    const dispatchClerk = getValue(deliveryData?.createdBy, null) || '';

    // Get current date formatted as "31 Dec 2025"
    const now = new Date();
    const day = now.getDate();
    const monthNames = ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'];
    const month = monthNames[now.getMonth()];
    const year = now.getFullYear();
    const currentDate = `${day} ${month} ${year}`;

    // Generate barcode image
    let barcodeImg = '';
    if (loadingOrderNo) {
      try {
        const canvas = document.createElement('canvas');
        JsBarcode(canvas, loadingOrderNo, {
          format: 'CODE128',
          width: 2,
          height: 40,
          displayValue: false
        });
        const barcodeDataUrl = canvas.toDataURL('image/png');
        barcodeImg = `<img src="${barcodeDataUrl}" class="barcode-image" style="width: 200px; height: 60px; display: block; margin: 10px auto;" />`;
      } catch (barcodeError) {
        console.error('Error generating barcode:', barcodeError);
      }
    }

    // Create HTML structure exactly as provided
    const htmlContent = `
<!DOCTYPE html>
<html lang="ar">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Loading Report</title>
    <style>
        * {
            margin: 0;
            padding: 0;
            box-sizing: border-box;
        }
        
        body {
            font-family: Arial, sans-serif;
            padding: 20px;
            background: #f5f5f5;
        }
        
        .container {
            max-width: 1000px;
            margin: 0 auto;
            background: white;
            border: 3px solid black;
            padding: 10px;
        }
        
        .inner-border {
            border: 2px solid black;
            padding: 20px;
        }
        
        .header {
            display: flex;
            justify-content: space-between;
            align-items: center;
            padding-bottom: 20px;
            border-bottom: 2px solid black;
            margin-bottom: 20px;
        }
        
        .header-text h1 {
            font-size: 32px;
            margin-bottom: 5px;
        }
        
        .header-text p {
            font-size: 24px;
            color: #333;
        }
        
        .logo {

            color: white;
            padding: 15px 25px;
            font-size: 18px;
            font-weight: bold;
            border: none !important;
            outline: none !important;
            box-shadow: none !important;
            display: flex;
            align-items: center;
            justify-content: center;
        }
        
        .logo * {
            border: none !important;
            outline: none !important;
            box-shadow: none !important;
        }
        
        .logo img {
            max-width: 180px;
            max-height: 110px;
            object-fit: contain;
            border: none !important;
            outline: none !important;
            box-shadow: none !important;
            display: block;
        }
        
        .info-grid {
            display: grid;
            grid-template-columns: repeat(4, 1fr);
            border: 2px solid black;
            margin-bottom: 20px;
        }
        
        .info-cell {
            border: 1px solid black;
            padding: 15px;
            text-align: right;
        }
        
        .info-cell .label {
            font-weight: bold;
            font-size: 14px;
        }
        
        .info-cell .value {
            font-size: 18px;
        }
        
        .full-width {
            grid-column: 1 / -1;
            border: 2px solid black;
            padding: 15px;
            text-align: right;
            font-size: 16px;
        }
        
        .transporter-section {
            display: grid;
            grid-template-columns: 1fr 1fr;
            border: 2px solid black;
            margin-bottom: 20px;
        }
        
        .trans-cell {
            border: 1px solid black;
            padding: 40px 15px;
            text-align: right;
        }
        
        .barcode-cell {
            text-align: center;
            padding: 20px;
        }
        
        .barcode {
            font-size: 12px;
            margin-top: 10px;
        }
        
        .details-grid {
            display: grid;
            grid-template-columns: repeat(2, 1fr);
            border: 2px solid black;
            margin-bottom: 20px;
        }
        
        .detail-cell {
            border: 1px solid black;
            padding: 20px 15px;
            text-align: right;
        }
        
        .signature-grid {
            display: grid;
            grid-template-columns: repeat(3, 1fr);
            border: 2px solid black;
            margin-bottom: 20px;
        }
        
        .sig-cell {
            border: 1px solid black;
            padding: 40px 15px;
            text-align: right;
        }
        
        .footer {
            text-align: center !important;
            padding: 5px;
            font-size: 12px;
            line-height: 1.6;
            margin: 0 auto;
            width: 100%;
            display: block;
            text-align: center;
            justify-content: center;
            align-items: center;
        }
        
        .footer br {
            display: block;
            content: "";
            margin: 5px 0;
        }
        
        .arabic {
            direction: rtl;
        }
        
        .barcode-image {
            width: 200px;
            height: 60px;
        }
    </style>
</head>
<body>
    <div class="container">
        <div class="inner-border">
            <div class="header">
                <div class="header-text arabic">
                    <h1>تقرير التحميل</h1>
                    <p>Loading Report</p>
                </div>
                <div class="logo">
                    <img src="${logoImage}" style="border: none !important; outline: none !important; box-shadow: none !important;" onerror="this.style.display='none'; this.parentElement.innerHTML='UCIC LOGO';" />
                </div>
            </div>
            
            <div class="info-grid">
                <div class="info-cell">
                    <div class="label arabic">رقم الإقامة / IQN</div>
                    <div class="value">${iqn}</div>
                </div>
                <div class="info-cell">
                    <div class="label arabic">اسم السائق / Driver</div>
                    <div class="value arabic">Name<br>${driverName}</div>
                </div>
                <div class="info-cell">
                    <div class="label arabic">رقم المركبة / Vehicle</div>
                    <div class="value arabic">.No<br>${vehicleNo}</div>
                </div>
                <div class="info-cell">
                    <div class="label arabic">رقم طلب العميل /</div>
                    <div class="value">Customer Order<br>${customerOrder}</div>
                </div>
            </div>
            
            <div class="full-width arabic">
                <strong>العميل / Customer</strong><br>
                ${customerName}
            </div>
            
            <div class="transporter-section">
                <div class="trans-cell arabic">
                    الناقل / Transporter<br>${transporter}
                </div>
                <div class="trans-cell barcode-cell">
                    <div class="arabic" style="text-align: right;">رقم أمر التحميل / Loading Order No</div>
                    ${barcodeImg}
                    <div class="barcode">${loadingOrderNo}</div>
                </div>
            </div>
            
            <div class="details-grid">
                <div class="detail-cell arabic">
                    <strong>منطقة التسليم / Delivery Area</strong><br>
                    ${deliveryArea}
                </div>
                <div class="detail-cell arabic">
                    <strong>نوع الاسمنت / Type of Cement</strong><br>
                    ${cementType}
                </div>
            </div>
            
            <div class="signature-grid">
                <div class="sig-cell arabic">
                    التاريخ / Date<br>${currentDate}
                </div>
                <div class="sig-cell arabic">
                    الختم / Stamp
                </div>
                <div class="sig-cell arabic">
                    كاتب التسليمات / Dispatch Clerk<br>${dispatchClerk}
                </div>
            </div>
            
            <div class="footer">
                P.O.Box 1020, Jeddah 21431 Unified No. 920026267 www.unitedcement.com.sa<br>
                Kingdom of Saudi Arabia Fax: +966126702444 Email: info@unitedcement.com.sa
            </div>
        </div>
    </div>
</body>
</html>
    `;

    // Create a temporary div to render the HTML
    const tempDiv = document.createElement('div');
    tempDiv.innerHTML = htmlContent;
    tempDiv.style.position = 'absolute';
    tempDiv.style.left = '-9999px';
    tempDiv.style.width = '1000px';
    document.body.appendChild(tempDiv);

    // Wait for images to load
    await new Promise(resolve => setTimeout(resolve, 500));

    // Convert HTML to canvas
    const canvas = await html2canvas(tempDiv.querySelector('.container') as HTMLElement, {
      scale: 2,
      useCORS: true,
      logging: false,
      backgroundColor: '#ffffff'
    });

    // Remove temporary div
    document.body.removeChild(tempDiv);

    // Create PDF from canvas
    const imgData = canvas.toDataURL('image/png');
    const pdf = new jsPDF('p', 'mm', 'a4');
    const imgWidth = 210; // A4 width in mm
    const pageHeight = 297; // A4 height in mm
    const imgHeight = (canvas.height * imgWidth) / canvas.width;
    let heightLeft = imgHeight;

    let position = 0;

    pdf.addImage(imgData, 'PNG', 0, position, imgWidth, imgHeight);
    heightLeft -= pageHeight;

    while (heightLeft > 0) {
      position = heightLeft - imgHeight;
      pdf.addPage();
      pdf.addImage(imgData, 'PNG', 0, position, imgWidth, imgHeight);
      heightLeft -= pageHeight;
    }

    // Save PDF
    const fileName = `Loading_Report_${loadingOrderNo || orderDetails.dealerOrderID}_${Date.now()}.pdf`;
    pdf.save(fileName);
  }

  parseText(text: string): string {
    return text.replace(/\s/g, "").toLowerCase();
  }
}
