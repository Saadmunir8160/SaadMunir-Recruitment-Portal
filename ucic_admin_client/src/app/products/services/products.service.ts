import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { PaginatedResponse } from '../../shared/interfaces/paginated-response.interface';

@Injectable({
  providedIn: 'root'
})
export class ProductsService {

  private apiUrl = `${environment.apiUrl}/Product`;
  
  constructor(private http: HttpClient) {}

  getallProducts(pageNumber?: number, pageSize?: number): Observable<any[] | PaginatedResponse<any>> {
    const pn = pageNumber ?? 1;
    const ps = pageSize === undefined ? 1000000 : (pageSize === 0 ? 1000000 : pageSize);
    const url = `${this.apiUrl}/GetAllProductsForAdmin?pageNumber=${pn}&pageSize=${ps}`;

    return this.http.get<PaginatedResponse<any>>(url);
  }

  deleteProduct(productId: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/Delete/${productId}`);
  }

  updateProduct(productId: number, data: FormData): Observable<any> {
    return this.http.put(`${this.apiUrl}/Update/${productId}`, data);
  }

  getProductById(productId: number): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/GetProductByIdForAdmin/${productId}`);
  }
  
  createProduct(data: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/Create`, data);
  }
}
