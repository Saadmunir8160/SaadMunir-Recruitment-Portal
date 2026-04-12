import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  ApiResponse,
  PaginatedResponse,
  PaginationParams,
  VacancyDto,
  VacancyDetailDto
} from '../../shared/interfaces/models';

@Injectable({ providedIn: 'root' })
export class VacancyService {
  private apiUrl = environment.recruitmentApiUrl;

  constructor(private http: HttpClient) {}

  getPublishedVacancies(params: PaginationParams & { search?: string }): Observable<PaginatedResponse<VacancyDto>> {
    let httpParams = new HttpParams()
      .set('pageNumber', params.pageNumber.toString())
      .set('pageSize', params.pageSize.toString());

    if (params.search) {
      httpParams = httpParams.set('search', params.search);
    }

    return this.http.get<PaginatedResponse<VacancyDto>>(`${this.apiUrl}/vacancies/published`, { params: httpParams });
  }

  getVacancyById(id: number): Observable<ApiResponse<VacancyDetailDto>> {
    return this.http.get<ApiResponse<VacancyDetailDto>>(`${this.apiUrl}/vacancies/${id}`);
  }
}
