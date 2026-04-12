import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

// API Response interface (matches backend DTO)
export interface DashboardStatsApiResponse {
  TotalOrders: number;
  PendingOrders: number;
  ConfirmedOrders: number;
  DeliveredOrders: number;
  CancelledOrders: number;
  MonthlySpent: number;
  YearlySpent: number;
  AverageOrderValue: number;
  LastOrderDate?: string | Date;
  NextDeliveryDate?: string | Date;
  AverageDeliveryTime: number;
  TotalProductsOrdered: number;
  FavoriteProducts: string[];
  PaymentStatus: {
    Outstanding: number;
    Overdue: number;
    Paid: number;
  };
}

// Frontend interface (for component use)
export interface DashboardStats {
  totalOrders: number;
  pendingOrders: number;
  confirmedOrders: number;
  deliveredOrders: number;
  cancelledOrders: number;
  monthlySpent: number;
  yearlySpent: number;
  averageOrderValue: number;
  recentOrderValue: number; // Added for template compatibility
  lastOrderDate?: Date | string;
  nextDeliveryDate?: Date | string;
  averageDeliveryTime: number; // in days
  totalProductsOrdered: number;
  favoriteProducts: string[];
  cartItems: number; // Added for cart status
  paymentStatus: {
    outstanding: number;
    overdue: number;
    paid: number;
  };
}

// API Response interface (matches backend DTO)
export interface RecentOrderApiResponse {
  Id: number;
  OrderNumber: string;
  Ln_OrderNumber?: string;  // NEW: LN Order Number from database
  OrderDate: string | Date;
  Status: string;
  TotalAmount: number;
  ItemCount: number;
  EstimatedDelivery?: string | Date;
  TrackingNumber?: string;
}

// Frontend interface (for component use)
export interface RecentOrder {
  id: number;
  orderNumber: string;
  customerOrderNumber?: string;
  portalOrderNumber?: string;
  lnOrderNumber?: string; // LN Order Number from database
  orderDate: Date | string;
  date: Date | string; // Added for template compatibility
  status: string;
  totalAmount: number;
  total: number; // Added for template compatibility
  itemCount: number;
  items: number; // Added for template compatibility
  deliveryAreaName?: string;
  deliveryAreaCode?: string;
  driverName?: string;
  vehicleName?: string;
  estimatedDelivery?: Date | string;
  trackingNumber?: string;
}

export interface DealerNotification {
  id: number;
  type: 'order_update' | 'payment_due' | 'promotion' | 'system' | 'delivery';
  title: string;
  message: string;
  isRead: boolean;
  createdAt: Date;
  actionUrl?: string;
  priority: 'low' | 'medium' | 'high' | 'urgent';
}

export interface MonthlyOrderSummary {
  month: string;
  year: number;
  orderCount: number;
  totalAmount: number;
  averageOrderValue: number;
}

export interface TopProduct {
  productId: number;
  productName: string;
  totalQuantity: number;
  totalValue: number;
  lastOrderDate: Date;
}

export interface PaymentSummary {
  totalOutstanding: number;
  overdueAmount: number;
  currentMonthSpent: number;
  creditLimit: number;
  availableCredit: number;
  nextPaymentDue?: Date;
}

// Daily Limits interface matching API response
export interface DealerDailyLimits {
  totalLimitTons: number;
  usedTodayTons: number;
  remainingTodayTons: number;
  totalLimitBags: number;
  usedTodayBags: number;
  remainingTodayBags: number;
  currentOrderTons: number;
  currentOrderBags: number;
  lastUpdated: string | Date;
  dealerId: number;
}

@Injectable({
  providedIn: 'root'
})
export class DealerDashboardService {
  private apiUrl = `${environment.apiUrl}/DealerPortal/Dashboard`;

  constructor(private http: HttpClient) {}

  // Get dashboard statistics (returns API response format)
  getDashboardStats(): Observable<DashboardStatsApiResponse> {
    return this.http.get<DashboardStatsApiResponse>(`${this.apiUrl}/stats`);
  }

  // Get recent orders (returns API response format)
  getRecentOrders(limit: number = 5): Observable<RecentOrderApiResponse[]> {
    return this.http.get<RecentOrderApiResponse[]>(`${this.apiUrl}/recent-orders`, {
      params: { limit: limit.toString() }
    });
  }

  // Get notifications
  getNotifications(limit?: number, unreadOnly?: boolean): Observable<DealerNotification[]> {
    let params: any = {};
    if (limit) params.limit = limit.toString();
    if (unreadOnly) params.unreadOnly = 'true';
    
    return this.http.get<DealerNotification[]>(`${this.apiUrl}/notifications`, { params });
  }

  // Mark notification as read
  markNotificationAsRead(notificationId: number): Observable<any> {
    return this.http.put(`${this.apiUrl}/notifications/${notificationId}/mark-read`, {});
  }

  // Mark all notifications as read
  markAllNotificationsAsRead(): Observable<any> {
    return this.http.put(`${this.apiUrl}/notifications/mark-all-read`, {});
  }

  // Get monthly order summary
  getMonthlyOrderSummary(months: number = 12): Observable<MonthlyOrderSummary[]> {
    return this.http.get<MonthlyOrderSummary[]>(`${this.apiUrl}/monthly-summary`, {
      params: { months: months.toString() }
    });
  }

  // Get top ordered products
  getTopProducts(limit: number = 5, period: 'month' | 'quarter' | 'year' = 'month'): Observable<TopProduct[]> {
    return this.http.get<TopProduct[]>(`${this.apiUrl}/top-products`, {
      params: { 
        limit: limit.toString(),
        period: period
      }
    });
  }

  // Get payment summary
  getPaymentSummary(): Observable<PaymentSummary> {
    return this.http.get<PaymentSummary>(`${this.apiUrl}/payment-summary`);
  }

  // Get order trends data for charts
  getOrderTrends(period: 'week' | 'month' | 'quarter' | 'year' = 'month'): Observable<any> {
    return this.http.get(`${this.apiUrl}/order-trends`, {
      params: { period }
    });
  }

  // Get product category wise spending
  getCategoryWiseSpending(period: 'month' | 'quarter' | 'year' = 'month'): Observable<any> {
    return this.http.get(`${this.apiUrl}/category-spending`, {
      params: { period }
    });
  }

  // Get delivery performance metrics
  getDeliveryMetrics(): Observable<{
    onTimeDeliveryRate: number;
    averageDeliveryTime: number;
    delayedDeliveries: number;
    upcomingDeliveries: any[];
  }> {
    return this.http.get<any>(`${this.apiUrl}/delivery-metrics`);
  }

  // Download dashboard report
  downloadDashboardReport(reportType: 'monthly' | 'quarterly' | 'yearly', format: 'pdf' | 'excel' = 'pdf'): Observable<Blob> {
    return this.http.get(`${this.apiUrl}/download-report`, {
      params: { reportType, format },
      responseType: 'blob'
    });
  }

  // Get unread notifications count
  getUnreadNotificationCount(): Observable<{ count: number }> {
    return this.http.get<{ count: number }>(`${this.apiUrl}/notifications/unread-count`);
  }

  // Get daily limits for current dealer
  getDailyLimits(): Observable<DealerDailyLimits> {
    return this.http.get<DealerDailyLimits>(`${environment.apiUrl}/DealerPortal/daily-limits`);
  }
}