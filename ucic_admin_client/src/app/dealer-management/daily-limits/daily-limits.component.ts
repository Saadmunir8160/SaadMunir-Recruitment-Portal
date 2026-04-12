import { Component, OnInit, CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { DealerManagementService, Dealer } from '../services/dealer-management.service';

interface DealerDailyLimit {
  dailyLimitID: number;
  dealerID: number | null;
  dealerName?: string;
  limitType: string;
  limitValue: number;
  effectiveDate: Date;
  isActive: boolean;
  createdDate: Date;
  updatedDate: Date;
}



@Component({
  selector: 'app-daily-limits',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule],
  templateUrl: './daily-limits.component.html',
  styleUrls: ['./daily-limits.component.scss'],
  schemas: [CUSTOM_ELEMENTS_SCHEMA]
})
export class DailyLimitsComponent implements OnInit {
  dailyLimits: DealerDailyLimit[] = [];
  allDailyLimits: DealerDailyLimit[] = []; // Store all data
  dealers: Dealer[] = [];
  loading = false;
  totalItems = 0;
  currentPage = 1;
  pageSize = 10;
  searchTerm = '';
  selectedDealerId: number | null = null;
  selectedLimitType = '';
  selectedStatus = '';
  showCreateForm = false;
  showEditForm = false;
  notification: { message: string; type: 'success' | 'error' | 'info' } | null = null;
  editingLimit: DealerDailyLimit | null = null;

  dailyLimitForm!: FormGroup;
  limitTypes: any[] = [
    { value: 'DAILY_BAGS_LIMIT', label: 'Daily Bags Limit' },
    { value: 'DAILY_TONS_LIMIT', label: 'Daily Tons Limit' }
  ];

  statusOptions: any[] = [
    { value: '', label: 'All Status' },
    { value: 'true', label: 'Active' },
    { value: 'false', label: 'Inactive' }
  ];

  // Make Math available in template
  Math = Math;

  constructor(
    private dealerService: DealerManagementService,
    private fb: FormBuilder,
    private router: Router
  ) {
    this.initializeForm();
  }

  ngOnInit(): void {
    this.loadDailyLimits();
    this.loadDealers();
  }

  initializeForm(): void {
    this.dailyLimitForm = this.fb.group({
      dealerID: [null], // null means general limit for all dealers
      limitType: ['', Validators.required],
      limitValue: ['', [Validators.required, Validators.min(0)]],
      effectiveDate: [new Date().toISOString().split('T')[0], Validators.required]
    });
  }

  loadDailyLimits(): void {
    // Only fetch from API if we don't have the data yet, or need to refresh
    if (this.allDailyLimits.length === 0) {
      this.fetchAllDailyLimits();
    } else {
      this.applyFiltersAndPagination();
    }
  }

  fetchAllDailyLimits(): void {
    this.loading = true;
    // Fetch all data without filters (large page size to get everything)
    const params = {
      pageNumber: 1,
      pageSize: 1000, // Get all records
    };

    this.dealerService.getDealerDailyLimits(params).subscribe({
      next: (response) => {
        console.log('Daily limits response:', response); // Debug log
        // Handle the API response structure: { success: true, data: { data: [...], metadata: {...} } }
        if (response && response.data && response.data.data) {
          this.allDailyLimits = Array.isArray(response.data.data) ? response.data.data : [];
          this.applyFiltersAndPagination();
        } else {
          this.allDailyLimits = [];
          this.dailyLimits = [];
          this.totalItems = 0;
        }
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading daily limits:', error);
        this.loading = false;
        this.allDailyLimits = [];
        this.dailyLimits = [];
        this.totalItems = 0;
      }
    });
  }

  applyFiltersAndPagination(): void {
    let filteredLimits = [...this.allDailyLimits];

    // Apply search filter
    if (this.searchTerm) {
      const searchLower = this.searchTerm.toLowerCase();
      filteredLimits = filteredLimits.filter(limit =>
        (limit.dealerName && limit.dealerName.toLowerCase().includes(searchLower)) ||
        limit.limitType.toLowerCase().includes(searchLower)
      );
    }

    // Apply dealer filter
    if (this.selectedDealerId !== null) {
      if (this.selectedDealerId === 0) {
        // General limits (dealerID is null)
        filteredLimits = filteredLimits.filter(limit => limit.dealerID === null);
      } else {
        filteredLimits = filteredLimits.filter(limit => limit.dealerID === this.selectedDealerId);
      }
    }

    // Apply limit type filter
    if (this.selectedLimitType) {
      filteredLimits = filteredLimits.filter(limit => limit.limitType === this.selectedLimitType);
    }

    // Apply status filter
    if (this.selectedStatus !== '') {
      const isActiveFilter = this.selectedStatus === 'true';
      filteredLimits = filteredLimits.filter(limit => limit.isActive === isActiveFilter);
    }

    // Update total count
    this.totalItems = filteredLimits.length;

    // Apply pagination
    const startIndex = (this.currentPage - 1) * this.pageSize;
    const endIndex = startIndex + this.pageSize;
    this.dailyLimits = filteredLimits.slice(startIndex, endIndex);
  }

  loadDealers(): void {
    console.log('Loading dealers...'); // Debug log
    // Load dealers for the dropdown using the simplified method
    this.dealerService.getAllDealersForDropdown().subscribe({
      next: (dealers) => {
        console.log('Dealers loaded successfully:', dealers); // Debug log
        console.log('Sample dealer:', dealers[0]); // Show first dealer structure
        this.dealers = dealers;
      },
      error: (error) => {
        console.error('Error loading dealers:', error);
        this.dealers = [];
      }
    });
  }

  onSearch(): void {
    this.currentPage = 1;
    this.applyFiltersAndPagination();
  }

  onFilterChange(): void {
    this.currentPage = 1;
    this.applyFiltersAndPagination();
  }

  onPageChange(page: number): void {
    this.currentPage = page;
    this.applyFiltersAndPagination();
  }

  clearFilters(): void {
    this.searchTerm = '';
    this.selectedDealerId = null;
    this.selectedLimitType = '';
    this.selectedStatus = '';
    this.currentPage = 1;
    this.applyFiltersAndPagination();
  }

  openCreateForm(): void {
    this.showCreateForm = true;
    this.showEditForm = false;
    this.editingLimit = null;
    this.initializeForm();
  }

  openEditForm(limit: DealerDailyLimit): void {
    this.showEditForm = true;
    this.showCreateForm = false;
    this.editingLimit = limit;
    
    this.dailyLimitForm.patchValue({
      dealerID: limit.dealerID,
      limitType: limit.limitType,
      limitValue: limit.limitValue,
      effectiveDate: new Date(limit.effectiveDate).toISOString().split('T')[0]
    });
  }

  closeForm(): void {
    this.showCreateForm = false;
    this.showEditForm = false;
    this.editingLimit = null;
    this.initializeForm();
  }

  onSubmit(): void {
    if (this.dailyLimitForm.valid) {
      const formData = this.dailyLimitForm.value;
      
      if (this.showEditForm && this.editingLimit) {
        this.updateDailyLimit(formData);
      } else {
        this.createDailyLimit(formData);
      }
    } else {
      this.markFormGroupTouched();
    }
  }

  createDailyLimit(limitData: any): void {
    this.loading = true;
    
    this.dealerService.createDealerDailyLimit(limitData).subscribe({
      next: (response) => {
        if (response.success) {
          this.closeForm();
          this.refreshData(); // Refresh cached data from API
          this.showNotification('Daily limit created successfully', 'success');
        } else {
          console.error('Error creating daily limit:', response.message);
          this.showNotification('Error creating daily limit: ' + response.message, 'error');
        }
        this.loading = false;
      },
      error: (error) => {
        console.error('Error creating daily limit:', error);
        this.showNotification('Error creating daily limit. Please try again.', 'error');
        this.loading = false;
      }
    });
  }

  updateDailyLimit(limitData: any): void {
    if (!this.editingLimit) return;
    
    this.loading = true;
    const updateData = { ...limitData, dailyLimitID: this.editingLimit.dailyLimitID };
    
    this.dealerService.updateDealerDailyLimit(updateData).subscribe({
      next: (response) => {
        if (response.success) {
          this.closeForm();
          this.refreshData(); // Refresh cached data from API
          this.showNotification('Daily limit updated successfully', 'success');
        } else {
          console.error('Error updating daily limit:', response.message);
          alert('Error updating daily limit: ' + response.message);
        }
        this.loading = false;
      },
      error: (error) => {
        console.error('Error updating daily limit:', error);
        alert('Error updating daily limit. Please try again.');
        this.loading = false;
      }
    });
  }

  deleteDailyLimit(limit: DealerDailyLimit): void {
    if (confirm(`Are you sure you want to delete this daily limit for ${this.getDealerName(limit.dealerID, limit.dealerName)}?`)) {
      this.loading = true;
      
      this.dealerService.deleteDealerDailyLimit(limit.dailyLimitID).subscribe({
        next: (response) => {
          if (response.success) {
            this.refreshData(); // Refresh cached data from API
            console.log('Daily limit deleted successfully');
          } else {
            console.error('Error deleting daily limit:', response.message);
            alert('Error deleting daily limit: ' + response.message);
          }
          this.loading = false;
        },
        error: (error) => {
          console.error('Error deleting daily limit:', error);
          alert('Error deleting daily limit. Please try again.');
          this.loading = false;
        }
      });
    }
  }

  toggleActiveStatus(limit: DealerDailyLimit): void {
    const action = limit.isActive ? 'deactivate' : 'activate';
    const dealerName = this.getDealerName(limit.dealerID, limit.dealerName);
    
    if (confirm(`Are you sure you want to ${action} this daily limit for ${dealerName}?`)) {
      this.loading = true;
      
      const updatedLimit = { 
        ...limit, 
        isActive: !limit.isActive 
      };
      
      this.dealerService.updateDealerDailyLimit(updatedLimit).subscribe({
        next: (response) => {
          if (response.success) {
            this.refreshData(); // Refresh cached data from API
            console.log(`Daily limit ${action}d successfully`);
          } else {
            console.error('Error updating daily limit status:', response.message);
            alert('Error updating daily limit status: ' + response.message);
          }
          this.loading = false;
        },
        error: (error) => {
          console.error('Error updating daily limit status:', error);
          alert('Error updating daily limit status. Please try again.');
          this.loading = false;
        }
      });
    }
  }

  private markFormGroupTouched(): void {
    Object.keys(this.dailyLimitForm.controls).forEach(key => {
      const control = this.dailyLimitForm.get(key);
      control?.markAsTouched();
    });
  }

  getValidationErrors(fieldName: string): string[] {
    if (!this.dailyLimitForm) {
      return [];
    }
    
    const control = this.dailyLimitForm.get(fieldName);
    const errors: string[] = [];

    if (control?.errors && control.touched) {
      if (control.errors['required']) {
        errors.push(`${this.getFieldLabel(fieldName)} is required`);
      }
      if (control.errors['min']) {
        errors.push(`${this.getFieldLabel(fieldName)} must be greater than or equal to 0`);
      }
    }

    return errors;
  }

  private getFieldLabel(fieldName: string): string {
    const labels: { [key: string]: string } = {
      'limitType': 'Limit Type',
      'limitValue': 'Limit Value',
      'effectiveDate': 'Effective Date'
    };
    return labels[fieldName] || fieldName;
  }

  getLimitTypeLabel(limitType: string): string {
    const type = this.limitTypes.find(t => t.value === limitType);
    return type ? type.label : limitType;
  }

  getDealerName(dealerID: number | null, dealerName?: string): string {
    // If dealerName is provided from API, use it
    if (dealerName) {
      return dealerName;
    }
    
    if (dealerID === null) {
      return 'General (All Dealers)';
    }
    
    const dealer = this.dealers.find(d => d.dealerId === dealerID);
    return dealer ? (dealer.dealerName || dealer.fullName || `Dealer ${dealer.dealerId}`) : `Dealer ID: ${dealerID}`;
  }

  formatLimitValue(limitType: string, limitValue: number): string {
    if (limitType === 'DAILY_TONS_LIMIT') {
      return `${limitValue} tons`;
    } else if (limitType === 'DAILY_BAGS_LIMIT') {
      return `${limitValue} bags`;
    }
    return limitValue.toString();
  }

  get totalPages(): number {
    return Math.ceil(this.totalItems / this.pageSize);
  }

  get pages(): number[] {
    const pages: number[] = [];
    const totalPages = this.totalPages;
    if (totalPages > 0) {
      for (let i = 1; i <= totalPages; i++) {
        pages.push(i);
      }
    }
    return pages;
  }

  refreshData(): void {
    this.allDailyLimits = []; // Clear cached data
    this.loadDailyLimits(); // Reload from API
    this.refreshParentCounts(); // Update parent component counts
  }

  refreshParentCounts(): void {
    // Emit event to parent component to refresh counts
    // For now, we'll use a simple approach - you can improve this with proper event emitters
    setTimeout(() => {
      window.dispatchEvent(new CustomEvent('refreshDealerCounts'));
    }, 500);
  }

  showNotification(message: string, type: 'success' | 'error' | 'info'): void {
    this.notification = { message, type };
    // Auto-hide after 5 seconds
    setTimeout(() => {
      this.notification = null;
    }, 5000);
  }

  dismissNotification(): void {
    this.notification = null;
  }

}