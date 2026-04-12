import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { DealerOrder, OrderStatus } from '../models/order.model';

@Injectable({
  providedIn: 'root'
})
export class DealerOrdersService {
  private apiUrl = `${environment.apiUrl}/DealerOrder`;

  constructor(private http: HttpClient) {}

  createOrder(order: DealerOrder): Observable<{ orderId: number; orderNumber: string }> {
    return this.http.post<{ orderId: number; orderNumber: string }>(`${this.apiUrl}/Create`, order);
  }

  getOrderHistory(dealerId?: number): Observable<any> {
    // Use the dealer-specific endpoint that filters by logged-in dealer
    return this.http.get<any>(`${this.apiUrl}/GetMyOrders`);
  }

  getOrderById(orderId: number): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/GetById/${orderId}`);
  }

  updateOrderStatus(orderId: number, status: OrderStatus): Observable<any> {
    return this.http.put(`${this.apiUrl}/${orderId}/status`, { status });
  }

  cancelOrder(orderId: number, reason?: string): Observable<any> {
    return this.http.put(`${this.apiUrl}/${orderId}/cancel`, { reason });
  }

  getOrdersByDateRange(startDate: Date, endDate: Date): Observable<DealerOrder[]> {
    return this.http.get<DealerOrder[]>(`${this.apiUrl}/date-range`, {
      params: {
        startDate: startDate.toISOString(),
        endDate: endDate.toISOString()
      }
    });
  }

  reorderPreviousOrder(orderId: number): Observable<any> {
    return this.http.post(`${this.apiUrl}/${orderId}/reorder`, {});
  }

  downloadOrderInvoice(orderId: number): Observable<Blob> {
    return this.http.get(`${this.apiUrl}/${orderId}/invoice`, { 
      responseType: 'blob' 
    });
  }

  getOrderStatusOptions(): { value: OrderStatus; label: string; color: string }[] {
    return [
      { value: OrderStatus.PENDING, label: 'Pending', color: 'warning' },
      { value: OrderStatus.CONFIRMED, label: 'Confirmed', color: 'info' },
      { value: OrderStatus.PROCESSING, label: 'Processing', color: 'primary' },
      { value: OrderStatus.SHIPPED, label: 'Shipped', color: 'secondary' },
      { value: OrderStatus.DELIVERED, label: 'Delivered', color: 'success' },
      { value: OrderStatus.CANCELLED, label: 'Cancelled', color: 'danger' }
    ];
  }
}
