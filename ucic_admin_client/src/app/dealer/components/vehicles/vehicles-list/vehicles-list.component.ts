import { Component, OnInit, CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';
import { Vehicle, VehicleService } from '../../../services/vehicle.service';
import { PaginationComponent } from '../../../../shared/components/pagination/pagination.component';

@Component({
  selector: 'app-vehicles-list',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule, TranslateModule, PaginationComponent],
  schemas: [CUSTOM_ELEMENTS_SCHEMA],
  template: `
    <div>
      <div class="page-header">
        <div class="header-content">
          <h1 class="page-title">
            <iconify-icon icon="solar:bus-bold-duotone"></iconify-icon>
            {{ 'DEALER.VEHICLES.TITLE' | translate }}
          </h1>
          <p class="page-subtitle">{{ 'DEALER.VEHICLES.SUBTITLE' | translate }}</p>
        </div>
        <div class="header-actions">
          <button class="btn btn-success fs-14" routerLink="/dealer/vehicles/create">
            <iconify-icon icon="solar:add-circle-bold-duotone" width="20" height="20"></iconify-icon>
            {{ 'DEALER.VEHICLES.ADD_VEHICLE' | translate }}
          </button>
        </div>
      </div>

      <div class="filter-section" *ngIf="vehicles.length > 0">
        <div class="filter-controls">
          <div class="search-box">
            <i class="material-icons search-icon">search</i>
            <input 
              type="text"
              class="search-input"
              [placeholder]="'DEALER.VEHICLES.SEARCH.PLACEHOLDER' | translate" 
              [(ngModel)]="searchTerm"
              (input)="filterVehicles()">
          </div>
          
          <select class="status-filter" [(ngModel)]="typeFilter" (change)="filterVehicles()">
            <option value="">{{ 'DEALER.VEHICLES.FILTERS.ALL_TYPES' | translate }}</option>
            <option value="truck">{{ 'DEALER.VEHICLES.FILTERS.TRUCK' | translate }}</option>
            <option value="van">{{ 'DEALER.VEHICLES.FILTERS.VAN' | translate }}</option>
            <option value="pickup">{{ 'DEALER.VEHICLES.FILTERS.PICKUP' | translate }}</option>
            <option value="trailer">{{ 'DEALER.VEHICLES.FILTERS.TRAILER' | translate }}</option>
          </select>
          
          <select class="status-filter" [(ngModel)]="statusFilter" (change)="filterVehicles()">
            <option value="">{{ 'DEALER.VEHICLES.FILTERS.ALL_STATUS' | translate }}</option>
            <option value="active">{{ 'DEALER.VEHICLES.FILTERS.ACTIVE' | translate }}</option>
            <option value="inactive">{{ 'DEALER.VEHICLES.FILTERS.INACTIVE' | translate }}</option>
          </select>
          
          <div class="page-size-control">
            <label for="pageSize">{{ 'DEALER.VEHICLES.PAGINATION.SHOW' | translate }}:</label>
            <select id="pageSize" class="page-size-select" [(ngModel)]="pageSize" (ngModelChange)="onPageSizeChange($event)">
              <option *ngFor="let size of pageSizeOptions" [ngValue]="size === 'All' ? 0 : size">
                {{ size === 'All' ? ('DEALER.VEHICLES.PAGINATION.ALL' | translate) : size }}
              </option>
            </select>
          </div>
        </div>
      </div>

      <div class="content-section">
        <div *ngIf="loading" class="loading-state">
          <div class="spinner"></div>
          <p>{{ 'DEALER.VEHICLES.LOADING.MESSAGE' | translate }}</p>
        </div>

        <div *ngIf="!loading && paginatedVehicles.length === 0 && vehicles.length === 0" class="empty-state">
          <i class="material-icons">add_business</i>
          <h3>{{ 'DEALER.VEHICLES.EMPTY_STATE.NO_VEHICLES_TITLE' | translate }}</h3>
          <p>{{ 'DEALER.VEHICLES.EMPTY_STATE.NO_VEHICLES_MESSAGE' | translate }}</p>
          <button class="btn btn-primary" routerLink="/dealer/vehicles/create">
            {{ 'DEALER.VEHICLES.EMPTY_STATE.ADD_FIRST_VEHICLE' | translate }}
          </button>
        </div>

        <div *ngIf="!loading && paginatedVehicles.length === 0 && vehicles.length > 0" class="empty-state">
          <i class="material-icons">search_off</i>
          <h3>{{ 'DEALER.VEHICLES.EMPTY_STATE.NO_MATCH_TITLE' | translate }}</h3>
          <p>{{ 'DEALER.VEHICLES.EMPTY_STATE.NO_MATCH_MESSAGE' | translate }}</p>
        </div>

        <div *ngIf="!loading && paginatedVehicles.length > 0" class="vehicles-table-container">
          <div class="table-responsive">
            <table class="table table-striped table-hover">
              <thead class="table-dark">
                <tr>
                  <th>{{ 'DEALER.VEHICLES.TABLE.PLATE_NUMBER' | translate }}</th>
                  <!-- <th>{{ 'DEALER.VEHICLES.TABLE.TYPE' | translate }}</th> -->
                  <!-- <th>{{ 'DEALER.VEHICLES.TABLE.CAPACITY' | translate }}</th> -->
                  <!-- <th>{{ 'DEALER.VEHICLES.TABLE.REGISTRATION_EXPIRY' | translate }}</th> -->
                  <!-- <th>{{ 'DEALER.VEHICLES.TABLE.INSURANCE_EXPIRY' | translate }}</th> -->
                  <th>{{ 'DEALER.VEHICLES.TABLE.STATUS' | translate }}</th>
                  <th width="150">{{ 'DEALER.VEHICLES.TABLE.ACTIONS' | translate }}</th>
                </tr>
              </thead>
            <tbody>
              <tr *ngFor="let vehicle of paginatedVehicles" class="vehicle-row">
                <td class="plate-number">{{ vehicle.plateNumber }}</td>
                <!-- <td>{{ 'DEALER.VEHICLES.TYPES.' + vehicle.type.toUpperCase() | translate }}</td> -->
                <!-- <td>{{ vehicle.capacity }} {{ 'DEALER.PRODUCTS.MEASUREMENT.TONS' | translate }}</td> -->
                <!-- <td>{{ vehicle.registrationExpiryDate | date:'mediumDate' }}</td> -->
                <!-- <td>{{ vehicle.insuranceExpiryDate | date:'mediumDate' }}</td> -->
                <td>
                  <span class="status-badge" [class.active]="vehicle.isActive" [class.inactive]="!vehicle.isActive">
                    {{ vehicle.isActive ? ('DEALER.VEHICLES.STATUS.ACTIVE' | translate) : ('DEALER.VEHICLES.STATUS.INACTIVE' | translate) }}
                  </span>
                </td>
                <td class="actions-cell">
                  <button 
                    class="btn btn-sm btn-outline-primary"
                    [routerLink]="['/dealer/vehicles/edit', vehicle.vehicleID]"
                    [title]="'DEALER.VEHICLES.ACTIONS.EDIT' | translate">
                    <i class="material-icons">edit</i>
                  </button>
                  <button 
                    class="btn btn-sm"
                    [class]="vehicle.isActive ? 'btn-outline-warning' : 'btn-outline-success'"
                    (click)="toggleVehicleStatus(vehicle)"
                    [title]="vehicle.isActive ? 'Deactivate Vehicle' : 'Activate Vehicle'">
                    <i class="material-icons">{{ vehicle.isActive ? 'pause' : 'play_arrow' }}</i>
                  </button>
                  <button 
                    class="btn btn-sm btn-outline-danger"
                    (click)="confirmDelete(vehicle)"
                    title="Delete Vehicle">
                    <i class="material-icons">delete</i>
                  </button>
                </td>
              </tr>
            </tbody>
          </table>
          </div>
        </div>

        <!-- Pagination -->
        <app-pagination 
          *ngIf="!loading && totalPages > 1"
          [currentPage]="currentPage"
          [totalPages]="totalPages"
          [hasPreviousPage]="currentPage > 1"
          [hasNextPage]="currentPage < totalPages"
          [pageSize]="pageSize"
          [totalCount]="totalCount"
          (pageChange)="goToPage($event)">
        </app-pagination>
      </div>
    </div>

    <!-- Delete Confirmation Modal -->
    <div class="modal-overlay" *ngIf="showDeleteModal" (click)="cancelDelete()">
      <div class="modal-content" (click)="$event.stopPropagation()">
        <div class="modal-header">
          <h3>Confirm Delete</h3>
          <button class="close-btn" (click)="cancelDelete()">×</button>
        </div>
        <div class="modal-body">
          <p>Are you sure you want to delete vehicle <strong>{{ vehicleToDelete?.plateNumber }}</strong>?</p>
          <p class="warning-text">This action cannot be undone.</p>
        </div>
        <div class="modal-actions">
          <button class="btn btn-secondary" (click)="cancelDelete()">Cancel</button>
          <button class="btn btn-danger" (click)="deleteVehicle()" [disabled]="deleting">
            <span *ngIf="deleting" class="spinner-sm"></span>
            Delete
          </button>
        </div>
      </div>
    </div>
  `,
  styleUrls: ['./vehicles-list.component.scss']
})
export class VehiclesListComponent implements OnInit {
  vehicles: Vehicle[] = [];
  filteredVehicles: Vehicle[] = [];
  paginatedVehicles: Vehicle[] = [];
  loading = true;
  searchTerm = '';
  typeFilter = '';
  statusFilter = '';
  showDeleteModal = false;
  vehicleToDelete: Vehicle | null = null;
  deleting = false;

  // Pagination state
  currentPage: number = 1;
  pageSize: number = 6; // 6 cards per page
  pageSizeOptions: (number | string)[] = [6, 12, 18, 24, 'All'];
  totalCount: number = 0;
  totalPages: number = 0;

  constructor(private vehicleService: VehicleService) {}

  ngOnInit(): void {
    this.loadVehicles();
  }

  loadVehicles(): void {
    this.loading = true;
    this.vehicleService.getVehicles().subscribe({
      next: (vehicles) => {
        this.vehicles = vehicles;
        this.filteredVehicles = vehicles;
        this.applyPagination();
        this.loading = false;
      },
      error: (error) => {
        this.loading = false;
      }
    });
  }

  filterVehicles(): void {
    let filtered = this.vehicles;

    // Filter by search term
    if (this.searchTerm.trim()) {
      const term = this.searchTerm.toLowerCase();
      filtered = filtered.filter(vehicle =>
        vehicle.plateNumber.toLowerCase().includes(term) ||
        vehicle.type.toLowerCase().includes(term)
      );
    }

    // Filter by type
    if (this.typeFilter) {
      filtered = filtered.filter(vehicle => vehicle.type === this.typeFilter);
    }

    // Filter by status
    if (this.statusFilter) {
      filtered = filtered.filter(vehicle =>
        this.statusFilter === 'active' ? vehicle.isActive : !vehicle.isActive
      );
    }

    this.filteredVehicles = filtered;
    this.applyPagination();
  }

  applyPagination(): void {
    this.totalCount = this.filteredVehicles.length;
    this.totalPages = this.pageSize === 0 ? 1 : Math.ceil(this.totalCount / this.pageSize);
    
    if (this.pageSize === 0) {
      this.paginatedVehicles = this.filteredVehicles;
    } else {
      const start = (this.currentPage - 1) * this.pageSize;
      this.paginatedVehicles = this.filteredVehicles.slice(start, start + this.pageSize);
    }
  }

  onPageSizeChange(size: number | string): void {
    this.pageSize = size === 'All' ? 0 : +size;
    this.currentPage = 1;
    this.applyPagination();
  }

  goToPage(page: number): void {
    this.currentPage = page;
    this.applyPagination();
  }

  toggleVehicleStatus(vehicle: Vehicle): void {
    const action = vehicle.isActive ? 
      this.vehicleService.deactivateVehicle(vehicle.vehicleID) : 
      this.vehicleService.activateVehicle(vehicle.vehicleID);

    action.subscribe({
      next: (success) => {
        if (success) {
          // Update the vehicle status locally
          const index = this.vehicles.findIndex(v => v.vehicleID === vehicle.vehicleID);
          if (index !== -1) {
            this.vehicles[index].isActive = !this.vehicles[index].isActive;
            this.filterVehicles();
          }
        }
      },
      error: (error) => {
      }
    });
  }

  confirmDelete(vehicle: Vehicle): void {
    this.vehicleToDelete = vehicle;
    this.showDeleteModal = true;
  }

  cancelDelete(): void {
    this.showDeleteModal = false;
    this.vehicleToDelete = null;
    this.deleting = false;
  }

  deleteVehicle(): void {
    if (!this.vehicleToDelete) return;

    this.deleting = true;
    this.vehicleService.deleteVehicle(this.vehicleToDelete.vehicleID).subscribe({
      next: (success) => {
        if (success) {
          this.vehicles = this.vehicles.filter(v => v.vehicleID !== this.vehicleToDelete!.vehicleID);
          this.filterVehicles();
          this.cancelDelete();
        }
      },
      error: (error) => {
        this.deleting = false;
      }
    });
  }
}