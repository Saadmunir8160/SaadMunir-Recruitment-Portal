import { Component, OnInit, OnDestroy, CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router, ActivatedRoute } from '@angular/router';
import { DealerManagementService, DealerProduct, DealerOrder, DealerDriver, DealerVehicle, Dealer } from './services/dealer-management.service';
import { PaginatedResponse } from '../shared/interfaces/paginated-response.interface';
import { DealerSupportTicketService } from './services/dealer-support-ticket.service';

@Component({
  selector: 'app-dealer-management',
  standalone: true,
  imports: [CommonModule, RouterModule],
  schemas: [CUSTOM_ELEMENTS_SCHEMA],
  template: `
    <div class="dealer-management-container">
      <!-- Main Header -->
      <div class="dealer-management-header">
        <div class="header-content">
          <div class="breadcrumb-section">
            <nav class="breadcrumb">
              <a routerLink="/admin/dashboard">Dashboard</a>
              <span>/</span>
              <span>Dealer Management</span>
            </nav>
          </div>
          
          <div class="page-title-section">
            <h1 class="page-title">
              <i class="material-icons">business_center</i>
              Customer Management Portal
            </h1>
            <p class="page-subtitle">Monitor and manage all customer operations across the platform</p>
          </div>
        </div>
        
        <!-- Global Actions -->
        <div class="header-actions">
          <button class="btn btn-primary" (click)="refreshData()">
            <i class="material-icons">refresh</i>
            Refresh
          </button>
        </div>
      </div>

      <!-- Navigation Buttons Header -->
      <div class="dealer-nav-header">
        <div class="nav-buttons">
          <button 
            class="nav-button"
            [class.active]="activeTab === 'overview'"
            (click)="navigateToTab('overview')">
            <iconify-icon icon="solar:widget-5-bold-duotone"></iconify-icon>
            <span>Overview</span>
          </button>
          
          <button 
            class="nav-button"
            [class.active]="activeTab === 'dealers'"
            (click)="navigateToTab('dealers')">
            <iconify-icon icon="solar:buildings-2-bold-duotone"></iconify-icon>
            <span>Customer</span>
            <span class="count-badge" *ngIf="totalDealers">{{ totalDealers }}</span>
          </button>
          
          <button 
            class="nav-button"
            [class.active]="activeTab === 'products'"
            (click)="navigateToTab('products')">
            <iconify-icon icon="solar:box-bold-duotone"></iconify-icon>
            <span>Products</span>
            <span class="count-badge" *ngIf="totalProducts">{{ totalProducts }}</span>
          </button>
          
          <button 
            class="nav-button"
            [class.active]="activeTab === 'orders'"
            (click)="navigateToTab('orders')">
            <iconify-icon icon="solar:bag-4-bold-duotone"></iconify-icon>
            <span>Orders</span>
            <span class="count-badge" *ngIf="totalOrders">{{ totalOrders }}</span>
          </button>
          
          <button 
            class="nav-button"
            [class.active]="activeTab === 'drivers'"
            (click)="navigateToTab('drivers')">
            <iconify-icon icon="solar:user-id-bold-duotone"></iconify-icon>
            <span>Drivers</span>
            <span class="count-badge" *ngIf="totalDrivers">{{ totalDrivers }}</span>
          </button>
          
          <button 
            class="nav-button"
            [class.active]="activeTab === 'vehicles'"
            (click)="navigateToTab('vehicles')">
            <iconify-icon icon="solar:delivery-bold-duotone"></iconify-icon>
            <span>Vehicles</span>
            <span class="count-badge" *ngIf="totalVehicles">{{ totalVehicles }}</span>
          </button>
          
          <button 
            class="nav-button"
            [class.active]="activeTab === 'areas'"
            (click)="navigateToTab('areas')">
            <iconify-icon icon="solar:map-bold-duotone"></iconify-icon>
            <span>Areas</span>
            <span class="count-badge" *ngIf="totalAreas">{{ totalAreas }}</span>
          </button>
          
          <button 
            class="nav-button"
            [class.active]="activeTab === 'daily-limits'"
            (click)="navigateToTab('daily-limits')">
            <iconify-icon icon="solar:settings-bold-duotone"></iconify-icon>
            <span>Daily Limits</span>
            <span class="count-badge" *ngIf="totalDailyLimits">{{ totalDailyLimits }}</span>
          </button>
          
          <button 
            class="nav-button"
            [class.active]="activeTab === 'support-tickets'"
            (click)="navigateToTab('support-tickets')">
            <iconify-icon icon="solar:headphones-round-sound-bold-duotone"></iconify-icon>
            <span>Support Tickets</span>
            <span class="count-badge" *ngIf="totalSupportTickets">{{ totalSupportTickets }}</span>
          </button>
        </div>
      </div>

      <!-- Content Area -->
      <div class="dealer-content">
        <router-outlet></router-outlet>
      </div>
    </div>
  `,
  styleUrls: ['./dealer-management.component.scss']
})
export class DealerManagementComponent implements OnInit, OnDestroy {
  activeTab: string = 'overview';
  totalDealers: number = 0;
  totalProducts: number = 0;
  totalOrders: number = 0;
  totalDrivers: number = 0;
  totalVehicles: number = 0;
  totalAreas: number = 0;
  totalAddresses: number = 0;
  totalDailyLimits: number = 0;
  totalSupportTickets: number = 0;

  constructor(
    private router: Router,
    private route: ActivatedRoute,
    private dealerService: DealerManagementService,
    private dealerSupportTicketService: DealerSupportTicketService
  ) { }

  ngOnInit(): void {
    // Set initial active tab based on current route
    const currentRoute = this.router.url.split('/').pop();
    this.activeTab = currentRoute || 'overview';

    // Load counts
    this.loadCounts();

    // Listen for refresh events from child components
    window.addEventListener('refreshDealerCounts', () => {
      this.loadCounts();
    });
  }

  navigateToTab(tab: string): void {
    this.activeTab = tab;
    this.router.navigate([tab], { relativeTo: this.route });
  }

  loadCounts(): void {
    // Load dealers count
    this.dealerService.getAllDealers(1, 1).subscribe((response) => {
      this.totalDealers = response.metadata?.totalCount || 0;
    });

    // Load product count
    this.dealerService.getDealerProducts(1, 1).subscribe((response: PaginatedResponse<DealerProduct>) => {
      this.totalProducts = response.metadata.totalCount;
    });

    // Load order count
    this.dealerService.getDealerOrders(1, 1).subscribe((response: PaginatedResponse<DealerOrder>) => {
      this.totalOrders = response.metadata.totalCount;
    });

    // Load driver count
    this.dealerService.getDealerDrivers(1, 1).subscribe((response: PaginatedResponse<DealerDriver>) => {
      this.totalDrivers = response.metadata.totalCount;
    });

    // Load vehicle count
    this.dealerService.getDealerVehicles(1, 1).subscribe((response: PaginatedResponse<DealerVehicle>) => {
      this.totalVehicles = response.metadata.totalCount;
    });

    // Load areas count
    this.dealerService.getDealerAreas(1, 1).subscribe((response: any) => {
      this.totalAreas = response.metadata?.totalCount || 0;
    });

    // Load addresses count
    this.dealerService.getDealerAddresses({ pageNumber: 1, pageSize: 1 }).subscribe((response: any) => {
      this.totalAddresses = response.data?.metadata?.totalCount || 0;
    });

    // Load daily limits count
    this.dealerService.getDealerDailyLimits({ pageNumber: 1, pageSize: 1 }).subscribe((response: any) => {
      this.totalDailyLimits = response.data?.metadata?.totalCount || 0;
    });

    // Load support tickets count
    this.dealerSupportTicketService.getAllTickets({ page: 1, pageSize: 1 }).subscribe((response: any) => {
      if (response && response.success && response.data?.metadata) {
        this.totalSupportTickets = response.data.metadata.totalCount || 0;
      } else {
        this.totalSupportTickets = 0;
      }
    });
  }

  exportAllData(): void {
    // Implement export functionality
    console.log('Exporting all dealer data...');
  }

  refreshData(): void {
    this.loadCounts();
    // Trigger refresh for active component
    window.location.reload();
  }

  ngOnDestroy(): void {
    // Clean up event listener
    window.removeEventListener('refreshDealerCounts', () => {
      this.loadCounts();
    });
  }
}