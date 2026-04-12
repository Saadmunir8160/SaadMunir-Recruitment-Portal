import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { MatDialog } from '@angular/material/dialog';
import { 
  DealerManagementService, 
  DealerDriver, 
  Dealer
} from '../services/dealer-management.service';
import { PaginatedResponse } from '../../shared/interfaces/paginated-response.interface';
import { DriverViewDialogComponent } from './driver-view-dialog.component';
import { DriverEditDialogComponent } from './driver-edit-dialog.component';

@Component({
  selector: 'app-dealer-drivers',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './dealer-drivers.component.html',
  styleUrls: ['./dealer-drivers.component.scss']
})
export class DealerDriversComponent implements OnInit {
  drivers: DealerDriver[] = [];
  dealers: Dealer[] = [];
  loading = false;
  error: string | null = null;

  // Pagination
  currentPage = 1;
  pageSize = 10;
  totalPages = 0;
  totalRecords = 0;

  // Filtering
  searchTerm = '';
  selectedDealerId: number | null = null;
  selectedStatus: boolean | null = null;

  // Status options
  statusOptions = [
    { value: null, label: 'All Status' },
    { value: true, label: 'Active' },
    { value: false, label: 'Inactive' }
  ];

  constructor(
    private dealerService: DealerManagementService,
    private router: Router,
    private dialog: MatDialog
  ) {}

  ngOnInit(): void {
    this.loadDealers();
    this.loadDrivers();
  }

  loadDealers(): void {
    this.dealerService.getAllDealers(1, 100).subscribe({
      next: (response) => {
        this.dealers = response.data || [];
      },
      error: (error) => {
        console.error('Error loading dealers:', error);
      }
    });
  }

  loadDrivers(): void {
    this.loading = true;
    this.error = null;

    this.dealerService.getDealerDrivers(
      this.currentPage,
      this.pageSize,
      this.searchTerm || undefined,
      this.selectedDealerId || undefined,
      this.selectedStatus ?? undefined
    ).subscribe({
      next: (response: PaginatedResponse<DealerDriver>) => {
        this.drivers = response.data;
        this.totalRecords = response.metadata.totalCount;
        this.totalPages = response.metadata.totalPages;
        this.loading = false;
      },
      error: (error) => {
        this.error = 'Failed to load dealer drivers';
        console.error('Error loading drivers:', error);
        this.loading = false;
      }
    });
  }

  onSearch(): void {
    this.currentPage = 1;
    this.loadDrivers();
  }

  onClearFilters(): void {
    this.searchTerm = '';
    this.selectedDealerId = null;
    this.selectedStatus = null;
    this.currentPage = 1;
    this.loadDrivers();
  }

  onPageChange(page: number): void {
    this.currentPage = page;
    this.loadDrivers();
  }

  viewDriverDetails(driver: DealerDriver): void {
    this.dialog.open(DriverViewDialogComponent, {
      width: '700px',
      data: driver
    });
  }

  editDriver(driver: DealerDriver): void {
    const dialogRef = this.dialog.open(DriverEditDialogComponent, {
      width: '600px',
      data: driver
    });

    dialogRef.afterClosed().subscribe((success: boolean) => {
      if (success) {
        // Reload the drivers list after successful update
        this.loadDrivers();
      }
    });
  }

  deleteDriver(driver: DealerDriver): void {
    if (confirm(`Are you sure you want to delete driver "${driver.driverName}"?`)) {
      this.dealerService.deleteDealerDriver(driver.driverID).subscribe({
        next: (response) => {
          if (response.success) {
            alert('Driver deleted successfully!');
            this.loadDrivers();
          } else {
            alert(`Failed to delete driver: ${response.message}`);
          }
        },
        error: (error) => {
          console.error('Error deleting driver:', error);
          alert('An error occurred while deleting the driver.');
        }
      });
    }
  }

  getStatusClass(isActive: boolean): string {
    return isActive ? 'badge-success' : 'badge-danger';
  }

  getStatusText(isActive: boolean): string {
    return isActive ? 'Active' : 'Inactive';
  }

  formatDate(date: Date): string {
    return new Date(date).toLocaleDateString();
  }

  formatPhoneNumber(phone: string): string {
    // Simple phone number formatting
    if (!phone) return 'N/A';
    const cleaned = phone.replace(/\D/g, '');
    if (cleaned.length === 10) {
      return `(${cleaned.slice(0, 3)}) ${cleaned.slice(3, 6)}-${cleaned.slice(6)}`;
    }
    return phone;
  }
}