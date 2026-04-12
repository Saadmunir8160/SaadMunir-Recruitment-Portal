import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { PaginatedResponse } from '../../shared/interfaces/paginated-response.interface';

@Injectable({
  providedIn: 'root'
})
export class OrdersService {

  private apiUrl = `${environment.apiUrl}/Order`;
  private apiCategoryUrl = `${environment.apiUrl}/Category`;
  
  constructor(private http: HttpClient) {}

  getOrders(pageNumber: number = 1, pageSize: number = 1000000): Observable<PaginatedResponse<any>> {
    return this.http.get<PaginatedResponse<any>>(`${this.apiUrl}/GetAllOrders?pageNumber=${pageNumber}&pageSize=${pageSize}`);
  }

  getDealers(): Observable<any[]> {
    return this.http.get<any>(`${this.apiCategoryUrl}/GetAllDealer`);
  }
  
  VerifyOrderPayment(id: number, order: FormData): Observable<any> {
    return this.http.put(`${this.apiUrl}/VerifyPaymentForOrder/${id}`, order);
  }

  OrderConfirm(id: number, order: FormData): Observable<any> {
    return this.http.put(`${this.apiUrl}/OrderConfirm/${id}`, order);
  }

  // Cancel an order with a reason
  cancelOrder(id: number, reason: string): Observable<any> {
    const body = { reason };
    return this.http.put(`${this.apiUrl}/CancelOrder/${id}`, body);
  }

  getOrderById(orderId: number): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/GetOrderByOrderId/${orderId}`);
  }
}
