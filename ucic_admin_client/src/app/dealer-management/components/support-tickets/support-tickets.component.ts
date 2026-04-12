import { Component, OnInit, CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { DealerManagementService } from '../../services/dealer-management.service';
import { DealerSupportTicketService } from '../../services/dealer-support-ticket.service';

interface SupportTicket {
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

interface TicketResponse {
  responseId: number;
  ticketId: number;
  message: string;
  createdBy: string;
  createdDate: string;
  isInternal: boolean;
}

interface Dealer {
  dealerId: number;
  dealerName?: string;
  fullName?: string;
  companyName?: string;
}

interface FilterOption {
  value: string;
  label: string;
}

interface Notification {
  type: 'success' | 'error' | 'info';
  message: string;
}

@Component({
  selector: 'app-support-tickets',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule],
  templateUrl: './support-tickets.component.html',
  styleUrls: ['./support-tickets.component.scss'],
  schemas: [CUSTOM_ELEMENTS_SCHEMA]
})
export class SupportTicketsComponent implements OnInit {
  // Data properties
  tickets: SupportTicket[] = [];
  dealers: Dealer[] = [];
  
  // Filter properties
  searchTerm: string = '';
  selectedDealerId: number | null = null;
  selectedStatus: string = '';
  selectedPriority: string = '';
  selectedCategory: string = '';
  
  // Pagination properties
  currentPage: number = 1;
  pageSize: number = 10;
  totalItems: number = 0;
  
  // UI state properties
  loading: boolean = false;
  showDetailsModal: boolean = false;
  showResponseModal: boolean = false;
  selectedTicket: SupportTicket | null = null;
  submittingResponse: boolean = false;
  notification: Notification | null = null;
  
  // Form
  responseForm!: FormGroup;
  
  // Filter options
  statuses: FilterOption[] = [
    { value: 'Open', label: 'Open' },
    { value: 'In Progress', label: 'In Progress' },
    { value: 'Resolved', label: 'Resolved' },
    { value: 'Closed', label: 'Closed' }
  ];
  
  priorities: FilterOption[] = [
    { value: 'Low', label: 'Low' },
    { value: 'Medium', label: 'Medium' },
    { value: 'High', label: 'High' },
    { value: 'Critical', label: 'Critical' }
  ];
  
  categories: FilterOption[] = [
    { value: 'Technical', label: 'Technical Support' },
    { value: 'Account', label: 'Account Issues' },
    { value: 'Orders', label: 'Order Management' },
    { value: 'Billing', label: 'Billing & Payments' },
    { value: 'General', label: 'General Inquiry' },
    { value: 'Bug Report', label: 'Bug Report' },
    { value: 'Feature Request', label: 'Feature Request' }
  ];

  constructor(
    private dealerSupportTicketService: DealerSupportTicketService,
    private dealerManagementService: DealerManagementService,
    private fb: FormBuilder
  ) {
    this.initializeResponseForm();
  }

  ngOnInit(): void {
    this.loadInitialData();
  }

  private initializeResponseForm(): void {
    this.responseForm = this.fb.group({
      message: ['', [Validators.required, Validators.minLength(10)]],
      status: [''],
      isInternal: [false]
    });
  }

  private async loadInitialData(): Promise<void> {
    this.loading = true;
    try {
      await Promise.all([
        this.loadDealers(),
        this.loadTickets()
      ]);
    } catch (error) {
      console.error('Error loading initial data:', error);
      this.showNotification('Error loading data. Please try again.', 'error');
    } finally {
      this.loading = false;
    }
  }

  private async loadDealers(): Promise<void> {
    try {
      const response = await this.dealerManagementService.getAllDealersForDropdown().toPromise();
      this.dealers = response || [];
      console.log('Loaded dealers for support tickets:', this.dealers.length);
    } catch (error) {
      console.error('Error loading dealers:', error);
      this.dealers = [];
    }
  }

  private async loadTickets(): Promise<void> {
    try {
      const params = this.buildSearchParams();
      const response = await this.dealerSupportTicketService.getAllTickets(params).toPromise();
      
      if (response && response.success) {
        // Backend returns nested structure: { success: true, data: { data: [], metadata: {} } }
        const data = response.data;
        this.tickets = data?.data || [];
        this.totalItems = data?.metadata?.totalCount || 0;
        console.log('Loaded support tickets:', this.tickets.length, 'Total count:', this.totalItems);
      } else {
        this.tickets = [];
        this.totalItems = 0;
      }
    } catch (error) {
      console.error('Error loading tickets:', error);
      this.tickets = [];
      this.totalItems = 0;
    }
  }

  private buildSearchParams(): any {
    return {
      page: this.currentPage,
      pageSize: this.pageSize,
      searchTerm: this.searchTerm || undefined,
      dealerId: this.selectedDealerId || undefined,
      status: this.selectedStatus || undefined,
      priority: this.selectedPriority || undefined,
      category: this.selectedCategory || undefined
    };
  }

  // Event handlers
  onSearch(): void {
    this.currentPage = 1;
    this.loadTickets();
  }

  onFilterChange(): void {
    this.currentPage = 1;
    this.loadTickets();
  }

  clearFilters(): void {
    this.searchTerm = '';
    this.selectedDealerId = null;
    this.selectedStatus = '';
    this.selectedPriority = '';
    this.selectedCategory = '';
    this.currentPage = 1;
    this.loadTickets();
  }

  refreshData(): void {
    this.loadInitialData();
  }

  // Pagination methods
  goToPage(page: number): void {
    if (page >= 1 && page <= this.getTotalPages()) {
      this.currentPage = page;
      this.loadTickets();
    }
  }

  getTotalPages(): number {
    return Math.ceil(this.totalItems / this.pageSize);
  }

  getPageNumbers(): number[] {
    const totalPages = this.getTotalPages();
    const pages: number[] = [];
    const startPage = Math.max(1, this.currentPage - 2);
    const endPage = Math.min(totalPages, this.currentPage + 2);

    for (let i = startPage; i <= endPage; i++) {
      pages.push(i);
    }

    return pages;
  }

  getDisplayStart(): number {
    return (this.currentPage - 1) * this.pageSize + 1;
  }

  getDisplayEnd(): number {
    return Math.min(this.currentPage * this.pageSize, this.totalItems);
  }

  // Ticket action methods
  viewTicketDetails(ticket: SupportTicket): void {
    this.selectedTicket = ticket;
    this.showDetailsModal = true;
  }

  openResponseModal(ticket: SupportTicket): void {
    if (ticket.status === 'Closed') {
      this.showNotification('Cannot add response to closed ticket', 'error');
      return;
    }
    
    this.selectedTicket = ticket;
    this.responseForm.reset();
    this.responseForm.patchValue({
      status: '',
      isInternal: false
    });
    this.showResponseModal = true;
  }

  async resolveTicket(ticket: SupportTicket): Promise<void> {
    try {
      this.loading = true;
      await this.dealerSupportTicketService.resolveTicket(ticket.ticketId).toPromise();
      
      // Update ticket status locally
      ticket.status = 'Resolved';
      ticket.resolvedDate = new Date().toISOString();
      ticket.assignedTo = 'Admin'; // You might want to get this from auth service
      
      this.showNotification('Ticket resolved successfully', 'success');
      this.closeDetailsModal();
    } catch (error) {
      console.error('Error resolving ticket:', error);
      this.showNotification('Error resolving ticket. Please try again.', 'error');
    } finally {
      this.loading = false;
    }
  }

  async reopenTicket(ticket: SupportTicket): Promise<void> {
    try {
      this.loading = true;
      await this.dealerSupportTicketService.reopenTicket(ticket.ticketId).toPromise();
      
      // Update ticket status locally
      ticket.status = 'Open';
      ticket.resolvedDate = undefined;
      ticket.assignedTo = undefined;
      
      this.showNotification('Ticket reopened successfully', 'success');
      this.closeDetailsModal();
    } catch (error) {
      console.error('Error reopening ticket:', error);
      this.showNotification('Error reopening ticket. Please try again.', 'error');
    } finally {
      this.loading = false;
    }
  }

  async submitResponse(): Promise<void> {
    if (this.responseForm.invalid || !this.selectedTicket) {
      return;
    }

    try {
      this.submittingResponse = true;
      const formData = this.responseForm.value;
      
      const responseData = {
        ticketId: this.selectedTicket.ticketId,
        message: formData.message,
        isInternal: formData.isInternal,
        newStatus: formData.status || undefined
      };

      await this.dealerSupportTicketService.addResponse(responseData).toPromise();
      
      // Update ticket locally if status was changed
      if (formData.status) {
        this.selectedTicket.status = formData.status;
      }
      
      // Add the response to the ticket
      if (!this.selectedTicket.responses) {
        this.selectedTicket.responses = [];
      }
      
      this.selectedTicket.responses.push({
        responseId: Date.now(), // Temporary ID
        ticketId: this.selectedTicket.ticketId,
        message: formData.message,
        createdBy: 'Admin', // You might want to get this from auth service
        createdDate: new Date().toISOString(),
        isInternal: formData.isInternal
      });

      this.showNotification('Response added successfully', 'success');
      this.closeResponseModal();
    } catch (error) {
      console.error('Error submitting response:', error);
      this.showNotification('Error adding response. Please try again.', 'error');
    } finally {
      this.submittingResponse = false;
    }
  }

  // Modal methods
  closeDetailsModal(): void {
    this.showDetailsModal = false;
    this.selectedTicket = null;
  }

  closeResponseModal(): void {
    this.showResponseModal = false;
    this.responseForm.reset();
  }

  // Utility methods
  getDealerName(dealerId: number): string {
    const dealer = this.dealers.find(d => d.dealerId === dealerId);
    return dealer?.dealerName || dealer?.fullName || dealer?.companyName || `Dealer ${dealerId}`;
  }

  getStatusClass(status: string): string {
    switch (status.toLowerCase()) {
      case 'open': return 'bg-info';
      case 'in progress': return 'bg-warning';
      case 'resolved': return 'bg-success';
      case 'closed': return 'bg-secondary';
      default: return 'bg-secondary';
    }
  }

  getPriorityClass(priority: string): string {
    switch (priority.toLowerCase()) {
      case 'low': return 'bg-info';
      case 'medium': return 'bg-warning';
      case 'high': return 'bg-danger';
      case 'critical': return 'bg-dark';
      default: return 'bg-secondary';
    }
  }

  showNotification(message: string, type: 'success' | 'error' | 'info'): void {
    this.notification = { message, type };
    setTimeout(() => {
      this.notification = null;
    }, 5000);
  }

  dismissNotification(): void {
    this.notification = null;
  }
}