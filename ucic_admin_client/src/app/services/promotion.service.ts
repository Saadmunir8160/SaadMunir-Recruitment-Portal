import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { PaginatedResponse } from '../shared/interfaces/paginated-response.interface';

export interface Promotion {
  promotionId: number;
  coverageAreaId: number;
  code: string;
  discountPercentage: number;
  validFrom: Date;
  validTo: Date;
}

@Injectable({
  providedIn: 'root'
})
export class PromotionService {
  private apiUrl = `${environment.apiUrl}/Promotion`;

  constructor(private http: HttpClient) { }

  getAllPromotions(pageNumber?: number, pageSize?: number): Observable<any | PaginatedResponse<Promotion>> {
    const pn = pageNumber ?? 1;
    const ps = pageSize === undefined ? 1000000 : (pageSize === 0 ? 1000000 : pageSize);
    const url = `${this.apiUrl}/GetAll?pageNumber=${pn}&pageSize=${ps}`;

    return this.http.get<PaginatedResponse<Promotion>>(url);
  }

  createPromotion(promotion: any): Observable<any> {
    const command = {
      code: promotion.code,
      coverageAreaId: promotion.coverageAreaId,
      discountPercentage: promotion.discountPercentage,
      validFrom: promotion.validFrom,
      validTo: promotion.validTo
    };
    return this.http.post(`${this.apiUrl}/Create`, command);
  }

  updatePromotion(promotion: any): Observable<any> {
    const command = {
      promotionId: promotion.promotionId,
      code: promotion.code,
      coverageAreaId: promotion.coverageAreaId,
      discountPercentage: promotion.discountPercentage,
      validFrom: promotion.validFrom,
      validTo: promotion.validTo
    };
    return this.http.put(`${this.apiUrl}/Update`, command);
  }

  deletePromotion(id: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/Delete/${id}`);
  }
} 