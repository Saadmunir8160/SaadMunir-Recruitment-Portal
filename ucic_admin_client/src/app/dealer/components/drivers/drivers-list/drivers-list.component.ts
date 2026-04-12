import { Component, OnInit, CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';
import { Driver, DriverService } from '../../../services/driver.service';
import { PaginationComponent } from '../../../../shared/components/pagination/pagination.component';

@Component({
  selector: 'app-drivers-list',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule, TranslateModule, PaginationComponent],
  schemas: [CUSTOM_ELEMENTS_SCHEMA],
  template: `
    <div>
      <div class="page-header">
        <div class="header-content">
          <h1 class="page-title">
            <iconify-icon icon="solar:user-bold-duotone"></iconify-icon>
            {{ 'DEALER.DRIVERS.TITLE' | translate }}
          </h1>
          <p class="page-subtitle">{{ 'DEALER.DRIVERS.SUBTITLE' | translate }}</p>
        </div>
        <div class="header-actions">
          <button class="btn btn-success fs-14" routerLink="/dealer/drivers/create">
            <iconify-icon icon="solar:add-circle-bold-duotone" width="20" height="20"></iconify-icon>
            {{ 'DEALER.DRIVERS.ADD_DRIVER' | translate }}
          </button>
        </div>
      </div>

      <div class="filter-section" *ngIf="drivers.length > 0">
        <div class="filter-controls">
          <div class="search-box">
            <i class="material-icons search-icon">search</i>
            <input 
              type="text"
              class="search-input"
              [placeholder]="'DEALER.DRIVERS.SEARCH.PLACEHOLDER' | translate" 
              [(ngModel)]="searchTerm"
              (input)="filterDrivers()">
          </div>
          
          <select class="status-filter" [(ngModel)]="statusFilter" (change)="filterDrivers()">
            <option value="">{{ 'DEALER.DRIVERS.FILTERS.ALL_STATUS' | translate }}</option>
            <option value="active">{{ 'DEALER.DRIVERS.FILTERS.ACTIVE' | translate }}</option>
            <option value="inactive">{{ 'DEALER.DRIVERS.FILTERS.INACTIVE' | translate }}</option>
          </select>
          
          <div class="page-size-control">
            <label for="pageSize">{{ 'DEALER.DRIVERS.PAGINATION.SHOW' | translate }}:</label>
            <select id="pageSize" class="page-size-select" [(ngModel)]="pageSize" (ngModelChange)="onPageSizeChange($event)">
              <option *ngFor="let size of pageSizeOptions" [ngValue]="size === 'All' ? 0 : size">{{ size === 'All' ? ('DEALER.DRIVERS.PAGINATION.ALL' | translate) : size }}</option>
            </select>
          </div>
        </div>
      </div>

      <div class="content-section">
        <div *ngIf="loading" class="loading-state">
          <div class="spinner"></div>
          <p>{{ 'DEALER.DRIVERS.LOADING.MESSAGE' | translate }}</p>
        </div>

        <div *ngIf="!loading && paginatedDrivers.length === 0 && drivers.length === 0" class="empty-state">
          <i class="material-icons">person_add</i>
          <h3>{{ 'DEALER.DRIVERS.EMPTY_STATE.NO_DRIVERS_TITLE' | translate }}</h3>
          <p>{{ 'DEALER.DRIVERS.EMPTY_STATE.NO_DRIVERS_MESSAGE' | translate }}</p>
          <button class="btn btn-primary" routerLink="/dealer/drivers/create">
            {{ 'DEALER.DRIVERS.EMPTY_STATE.ADD_FIRST_DRIVER' | translate }}
          </button>
        </div>

        <div *ngIf="!loading && paginatedDrivers.length === 0 && drivers.length > 0" class="empty-state">
          <i class="material-icons">search_off</i>
          <h3>{{ 'DEALER.DRIVERS.EMPTY_STATE.NO_MATCH_TITLE' | translate }}</h3>
          <p>{{ 'DEALER.DRIVERS.EMPTY_STATE.NO_MATCH_MESSAGE' | translate }}</p>
        </div>

        <div *ngIf="!loading && paginatedDrivers.length > 0" class="drivers-table-container">
          <div class="table-responsive">
            <table class="table table-striped table-hover">
              <thead class="table-dark">
                <tr>
                  <th>{{ 'DEALER.DRIVERS.TABLE.NAME' | translate }}</th>
                  <!-- <th>{{ 'DEALER.DRIVERS.TABLE.USERNAME' | translate }}</th> -->
                  <!-- <th>{{ 'DEALER.DRIVERS.TABLE.PHONE' | translate }}</th> -->
                  <!-- <th>{{ 'DEALER.DRIVERS.TABLE.EMAIL' | translate }}</th> -->
                  <!-- <th>{{ 'DEALER.DRIVERS.TABLE.LICENSE_ID' | translate }}</th> -->
                  <th>{{ 'DEALER.DRIVERS.TABLE.IQAMA_NUMBER' | translate }}</th>
                  <th>{{ 'DEALER.DRIVERS.TABLE.STATUS' | translate }}</th>
                  <th>{{ 'DEALER.DRIVERS.TABLE.ACTIONS' | translate }}</th>
                </tr>
              </thead>
            <tbody>
              <tr *ngFor="let driver of paginatedDrivers" class="driver-row">
                <td class="driver-name">{{ driver.fullName || driver.name }}</td>
                <!-- <td class="username">{{ '@' + driver.userName }}</td> -->
                <!-- <td>{{ driver.phoneNumber || '-' }}</td> -->
                <!-- <td>{{ driver.email || '-' }}</td> -->
                <!-- <td>{{ driver.ln_ID || '-' }}</td> -->
                <td>{{ driver.iqamaNumber || '-' }}</td>
                <td>
                  <span class="status-badge" [class.active]="driver.isActive" [class.inactive]="!driver.isActive">
                    {{ driver.isActive ? ('DEALER.DRIVERS.STATUS.ACTIVE' | translate) : ('DEALER.DRIVERS.STATUS.INACTIVE' | translate) }}
                  </span>
                </td>
                <td class="actions-cell">
                  <button 
                    class="btn btn-sm btn-outline-primary"
                    [routerLink]="['/dealer/drivers/edit', driver.id]"
                    [title]="'DEALER.DRIVERS.ACTIONS.EDIT' | translate">
                    <i class="material-icons">edit</i>
                  </button>
                  <button 
                    class="btn btn-sm"
                    [class]="driver.isActive ? 'btn-outline-warning' : 'btn-outline-success'"
                    (click)="toggleDriverStatus(driver)"
                    [title]="driver.isActive ? ('DEALER.DRIVERS.ACTIONS.DEACTIVATE' | translate) : ('DEALER.DRIVERS.ACTIONS.ACTIVATE' | translate)">
                    <i class="material-icons">{{ driver.isActive ? 'pause' : 'play_arrow' }}</i>
                  </button>
                  <button 
                    class="btn btn-sm btn-outline-danger"
                    (click)="confirmDelete(driver)"
                    [title]="'DEALER.DRIVERS.ACTIONS.DELETE' | translate">
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
          <h3>{{ 'DEALER.DRIVERS.DELETE_MODAL.TITLE' | translate }}</h3>
          <button class="close-btn" (click)="cancelDelete()">×</button>
        </div>
        <div class="modal-body">
          <p>{{ 'DEALER.DRIVERS.DELETE_MODAL.MESSAGE' | translate }} <strong>{{ driverToDelete?.name }}</strong>?</p>
          <p class="warning-text">{{ 'DEALER.DRIVERS.DELETE_MODAL.WARNING' | translate }}</p>
        </div>
        <div class="modal-actions">
          <button class="btn btn-secondary" (click)="cancelDelete()">{{ 'DEALER.DRIVERS.DELETE_MODAL.CANCEL' | translate }}</button>
          <button class="btn btn-danger" (click)="deleteDriver()" [disabled]="deleting">
            <span *ngIf="deleting" class="spinner-sm"></span>
            {{ 'DEALER.DRIVERS.DELETE_MODAL.DELETE' | translate }}
          </button>
        </div>
      </div>
    </div>
  `,
  styleUrls: ['./drivers-list.component.scss']
})
export class DriversListComponent implements OnInit {
  drivers: Driver[] = [];
  filteredDrivers: Driver[] = [];
  paginatedDrivers: Driver[] = [];
  loading = true;
  searchTerm = '';
  statusFilter = '';
  showDeleteModal = false;
  driverToDelete: Driver | null = null;
  deleting = false;

  // Pagination state
  currentPage: number = 1;
  pageSize: number = 6; // 6 cards per page
  pageSizeOptions: (number | string)[] = [6, 12, 18, 24, 'All'];
  totalCount: number = 0;
  totalPages: number = 0;

  constructor(private driverService: DriverService) {}

  ngOnInit(): void {
    this.loadDrivers();
  }

  loadDrivers(): void {
    this.loading = true;
    this.driverService.getDrivers().subscribe({
      next: (drivers) => {
        this.drivers = drivers;
        this.filteredDrivers = drivers;
        this.applyPagination();
        this.loading = false;
      },
      error: (error) => {
        this.loading = false;
      }
    });
  }

  filterDrivers(): void {
    let filtered = this.drivers;

    // Filter by search term
    if (this.searchTerm.trim()) {
      const term = this.searchTerm.toLowerCase();
      filtered = filtered.filter(driver =>
        (driver.name?.toLowerCase().includes(term) ||
        driver.fullName.toLowerCase().includes(term) ||
        driver.userName.toLowerCase().includes(term) ||
        (driver.licenseNumber && driver.licenseNumber.toLowerCase().includes(term)) ||
        (driver.phoneNumber && driver.phoneNumber.includes(term)) ||
        (driver.email && driver.email.toLowerCase().includes(term)) ||
        (driver.iqamaNumber && driver.iqamaNumber.toLowerCase().includes(term)))
      );
    }

    // Filter by status
    if (this.statusFilter) {
      filtered = filtered.filter(driver =>
        this.statusFilter === 'active' ? driver.isActive : !driver.isActive
      );
    }

    this.filteredDrivers = filtered;
    this.applyPagination();
  }

  applyPagination(): void {
    this.totalCount = this.filteredDrivers.length;
    this.totalPages = this.pageSize === 0 ? 1 : Math.ceil(this.totalCount / this.pageSize);
    
    if (this.pageSize === 0) {
      this.paginatedDrivers = this.filteredDrivers;
    } else {
      const start = (this.currentPage - 1) * this.pageSize;
      this.paginatedDrivers = this.filteredDrivers.slice(start, start + this.pageSize);
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

  toggleDriverStatus(driver: Driver): void {
    const action = driver.isActive ? 
      this.driverService.deactivateDriver(driver.driverID) :
      this.driverService.activateDriver(driver.driverID);    action.subscribe({
      next: (updatedDriver) => {
        const index = this.drivers.findIndex(d => d.id === driver.id);
        if (index !== -1) {
          this.drivers[index] = {...this.drivers[index], isActive: !driver.isActive};
          this.filterDrivers();
        }
      },
      error: (error) => {
      }
    });
  }

  confirmDelete(driver: Driver): void {
    this.driverToDelete = driver;
    this.showDeleteModal = true;
  }

  cancelDelete(): void {
    this.showDeleteModal = false;
    this.driverToDelete = null;
    this.deleting = false;
  }

  deleteDriver(): void {
    if (!this.driverToDelete) return;

    this.deleting = true;
    this.driverService.deleteDriver(this.driverToDelete.driverID).subscribe({
      next: () => {
        this.drivers = this.drivers.filter(d => d.id !== this.driverToDelete!.id);
        this.filterDrivers();
        this.cancelDelete();
      },
      error: (error) => {
        this.deleting = false;
      }
    });
  }
}