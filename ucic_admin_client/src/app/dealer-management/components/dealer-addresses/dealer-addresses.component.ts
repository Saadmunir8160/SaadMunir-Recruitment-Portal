import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { DealerManagementService } from '../../services/dealer-management.service';

interface DealerAddress {
  addressId: number;
  dealerId: number;
  dealerName?: string;
  addressLine1: string;
  addressLine2?: string;
  city: string;
  state: string;
  postalCode: string;
  country: string;
  addressType: string; // 'Billing', 'Shipping', 'Both'
  isActive: boolean;
  isDefault: boolean;
  createdDate: Date;
  modifiedDate: Date;
}



@Component({
  selector: 'app-dealer-addresses',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './dealer-addresses.component.html',
  styleUrls: ['./dealer-addresses.component.scss']
})
export class DealerAddressesComponent implements OnInit {
  addresses: DealerAddress[] = [];
  loading = false;
  totalItems = 0;
  currentPage = 1;
  pageSize = 10;
  searchTerm = '';
  // Admin read-only view - simplified for shipping addresses only

  // Make Math available in template
  Math = Math;

  constructor(
    private dealerService: DealerManagementService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.loadAddresses();
  }

  loadAddresses(): void {
    this.loading = true;
    const params = {
      pageNumber: this.currentPage,
      pageSize: this.pageSize,
      search: this.searchTerm || undefined
    };

    this.dealerService.getDealerAddresses(params).subscribe({
      next: (response: any) => {
        // Handle the nested data structure from API response
        if (response?.data?.data && Array.isArray(response.data.data)) {
          this.addresses = response.data.data;
          this.totalItems = response.data.metadata?.totalCount || 0;
        } else if (response?.data && Array.isArray(response.data)) {
          // Fallback for direct data array
          this.addresses = response.data;
          this.totalItems = response.metadata?.totalCount || 0;
        } else {
          this.addresses = [];
          this.totalItems = 0;
        }
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading addresses:', error);
        this.loading = false;
        this.addresses = [];
        this.totalItems = 0;
      }
    });
  }



  onSearch(): void {
    this.currentPage = 1;
    this.loadAddresses();
  }

  onFilterChange(): void {
    this.currentPage = 1;
    this.loadAddresses();
  }

  onPageChange(page: number): void {
    this.currentPage = page;
    this.loadAddresses();
  }



  // Admin read-only view - removed form and CRUD operations

  getDealerName(address: DealerAddress): string {
    return address.dealerName || `Dealer ID: ${address.dealerId}`;
  }

  formatAddress(address: DealerAddress): string {
    const parts = [
      address.addressLine1,
      address.addressLine2,
      address.city,
      address.state,
      address.postalCode
    ].filter(part => part && part.trim());
    
    return parts.join(', ');
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

  goBack(): void {
    this.router.navigate(['/admin/dealer-management']);
  }
}