import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface SupportTicket {
  id: number;
  ticketNumber: string;
  subject: string;
  description: string;
  category: TicketCategory;
  priority: TicketPriority;
  status: TicketStatus;
  dealerId: number;
  assignedTo?: string;
  createdAt: Date;
  updatedAt: Date;
  resolvedAt?: Date;
  attachments: TicketAttachment[];
  messages: TicketMessage[];
  orderId?: number; // If ticket is related to an order
  productId?: number; // If ticket is related to a product
}

export interface CreateTicketRequest {
  subject: string;
  description: string;
  category: string; // Allow string values for flexibility
  priority: string; // Allow string values for flexibility  
  orderId?: number;
  productId?: number;
  attachments?: File[];
}

export interface TicketMessage {
  id: number;
  ticketId: number;
  message: string;
  isFromDealer: boolean;
  senderName: string;
  sentAt: Date;
  attachments: TicketAttachment[];
}

export interface TicketAttachment {
  id: number;
  fileName: string;
  filePath: string;
  fileSize: number;
  uploadedAt: Date;
}

export interface FAQ {
  id: number;
  question: string;
  answer: string;
  category: string;
  tags: string[];
  isHelpful?: boolean;
  helpfulCount: number;
  viewCount: number;
  lastUpdated: Date;
}

export interface ContactInfo {
  supportEmail: string;
  supportPhone: string;
  emergencyPhone: string;
  businessHours: string;
  address: string;
  socialMedia: {
    facebook?: string;
    twitter?: string;
    linkedin?: string;
    whatsapp?: string;
  };
}

export enum TicketCategory {
  ORDER_INQUIRY = 'order_inquiry',
  PRODUCT_INQUIRY = 'product_inquiry',
  PAYMENT_ISSUE = 'payment_issue',
  DELIVERY_ISSUE = 'delivery_issue',
  TECHNICAL_SUPPORT = 'technical_support',
  ACCOUNT_ISSUE = 'account_issue',
  COMPLAINT = 'complaint',
  SUGGESTION = 'suggestion',
  OTHER = 'other'
}

export enum TicketPriority {
  LOW = 'low',
  MEDIUM = 'medium',
  HIGH = 'high',
  URGENT = 'urgent'
}

export enum TicketStatus {
  OPEN = 'open',
  IN_PROGRESS = 'in_progress',
  WAITING_FOR_CUSTOMER = 'waiting_for_customer',
  RESOLVED = 'resolved',
  CLOSED = 'closed',
  CANCELLED = 'cancelled'
}

@Injectable({
  providedIn: 'root'
})
export class DealerSupportService {
  private apiUrl = `${environment.apiUrl}/DealerSupport`;

  constructor(private http: HttpClient) {}

  // Create new support ticket
  createTicket(ticketData: CreateTicketRequest): Observable<any> {
    // Send JSON to match backend expectations [FromBody]
    const payload = {
      subject: ticketData.subject,
      description: ticketData.description,
      category: ticketData.category,
      priority: ticketData.priority,
      orderId: ticketData.orderId || null,
      productId: ticketData.productId || null
    };
    
    // Angular HttpClient automatically sets Content-Type for JSON objects
    return this.http.post<any>(`${this.apiUrl}/tickets`, payload);
  }

  // Get all tickets for current dealer
  getTickets(status?: TicketStatus, category?: TicketCategory): Observable<SupportTicket[]> {
    let params: any = {};
    if (status) params.status = status;
    if (category) params.category = category;
    
    return this.http.get<SupportTicket[]>(`${this.apiUrl}/tickets`, { params });
  }

  // Get ticket by ID
  getTicketById(ticketId: number): Observable<SupportTicket> {
    return this.http.get<SupportTicket>(`${this.apiUrl}/tickets/${ticketId}`);
  }

  // Add message to ticket
  addMessageToTicket(ticketId: number, message: string, attachments?: File[]): Observable<TicketMessage> {
    // Backend expects JSON payload with AddMessageToTicketDTO format
    const payload = {
      message: message
    };
    
    // Note: File attachments are not currently supported by the backend AddMessageToTicketCommand
    // The backend would need to be updated to handle file uploads if attachments are required
    if (attachments && attachments.length > 0) {
      console.warn('File attachments are not currently supported by the backend API');
    }
    
    return this.http.post<TicketMessage>(`${this.apiUrl}/tickets/${ticketId}/messages`, payload);
  }

  // Update ticket status (close/reopen)
  updateTicketStatus(ticketId: number, status: TicketStatus): Observable<any> {
    return this.http.put(`${this.apiUrl}/tickets/${ticketId}/status`, { status });
  }

  // Get FAQs
  getFAQs(category?: string, searchTerm?: string): Observable<FAQ[]> {
    let params: any = {};
    if (category) params.category = category;
    if (searchTerm) params.search = searchTerm;
    
    return this.http.get<FAQ[]>(`${this.apiUrl}/faqs`, { params });
  }

  // Get FAQ categories
  getFAQCategories(): Observable<string[]> {
    return this.http.get<string[]>(`${this.apiUrl}/faqs/categories`);
  }

  // Mark FAQ as helpful
  markFAQAsHelpful(faqId: number, isHelpful: boolean): Observable<any> {
    return this.http.post(`${this.apiUrl}/faqs/${faqId}/feedback`, { isHelpful });
  }

  // Get contact information
  getContactInfo(): Observable<ContactInfo> {
    return this.http.get<ContactInfo>(`${this.apiUrl}/contact-info`);
  }

  // Download ticket attachment
  downloadAttachment(attachmentId: number): Observable<Blob> {
    return this.http.get(`${this.apiUrl}/attachments/${attachmentId}/download`, {
      responseType: 'blob'
    });
  }

  // Get ticket statistics
  getTicketStats(): Observable<{
    totalTickets: number;
    openTickets: number;
    resolvedTickets: number;
    averageResolutionTime: number;
    ticketsByCategory: { [key: string]: number };
  }> {
    return this.http.get<any>(`${this.apiUrl}/tickets/stats`);
  }

  // Request callback
  requestCallback(contactNumber: string, preferredTime: string, message?: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/request-callback`, {
      contactNumber,
      preferredTime,
      message
    });
  }

  // Submit feedback
  submitFeedback(rating: number, feedback: string, category: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/feedback`, {
      rating,
      feedback,
      category
    });
  }

  // Get support categories and priorities for dropdowns
  getSupportMetadata(): Observable<{
    categories: { value: TicketCategory; label: string }[];
    priorities: { value: TicketPriority; label: string; color: string }[];
    statuses: { value: TicketStatus; label: string; color: string }[];
  }> {
    return this.http.get<any>(`${this.apiUrl}/metadata`);
  }

  // Get attachment URL
  getAttachmentUrl(filePath: string): string {
    if (!filePath) return '';
    if (filePath.startsWith('http')) return filePath;
    return `${environment.apiUrl.replace('/api', '')}/uploads/support/${filePath}`;
  }
}