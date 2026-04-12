import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { PaginatedResponse } from '../../shared/interfaces/paginated-response.interface';

@Injectable({
  providedIn: 'root'
})
export class JobsService {
  private apiUrl = `${environment.apiUrl}/Jobs`;
  
  constructor(private http: HttpClient) {}

  getJobs(pageNumber?: number, pageSize?: number): Observable<any[] | PaginatedResponse<any>> {
    const pn = pageNumber ?? 1;
    const ps = pageSize === undefined ? 1000000 : (pageSize === 0 ? 1000000 : pageSize);
    const url = `${this.apiUrl}/GetAll?pageNumber=${pn}&pageSize=${ps}`;

    return this.http.get<PaginatedResponse<any>>(url);
  }

  getJobById(id: number): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/GetJob/${id}`);
  }

  addJobs(Jobs: FormData): Observable<any> {
    return this.http.post(`${this.apiUrl}/Create`, Jobs);
  }

  updateJobs(id: number, Jobs: FormData): Observable<any> {
    return this.http.put(`${this.apiUrl}/Update/${id}`, Jobs);
  }
  
  deleteJobs(id: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/Delete/${id}`);
  }
}
