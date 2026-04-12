import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface SupportTicket {
  ticketId: number;
  title: string;
  description: string;
  dealerId: number;
  category: string;
  priority: 'Low' | 'Medium' | 'High' | 'Critical';
  status: 'Open' | 'In Progress' | 'Resolved' | 'Closed';
  createdDate: string;
  updatedDate: string;
  resolvedDate?: string;
  assignedTo?: string;
  responses?: TicketResponse[];
}

export interface TicketResponse {
  responseId: number;
  ticketId: number;
  message: string;
  createdBy: string;
  createdDate: string;
  isInternal: boolean;
}

export interface TicketSearchParams {
  page?: number;
  pageSize?: number;
  searchTerm?: string;
  dealerId?: number;
  status?: string;
  priority?: string;
  category?: string;
}

@Injectable({
  providedIn: 'root'
})
export class DealerSupportTicketService {
  private apiUrl = `${environment.apiUrl}/Admin/DealerManagement`;

  constructor(private http: HttpClient) { }

  getAllTickets(params: TicketSearchParams): Observable<any> {
    let httpParams = new HttpParams();
    
    if (params.page) httpParams = httpParams.set('pageNumber', params.page.toString());
    if (params.pageSize) httpParams = httpParams.set('pageSize', params.pageSize.toString());
    if (params.searchTerm) httpParams = httpParams.set('search', params.searchTerm);
    if (params.dealerId) httpParams = httpParams.set('dealerId', params.dealerId.toString());
    if (params.status) httpParams = httpParams.set('status', params.status);
    if (params.priority) httpParams = httpParams.set('priority', params.priority);
    if (params.category) httpParams = httpParams.set('category', params.category);

    return this.http.get(`${this.apiUrl}/SupportTickets`, { params: httpParams });
  }

  getTicketById(ticketId: number): Observable<SupportTicket> {
    return this.http.get<SupportTicket>(`${this.apiUrl}/SupportTickets/${ticketId}`);
  }

  addResponse(request: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/SupportTickets/AddResponse`, {
      ticketId: request.ticketId,
      message: request.message,
      isInternal: request.isInternal,
      newStatus: request.newStatus
    });
  }

  resolveTicket(ticketId: number): Observable<any> {
    return this.http.put(`${this.apiUrl}/SupportTickets/UpdateStatus`, {
      ticketId: ticketId,
      status: 'Resolved'
    });
  }

  reopenTicket(ticketId: number): Observable<any> {
    return this.http.put(`${this.apiUrl}/SupportTickets/UpdateStatus`, {
      ticketId: ticketId,
      status: 'Open'
    });
  }

  updateTicketStatus(ticketId: number, status: string): Observable<any> {
    return this.http.put(`${this.apiUrl}/SupportTickets/UpdateStatus`, {
      ticketId: ticketId,
      status: status
    });
  }
}