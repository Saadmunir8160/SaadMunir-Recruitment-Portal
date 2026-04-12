import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../environments/environment';
import { Observable, map, catchError } from 'rxjs';
import { PaginatedResponse } from '../shared/interfaces/paginated-response.interface';

export interface CoverageArea {
  coverageAreaId: number;
  name: string;
  arabicName?: string;
  imagePath?: string; // Making it optional since it might not always be present
}

@Injectable({
  providedIn: 'root'
})
export class CoverageAreaService {
  private apiUrl = `${environment.apiUrl}/CoverageArea`;

  constructor(private http: HttpClient) { }

  getAllCoverageAreas(pageNumber?: number, pageSize?: number): Observable<CoverageArea[] | PaginatedResponse<CoverageArea>> {
    const pn = pageNumber ?? 1;
    const ps = pageSize === undefined ? 1000000 : (pageSize === 0 ? 1000000 : pageSize);
    const url = `${this.apiUrl}/GetAll?pageNumber=${pn}&pageSize=${ps}`;

    return this.http.get<PaginatedResponse<any>>(url).pipe(
      map(response => ({
        ...response,
        data: response.data.map((item: any) => ({
          coverageAreaId: item.coverageAreaId || item.id,
          name: item.name,
          arabicName: item.arabicName,
          imagePath: item.imagePath || null
        }))
      })),
      catchError(error => {
        console.error('Service - Error fetching coverage areas:', error);
        throw error;
      })
    );
  }

  getCoverageAreas(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/GetAll`);
  }

  createCoverageArea(coverageArea: Partial<CoverageArea>): Observable<any> {
    return this.http.post(`${this.apiUrl}/Create`, coverageArea);
  }

  updateCoverageArea(coverageArea: CoverageArea): Observable<any> {
    ;
    return this.http.put(`${this.apiUrl}/Update`, coverageArea);
  }

  deleteCoverageArea(id: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/Delete/${id}`);
  }
} 