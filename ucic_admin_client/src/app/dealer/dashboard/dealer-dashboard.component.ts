import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
import { CartService } from '../shop/services/cart.service';
import { DealerDashboardService, DashboardStats, DashboardStatsApiResponse, RecentOrder, RecentOrderApiResponse, DealerNotification, DealerDailyLimits } from '../services/dealer-dashboard.service';
import { DealerProfileService, ApiResponse } from '../services/dealer-profile.service';
import { DealerOrdersService } from '../shop/services/dealer-orders.service';

@Component({
  selector: 'app-dealer-dashboard',
  standalone: true,
  imports: [CommonModule, TranslateModule],
  templateUrl: './dealer-dashboard.component.html',
  styleUrl: './dealer-dashboard.component.scss'
})
export class DealerDashboardComponent implements OnInit {
  dashboardData: DashboardStats = {
    totalOrders: 0,
    pendingOrders: 0,
    confirmedOrders: 0,
    deliveredOrders: 0,
    cancelledOrders: 0,
    monthlySpent: 0,
    yearlySpent: 0,
    averageOrderValue: 0,
    recentOrderValue: 0,
    lastOrderDate: undefined,
    nextDeliveryDate: undefined,
    averageDeliveryTime: 0,
    totalProductsOrdered: 0,
    favoriteProducts: [],
    cartItems: 0,
    paymentStatus: {
      outstanding: 0,
      overdue: 0,
      paid: 0
    }
  };

  recentOrders: RecentOrder[] = [];
  notifications: DealerNotification[] = [];
  dailyLimits: DealerDailyLimits | null = null;
  availableCredit: number | null = null;
  isLoading = true;
  error: string | null = null;

  quickActions = [
    {
      title: 'DEALER.DASHBOARD.QUICK_ACTIONS.BROWSE_PRODUCTS',
      description: 'DEALER.DASHBOARD.QUICK_ACTIONS.BROWSE_PRODUCTS_DESC',
      icon: 'shopping_basket',
      route: '/dealer/shop/products',
      color: 'primary'
    },
    {
      title: 'DEALER.DASHBOARD.QUICK_ACTIONS.VIEW_CART',
      description: 'DEALER.DASHBOARD.QUICK_ACTIONS.VIEW_CART_DESC',
      icon: 'shopping_cart',
      route: '/dealer/shop/cart',
      color: 'success'
    },
    {
      title: 'DEALER.DASHBOARD.QUICK_ACTIONS.ORDER_HISTORY',
      description: 'DEALER.DASHBOARD.QUICK_ACTIONS.ORDER_HISTORY_DESC',
      icon: 'receipt_long',
      route: '/dealer/orders',
      color: 'info'
    },
    {
      title: 'DEALER.DASHBOARD.QUICK_ACTIONS.CONTACT_SUPPORT',
      description: 'DEALER.DASHBOARD.QUICK_ACTIONS.CONTACT_SUPPORT_DESC',
      icon: 'support_agent',
      route: '/dealer/support',
      color: 'warning'
    }
  ];

  constructor(
    private router: Router,
    private cartService: CartService,
    private dashboardService: DealerDashboardService,
    private profileService: DealerProfileService,
    private dealerOrdersService: DealerOrdersService
  ) {}

  ngOnInit(): void {
    this.loadDashboardData();
  }

  parseText(text: string): string {
    return text.replace(/\s/g, "").toLowerCase();
  }

  private loadDashboardData(): void {
    this.isLoading = true;
    this.error = null;

    // Load dashboard statistics
    this.dashboardService.getDashboardStats().subscribe({
      next: (response: any) => {
        
        // Handle wrapped responses (e.g., {success: true, data: {...}})
        let stats = response;
        if (response && response.data) {
          stats = response.data;
        }
        
        if (stats && typeof stats === 'object') {
          
          // Handle both PascalCase and camelCase responses
          this.dashboardData = {
            totalOrders: stats.TotalOrders || stats.totalOrders || 0,
            pendingOrders: stats.PendingOrders || stats.pendingOrders || 0,
            confirmedOrders: stats.ConfirmedOrders || stats.confirmedOrders || 0,
            deliveredOrders: stats.DeliveredOrders || stats.deliveredOrders || 0,
            cancelledOrders: stats.CancelledOrders || stats.cancelledOrders || 0,
            monthlySpent: stats.MonthlySpent || stats.monthlySpent || 0,
            yearlySpent: stats.YearlySpent || stats.yearlySpent || 0,
            averageOrderValue: stats.AverageOrderValue || stats.averageOrderValue || 0,
            recentOrderValue: stats.RecentOrderValue || stats.recentOrderValue || 0,
            lastOrderDate: stats.LastOrderDate || stats.lastOrderDate,
            nextDeliveryDate: stats.NextDeliveryDate || stats.nextDeliveryDate,
            averageDeliveryTime: stats.AverageDeliveryTime || stats.averageDeliveryTime || 0,
            totalProductsOrdered: stats.TotalProductsOrdered || stats.totalProductsOrdered || 0,
            favoriteProducts: stats.FavoriteProducts || stats.favoriteProducts || [],
            cartItems: 0, // Will be set from cart service
            paymentStatus: {
              outstanding: stats.PaymentStatus?.Outstanding || stats.paymentStatus?.outstanding || 0,
              overdue: stats.PaymentStatus?.Overdue || stats.paymentStatus?.overdue || 0,
              paid: stats.PaymentStatus?.Paid || stats.paymentStatus?.paid || 0
            }
          };
          
        } else {
        }
        // Also get cart items count from cart service
        this.dashboardData.cartItems = this.cartService.getCartItemCount();
        this.isLoading = false;
      },
      error: (error) => {
        this.error = 'Failed to load dashboard data';
        this.isLoading = false;
        // Load cart items count even if API fails
        this.dashboardData.cartItems = this.cartService.getCartItemCount();
      }
    });

    // Load recent orders using DealerOrder/GetMyOrders API
    this.dealerOrdersService.getOrderHistory().subscribe({
      next: (response: any) => {
        
        // Handle wrapped responses
        let orders = [];
        if (response) {
          if (Array.isArray(response)) {
            orders = response;
          } else if (response.success && response.data && Array.isArray(response.data)) {
            orders = response.data;
          } else if (response.data && Array.isArray(response.data)) {
            orders = response.data;
          }
        }
        
        if (Array.isArray(orders)) {
          
          // Map to new structure with fields from DealerOrder/GetMyOrders API and limit to 4 orders
          this.recentOrders = orders.slice(0, 4).map(order => ({
            id: order.dealerOrderID || order.DealerOrderID || 0,
            customerOrderNumber: order.customerOrderNumber || order.CustomerOrderNumber || 'N/A',
            portalOrderNumber: order.portalOrderNumber || order.PortalOrderNumber || 'N/A',
            orderNumber: order.portalOrderNumber || order.PortalOrderNumber || 'N/A', // For backwards compatibility
            lnOrderNumber: order.ln_OrderNumber || order.Ln_OrderNumber || 'N/A',
            orderDate: order.orderDate || order.OrderDate,
            date: order.orderDate || order.OrderDate, // Map for template compatibility
            status: order.status,
            deliveryAreaName: order.deliveryAreaName || order.DeliveryAreaName || '-',
            deliveryAreaCode: order.deliveryAreaCode || order.DeliveryAreaCode || '',
            driverName: order.driverName || order.DriverName || '-',
            vehicleName: order.vehicleName || order.VehicleName || '-',
            totalAmount: order.totalAmount || order.TotalAmount || 0,
            total: order.totalAmount || order.TotalAmount || 0, // Map for template compatibility
            itemCount: order.dealerOrderItems?.length || order.DealerOrderItems?.length || 0,
            items: order.dealerOrderItems?.length || order.DealerOrderItems?.length || 0, // Map for template compatibility
            estimatedDelivery: order.estimatedDelivery || order.EstimatedDelivery,
            trackingNumber: order.trackingNumber || order.TrackingNumber
          }));
          
          
          // // If no orders returned, add some test data to verify UI is working
          // if (this.recentOrders.length === 0) {
          //   this.recentOrders = [
          //     {
          //       id: 1,
          //       orderNumber: 'ORD-001001',
          //       orderDate: '2025-09-25',
          //       date: '2025-09-25',
          //       status: 'Pending',
          //       totalAmount: 4200,
          //       total: 4200,
          //       itemCount: 50,
          //       items: 50,
          //       estimatedDelivery: '2025-09-28',
          //       trackingNumber: 'TRK-123456'
          //     },
          //     {
          //       id: 2,
          //       orderNumber: 'ORD-001002',
          //       orderDate: '2025-09-24',
          //       date: '2025-09-24',
          //       status: 'Processing',
          //       totalAmount: 3600,
          //       total: 3600,
          //       itemCount: 40,
          //       items: 40,
          //       estimatedDelivery: '2025-09-27',
          //       trackingNumber: 'TRK-123457'
          //     },
          //     {
          //       id: 3,
          //       orderNumber: 'ORD-001003',
          //       orderDate: '2025-09-23',
          //       date: '2025-09-23',
          //       status: 'Delivered',
          //       totalAmount: 2400,
          //       total: 2400,
          //       itemCount: 30,
          //       items: 30,
          //       estimatedDelivery: '2025-09-26',
          //       trackingNumber: 'TRK-123458'
          //     }
          //   ];
          // }
        } else {
          this.recentOrders = [];
        }
      },
      error: (error) => {
      }
    });

    // Load notifications
    this.dashboardService.getNotifications(5, true).subscribe({
      next: (notifications) => {
        this.notifications = notifications;
      },
      error: (error) => {
      }
    });

    // Load daily limits
    this.dashboardService.getDailyLimits().subscribe({
      next: (limits) => {
        this.dailyLimits = limits;
      },
      error: (error) => {
        console.error('Failed to load daily limits:', error);
        // Set default values if loading fails
        this.dailyLimits = {
          totalLimitTons: 0,
          usedTodayTons: 0,
          remainingTodayTons: 0,
          totalLimitBags: 0,
          usedTodayBags: 0,
          remainingTodayBags: 0,
          currentOrderTons: 0,
          currentOrderBags: 0,
          lastUpdated: new Date(),
          dealerId: 0
        };
      }
    });

    // Subscribe to cart changes
    this.cartService.getCartSummary().subscribe(summary => {
      this.dashboardData.cartItems = summary.itemCount;
    });

    // Load credit limit from LN API
    this.loadCreditLimit();
  }

  private loadCreditLimit(): void {
    this.profileService.getCreditLimit().subscribe({
      next: (response: ApiResponse<any>) => {
        console.log('Credit limit API response:', response);
        if (response.success && response.data) {
          // Handle both camelCase and PascalCase response
          const creditValue = response.data.availableCredit || response.data.AvailableCredit;
          if (creditValue !== undefined && creditValue !== null) {
            this.availableCredit = creditValue;
            console.log('Credit limit loaded:', this.availableCredit);
          } else {
            console.warn('Available credit value not found in response data:', response.data);
            this.availableCredit = 0;
          }
        } else {
          console.warn('Failed to load credit limit from API:', response.message);
          this.availableCredit = 0;
        }
      },
      error: (error) => {
        console.error('Credit limit API error:', error);
        this.availableCredit = 0;
      }
    });
  }

  navigateTo(route: string): void {
    this.router.navigate([route]);
  }

  formatDate(date: any): string {
    // Handle null, undefined, or empty values
    if (!date || date === '' || date === 'null' || date === 'undefined') {
      return 'N/A';
    }
    
    let dateObj: Date;
    
    try {
      // Handle different input types
      if (typeof date === 'string') {
        // Handle various date string formats
        dateObj = new Date(date);
      } else if (date instanceof Date) {
        dateObj = date;
      } else if (typeof date === 'number') {
        // Handle timestamp
        dateObj = new Date(date);
      } else {
        return 'Invalid Date';
      }
      
      // Check if date is valid
      if (isNaN(dateObj.getTime())) {
        return 'Invalid Date';
      }
      
      return dateObj.toLocaleDateString('en-US', {
        year: 'numeric',
        month: 'short',
        day: 'numeric'
      });
    } catch (error) {
      return 'Invalid Date';
    }
  }

  getStatusClass(status: string): string {
    const statusClasses: { [key: string]: string } = {
      'Pending': 'status-pending',
      'Processing': 'status-processing',
      'Shipped': 'status-shipped',
      'Delivered': 'status-delivered',
      'Cancelled': 'status-cancelled'
    };
    return statusClasses[status] || 'status-default';
  }

  // Helper methods for daily limits
  getUsagePercentage(used: number, total: number): number {
    return total > 0 ? Math.round((used / total) * 100) : 0;
  }

  getProgressBarClass(percentage: number): string {
    if (percentage >= 90) return 'progress-danger';
    if (percentage >= 75) return 'progress-warning';
    return 'progress-success';
  }
}
