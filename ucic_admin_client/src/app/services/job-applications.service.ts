import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../environments/environment';
import { Observable, map, catchError } from 'rxjs';
import { PaginatedResponse } from '../shared/interfaces/paginated-response.interface';

export interface JobApplication {
  jobApplicationsId: number;
  fullName: string;
  email: string;
  phone: string;
  resumePath: string;
  jobTitle: string;
}

@Injectable({ providedIn: 'root' })
export class JobApplicationsService {
  private apiUrl = `${environment.apiUrl}/JobApplications`;

  constructor(private http: HttpClient) {}

  getAll(pageNumber?: number, pageSize?: number): Observable<JobApplication[] | PaginatedResponse<JobApplication>> {
    const pn = pageNumber ?? 1;
    const ps = pageSize === undefined ? 1000000 : (pageSize === 0 ? 1000000 : pageSize);
    const url = `${this.apiUrl}/GetAll?pageNumber=${pn}&pageSize=${ps}`;

    return this.http.get<PaginatedResponse<any>>(url).pipe(
        map(response => ({
          ...response,
          data: response.data.map((item: any) => ({
            jobApplicationsId: item.jobApplicationsId,
            fullName: item.fullName,
            email: item.email,
            phone: item.phone,
            resumePath: item.resumePath,
            jobTitle: item.title || item.jobTitle // Handle both possible property names
          }))
        })),
        catchError(error => {
          console.error('Error fetching job applications:', error);
          throw error;
        })
    );
  }

  downloadResume(id: number): Observable<Blob> {
    return this.http.get(`${this.apiUrl}/DownloadResume/${id}`, { responseType: 'blob' });
  }
} 