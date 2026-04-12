import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { ProductFilter } from '../models/product.model';
import { DealerProduct, DealerProductDetails } from '../models/order.model';

@Injectable({
  providedIn: 'root'
})
export class DealerProductsService {
  private apiUrl = `${environment.apiUrl}/Dealer/Products`;

  constructor(private http: HttpClient) {}

  getProducts(filter?: ProductFilter, pageNumber: number = 1, pageSize: number = 10): Observable<any> {
    let params: any = {
      pageNumber: pageNumber.toString(),
      pageSize: pageSize.toString()
    };
    
    if (filter) {
      if (filter.search) params.search = filter.search;
      if (filter.category) params.category = filter.category;
      if (filter.minPrice) params.minPrice = filter.minPrice;
      if (filter.maxPrice) params.maxPrice = filter.maxPrice;
      if (filter.sortBy) params.sortBy = filter.sortBy;
      if (filter.sortOrder) params.sortOrder = filter.sortOrder;
    }

    return this.http.get<any>(this.apiUrl, { params });
  }

  getProductById(dealerProductID: number): Observable<DealerProductDetails> {
    return this.http.get<DealerProductDetails>(`${this.apiUrl}/${dealerProductID}`);
  }

  getProductCategories(): Observable<string[]> {
    return this.http.get<string[]>(`${this.apiUrl}/categories`);
  }

  checkProductAvailability(dealerProductID: number, quantity: number): Observable<boolean> {
    return this.http.post<boolean>(`${this.apiUrl}/${dealerProductID}/check-availability`, { quantity });
  }

  getProductImage(imagePath: string): string {
    if (!imagePath) {
      return 'assets/images/default-product.jpg';
    }
    
    if (imagePath.startsWith('http')) {
      return imagePath;
    }
    
    return `${environment.apiUrl.replace('/api', '')}/${imagePath}`;
  }
}
