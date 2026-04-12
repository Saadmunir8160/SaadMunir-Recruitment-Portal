import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  ApiResponse,
  PaginatedResponse,
  PaginationParams,
  ApplicationDto,
  ApplicationDetailDto,
  CreateApplicationDto
} from '../../shared/interfaces/models';

@Injectable({ providedIn: 'root' })
export class ApplicationService {
  private apiUrl = environment.recruitmentApiUrl;

  constructor(private http: HttpClient) {}

  applyToVacancy(vacancyId: number): Observable<ApiResponse<number>> {
    return this.http.post<ApiResponse<number>>(`${this.apiUrl}/applications`, { vacancyId });
  }

  getMyApplications(params: PaginationParams): Observable<PaginatedResponse<ApplicationDto>> {
    const httpParams = new HttpParams()
      .set('pageNumber', params.pageNumber.toString())
      .set('pageSize', params.pageSize.toString());

    return this.http.get<PaginatedResponse<ApplicationDto>>(`${this.apiUrl}/applications/my-applications`, { params: httpParams });
  }

  getApplicationById(id: number): Observable<ApiResponse<ApplicationDetailDto>> {
    return this.http.get<ApiResponse<ApplicationDetailDto>>(`${this.apiUrl}/applications/${id}`);
  }
}
