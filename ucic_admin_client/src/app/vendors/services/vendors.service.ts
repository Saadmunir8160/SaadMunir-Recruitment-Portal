import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { PaginatedResponse } from '../../shared/interfaces/paginated-response.interface';

@Injectable({
  providedIn: 'root'
})
export class VendorsService {

  private apiUrl = `${environment.apiUrl}/Vendors`;
  
  constructor(private http: HttpClient) {}

  getVendors(pageNumber: number, pageSize: number): Observable<PaginatedResponse<any>> {
    const url = `${this.apiUrl}/GetAll`;
    return this.http.get<PaginatedResponse<any>>(url, {
      params: {
        pageNumber: pageNumber.toString(),
        pageSize: pageSize.toString()
      }
    });
  }
  
  approveVendors(id: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/Approve/${id}`);
  }

  deleteVendor(id: number): Observable<boolean> {
    return this.http.delete<boolean>(`${this.apiUrl}/${id}`);
  }
}
