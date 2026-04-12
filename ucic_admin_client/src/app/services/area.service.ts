import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface DealerArea {
  areaID: number;
  areaName: string;
  areaCode: string;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface DealerAreaResponse {
  success: boolean;
  data: DealerArea[];
  message: string;
}

export interface DealerAreaSingleResponse {
  success: boolean;
  data: DealerArea;
  message: string;
}

@Injectable({
  providedIn: 'root'
})
export class AreaService {
  private apiUrl = `${environment.apiUrl}/dealerarea`;

  constructor(private http: HttpClient) { }

  getAreas(pageNumber: number = 1, pageSize: number = 100): Observable<DealerAreaResponse> {
    return this.http.get<DealerAreaResponse>(`${this.apiUrl}/GetAll?pageNumber=${pageNumber}&pageSize=${pageSize}`);
  }

  getAreaById(areaID: number): Observable<DealerAreaSingleResponse> {
    return this.http.get<DealerAreaSingleResponse>(`${this.apiUrl}/GetById/${areaID}`);
  }

  createArea(areaName: string, areaCode: string): Observable<DealerAreaSingleResponse> {
    return this.http.post<DealerAreaSingleResponse>(`${this.apiUrl}/Create`, { areaName, areaCode });
  }

  updateArea(areaID: number, areaName: string, areaCode: string, isActive: boolean): Observable<DealerAreaSingleResponse> {
    return this.http.put<DealerAreaSingleResponse>(`${this.apiUrl}/Update/${areaID}`, { areaID, areaName, areaCode, isActive });
  }

  deleteArea(areaID: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/Delete/${areaID}`);
  }
}
