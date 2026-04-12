import { Component, OnInit, CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';
import { DealerAddressService, DealerAddress } from '../../../services/dealer-address.service';
import { AuthService } from '../../../../auth/auth.service';
import { PaginationComponent } from '../../../../shared/components/pagination/pagination.component';

@Component({
  selector: 'app-addresses-list',
  imports: [CommonModule, RouterModule, FormsModule, TranslateModule, PaginationComponent],
  templateUrl: './addresses-list.component.html',
  styleUrl: './addresses-list.component.scss',
  schemas: [CUSTOM_ELEMENTS_SCHEMA]
})
export class AddressesListComponent implements OnInit {
  addresses: DealerAddress[] = [];
  filteredAddresses: DealerAddress[] = [];
  paginatedAddresses: DealerAddress[] = [];
  
  // Pagination
  currentPage: number = 1;
  pageSize: number = 6;
  totalPages: number = 0;
  totalCount: number = 0;
  
  // Search and Filter
  searchTerm: string = '';
  statusFilter: string = 'all';
  
  // UI State
  loading: boolean = false;
  
  // Messages
  successMessage: string = '';
  errorMessage: string = '';
  
  // Math object for template
  Math = Math;
  
  // User context
  currentUserId: number = 0;

  constructor(
    private dealerAddressService: DealerAddressService,
    private authService: AuthService
  ) {}

  ngOnInit(): void {
    // Load addresses directly since the API endpoint handles authentication
    this.loadAddresses();
    
    // Also load user info for any future use
    this.loadCurrentUser();
  }

  private loadCurrentUser(): void {
    this.authService.getCurrentUser().subscribe({
      next: (user) => {
        if (user?.id) {
          this.currentUserId = user.id;
        }
      },
      error: (error) => {
      }
    });
  }

  loadAddresses(): void {
    this.loading = true;
    this.clearMessages();

    this.dealerAddressService.getAddresses().subscribe({
      next: (addresses) => {
        this.addresses = addresses || [];
        this.applyFilters();
        this.loading = false;
        

      },
      error: (error) => {
        this.errorMessage = 'Failed to load addresses. Please try again.';
        this.loading = false;
      }
    });
  }

  onSearch(): void {
    this.currentPage = 1;
    this.applyFilters();
  }

  clearSearch(): void {
    this.searchTerm = '';
    this.onSearch();
  }

  onPageSizeChange(): void {
    this.currentPage = 1;
    this.applyFilters();
  }

  onStatusFilterChange(): void {
    this.currentPage = 1;
    this.applyFilters();
  }

  private applyFilters(): void {
    let filtered = [...this.addresses];

    // Apply status filter
    if (this.statusFilter && this.statusFilter !== 'all') {
      const isActive = this.statusFilter === 'active';
      filtered = filtered.filter(address => address.isActive === isActive);
    }

    // Apply search filter
    if (this.searchTerm) {
      const term = this.searchTerm.toLowerCase();
      filtered = filtered.filter(address => 
        address.addressLine1?.toLowerCase().includes(term) ||
        address.addressLine2?.toLowerCase().includes(term) ||
        address.city?.toLowerCase().includes(term) ||
        address.state?.toLowerCase().includes(term) ||
        address.postalCode?.toLowerCase().includes(term) ||
        address.country?.toLowerCase().includes(term)
      );
    }

    this.filteredAddresses = filtered;
    this.updatePagination();
  }

  private updatePagination(): void {
    this.totalCount = this.filteredAddresses.length;
    this.totalPages = Math.ceil(this.totalCount / this.pageSize);
        
    if (this.currentPage > this.totalPages) {
      this.currentPage = 1;
    }

    const startIndex = (this.currentPage - 1) * this.pageSize;
    const endIndex = startIndex + this.pageSize;
    this.paginatedAddresses = this.filteredAddresses.slice(startIndex, endIndex);
    
  }

  goToPage(page: number): void {
    if (page >= 1 && page <= this.totalPages) {
      this.currentPage = page;
      this.updatePagination();
    }
  }

  deleteAddress(addressId: string): void {
    if (confirm('Are you sure you want to delete this address?')) {
      this.clearMessages();

      this.dealerAddressService.deleteAddress(addressId).subscribe({
        next: () => {
          this.successMessage = 'Address deleted successfully';
          this.loadAddresses(); // Reload to refresh the list
          
          // Clear success message after 3 seconds
          setTimeout(() => {
            this.successMessage = '';
          }, 3000);
        },
        error: (error) => {
          this.errorMessage = 'Failed to delete address. Please try again.';
          
          // Clear error message after 5 seconds
          setTimeout(() => {
            this.errorMessage = '';
          }, 5000);
        }
      });
    }
  }

  private clearMessages(): void {
    this.successMessage = '';
    this.errorMessage = '';
  }
}
