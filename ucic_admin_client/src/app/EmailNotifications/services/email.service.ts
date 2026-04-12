import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class EmailService {

  private apiUrl = `${environment.apiUrl}/EmailNotification`;
  private apiUrlDepartment = `${environment.apiUrl}/Department`;
  
  constructor(private http: HttpClient) {}

  getall(): Observable<any[]> {
    return this.http.get<any>(`${this.apiUrl}/GetAll`);
  }

  getallDepartments(): Observable<any[]> {
    return this.http.get<any>(`${this.apiUrlDepartment}/GetAll`);
  }

  delete(id: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/Delete/${id}`);
  }

  createItem(data: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/Create`, data);
  }
  
}
