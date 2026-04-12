import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { News } from '../news.model';
import { PaginatedResponse } from '../../shared/interfaces/paginated-response.interface';

@Injectable({
  providedIn: 'root'
})
export class NewsService {
  private apiUrl = `${environment.apiUrl}/News`;
  
  constructor(private http: HttpClient) {}

  getNews(pageNumber?: number, pageSize?: number): Observable<News[] | PaginatedResponse<News>> {
    const pn = pageNumber ?? 1;
    const ps = pageSize === undefined ? 1000000 : (pageSize === 0 ? 1000000 : pageSize);
    const url = `${this.apiUrl}/GetAll?pageNumber=${pn}&pageSize=${ps}`;

    return this.http.get<PaginatedResponse<News>>(url);
  }

  getNewsById(id: number): Observable<News> {
    return this.http.get<News>(`${this.apiUrl}/GetNews/${id}`);
  }

  addNews(news: FormData): Observable<any> {
    return this.http.post(`${this.apiUrl}/Create`, news);
  }

  updateNews(id: number, news: FormData): Observable<any> {
    return this.http.put(`${this.apiUrl}/Update/${id}`, news);
  }
  
  deleteNews(id: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/Delete/${id}`);
  }
}
