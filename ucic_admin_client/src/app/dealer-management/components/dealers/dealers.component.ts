import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators, AbstractControl } from '@angular/forms';
import { RouterModule, Router } from '@angular/router';
import { DealerManagementService } from '../../services/dealer-management.service';

export interface Dealer {
  dealerId: number;
  userId: string;
  dealerName: string;
  email?: string;
  phoneNumber?: string;
  fullName?: string;
  userName?: string;
  creditLimit?: number;
  currentBalance?: number;
  ln_ID?: string;
  isActive: boolean;
  createdDate?: Date;
  modifiedDate?: Date;
}

export interface CreateDealerUserRequest {
  fullName: string;
  userName: string;
  email: string;
  phone: string;
  password: string;
  confirmationPassword: string;
  dealerName: string;
  creditLimit?: number;
  currentBalance?: number;
  ln_ID?: string;
}

export interface UpdateDealerUserRequest {
  fullName: string;
  userName: string;
  email: string;
  phone: string;
  dealerName: string;
  creditLimit?: number;
  currentBalance?: number;
  ln_ID?: string;
  isActive?: boolean;
}

@Component({
  selector: 'app-dealers',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule, RouterModule],
  template: `
    <div class="dealers-container">
      <!-- Header Section -->
      <div class="dealers-header">
        <!-- <div class="header-content">
          <h2 class="section-title">
            <i class="material-icons">business</i>
            Customer Portal
            <span class="badge badge-primary ms-2" *ngIf="totalCount > 0">{{ totalCount }}</span>
          </h2>
          <p class="section-subtitle">Manage customer accounts and their information</p>
        </div> -->
        <div class="header-actions">
          <button class="btn btn-primary" (click)="openCreateDealerModal()">
            <i class="material-icons">add</i>
            Create New Customer
          </button>
        </div>
      </div>

      <!-- Filters Section -->
      <div class="filters-section">
        <div class="row g-3">
          <div class="col-md-8">
            <label class="form-label fw-semibold">Search</label>
            <input type="text" 
                   class="form-control" 
                   placeholder="Search Customers..." 
                   [(ngModel)]="searchTerm"
                   (keyup.enter)="applyFilters()">
          </div>
          
          <div class="col-md-2">
            <label class="form-label fw-semibold">Status</label>
            <select class="form-select" [(ngModel)]="selectedStatus" (ngModelChange)="applyFilters()">
              <option value="">All Status</option>
              <option value="true">Active</option>
              <option value="false">Inactive</option>
            </select>
          </div>
          
          <div class="col-md-2 d-flex align-items-end">
            <label class="form-label">&nbsp;</label>
            <button class="btn btn-primary" (click)="applyFilters()">
              <i class="material-icons">filter_list</i>
              Apply Filters
            </button>
          </div>
        </div>
      </div>

      <!-- Dealers Table -->
      <div class="dealers-table-container">
        <div class="card">
          <div class="card-body">
            <div class="table-responsive">
              <table class="table table-hover mb-0 border">
                <thead class="table-header">
                  <tr>
                    <th>Customer Name</th>
                    <th>LN-ID</th>
                    <th>Email</th>
                    <th>Phone</th>
                    <th>Username</th>
                    <th>Status</th>
                    <th>Created Date</th>
                    <th>Actions</th>
                  </tr>
                </thead>
                <tbody>
                  <tr *ngFor="let dealer of dealers" class="dealer-row">
                    <td>
                      <div class="dealer-name">
                        <strong>{{ dealer.dealerName }}</strong>
                        <small *ngIf="dealer.fullName" class="text-muted d-block">{{ dealer.fullName }}</small>
                      </div>
                    </td>
                    <td>{{ dealer.ln_ID || 'N/A' }}</td>
                    <td>{{ dealer.email || 'N/A' }}</td>
                    <td>{{ dealer.phoneNumber || 'N/A' }}</td>
                    <td>{{ dealer.userName || 'N/A' }}</td>
                    <td>
                      <span class="badge" [class]="dealer.isActive ? 'badge-success' : 'badge-danger'">
                        {{ dealer.isActive ? 'Active' : 'Inactive' }}
                      </span>
                    </td>
                    <td>{{ dealer.createdDate | date:'short' }}</td>
                    <td>
                      <div class="action-buttons">
                        <button class="btn btn-sm btn-outline-info" 
                                (click)="viewDealerDetails(dealer)"
                                title="View Details">
                          <i class="material-icons">visibility</i>
                        </button>
                        <button class="btn btn-sm btn-outline-primary" 
                                (click)="editDealer(dealer)"
                                title="Edit Dealer">
                          <i class="material-icons">edit</i>
                        </button>
                        <button class="btn btn-sm btn-outline-danger" 
                                (click)="deleteDealer(dealer)"
                                title="Delete Dealer">
                          <i class="material-icons">delete</i>
                        </button>
                      </div>
                    </td>
                  </tr>
                  
                  <tr *ngIf="dealers.length === 0 && !loading">
                    <td colspan="9" class="text-center py-4">
                      <div class="no-data">
                        <i class="material-icons">business_center</i>
                        <p>No dealers found</p>
                      </div>
                    </td>
                  </tr>
                </tbody>
              </table>
            </div>
            
            <!-- Loading State -->
            <div *ngIf="loading" class="loading-container">
              <div class="spinner-border text-primary" role="status">
                <span class="visually-hidden">Loading...</span>
              </div>
              <p>Loading dealers...</p>
            </div>
          </div>
        </div>
      </div>

      <!-- Pagination -->
      <div class="pagination-section" *ngIf="totalCount > pageSize">
        <nav>
          <ul class="pagination justify-content-center">
            <li class="page-item" [class.disabled]="currentPage === 1">
              <button class="page-link" (click)="goToPage(currentPage - 1)">Previous</button>
            </li>
            
            <li *ngFor="let page of getPageNumbers()" 
                class="page-item" 
                [class.active]="page === currentPage">
              <button class="page-link" (click)="goToPage(page)">{{ page }}</button>
            </li>
            
            <li class="page-item" [class.disabled]="currentPage >= totalPages">
              <button class="page-link" (click)="goToPage(currentPage + 1)">Next</button>
            </li>
          </ul>
        </nav>
        
        <div class="pagination-info">
          Showing {{ (currentPage - 1) * pageSize + 1 }} to {{ Math.min(currentPage * pageSize, totalCount) }} of {{ totalCount }} dealers
        </div>
      </div>
    </div>

    <!-- Create/Edit Dealer Modal -->
    <div class="modal fade" 
         [class.show]="showDealerModal" 
         [style.display]="showDealerModal ? 'block' : 'none'"
         tabindex="-1">
      <div class="modal-dialog modal-lg">
        <div class="modal-content">
          <div class="modal-header">
            <h5 class="modal-title">
              {{ editingDealer ? 'Edit Dealer' : 'Create New Dealer' }}
            </h5>
            <button type="button" class="btn-close" (click)="closeDealerModal()"></button>
          </div>
          
          <form [formGroup]="dealerForm" (ngSubmit)="saveDealerUser()">
            <div class="modal-body">
              <div class="row">
                <div class="col-md-6">
                  <div class="form-group">
                    <label for="firstName" class="form-label">First Name *</label>
                    <input type="text" id="firstName" formControlName="firstName" 
                           class="form-control"
                           [class.is-invalid]="dealerForm.get('firstName')?.invalid && dealerForm.get('firstName')?.touched">
                    <div *ngIf="dealerForm.get('firstName')?.invalid && dealerForm.get('firstName')?.touched" 
                         class="invalid-feedback">
                      First name is required
                    </div>
                  </div>
                </div>
                
                <div class="col-md-6">
                  <div class="form-group">
                    <label for="lastName" class="form-label">Last Name *</label>
                    <input type="text" id="lastName" formControlName="lastName" 
                           class="form-control"
                           [class.is-invalid]="dealerForm.get('lastName')?.invalid && dealerForm.get('lastName')?.touched">
                    <div *ngIf="dealerForm.get('lastName')?.invalid && dealerForm.get('lastName')?.touched" 
                         class="invalid-feedback">
                      Last name is required
                    </div>
                  </div>
                </div>
              </div>

              <div class="row">
                <div class="col-md-6">
                  <div class="form-group">
                    <label for="email" class="form-label">Email *</label>
                    <input type="email" id="email" formControlName="email" 
                           class="form-control"
                           [class.is-invalid]="dealerForm.get('email')?.invalid && dealerForm.get('email')?.touched">
                    <div *ngIf="dealerForm.get('email')?.invalid && dealerForm.get('email')?.touched" 
                         class="invalid-feedback">
                      Valid email is required
                    </div>
                  </div>
                </div>
                
                <div class="col-md-6">
                  <div class="form-group">
                    <label for="phoneNumber" class="form-label">Phone Number *</label>
                    <input type="tel" id="phoneNumber" formControlName="phoneNumber" 
                           class="form-control"
                           [class.is-invalid]="dealerForm.get('phoneNumber')?.invalid && dealerForm.get('phoneNumber')?.touched">
                    <div *ngIf="dealerForm.get('phoneNumber')?.invalid && dealerForm.get('phoneNumber')?.touched" 
                         class="invalid-feedback">
                      Phone number is required
                    </div>
                  </div>
                </div>
              </div>

              <div class="form-group">
                <label for="dealerName" class="form-label">Dealer/Company Name *</label>
                <input type="text" id="dealerName" formControlName="dealerName" 
                       class="form-control"
                       [class.is-invalid]="dealerForm.get('dealerName')?.invalid && dealerForm.get('dealerName')?.touched">
                <div *ngIf="dealerForm.get('dealerName')?.invalid && dealerForm.get('dealerName')?.touched" 
                     class="invalid-feedback">
                  Dealer name is required
                </div>
              </div>

              <div class="form-group">
                <label for="lnId" class="form-label">LN-ID *</label>
                <input type="text" id="lnId" formControlName="lnId" 
                       class="form-control"
                       [class.is-invalid]="dealerForm.get('lnId')?.invalid && dealerForm.get('lnId')?.touched">
                <div *ngIf="dealerForm.get('lnId')?.invalid && dealerForm.get('lnId')?.touched" 
                     class="invalid-feedback">
                  LN-ID is required
                </div>
              </div>

              <div class="form-group" *ngIf="editingDealer">
                <label class="form-label">Status</label>
                <div class="form-check form-switch">
                  <input class="form-check-input" type="checkbox" id="isActive" formControlName="isActive">
                  <label class="form-check-label" for="isActive">
                    {{ dealerForm.get('isActive')?.value ? 'Active' : 'Inactive' }}
                  </label>
                </div>
              </div>

              <div class="row" *ngIf="!editingDealer">
                <div class="col-md-6">
                  <div class="form-group">
                    <label for="password" class="form-label">Password *</label>
                    <input type="password" id="password" formControlName="password" 
                           class="form-control"
                           [class.is-invalid]="dealerForm.get('password')?.invalid && dealerForm.get('password')?.touched">
                    <div *ngIf="dealerForm.get('password')?.invalid && dealerForm.get('password')?.touched" 
                         class="invalid-feedback">
                      Password is required (minimum 6 characters)
                    </div>
                  </div>
                </div>
                
                <div class="col-md-6">
                  <div class="form-group">
                    <label for="confirmPassword" class="form-label">Confirm Password *</label>
                    <input type="password" id="confirmPassword" formControlName="confirmPassword" 
                           class="form-control"
                           [class.is-invalid]="dealerForm.get('confirmPassword')?.invalid && dealerForm.get('confirmPassword')?.touched">
                    <div *ngIf="dealerForm.get('confirmPassword')?.invalid && dealerForm.get('confirmPassword')?.touched" 
                         class="invalid-feedback">
                      <span *ngIf="dealerForm.get('confirmPassword')?.errors?.['required']">Confirm password is required</span>
                      <span *ngIf="dealerForm.get('confirmPassword')?.errors?.['passwordMismatch']">Passwords do not match</span>
                    </div>
                  </div>
                </div>
              </div>

              <div *ngIf="errorMessage" class="alert alert-danger">
                {{ errorMessage }}
              </div>
            </div>
            
            <div class="modal-footer">
              <button type="button" class="btn btn-secondary" (click)="closeDealerModal()">
                Cancel
              </button>
              <button type="submit" 
                      class="btn btn-primary" 
                      [disabled]="dealerForm.invalid || saving">
                <span *ngIf="saving" class="spinner-border spinner-border-sm me-2"></span>
                {{ editingDealer ? 'Update' : 'Create' }} Dealer
              </button>
            </div>
          </form>
        </div>
      </div>
    </div>
    
    <!-- View Dealer Modal -->
    <div class="modal fade" 
         [class.show]="showViewModal" 
         [style.display]="showViewModal ? 'block' : 'none'"
         tabindex="-1">
      <div class="modal-dialog modal-lg">
        <div class="modal-content">
          <div class="modal-header">
            <h5 class="modal-title">
              <i class="material-icons">visibility</i>
              View Dealer Details
            </h5>
            <button type="button" class="btn-close" (click)="closeViewModal()"></button>
          </div>
          
          <div class="modal-body" *ngIf="viewingDealer">
            <div class="row mb-3">
              <div class="col-md-6">
                <div class="detail-group">
                  <label class="detail-label">Dealer ID</label>
                  <p class="detail-value">{{ viewingDealer.dealerId }}</p>
                </div>
              </div>
              <div class="col-md-6">
                <div class="detail-group">
                  <label class="detail-label">Status</label>
                  <p class="detail-value">
                    <span class="badge" [class]="viewingDealer.isActive ? 'badge-success' : 'badge-danger'">
                      {{ viewingDealer.isActive ? 'Active' : 'Inactive' }}
                    </span>
                  </p>
                </div>
              </div>
            </div>

            <div class="row mb-3">
              <div class="col-md-6">
                <div class="detail-group">
                  <label class="detail-label">Full Name</label>
                  <p class="detail-value">{{ viewingDealer.fullName || 'N/A' }}</p>
                </div>
              </div>
              <div class="col-md-6">
                <div class="detail-group">
                  <label class="detail-label">Dealer/Company Name</label>
                  <p class="detail-value">{{ viewingDealer.dealerName }}</p>
                </div>
              </div>
            </div>

            <div class="row mb-3">
              <div class="col-md-6">
                <div class="detail-group">
                  <label class="detail-label">LN-ID</label>
                  <p class="detail-value">{{ viewingDealer.ln_ID || 'N/A' }}</p>
                </div>
              </div>
              <div class="col-md-6">
                <div class="detail-group">
                  <label class="detail-label">Username</label>
                  <p class="detail-value">{{ viewingDealer.userName || 'N/A' }}</p>
                </div>
              </div>
            </div>

            <div class="row mb-3">
              <div class="col-md-6">
                <div class="detail-group">
                  <label class="detail-label">Email</label>
                  <p class="detail-value">{{ viewingDealer.email || 'N/A' }}</p>
                </div>
              </div>
              <div class="col-md-6">
                <div class="detail-group">
                  <label class="detail-label">Phone Number</label>
                  <p class="detail-value">{{ viewingDealer.phoneNumber || 'N/A' }}</p>
                </div>
              </div>
            </div>

            <div class="row mb-3">
              <div class="col-md-6">
                <div class="detail-group">
                  <label class="detail-label">Credit Limit</label>
                  <p class="detail-value">{{ viewingDealer.creditLimit || 0 | number:'1.2-2' }}</p>
                </div>
              </div>
              <div class="col-md-6">
                <div class="detail-group">
                  <label class="detail-label">Current Balance</label>
                  <p class="detail-value">{{ viewingDealer.currentBalance || 0 | number:'1.2-2' }}</p>
                </div>
              </div>
            </div>

            <div class="row mb-3">
              <div class="col-md-6">
                <div class="detail-group">
                  <label class="detail-label">Created Date</label>
                  <p class="detail-value">{{ viewingDealer.createdDate | date:'medium' }}</p>
                </div>
              </div>
              <div class="col-md-6">
                <div class="detail-group">
                  <label class="detail-label">Modified Date</label>
                  <p class="detail-value">{{ viewingDealer.modifiedDate ? (viewingDealer.modifiedDate | date:'medium') : 'N/A' }}</p>
                </div>
              </div>
            </div>

            <div class="row mb-3">
              <div class="col-md-12">
                <div class="detail-group">
                  <label class="detail-label">User ID</label>
                  <p class="detail-value">{{ viewingDealer.userId }}</p>
                </div>
              </div>
            </div>
          </div>
          
          <div class="modal-footer">
            <button type="button" class="btn btn-secondary" (click)="closeViewModal()">
              Close
            </button>
            <button type="button" class="btn btn-primary" (click)="editDealer(viewingDealer!); closeViewModal();">
              <i class="material-icons">edit</i>
              Edit Dealer
            </button>
          </div>
        </div>
      </div>
    </div>

    <!-- Modal backdrop -->
    <div class="modal-backdrop fade" 
         [class.show]="showDealerModal || showViewModal" 
         *ngIf="showDealerModal || showViewModal"
         (click)="showDealerModal ? closeDealerModal() : closeViewModal()"></div>
  `,
  styleUrls: ['./dealers.component.scss']
})
export class DealersComponent implements OnInit {
  dealers: Dealer[] = [];
  loading = false;
  searchTerm = '';
  selectedStatus = '';
  
  // Pagination
  currentPage = 1;
  pageSize = 10;
  totalCount = 0;
  totalPages = 0;
  
  // Modal
  showDealerModal = false;
  showViewModal = false;
  viewingDealer: Dealer | null = null;
  editingDealer: Dealer | null = null;
  dealerForm!: FormGroup;
  saving = false;
  errorMessage = '';

  // Expose Math to template
  Math = Math;

  constructor(
    private dealerService: DealerManagementService,
    private fb: FormBuilder,
    private router: Router
  ) {
    this.initForm();
  }

  ngOnInit(): void {
    this.loadDealers();
  }

  initForm(): void {
    this.dealerForm = this.fb.group({
      firstName: ['', Validators.required],
      lastName: ['', Validators.required],
      email: ['', [Validators.required, Validators.email]],
      phoneNumber: ['', Validators.required],
      dealerName: ['', Validators.required],
      lnId: ['', Validators.required],
      password: ['', [Validators.required, Validators.minLength(6)]],
      confirmPassword: ['', Validators.required],
      isActive: [true]
    }, { validators: this.passwordMatchValidator });
  }

  passwordMatchValidator(group: AbstractControl): any {
    const password = group.get('password');
    const confirmPassword = group.get('confirmPassword');
    
    if (!password || !confirmPassword) {
      return null;
    }
    
    if (password.value !== confirmPassword.value) {
      confirmPassword.setErrors({ passwordMismatch: true });
      return { passwordMismatch: true };
    } else {
      confirmPassword.setErrors(null);
      return null;
    }
  }

  loadDealers(): void {
    this.loading = true;
    
    const isActiveFilter = this.selectedStatus === '' ? undefined : this.selectedStatus === 'true';
    const searchTermFilter = this.searchTerm.trim() || undefined;
    
    this.dealerService.getAllDealers(this.currentPage, this.pageSize, searchTermFilter, isActiveFilter).subscribe({
      next: (response) => {
        this.dealers = response.data || [];
        this.totalCount = response.metadata?.totalCount || 0;
        this.totalPages = response.metadata?.totalPages || Math.ceil(this.totalCount / this.pageSize);
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading dealers:', error);
        this.dealers = [];
        this.totalCount = 0;
        this.totalPages = 0;
        this.loading = false;
      }
    });
  }

  applyFilters(): void {
    this.currentPage = 1;
    this.loadDealers();
  }

  openCreateDealerModal(): void {
    this.editingDealer = null;
    this.dealerForm.reset();
    this.errorMessage = '';
    this.showDealerModal = true;
    
    // For creating new dealer, password and confirmPassword are required
    this.dealerForm.get('password')?.setValidators([Validators.required, Validators.minLength(6)]);
    this.dealerForm.get('confirmPassword')?.setValidators([Validators.required]);
    this.dealerForm.get('password')?.updateValueAndValidity();
    this.dealerForm.get('confirmPassword')?.updateValueAndValidity();
  }

  editDealer(dealer: Dealer): void {
    this.editingDealer = dealer;
    
    // Split fullName if available
    const names = dealer.fullName?.split(' ') || ['', ''];
    const firstName = names[0] || '';
    const lastName = names.length > 1 ? names.slice(1).join(' ') : '';
    
    this.dealerForm.patchValue({
      firstName: firstName,
      lastName: lastName,
      email: dealer.email || '',
      phoneNumber: dealer.phoneNumber || '',
      dealerName: dealer.dealerName,
      lnId: dealer.ln_ID || '',
      isActive: dealer.isActive
    });
    
    // For editing, password and confirmPassword are not required
    this.dealerForm.get('password')?.clearValidators();
    this.dealerForm.get('confirmPassword')?.clearValidators();
    this.dealerForm.get('password')?.updateValueAndValidity();
    this.dealerForm.get('confirmPassword')?.updateValueAndValidity();
    
    this.errorMessage = '';
    this.showDealerModal = true;
  }

  closeDealerModal(): void {
    this.showDealerModal = false;
    this.editingDealer = null;
    this.dealerForm.reset();
    this.errorMessage = '';
  }

  saveDealerUser(): void {
    if (this.dealerForm.invalid) return;

    this.saving = true;
    this.errorMessage = '';

    const formValue = this.dealerForm.value;
    const fullName = `${formValue.firstName} ${formValue.lastName}`.trim();
    const userName = formValue.email; // Use email as username
    
    if (this.editingDealer) {
      // Update existing dealer
      const updateRequest: UpdateDealerUserRequest = {
        fullName: fullName,
        userName: userName,
        email: formValue.email,
        phone: formValue.phoneNumber,
        dealerName: formValue.dealerName,
        creditLimit: this.editingDealer.creditLimit || 0,
        currentBalance: this.editingDealer.currentBalance || 0,
        ln_ID: formValue.lnId || '',
        isActive: formValue.isActive
      };

      this.dealerService.updateDealerUser(this.editingDealer.dealerId, updateRequest).subscribe({
        next: (response) => {
          this.saving = false;
          if (response.success) {
            this.closeDealerModal();
            this.loadDealers(); // Refresh the list
          } else {
            this.errorMessage = response.message || 'Failed to update dealer';
          }
        },
        error: (error) => {
          this.saving = false;
          this.errorMessage = error.error?.message || 'An error occurred while updating dealer';
        }
      });
    } else {
      // Create new dealer
      const createRequest: CreateDealerUserRequest = {
        fullName: fullName,
        userName: userName,
        email: formValue.email,
        phone: formValue.phoneNumber,
        password: formValue.password || 'TempPassword123!',
        confirmationPassword: formValue.confirmPassword || formValue.password || 'TempPassword123!',
        dealerName: formValue.dealerName,
        creditLimit: 0,
        currentBalance: 0,
        ln_ID: formValue.lnId || ''
      };

      this.dealerService.createDealerUser(createRequest).subscribe({
        next: (response) => {
          this.saving = false;
          if (response.success) {
            this.closeDealerModal();
            this.loadDealers(); // Refresh the list
          } else {
            this.errorMessage = response.message || 'Failed to create dealer';
          }
        },
        error: (error) => {
          this.saving = false;
          this.errorMessage = error.error?.message || 'An error occurred while creating dealer';
        }
      });
    }
  }

  viewDealerDetails(dealer: Dealer): void {
    this.viewingDealer = dealer;
    this.showViewModal = true;
  }

  closeViewModal(): void {
    this.showViewModal = false;
    this.viewingDealer = null;
  }

  deleteDealer(dealer: Dealer): void {
    if (!confirm(`Are you sure you want to delete dealer "${dealer.dealerName}"? This action cannot be undone.`)) {
      return;
    }

    this.dealerService.deleteDealer(dealer.dealerId).subscribe({
      next: (response) => {
        if (response.success) {
          // Refresh the list after successful deletion
          this.loadDealers();
        } else {
          alert(response.message || 'Failed to delete dealer');
        }
      },
      error: (error) => {
        console.error('Error deleting dealer:', error);
        alert(error.error?.message || 'An error occurred while deleting dealer');
      }
    });
  }

  goToPage(page: number): void {
    if (page >= 1 && page <= this.totalPages) {
      this.currentPage = page;
      this.loadDealers();
    }
  }

  getPageNumbers(): number[] {
    const pages: number[] = [];
    const maxPagesToShow = 5;
    let startPage = Math.max(1, this.currentPage - Math.floor(maxPagesToShow / 2));
    let endPage = Math.min(this.totalPages, startPage + maxPagesToShow - 1);
    
    if (endPage - startPage < maxPagesToShow - 1) {
      startPage = Math.max(1, endPage - maxPagesToShow + 1);
    }
    
    for (let i = startPage; i <= endPage; i++) {
      pages.push(i);
    }
    
    return pages;
  }
}