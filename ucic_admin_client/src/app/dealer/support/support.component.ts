import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators, FormsModule } from '@angular/forms';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { DealerSupportService } from '../services/dealer-support.service';

export interface SupportTicket {
  id: number;
  ticketNumber?: string;
  subject: string;
  category: string;
  priority: 'Low' | 'Medium' | 'High' | 'Urgent';
  status: 'Open' | 'In Progress' | 'Resolved' | 'Closed';
  createdDate: string;
  lastUpdated: string;
  description: string;
  responses: Array<{
    from: 'customer' | 'support';
    message: string;
    timestamp: string;
    attachments?: string[];
  }>;
}

export interface FAQ {
  id?: number;
  question: string;
  answer: string;
  category: string;
}

@Component({
  selector: 'app-support',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, FormsModule, TranslateModule],
  template: `
    <div >
      <div class="page-header">
        <div class="header-content">
          <h1 class="page-title">
            <span class="material-icons">support_agent</span>
            {{ 'DEALER.SUPPORT.TITLE' | translate }}
          </h1>
          <p class="page-subtitle">{{ 'DEALER.SUPPORT.SUBTITLE' | translate }}</p>
        </div>
      </div>

      <!-- Support Tabs -->
      <div class="support-tabs">
        <button 
          class="tab-btn"
          [class.active]="activeTab === 'contact'"
          (click)="activeTab = 'contact'">
          <span class="material-icons">contact_support</span>
          {{ 'DEALER.SUPPORT.TABS.CONTACT_SUPPORT' | translate }}
        </button>
        <button 
          class="tab-btn"
          [class.active]="activeTab === 'tickets'"
          (click)="activeTab = 'tickets'">
          <span class="material-icons">confirmation_number</span>
          {{ 'DEALER.SUPPORT.TABS.MY_TICKETS' | translate }}
        </button>
        <!-- <button 
          class="tab-btn"
          [class.active]="activeTab === 'faq'"
          (click)="activeTab = 'faq'">
          <span class="material-icons">help</span>
          {{ 'DEALER.SUPPORT.TABS.FAQ' | translate }}
        </button> -->
      </div>

      <!-- Contact Support Tab -->
      <div class="tab-content" *ngIf="activeTab === 'contact'">
        <div class="support-grid">
          <!-- Contact Form -->
          <div class="contact-form-section">
            <div class="section-card">
              <h3>
                <span class="material-icons">message</span>
                {{ 'DEALER.SUPPORT.CONTACT.CREATE_TICKET' | translate }}
              </h3>
              
              <form [formGroup]="supportForm" (ngSubmit)="submitTicket()">
                <div class="form-group">
                  <label for="subject">{{ 'DEALER.SUPPORT.CONTACT.FORM.SUBJECT' | translate }} *</label>
                  <input 
                    type="text" 
                    id="subject"
                    formControlName="subject"
                    [placeholder]="'DEALER.SUPPORT.CONTACT.FORM.PLACEHOLDERS.SUBJECT' | translate">
                </div>

                <div class="form-group">
                  <label for="category">{{ 'DEALER.SUPPORT.CONTACT.FORM.CATEGORY' | translate }} *</label>
                  <select id="category" formControlName="category">
                    <option value="">{{ 'DEALER.SUPPORT.CONTACT.FORM.SELECT_CATEGORY' | translate }}</option>
                    <!-- <option value="Order Issues">{{ 'DEALER.SUPPORT.CONTACT.FORM.CATEGORIES.ORDER_ISSUES' | translate }}</option> -->
                    <!-- <option value="Product Inquiry">{{ 'DEALER.SUPPORT.CONTACT.FORM.CATEGORIES.PRODUCT_INQUIRY' | translate }}</option> -->
                    <!-- <option value="Payment">{{ 'DEALER.SUPPORT.CONTACT.FORM.CATEGORIES.PAYMENT' | translate }}</option> -->
                    <!-- <option value="Shipping">{{ 'DEALER.SUPPORT.CONTACT.FORM.CATEGORIES.SHIPPING' | translate }}</option> -->
                    <option value="Account">{{ 'DEALER.SUPPORT.CONTACT.FORM.CATEGORIES.ACCOUNT' | translate }}</option>
                    <option value="Statement">{{ 'DEALER.SUPPORT.CONTACT.FORM.CATEGORIES.STATEMENT' | translate }}</option>
                    <!-- <option value="Technical">{{ 'DEALER.SUPPORT.CONTACT.FORM.CATEGORIES.TECHNICAL' | translate }}</option> -->
                    <!-- <option value="Other">{{ 'DEALER.SUPPORT.CONTACT.FORM.CATEGORIES.OTHER' | translate }}</option> -->
                  </select>
                </div>

                <div class="form-group">
                  <label for="priority">{{ 'DEALER.SUPPORT.CONTACT.FORM.PRIORITY' | translate }}</label>
                  <select id="priority" formControlName="priority">
                    <option value="Low">{{ 'DEALER.SUPPORT.CONTACT.FORM.PRIORITIES.LOW' | translate }}</option>
                    <option value="Medium">{{ 'DEALER.SUPPORT.CONTACT.FORM.PRIORITIES.MEDIUM' | translate }}</option>
                    <option value="High">{{ 'DEALER.SUPPORT.CONTACT.FORM.PRIORITIES.HIGH' | translate }}</option>
                    <option value="Urgent">{{ 'DEALER.SUPPORT.CONTACT.FORM.PRIORITIES.URGENT' | translate }}</option>
                  </select>
                </div>

                <div class="form-group">
                  <label for="description">{{ 'DEALER.SUPPORT.CONTACT.FORM.DESCRIPTION' | translate }} *</label>
                  <textarea 
                    id="description"
                    formControlName="description"
                    rows="6"
                    [placeholder]="'DEALER.SUPPORT.CONTACT.FORM.PLACEHOLDERS.DESCRIPTION' | translate"></textarea>
                </div>

                <div class="form-group">
                  <label>{{ 'DEALER.SUPPORT.CONTACT.FORM.ATTACHMENTS' | translate }}</label>
                  <div class="file-upload">
                    <span class="material-icons">attach_file</span>
                    <span>{{ 'DEALER.SUPPORT.CONTACT.FORM.UPLOAD_FILES' | translate }}</span>
                    <input type="file" multiple accept=".jpg,.jpeg,.png,.pdf,.doc,.docx">
                  </div>
                </div>

                <button type="submit" class="btn btn-primary" [disabled]="supportForm.invalid">
                  <span class="material-icons">send</span>
                  {{ 'DEALER.SUPPORT.CONTACT.FORM.SUBMIT_TICKET' | translate }}
                </button>
              </form>
            </div>
          </div>

          <!-- Contact Information -->
          <div class="contact-info-section">
            <div class="section-card">
              <h3>
                <span class="material-icons">contact_phone</span>
                {{ 'DEALER.SUPPORT.CONTACT.CONTACT_INFORMATION' | translate }}
              </h3>
              
              <div class="contact-methods">
                <div class="contact-method">
                  <div class="method-icon phone">
                    <span class="material-icons">phone</span>
                  </div>
                  <div class="method-content">
                    <h4>{{ 'DEALER.SUPPORT.CONTACT.PHONE_SUPPORT' | translate }}</h4>
                    <p style="direction: ltr;">
                      <span>+966 9200 26267</span>
                    </p>
                    <!-- <span class="availability">{{ 'DEALER.SUPPORT.CONTACT.PHONE_HOURS' | translate }}</span> -->
                  </div>
                </div>

                <div class="contact-method">
                  <div class="method-icon email">
                    <span class="material-icons">email</span>
                  </div>
                  <div class="method-content">
                    <h4>{{ 'DEALER.SUPPORT.CONTACT.EMAIL_SUPPORT' | translate }}</h4>
                    <p>info&#64;unitedcement.com.sa</p>
                    <span class="availability">{{ 'DEALER.SUPPORT.CONTACT.EMAIL_RESPONSE' | translate }}</span>
                  </div>
                </div>

                <!-- <div class="contact-method">
                  <div class="method-icon chat">
                    <span class="material-icons">chat</span>
                  </div>
                  <div class="method-content">
                    <h4>{{ 'DEALER.SUPPORT.CONTACT.LIVE_CHAT' | translate }}</h4>
                    <p>{{ 'DEALER.SUPPORT.CONTACT.CHAT_DESCRIPTION' | translate }}</p>
                    <button class="btn btn-outline-primary btn-sm">
                      <span class="material-icons">chat_bubble</span>
                      {{ 'DEALER.SUPPORT.CONTACT.START_CHAT' | translate }}
                    </button>
                  </div>
                </div> -->

                <div class="contact-method">
                  <div class="method-icon whatsapp">
                    <span class="material-icons">message</span>
                  </div>
                  <div class="method-content">
                    <h4>{{ 'DEALER.SUPPORT.CONTACT.WHATSAPP' | translate }}</h4>
                    <p style="direction: ltr;">+966 9200 26267</p>
                    <!-- <button class="btn btn-outline-success btn-sm">
                      <span class="material-icons">open_in_new</span>
                      {{ 'DEALER.SUPPORT.CONTACT.OPEN_WHATSAPP' | translate }}
                    </button> -->
                  </div>
                </div>
              </div>
            </div>

            <!-- Quick Links -->
            <!-- <div class="section-card">
              <h3>
                <span class="material-icons">link</span>
                {{ 'DEALER.SUPPORT.CONTACT.QUICK_LINKS' | translate }}
              </h3>
              
              <div class="quick-links">
                <a href="#" class="quick-link">
                  <span class="material-icons">local_shipping</span>
                  {{ 'DEALER.SUPPORT.CONTACT.QUICK_LINKS_ITEMS.TRACK_ORDER' | translate }}
                </a>
                <a href="#" class="quick-link">
                  <span class="material-icons">receipt</span>
                  {{ 'DEALER.SUPPORT.CONTACT.QUICK_LINKS_ITEMS.DOWNLOAD_INVOICE' | translate }}
                </a>
                <a href="#" class="quick-link">
                  <span class="material-icons">refresh</span>
                  {{ 'DEALER.SUPPORT.CONTACT.QUICK_LINKS_ITEMS.RETURN_EXCHANGE' | translate }}
                </a>
                <a href="#" class="quick-link">
                  <span class="material-icons">account_circle</span>
                  {{ 'DEALER.SUPPORT.CONTACT.QUICK_LINKS_ITEMS.UPDATE_PROFILE' | translate }}
                </a>
              </div>
            </div> -->
          </div>
        </div>
      </div>

      <!-- My Tickets Tab -->
      <div class="tab-content" *ngIf="activeTab === 'tickets'">
        <div class="tickets-header">
          <h3>{{ 'DEALER.SUPPORT.TICKETS.TITLE' | translate }}</h3>
          <button class="btn btn-primary" (click)="activeTab = 'contact'">
            <span class="material-icons">add</span>
            {{ 'DEALER.SUPPORT.TICKETS.NEW_TICKET' | translate }}
          </button>
        </div>

        <!-- Search and Filters -->
        <div class="tickets-controls">
          <div class="search-section">
            <div class="search-input">
              <span class="material-icons">search</span>
              <input 
                type="text" 
                [placeholder]="'DEALER.SUPPORT.TICKETS.SEARCH_PLACEHOLDER' | translate"
                [(ngModel)]="searchTerm"
                (input)="applyFilters()">
            </div>
          </div>
          
          <div class="filters-section">
            <div class="filter-group">
              <label>{{ 'DEALER.SUPPORT.TICKETS.FILTERS.STATUS' | translate }}:</label>
              <select [(ngModel)]="selectedStatus" (change)="applyFilters()">
                <option value="">{{ 'DEALER.SUPPORT.TICKETS.FILTERS.ALL_STATUS' | translate }}</option>
                <option value="Open">{{ 'DEALER.SUPPORT.TICKETS.STATUS.OPEN' | translate }}</option>
                <option value="In Progress">{{ 'DEALER.SUPPORT.TICKETS.STATUS.IN_PROGRESS' | translate }}</option>
                <option value="Resolved">{{ 'DEALER.SUPPORT.TICKETS.STATUS.RESOLVED' | translate }}</option>
                <option value="Closed">{{ 'DEALER.SUPPORT.TICKETS.STATUS.CLOSED' | translate }}</option>
              </select>
            </div>
            
            <div class="filter-group">
              <label>{{ 'DEALER.SUPPORT.TICKETS.FILTERS.PRIORITY' | translate }}:</label>
              <select [(ngModel)]="selectedPriority" (change)="applyFilters()">
                <option value="">{{ 'DEALER.SUPPORT.TICKETS.FILTERS.ALL_PRIORITY' | translate }}</option>
                <option value="Low">{{ 'DEALER.SUPPORT.CONTACT.FORM.PRIORITIES.LOW' | translate }}</option>
                <option value="Medium">{{ 'DEALER.SUPPORT.CONTACT.FORM.PRIORITIES.MEDIUM' | translate }}</option>
                <option value="High">{{ 'DEALER.SUPPORT.CONTACT.FORM.PRIORITIES.HIGH' | translate }}</option>
                <option value="Urgent">{{ 'DEALER.SUPPORT.CONTACT.FORM.PRIORITIES.URGENT' | translate }}</option>
              </select>
            </div>
            
            <div class="filter-group">
              <label>{{ 'DEALER.SUPPORT.TICKETS.FILTERS.CATEGORY' | translate }}:</label>
              <select [(ngModel)]="selectedCategory" (change)="applyFilters()">
                <option value="">{{ 'DEALER.SUPPORT.TICKETS.FILTERS.ALL_CATEGORIES' | translate }}</option>
                <option value="Order Issues">{{ 'DEALER.SUPPORT.CONTACT.FORM.CATEGORIES.ORDER_ISSUES' | translate }}</option>
                <option value="Product Inquiry">{{ 'DEALER.SUPPORT.CONTACT.FORM.CATEGORIES.PRODUCT_INQUIRY' | translate }}</option>
                <option value="Payment">{{ 'DEALER.SUPPORT.CONTACT.FORM.CATEGORIES.PAYMENT' | translate }}</option>
                <option value="Shipping">{{ 'DEALER.SUPPORT.CONTACT.FORM.CATEGORIES.SHIPPING' | translate }}</option>
                <option value="Account">{{ 'DEALER.SUPPORT.CONTACT.FORM.CATEGORIES.ACCOUNT' | translate }}</option>
                <option value="Technical">{{ 'DEALER.SUPPORT.CONTACT.FORM.CATEGORIES.TECHNICAL' | translate }}</option>
                <option value="Other">{{ 'DEALER.SUPPORT.CONTACT.FORM.CATEGORIES.OTHER' | translate }}</option>
              </select>
            </div>
            
            <button class="btn btn-secondary btn-clear" (click)="clearFilters()">
              <span class="material-icons">clear</span>
              {{ 'DEALER.SUPPORT.TICKETS.FILTERS.CLEAR_FILTERS' | translate }}
            </button>
          </div>
        </div>

        <!-- Results Info -->
        <div class="results-info" *ngIf="filteredTickets.length > 0">
          <span>{{ 'DEALER.SUPPORT.TICKETS.PAGINATION.SHOWING' | translate }} {{ getDisplayStart() }} {{ 'DEALER.SUPPORT.TICKETS.PAGINATION.TO' | translate }} {{ getDisplayEnd() }} {{ 'DEALER.SUPPORT.TICKETS.PAGINATION.OF' | translate }} {{ filteredTickets.length }} {{ 'DEALER.SUPPORT.TICKETS.PAGINATION.TICKETS' | translate }}</span>
        </div>

        <div class="tickets-table-container" *ngIf="paginatedTickets.length > 0">
          <div class="table-responsive">
            <table class="table table-striped table-hover">
              <thead class="table-dark" style="background-color: #323a46 !important;">
                <tr style="background-color: #323a46 !important;">
                  <th style="background-color: #323a46 !important; color: #ffffff !important; border-color: #474e59 !important; text-transform: none !important;">{{ 'DEALER.SUPPORT.TICKETS.TABLE.TICKET_NUMBER' | translate }}</th>
                  <th style="background-color: #323a46 !important; color: #ffffff !important; border-color: #474e59 !important; text-transform: none !important;">{{ 'DEALER.SUPPORT.TICKETS.TABLE.SUBJECT' | translate }}</th>
                  <th style="background-color: #323a46 !important; color: #ffffff !important; border-color: #474e59 !important; text-transform: none !important;">{{ 'DEALER.SUPPORT.TICKETS.TABLE.CATEGORY' | translate }}</th>
                  <th style="background-color: #323a46 !important; color: #ffffff !important; border-color: #474e59 !important; text-transform: none !important;">{{ 'DEALER.SUPPORT.TICKETS.TABLE.PRIORITY' | translate }}</th>
                  <th style="background-color: #323a46 !important; color: #ffffff !important; border-color: #474e59 !important; text-transform: none !important;">{{ 'DEALER.SUPPORT.TICKETS.TABLE.STATUS' | translate }}</th>
                  <th style="background-color: #323a46 !important; color: #ffffff !important; border-color: #474e59 !important; text-transform: none !important;">{{ 'DEALER.SUPPORT.TICKETS.TABLE.CREATED' | translate }}</th>
                  <th style="background-color: #323a46 !important; color: #ffffff !important; border-color: #474e59 !important; text-transform: none !important;">{{ 'DEALER.SUPPORT.TICKETS.TABLE.LAST_UPDATED' | translate }}</th>
                  <th style="background-color: #323a46 !important; color: #ffffff !important; border-color: #474e59 !important; text-transform: none !important;">{{ 'DEALER.SUPPORT.TICKETS.TABLE.ACTION' | translate }}</th>
                </tr>
              </thead>
              <tbody>
                <tr *ngFor="let ticket of paginatedTickets" class="ticket-row">
                  <td class="ticket-number">#{{ ticket.id }}</td>
                  <td class="ticket-subject">
                    <div class="subject-content">
                      <span class="subject-title">{{ ticket.subject }}</span>
                      <p class="subject-description">{{ ticket.description | slice:0:80 }}{{ ticket.description.length > 80 ? '...' : '' }}</p>
                    </div>
                  </td>
                  <td class="ticket-category">{{ 'DEALER.SUPPORT.CONTACT.FORM.CATEGORIES.' + ticket.category.toUpperCase().replace(' ', '_') | translate }}</td>
                  <td class="ticket-priority">
                    <span class="priority-badge" [ngClass]="'priority-' + ticket.priority.toLowerCase()">
                      {{ 'DEALER.SUPPORT.CONTACT.FORM.PRIORITIES.' + ticket.priority.toUpperCase() | translate }}
                    </span>
                  </td>
                  <td class="ticket-status">
                    <span class="status-badge" [ngClass]="'status-' + ticket.status.toLowerCase().replace(' ', '-')">
                      {{ 'DEALER.SUPPORT.TICKETS.STATUS.' + ticket.status.toUpperCase().replace(' ', '_') | translate }}
                    </span>
                  </td>
                  <td class="ticket-created">{{ ticket.createdDate | date:'MMM dd, yyyy' }}</td>
                  <td class="ticket-updated">{{ ticket.lastUpdated | date:'MMM dd, yyyy' }}</td>
                  <td class="ticket-actions">
                    <button class="btn btn-view" (click)="viewTicketDetails(ticket)" [title]="'DEALER.SUPPORT.TICKETS.TABLE.VIEW_DETAILS' | translate">
                      <span class="material-icons">visibility</span>
                      {{ 'DEALER.SUPPORT.TICKETS.TABLE.VIEW_DETAILS' | translate }}
                    </button>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>

        <!-- Pagination -->
        <div class="pagination-controls" *ngIf="filteredTickets.length > 0">
          <div class="pagination-info">
            <div class="page-size-selector">
              <label>{{ 'DEALER.SUPPORT.TICKETS.PAGINATION.SHOW' | translate }}:</label>
              <select [(ngModel)]="pageSize" (change)="onPageSizeChange()">
                <option value="5">5</option>
                <option value="10">10</option>
                <option value="25">25</option>
                <option value="50">50</option>
              </select>
              <span>{{ 'DEALER.SUPPORT.TICKETS.PAGINATION.PER_PAGE' | translate }}</span>
            </div>
          </div>
          
          <div class="pagination-nav">
            <button 
              class="btn btn-pagination" 
              [disabled]="currentPage === 1" 
              (click)="goToPage(currentPage - 1)">
              <span class="material-icons">chevron_left</span>
              {{ 'DEALER.SUPPORT.TICKETS.PAGINATION.PREVIOUS' | translate }}
            </button>
            
            <div class="page-numbers">
              <button 
                *ngFor="let page of getPageNumbers()" 
                class="btn btn-page" 
                [class.active]="page === currentPage"
                (click)="goToPage(page)">
                {{ page }}
              </button>
            </div>
            
            <button 
              class="btn btn-pagination" 
              [disabled]="currentPage === totalPages" 
              (click)="goToPage(currentPage + 1)">
              {{ 'DEALER.SUPPORT.TICKETS.PAGINATION.NEXT' | translate }}
              <span class="material-icons">chevron_right</span>
            </button>
          </div>
        </div>

        <div class="empty-state" *ngIf="filteredTickets.length === 0 && tickets.length > 0">
          <span class="material-icons">search_off</span>
          <h3>{{ 'DEALER.SUPPORT.TICKETS.EMPTY_STATES.NO_RESULTS_TITLE' | translate }}</h3>
          <p>{{ 'DEALER.SUPPORT.TICKETS.EMPTY_STATES.NO_RESULTS_MESSAGE' | translate }}</p>
          <button class="btn btn-secondary" (click)="clearFilters()">
            <span class="material-icons">clear</span>
            {{ 'DEALER.SUPPORT.TICKETS.FILTERS.CLEAR_FILTERS' | translate }}
          </button>
        </div>

        <div class="empty-state" *ngIf="tickets.length === 0">
          <span class="material-icons">confirmation_number</span>
          <h3>{{ 'DEALER.SUPPORT.TICKETS.EMPTY_STATES.NO_TICKETS_TITLE' | translate }}</h3>
          <p>{{ 'DEALER.SUPPORT.TICKETS.EMPTY_STATES.NO_TICKETS_MESSAGE' | translate }}</p>
          <button class="btn btn-primary" (click)="activeTab = 'contact'">
            <span class="material-icons">add</span>
            {{ 'DEALER.SUPPORT.TICKETS.EMPTY_STATES.CREATE_FIRST_TICKET' | translate }}
          </button>
        </div>
      </div>

      <!-- Ticket Details Modal -->
      <div class="ticket-details-modal" *ngIf="showTicketDetails && selectedTicket">
        <div class="modal-backdrop"></div>
          <div class="modal-container" (click)="$event.stopPropagation()">
          <div class="modal-header">
            <div class="ticket-header-info">
              <h3>{{ selectedTicket.subject }}</h3>
              <div class="ticket-badges">
                <span class="ticket-number">{{ 'DEALER.SUPPORT.TICKETS.TABLE.TICKET_NUMBER' | translate }} #{{ selectedTicket.ticketNumber || selectedTicket.id }}</span>
                <span class="status-badge" [ngClass]="'status-' + selectedTicket.status.toLowerCase().replace(' ', '-')">
                  {{ 'DEALER.SUPPORT.TICKETS.STATUS.' + selectedTicket.status.toUpperCase().replace(' ', '_') | translate }}
                </span>
                <span class="priority-badge" [ngClass]="'priority-' + selectedTicket.priority.toLowerCase()">
                  {{ 'DEALER.SUPPORT.CONTACT.FORM.PRIORITIES.' + selectedTicket.priority.toUpperCase() | translate }}
                </span>
              </div>
            </div>
            <button class="close-btn" (click)="closeTicketDetails()" [attr.aria-label]="'DEALER.SUPPORT.TICKET_DETAILS.CLOSE' | translate">
              <span class="material-icons">close</span>
            </button>
          </div>

          <div class="modal-body">
            <div class="ticket-info-section">
              <div class="info-grid">
                <div class="info-item">
                  <label>{{ 'DEALER.SUPPORT.TICKET_DETAILS.CATEGORY' | translate }}:</label>
                  <span>{{ 'DEALER.SUPPORT.CONTACT.FORM.CATEGORIES.' + selectedTicket.category.toUpperCase().replace(' ', '_') | translate }}</span>
                </div>
                <div class="info-item">
                  <label>{{ 'DEALER.SUPPORT.TICKET_DETAILS.CREATED' | translate }}:</label>
                  <span>{{ selectedTicket.createdDate | date:'MMM dd, yyyy HH:mm' }}</span>
                </div>
                <div class="info-item">
                  <label>{{ 'DEALER.SUPPORT.TICKET_DETAILS.LAST_UPDATED' | translate }}:</label>
                  <span>{{ selectedTicket.lastUpdated | date:'MMM dd, yyyy HH:mm' }}</span>
                </div>
              </div>

              <div class="description-section">
                <h4>{{ 'DEALER.SUPPORT.TICKET_DETAILS.DESCRIPTION' | translate }}</h4>
                <div class="description-content">{{ selectedTicket.description }}</div>
              </div>
            </div>

            <!-- Messages Section -->
            <div class="messages-section" *ngIf="selectedTicket.responses && selectedTicket.responses.length > 0">
              <h4>{{ 'DEALER.SUPPORT.TICKET_DETAILS.CONVERSATION_HISTORY' | translate }}</h4>
              <div class="messages-container">
                <div class="message-item" 
                     *ngFor="let message of selectedTicket.responses"
                     [ngClass]="{'message-from-dealer': message.from === 'customer', 'message-from-support': message.from === 'support'}">
                  <div class="message-header">
                    <div class="sender-avatar" [ngClass]="message.from === 'customer' ? 'dealer-avatar' : 'support-avatar'">
                      <span class="material-icons">{{ message.from === 'customer' ? 'person' : 'support_agent' }}</span>
                    </div>
                    <div class="sender-info">
                      <span class="sender-name">{{ message.from === 'customer' ? ('DEALER.SUPPORT.TICKET_DETAILS.YOU_DEALER' | translate) : ('DEALER.SUPPORT.TICKET_DETAILS.SUPPORT_TEAM' | translate) }}</span>
                      <span class="message-time">{{ message.timestamp | date:'MMM dd, yyyy HH:mm' }}</span>
                    </div>
                  </div>
                  <div class="message-content">
                    <p>{{ message.message }}</p>
                    <div class="message-attachments" *ngIf="message.attachments && message.attachments.length > 0">
                      <div class="attachments-header">
                        <span class="material-icons">attach_file</span>
                        <strong>{{ 'DEALER.SUPPORT.TICKET_DETAILS.ATTACHMENTS' | translate }}:</strong>
                      </div>
                      <ul class="attachments-list">
                        <li *ngFor="let attachment of message.attachments">
                          <span class="material-icons">description</span>
                          {{ attachment }}
                        </li>
                      </ul>
                    </div>
                  </div>
                </div>
              </div>
            </div>

            <!-- Reply Section -->
            <div class="reply-section" *ngIf="selectedTicket.status !== 'Closed' && selectedTicket.status !== 'Resolved'">
              <h4>
                <span class="material-icons">reply</span>
                {{ 'DEALER.SUPPORT.TICKET_DETAILS.ADD_REPLY' | translate }}
              </h4>
              <form [formGroup]="replyForm" (ngSubmit)="submitReply()" class="reply-form">
                <div class="form-group">
                  <textarea 
                    formControlName="message"
                    [placeholder]="'DEALER.SUPPORT.TICKET_DETAILS.REPLY_PLACEHOLDER' | translate"
                    rows="4"
                    class="reply-textarea"></textarea>
                </div>
                <div class="reply-actions">
                  <button type="button" class="btn btn-cancel" (click)="cancelReply()">
                    <span class="material-icons">close</span>
                    {{ 'DEALER.SUPPORT.TICKET_DETAILS.CANCEL' | translate }}
                  </button>
                  <button type="submit" class="btn btn-send" [disabled]="!replyForm.valid || isSubmittingReply">
                    <span *ngIf="isSubmittingReply" class="material-icons spinning">sync</span>
                    <span class="material-icons" *ngIf="!isSubmittingReply">send</span>
                    {{ isSubmittingReply ? ('DEALER.SUPPORT.TICKET_DETAILS.SENDING' | translate) : ('DEALER.SUPPORT.TICKET_DETAILS.SEND_REPLY' | translate) }}
                  </button>
                </div>
              </form>
            </div>

            <div class="closed-ticket-notice" *ngIf="selectedTicket.status === 'Closed' || selectedTicket.status === 'Resolved'">
              <div class="notice-icon">
                <span class="material-icons">{{ selectedTicket.status === 'Resolved' ? 'check_circle' : 'lock' }}</span>
              </div>
              <div class="notice-content">
                <h5>{{ selectedTicket.status === 'Resolved' ? ('DEALER.SUPPORT.TICKET_DETAILS.TICKET_RESOLVED' | translate) : ('DEALER.SUPPORT.TICKET_DETAILS.TICKET_CLOSED' | translate) }}</h5>
                <p>{{ selectedTicket.status === 'Resolved' ? ('DEALER.SUPPORT.TICKET_DETAILS.RESOLVED_MESSAGE' | translate) : ('DEALER.SUPPORT.TICKET_DETAILS.CLOSED_MESSAGE' | translate) }}</p>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- FAQ Tab -->
      <div class="tab-content" *ngIf="activeTab === 'faq'">
        <div class="faq-header">
          <h3>{{ 'DEALER.SUPPORT.FAQ.TITLE' | translate }}</h3>
          <div class="faq-search">
            <span class="material-icons">search</span>
            <input 
              type="text" 
              [placeholder]="'DEALER.SUPPORT.FAQ.SEARCH_PLACEHOLDER' | translate" 
              [(ngModel)]="faqSearchTerm"
              (input)="filterFAQs()">
          </div>
        </div>

        <div class="faq-categories">
          <button 
            class="category-btn"
            [class.active]="selectedFAQCategory === 'All'"
            (click)="filterFAQsByCategory('All')">
            {{ 'DEALER.SUPPORT.FAQ.ALL_CATEGORIES' | translate }}
          </button>
          <button 
            class="category-btn"
            [class.active]="selectedFAQCategory === 'Orders'"
            (click)="filterFAQsByCategory('Orders')">
            {{ 'DEALER.SUPPORT.CONTACT.FORM.CATEGORIES.ORDER_ISSUES' | translate }}
          </button>
          <button 
            class="category-btn"
            [class.active]="selectedFAQCategory === 'Products'"
            (click)="filterFAQsByCategory('Products')">
            {{ 'DEALER.SUPPORT.CONTACT.FORM.CATEGORIES.PRODUCT_INQUIRY' | translate }}
          </button>
          <button 
            class="category-btn"
            [class.active]="selectedFAQCategory === 'Shipping'"
            (click)="filterFAQsByCategory('Shipping')">
            {{ 'DEALER.SUPPORT.CONTACT.FORM.CATEGORIES.SHIPPING' | translate }}
          </button>
          <button 
            class="category-btn"
            [class.active]="selectedFAQCategory === 'Payment'"
            (click)="filterFAQsByCategory('Payment')">
            {{ 'DEALER.SUPPORT.CONTACT.FORM.CATEGORIES.PAYMENT' | translate }}
          </button>
          <button 
            class="category-btn"
            [class.active]="selectedFAQCategory === 'Account'"
            (click)="filterFAQsByCategory('Account')">
            {{ 'DEALER.SUPPORT.CONTACT.FORM.CATEGORIES.ACCOUNT' | translate }}
          </button>
        </div>

        <div class="faq-list">
          <div class="faq-item" *ngFor="let faq of filteredFAQs; let i = index">
            <div class="faq-question" (click)="toggleFAQ(i)">
              <h4>{{ faq.question | translate }}</h4>
              <span class="material-icons">{{ expandedFAQ === i ? 'expand_less' : 'expand_more' }}</span>
            </div>
            <div class="faq-answer" [class.expanded]="expandedFAQ === i">
              <p>{{ faq.answer | translate }}</p>
            </div>
          </div>
        </div>
      </div>
    </div>
  `,
  styleUrls: ['./support.component.scss']
})
export class SupportComponent implements OnInit {
  activeTab: 'contact' | 'tickets' | 'faq' = 'contact';
  supportForm!: FormGroup;
  faqSearchTerm: string = '';
  selectedFAQCategory: string = 'All';
  expandedFAQ: number | null = null;
  selectedTicket: SupportTicket | null = null;
  showTicketDetails: boolean = false;
  replyForm!: FormGroup;
  isSubmittingReply: boolean = false;

  // Search and filter properties
  searchTerm: string = '';
  selectedStatus: string = '';
  selectedPriority: string = '';
  selectedCategory: string = '';
  
  // Pagination properties
  currentPage: number = 1;
  pageSize: number = 10;
  totalPages: number = 1;
  
  // Filtered and paginated data
  filteredTickets: SupportTicket[] = [];
  paginatedTickets: SupportTicket[] = [];

  tickets: SupportTicket[] = [];

  faqs: FAQ[] = [
    {
      question: 'DEALER.SUPPORT.FAQ.QUESTIONS.HOW_TO_TRACK_ORDER',
      answer: 'DEALER.SUPPORT.FAQ.ANSWERS.HOW_TO_TRACK_ORDER',
      category: 'Orders'
    },
    {
      question: 'DEALER.SUPPORT.FAQ.QUESTIONS.RETURN_POLICY',
      answer: 'DEALER.SUPPORT.FAQ.ANSWERS.RETURN_POLICY',
      category: 'Orders'
    },
    {
      question: 'DEALER.SUPPORT.FAQ.QUESTIONS.PART_COMPATIBILITY',
      answer: 'DEALER.SUPPORT.FAQ.ANSWERS.PART_COMPATIBILITY',
      category: 'Products'
    },
    {
      question: 'DEALER.SUPPORT.FAQ.QUESTIONS.SHIPPING_CHARGES',
      answer: 'DEALER.SUPPORT.FAQ.ANSWERS.SHIPPING_CHARGES',
      category: 'Shipping'
    },
    {
      question: 'DEALER.SUPPORT.FAQ.QUESTIONS.PAYMENT_METHODS',
      answer: 'DEALER.SUPPORT.FAQ.ANSWERS.PAYMENT_METHODS',
      category: 'Payment'
    },
    {
      question: 'DEALER.SUPPORT.FAQ.QUESTIONS.ORDER_CANCELLATION',
      answer: 'DEALER.SUPPORT.FAQ.ANSWERS.ORDER_CANCELLATION',
      category: 'Orders'
    }
  ];

  filteredFAQs: FAQ[] = [];

  constructor(
    private fb: FormBuilder,
    private dealerSupportService: DealerSupportService,
    private translateService: TranslateService
  ) {}

  ngOnInit() {
    this.initializeForm();
    this.initializeReplyForm();
    this.loadFAQs();
    this.loadTickets();
    this.applyFilters(); // Initialize filters and pagination
    
    // Subscribe to language changes to refresh FAQ filtering
    this.translateService.onLangChange.subscribe(() => {
      this.filterFAQs(); // Re-filter FAQs when language changes
    });
  }

  initializeForm() {
    this.supportForm = this.fb.group({
      subject: ['', Validators.required],
      category: ['', Validators.required],
      priority: ['Medium'],
      description: ['', Validators.required]
    });
  }

  initializeReplyForm() {
    this.replyForm = this.fb.group({
      message: ['', [Validators.required, Validators.minLength(5)]]
    });
  }

  loadFAQs() {
    this.dealerSupportService.getFAQs().subscribe({
      next: (faqs: any) => {
        // Map backend FAQ format to frontend format with translation keys
        this.faqs = faqs.map((faq: any, index: number) => {
          // Check if FAQ comes with translation keys or needs to be mapped
          if (faq.question.startsWith('DEALER.SUPPORT.FAQ.')) {
            // Already has translation keys
            return {
              id: index + 1,
              question: faq.question,
              answer: faq.answer,
              category: faq.category
            };
          } else {
            // Map API response to translation keys based on content
            const mappedFAQ = this.mapFAQToTranslationKeys(faq);
            return {
              id: index + 1,
              question: mappedFAQ.question,
              answer: mappedFAQ.answer,
              category: faq.category
            };
          }
        });
        this.filteredFAQs = [...this.faqs];
      },
      error: (error) => {
        console.error('Error loading FAQs:', error);
        // Keep using static data with translation keys as fallback
        this.filteredFAQs = [...this.faqs];
      }
    });
  }

  private mapFAQToTranslationKeys(faq: any): { question: string, answer: string } {
    // Map common FAQ questions to translation keys
    const questionLower = faq.question.toLowerCase();
    
    if (questionLower.includes('track') && questionLower.includes('order')) {
      return {
        question: 'DEALER.SUPPORT.FAQ.QUESTIONS.HOW_TO_TRACK_ORDER',
        answer: 'DEALER.SUPPORT.FAQ.ANSWERS.HOW_TO_TRACK_ORDER'
      };
    } else if (questionLower.includes('return') && questionLower.includes('policy')) {
      return {
        question: 'DEALER.SUPPORT.FAQ.QUESTIONS.RETURN_POLICY',
        answer: 'DEALER.SUPPORT.FAQ.ANSWERS.RETURN_POLICY'
      };
    } else if (questionLower.includes('compatible') || questionLower.includes('compatibility')) {
      return {
        question: 'DEALER.SUPPORT.FAQ.QUESTIONS.PART_COMPATIBILITY',
        answer: 'DEALER.SUPPORT.FAQ.ANSWERS.PART_COMPATIBILITY'
      };
    } else if (questionLower.includes('shipping') && questionLower.includes('charge')) {
      return {
        question: 'DEALER.SUPPORT.FAQ.QUESTIONS.SHIPPING_CHARGES',
        answer: 'DEALER.SUPPORT.FAQ.ANSWERS.SHIPPING_CHARGES'
      };
    } else if (questionLower.includes('payment') && (questionLower.includes('method') || questionLower.includes('accept'))) {
      return {
        question: 'DEALER.SUPPORT.FAQ.QUESTIONS.PAYMENT_METHODS',
        answer: 'DEALER.SUPPORT.FAQ.ANSWERS.PAYMENT_METHODS'
      };
    } else if (questionLower.includes('cancel') && questionLower.includes('order')) {
      return {
        question: 'DEALER.SUPPORT.FAQ.QUESTIONS.ORDER_CANCELLATION',
        answer: 'DEALER.SUPPORT.FAQ.ANSWERS.ORDER_CANCELLATION'
      };
    } else {
      // For unmapped FAQs, return the original text (they won't be translated)
      console.warn('FAQ not mapped to translation key:', faq.question);
      return {
        question: faq.question,
        answer: faq.answer
      };
    }
  }

  loadTickets() {
    this.dealerSupportService.getTickets().subscribe({
      next: (response: any) => {
        // Handle API response format
        const ticketsData = response.data || response;
        if (Array.isArray(ticketsData)) {
          this.tickets = ticketsData.map((ticket: any) => ({
            id: ticket.id || ticket.ticketId, // Use numeric ID for API calls
            ticketNumber: ticket.ticketNumber, // Keep ticket number for display
            subject: ticket.subject,
            category: ticket.category,
            priority: ticket.priority,
            status: ticket.status,
            createdDate: ticket.createdAt || ticket.createdDate,
            lastUpdated: ticket.updatedAt || ticket.lastUpdated,
            description: ticket.description,
            responses: ticket.messages ? ticket.messages.map((msg: any) => ({
              from: msg.isFromDealer ? 'customer' : 'support',
              message: msg.message,
              timestamp: msg.sentAt || msg.timestamp,
              attachments: msg.attachments ? msg.attachments.map((att: any) => att.fileName) : []
            })) : []
          }));
        }
        // Apply filters and pagination after loading tickets
        this.applyFilters();
      },
      error: (error) => {
        console.error('Error loading tickets:', error);
        // Keep using static data as fallback
        this.applyFilters(); // Apply filters even on error to handle empty state
      }
    });
  }

  submitTicket() {
    if (this.supportForm.valid) {
      const formData = this.supportForm.value;
      const ticketData = {
        subject: formData.subject,
        description: formData.description,
        category: formData.category,
        priority: formData.priority
      };

      this.dealerSupportService.createTicket(ticketData).subscribe({
        next: (response: any) => {
          console.log('Ticket created:', response);
          // Handle successful creation - check if response has success property
          if (response && (response.success || response.data)) {
            alert('Support ticket created successfully!');
            this.supportForm.reset();
            this.initializeForm(); // Reset form to default values
            this.activeTab = 'tickets';
            this.loadTickets(); // Reload tickets after creation
          } else {
            console.error('Unexpected response format:', response);
            alert('Ticket may have been created, but response was unexpected.');
          }
        },
        error: (error) => {
          console.error('Error creating ticket:', error);
          let errorMessage = 'Error creating support ticket. ';
          if (error.status === 415) {
            errorMessage += 'Invalid request format.';
          } else if (error.status === 401) {
            errorMessage += 'Please log in again.';
          } else if (error.status === 403) {
            errorMessage += 'Access denied.';
          } else {
            errorMessage += 'Please try again later.';
          }
          alert(errorMessage);
        }
      });
    }
  }

  filterFAQs() {
    this.filteredFAQs = this.faqs.filter(faq => {
      const matchesSearch = !this.faqSearchTerm || this.searchInTranslatedFAQ(faq, this.faqSearchTerm);
      const matchesCategory = this.selectedFAQCategory === 'All' || faq.category === this.selectedFAQCategory;
      return matchesSearch && matchesCategory;
    });
  }

  private searchInTranslatedFAQ(faq: FAQ, searchTerm: string): boolean {
    try {
      // Get translated question and answer
      const translatedQuestion = this.translateService.instant(faq.question);
      const translatedAnswer = this.translateService.instant(faq.answer);
      
      const lowercaseSearch = searchTerm.toLowerCase();
      
      // Search in both translated question and answer
      return translatedQuestion.toLowerCase().includes(lowercaseSearch) ||
             translatedAnswer.toLowerCase().includes(lowercaseSearch);
    } catch (error) {
      // Fallback to searching in the translation keys if translation fails
      console.warn('Translation search failed, falling back to key search:', error);
      return faq.question.toLowerCase().includes(searchTerm.toLowerCase()) ||
             faq.answer.toLowerCase().includes(searchTerm.toLowerCase());
    }
  }

  filterFAQsByCategory(category: string) {
    this.selectedFAQCategory = category;
    this.filterFAQs();
  }

  toggleFAQ(index: number) {
    this.expandedFAQ = this.expandedFAQ === index ? null : index;
  }

  viewTicketDetails(ticket: SupportTicket) {
    // Set the selected ticket and show details view
    this.selectedTicket = ticket;
    this.showTicketDetails = true;
    console.log('Viewing ticket details:', ticket);
    
    // Load detailed ticket information with messages from the API
    this.dealerSupportService.getTicketById(ticket.id).subscribe({
      next: (detailedTicket: any) => {
        console.log('Loaded detailed ticket:', detailedTicket);
        
        // Update the selected ticket with detailed information including messages
        if (this.selectedTicket && detailedTicket) {
          // Handle different response formats (detailedTicket.data or detailedTicket directly)
          const ticketData = detailedTicket.data || detailedTicket;
          
          // Map messages from API response to frontend format
          if (ticketData.messages && Array.isArray(ticketData.messages)) {
            this.selectedTicket.responses = ticketData.messages.map((msg: any) => ({
              from: msg.isFromDealer ? 'customer' : 'support',
              message: msg.message,
              timestamp: msg.sentAt || msg.timestamp,
              attachments: msg.attachments ? msg.attachments.map((att: any) => att.fileName) : []
            }));
          }
          
          // Update other ticket details if available
          if (ticketData.description) {
            this.selectedTicket.description = ticketData.description;
          }
        }
      },
      error: (error) => {
        console.error('Error loading ticket details:', error);
        // Show error message to user but still allow viewing the modal with cached data
        alert('Could not load latest ticket messages. Showing cached information.');
      }
    });
  }

  closeTicketDetails() {
    this.selectedTicket = null;
    this.showTicketDetails = false;
  }

  submitReply() {
    if (this.replyForm.valid && this.selectedTicket) {
      // Check if ticket is resolved or closed
      if (this.selectedTicket.status === 'Resolved' || this.selectedTicket.status === 'Closed') {
        alert('Cannot add replies to resolved or closed tickets.');
        return;
      }
      
      this.isSubmittingReply = true;
      const message = this.replyForm.get('message')?.value.trim();
      
      // Use the numeric ID directly
      const ticketId = this.selectedTicket.id;
      
      // Ensure ticketId is a number and valid
      if (!ticketId || isNaN(ticketId)) {
        alert('Ticket ID is invalid. Please contact support.');
        this.isSubmittingReply = false;
        return;
      }
      this.dealerSupportService.addMessageToTicket(Number(ticketId), message).subscribe({
        next: (response: any) => {
          console.log('Reply sent:', response);
          this.isSubmittingReply = false;
          this.replyForm.reset();
          
          // Add the new message to the current ticket's responses for immediate UI update
          if (this.selectedTicket) {
            const newMessage = {
              from: 'customer' as const,
              message: message,
              timestamp: new Date().toISOString(),
              attachments: []
            };
            this.selectedTicket.responses = this.selectedTicket.responses || [];
            this.selectedTicket.responses.push(newMessage);
          }
          
          // Also reload tickets to get the latest data
          this.loadTickets();
        },
        error: (error: any) => {
          console.error('Error sending reply:', error);
          this.isSubmittingReply = false;
          alert('Error sending reply. Please check the console for details.');
        }
      });
    }
  }

  cancelReply() {
    this.replyForm.reset();
    // Also close the modal when cancel is clicked
    this.closeTicketDetails();
  }

  replyToTicket(ticket: SupportTicket) {
    // This method is now deprecated, but keeping for backward compatibility
    // The reply functionality is now integrated in the ticket details view
    this.viewTicketDetails(ticket);
  }

  // Search, Filter, and Pagination Methods
  applyFilters(): void {
    // Filter tickets based on search term and filters
    this.filteredTickets = this.tickets.filter(ticket => {
      // Search in subject, description, and ticket number
      const searchMatch = !this.searchTerm || 
        ticket.subject.toLowerCase().includes(this.searchTerm.toLowerCase()) ||
        ticket.description.toLowerCase().includes(this.searchTerm.toLowerCase()) ||
        (ticket.ticketNumber && ticket.ticketNumber.toLowerCase().includes(this.searchTerm.toLowerCase())) ||
        ticket.id.toString().includes(this.searchTerm);

      // Filter by status
      const statusMatch = !this.selectedStatus || ticket.status === this.selectedStatus;

      // Filter by priority
      const priorityMatch = !this.selectedPriority || ticket.priority === this.selectedPriority;

      // Filter by category
      const categoryMatch = !this.selectedCategory || ticket.category === this.selectedCategory;

      return searchMatch && statusMatch && priorityMatch && categoryMatch;
    });

    // Calculate total pages
    this.totalPages = Math.ceil(this.filteredTickets.length / this.pageSize);

    // Reset to first page if current page exceeds total pages
    if (this.currentPage > this.totalPages) {
      this.currentPage = 1;
    }

    // Apply pagination
    this.updatePaginatedTickets();
  }

  updatePaginatedTickets(): void {
    const startIndex = (this.currentPage - 1) * this.pageSize;
    const endIndex = startIndex + this.pageSize;
    this.paginatedTickets = this.filteredTickets.slice(startIndex, endIndex);
  }

  clearFilters(): void {
    this.searchTerm = '';
    this.selectedStatus = '';
    this.selectedPriority = '';
    this.selectedCategory = '';
    this.currentPage = 1;
    this.applyFilters();
  }

  // Pagination Methods
  goToPage(page: number): void {
    if (page >= 1 && page <= this.totalPages) {
      this.currentPage = page;
      this.updatePaginatedTickets();
    }
  }

  onPageSizeChange(): void {
    this.currentPage = 1;
    this.applyFilters();
  }

  getPageNumbers(): number[] {
    const pages: number[] = [];
    const maxVisiblePages = 5;
    const startPage = Math.max(1, this.currentPage - Math.floor(maxVisiblePages / 2));
    const endPage = Math.min(this.totalPages, startPage + maxVisiblePages - 1);

    for (let i = startPage; i <= endPage; i++) {
      pages.push(i);
    }

    return pages;
  }

  getDisplayStart(): number {
    return (this.currentPage - 1) * this.pageSize + 1;
  }

  getDisplayEnd(): number {
    return Math.min(this.currentPage * this.pageSize, this.filteredTickets.length);
  }
}
