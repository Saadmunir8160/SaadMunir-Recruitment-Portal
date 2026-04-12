import { Component, OnInit } from '@angular/core';
import { VendorsService } from '../services/vendors.service';
import { VendorDetailsModalComponent } from '../vendor-details-modal/vendor-details-modal.component';
import { CommonModule } from '@angular/common';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { environment } from '../../../environments/environment';
import { CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatSnackBar } from '@angular/material/snack-bar';
import { SuccessSnackbarComponent } from '../../shared/components/success-snackbar/success-snackbar.component';
import { PaginationComponent } from '../../shared/components/pagination/pagination.component';
import { PaginatedResponse } from '../../shared/interfaces/paginated-response.interface';

@Component({
  selector: 'app-vendors-list',
  standalone: true,
  imports: [
    CommonModule,
    MatDialogModule,
    MatButtonModule,
    FormsModule,
    PaginationComponent
  ],
  schemas: [CUSTOM_ELEMENTS_SCHEMA],
  templateUrl: './vendors-list.component.html',
  styleUrl: './vendors-list.component.scss'
})
export class VendorsListComponent implements OnInit {
  allVendorsList: any[] = [];
  filteredVendorsList: any[] = [];
  searchText: string = '';
  loadingVendorIds: Set<number> = new Set();
  deletingVendorIds: Set<number> = new Set();
  showNotApprovedOnly: boolean = false;
  showApprovedOnly: boolean = false;

  // Pagination state
  currentPage: number = 1;
  pageSize: number = 10;
  pageSizeOptions: (number | string)[] = [5, 10, 15, 20, 30, 50, 'All'];
  totalCount: number = 0;
  totalPages: number = 0;
  loading: boolean = false;
  // Removed isServerSidePagination, always client-side

  constructor(
    private vendorsService: VendorsService,
    private dialog: MatDialog,
    private snackBar: MatSnackBar
  ) {}

  ngOnInit(): void {
    this.getAllVendors();
  }

  getAllVendors(): void {
    this.loading = true;
    const pageSizeToSend = this.pageSize === 0 ? 1000000 : this.pageSize;
    this.vendorsService.getVendors(this.currentPage, pageSizeToSend).subscribe({
      next: (res) => {
        let vendorsData: any[] = Array.isArray(res.data) ? res.data : [];
        // Transform file paths before assigning to vendorsList
        this.allVendorsList = vendorsData.map((vendor: any) => ({
          vendorId: vendor.vendorId ?? vendor.VendorID ?? vendor.id,
          name: vendor.name ?? vendor.Name,
          contactPerson: vendor.contactPerson ?? vendor.ContactPerson,
          email: vendor.email ?? vendor.Email,
          phoneNo: vendor.phoneNo ?? vendor.PhoneNo,
          isApproved: vendor.isApproved ?? vendor.IsApproved,
          bpId: vendor.bpId ?? vendor.BpId ?? vendor.BPID,
          crCertificateFilePath: this.transformFilePath(vendor.crCertificateFilePath ?? vendor.CrCertificateFilePath),
          vatCertificateFilePath: this.transformFilePath(vendor.vatCertificateFilePath ?? vendor.VatCertificateFilePath),
          iSO9001_2015FilePath: this.transformFilePath(vendor.iSO9001_2015FilePath ?? vendor.ISO9001_2015FilePath),
          iSO14001_2015FilePath: this.transformFilePath(vendor.iSO14001_2015FilePath ?? vendor.ISO14001_2015FilePath),
          iSO45001_2018FilePath: this.transformFilePath(vendor.iSO45001_2018FilePath ?? vendor.ISO45001_2018FilePath),
          companyProfileFilePath: this.transformFilePath(vendor.companyProfileFilePath ?? vendor.CompanyProfileFilePath),
          address: vendor.address ?? vendor.Address,
          faxNo: vendor.faxNo ?? vendor.FaxNo,
          contactPersonEmail: vendor.contactPersonEmail ?? vendor.ContactPersonEmail,
          contactPersonPhoneNo: vendor.contactPersonPhoneNo ?? vendor.ContactPersonPhoneNo,
          category: vendor.category ?? vendor.Category,
          employees: vendor.employees ?? vendor.Employees,
          taxRegistrationNo: vendor.taxRegistrationNo ?? vendor.TaxRegistrationNo,
          crNo: vendor.crNo ?? vendor.CrNo,
          productDetails: vendor.productDetails ?? vendor.ProductDetails,
          annualTurnover: vendor.annualTurnover ?? vendor.AnnualTurnover,
          majorCustomers: vendor.majorCustomers ?? vendor.MajorCustomers,
          createdDate: vendor.createdDate ?? vendor.CreatedDate
        })).sort((a, b) => {
          const dateA = a.createdDate ? new Date(a.createdDate).getTime() : 0;
          const dateB = b.createdDate ? new Date(b.createdDate).getTime() : 0;
          return dateB - dateA;
        });
        this.totalCount = res.metadata?.totalCount || vendorsData.length;
        this.totalPages = res.metadata?.totalPages || 1;
        this.filteredVendorsList = this.allVendorsList;
        this.loading = false;
      },
      error: (err) => {
        console.error('Error fetching vendors:', err);
        this.loading = false;
      }
    });
  }

  // Remove client-side pagination from applyFiltersAndPaginate
  applyFiltersAndPaginate(): void {
    // Only apply filters, not pagination
    let filtered = [...this.allVendorsList];
    if (this.showNotApprovedOnly) {
      filtered = filtered.filter(vendor => !vendor.isApproved);
    } else if (this.showApprovedOnly) {
      filtered = filtered.filter(vendor => vendor.isApproved);
    }
    if (this.searchText) {
      const searchTerm = this.searchText.toLowerCase().trim();
      filtered = filtered.filter(vendor =>
        vendor.name?.toLowerCase().includes(searchTerm) ||
        vendor.address?.toLowerCase().includes(searchTerm) ||
        vendor.email?.toLowerCase().includes(searchTerm) ||
        vendor.contactPerson?.toLowerCase().includes(searchTerm) ||
        vendor.phoneNo?.toLowerCase().includes(searchTerm) ||
        vendor.vendorId?.toString().includes(searchTerm) ||
        vendor.bpId?.toString().includes(searchTerm) ||
        (vendor.isApproved !== undefined && (vendor.isApproved ? 'approved' : 'not approved').includes(searchTerm))
      );
    }
    this.filteredVendorsList = filtered;
  }

  onFilterChange(): void {
    this.currentPage = 1;
    this.applyFiltersAndPaginate();
  }

  onSearch(): void {
    this.currentPage = 1;
    this.applyFiltersAndPaginate();
  }

  goToPage(page: number): void {
    this.currentPage = page;
    this.getAllVendors();
  }

  onPageSizeChange(size: number | string): void {
    this.pageSize = Number(size);
    this.currentPage = 1;
    this.getAllVendors();
  }

  transformFilePath(path: string): string {
    if (!path) return '';
    
    // Extract just the filename from the path
    const parts = path.split(/[/\\]/);
    const filename = parts[parts.length - 1];
    
    // Construct the final URL with the correct structure
    return `${environment.apiUrl.replace('/api', '')}/static/Vendors/${filename}`;
  }

  approveVendors(vendorId: number): void {
    if (confirm('Are you sure you want to approve this vendor?')) {
      this.loadingVendorIds.add(vendorId);
      
      this.vendorsService.approveVendors(vendorId).subscribe({
        next: (bpId) => {
          // Update vendor in the lists
          const vendor = this.allVendorsList.find(v => v.vendorId === vendorId);
          if (vendor) {
            vendor.isApproved = true;
            vendor.bpId = bpId;
          }
          
          // Show success message with custom snackbar
          this.snackBar.openFromComponent(SuccessSnackbarComponent, {
            data: {
              message: `Vendor is approved and created in LN with Business Partner ID ${bpId}`
            },
            duration: 5000,
            horizontalPosition: 'center',
            verticalPosition: 'top',
            panelClass: ['custom-snackbar']
          });
          
          // Refresh the current page after approval
          this.getAllVendors();
        },
        error: (err) => {
          console.error('Error approving vendor:', err);
          this.snackBar.open('Error approving vendor', 'Close', {
            duration: 3000,
            horizontalPosition: 'center',
            verticalPosition: 'top'
          });
        },
        complete: () => {
          this.loadingVendorIds.delete(vendorId);
        }
      });
    }
  }

  deleteVendor(vendorId: number): void {
    if (confirm('Are you sure you want to delete this vendor?')) {
      this.deletingVendorIds.add(vendorId);
      
      this.vendorsService.deleteVendor(vendorId).subscribe({
        next: (success) => {
          if (success) {
            // Remove vendor from the lists
            this.allVendorsList = this.allVendorsList.filter(v => v.vendorId !== vendorId);
            
            // Show success message
            this.snackBar.open('Vendor deleted successfully', 'Close', {
              duration: 3000,
              horizontalPosition: 'center',
              verticalPosition: 'top'
            });
            
            // Refresh the current page after deletion
            this.getAllVendors();
          } else {
            this.snackBar.open('Failed to delete vendor', 'Close', {
              duration: 3000,
              horizontalPosition: 'center',
              verticalPosition: 'top'
            });
          }
        },
        error: (err) => {
          console.error('Error deleting vendor:', err);
          this.snackBar.open('Error deleting vendor', 'Close', {
            duration: 3000,
            horizontalPosition: 'center',
            verticalPosition: 'top'
          });
        },
        complete: () => {
          this.deletingVendorIds.delete(vendorId);
        }
      });
    }
  }

  showVendorDetails(vendor: any): void {
    const dialogRef = this.dialog.open(VendorDetailsModalComponent, {
      width: '800px',
      data: { vendor: vendor }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        // Refresh the current page after any changes
        this.getAllVendors();
      }
    });
  }

  isLoading(vendorId: number): boolean {
    return this.loadingVendorIds.has(vendorId);
  }

  isDeleting(vendorId: number): boolean {
    return this.deletingVendorIds.has(vendorId);
  }

  onNotApprovedChange(): void {
    if (this.showNotApprovedOnly) {
      this.showApprovedOnly = false;
    }
    this.onFilterChange();
  }

  onApprovedChange(): void {
    if (this.showApprovedOnly) {
      this.showNotApprovedOnly = false;
    }
    this.onFilterChange();
  }

  trackByVendorId(index: number, vendor: any): number {
    return vendor.vendorId;
  }
}
