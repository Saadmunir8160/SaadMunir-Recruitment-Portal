import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { MatDialog } from '@angular/material/dialog';
import { 
  DealerManagementService, 
  DealerVehicle, 
  Dealer
} from '../services/dealer-management.service';
import { PaginatedResponse } from '../../shared/interfaces/paginated-response.interface';
import { VehicleViewDialogComponent } from './vehicle-view-dialog.component';
import { VehicleEditDialogComponent } from './vehicle-edit-dialog.component';

@Component({
  selector: 'app-dealer-vehicles',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './dealer-vehicles.component.html',
  styleUrls: ['./dealer-vehicles.component.scss']
})
export class DealerVehiclesComponent implements OnInit {
  vehicles: DealerVehicle[] = [];
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
  selectedVehicleType = '';

  // Vehicle types (you can expand this based on your business needs)
  vehicleTypes = [
    { value: '', label: 'All Types' },
    { value: 'Truck', label: 'Truck' },
    { value: 'Van', label: 'Van' },
    { value: 'Car', label: 'Car' },
    { value: 'Motorcycle', label: 'Motorcycle' },
    { value: 'Other', label: 'Other' }
  ];

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
    this.loadVehicles();
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

  loadVehicles(): void {
    this.loading = true;
    this.error = null;

    this.dealerService.getDealerVehicles(
      this.currentPage,
      this.pageSize,
      this.searchTerm || undefined,
      this.selectedDealerId || undefined,
      this.selectedStatus ?? undefined,
      this.selectedVehicleType || undefined
    ).subscribe({
      next: (response: PaginatedResponse<DealerVehicle>) => {
        this.vehicles = response.data;
        this.totalRecords = response.metadata.totalCount;
        this.totalPages = response.metadata.totalPages;
        this.loading = false;
      },
      error: (error) => {
        this.error = 'Failed to load dealer vehicles';
        console.error('Error loading vehicles:', error);
        this.loading = false;
      }
    });
  }

  onSearch(): void {
    this.currentPage = 1;
    this.loadVehicles();
  }

  onClearFilters(): void {
    this.searchTerm = '';
    this.selectedDealerId = null;
    this.selectedStatus = null;
    this.selectedVehicleType = '';
    this.currentPage = 1;
    this.loadVehicles();
  }

  onPageChange(page: number): void {
    this.currentPage = page;
    this.loadVehicles();
  }

  viewVehicleDetails(vehicle: DealerVehicle): void {
    this.dialog.open(VehicleViewDialogComponent, {
      width: '700px',
      data: vehicle
    });
  }

  editVehicle(vehicle: DealerVehicle): void {
    const dialogRef = this.dialog.open(VehicleEditDialogComponent, {
      width: '600px',
      data: vehicle
    });

    dialogRef.afterClosed().subscribe((success: boolean) => {
      if (success) {
        // Reload the vehicles list after successful update
        this.loadVehicles();
      }
    });
  }

  deleteVehicle(vehicle: DealerVehicle): void {
    if (confirm(`Are you sure you want to delete vehicle "${vehicle.vehicleName}"?`)) {
      this.dealerService.deleteDealerVehicle(vehicle.vehicleID).subscribe({
        next: (response) => {
          if (response.success) {
            alert('Vehicle deleted successfully!');
            this.loadVehicles();
          } else {
            alert(`Failed to delete vehicle: ${response.message}`);
          }
        },
        error: (error) => {
          console.error('Error deleting vehicle:', error);
          alert('An error occurred while deleting the vehicle.');
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

  getVehicleTypeClass(vehicleType: string): string {
    switch (vehicleType?.toLowerCase()) {
      case 'truck':
        return 'badge-primary';
      case 'van':
        return 'badge-info';
      case 'car':
        return 'badge-success';
      case 'motorcycle':
        return 'badge-warning';
      default:
        return 'badge-secondary';
    }
  }

  getVehicleTypeIcon(vehicleType: string): string {
    switch (vehicleType?.toLowerCase()) {
      case 'truck':
        return 'fas fa-truck';
      case 'van':
        return 'fas fa-shuttle-van';
      case 'car':
        return 'fas fa-car';
      case 'motorcycle':
        return 'fas fa-motorcycle';
      default:
        return 'fas fa-car-side';
    }
  }

  formatDate(date: Date): string {
    return new Date(date).toLocaleDateString();
  }

  formatCapacity(capacity: number): string {
    return `${capacity} units`;
  }
}