import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { DealerManagementService, DealerProduct, DealerOrder, DealerDriver, DealerVehicle } from '../../services/dealer-management.service';
import { PaginatedResponse } from '../../../shared/interfaces/paginated-response.interface';

@Component({
  selector: 'app-dealer-overview',
  standalone: true,
  imports: [CommonModule, RouterModule],
  template: `
    <div class="overview-container">
      <!-- <div class="overview-header">
        <div class="header-content">
          <h2 class="section-title">
            <i class="material-icons">dashboard</i>
            Customer Management Overview
          </h2>
          <p class="section-subtitle">Quick statistics and insights across all dealer operations</p>
        </div>
      </div> -->

      <!-- Statistics Grid -->
      <div class="stats-grid">
        <div class="stat-card">
          <div class="stat-icon">
            <i class="material-icons">inventory</i>
          </div>
          <div class="stat-content">
            <h3>{{ totalProducts }}</h3>
            <p>Total Products</p>
            <span class="stat-change positive">+12% this month</span>
          </div>
          <button class="view-btn" routerLink="../products">View All</button>
        </div>

        <div class="stat-card">
          <div class="stat-icon">
            <i class="material-icons">shopping_cart</i>
          </div>
          <div class="stat-content">
            <h3>{{ totalOrders }}</h3>
            <p>Total Orders</p>
            <span class="stat-change positive">+8% this month</span>
          </div>
          <button class="view-btn" routerLink="../orders">View All</button>
        </div>

        <div class="stat-card">
          <div class="stat-icon">
            <i class="material-icons">local_shipping</i>
          </div>
          <div class="stat-content">
            <h3>{{ totalDrivers }}</h3>
            <p>Active Drivers</p>
            <span class="stat-change positive">+5% this month</span>
          </div>
          <button class="view-btn" routerLink="../drivers">View All</button>
        </div>

        <div class="stat-card">
          <div class="stat-icon">
            <i class="material-icons">directions_car</i>
          </div>
          <div class="stat-content">
            <h3>{{ totalVehicles }}</h3>
            <p>Fleet Vehicles</p>
            <span class="stat-change neutral">No change</span>
          </div>
          <button class="view-btn" routerLink="../vehicles">View All</button>
        </div>
      </div>

      <!-- Quick Actions -->
      <div class="quick-actions">
        <h3>Quick Actions</h3>
        <div class="actions-grid">
          <button class="action-card" routerLink="../products">
            <i class="material-icons">add_circle</i>
            <span>Manage Products</span>
          </button>
          <button class="action-card" routerLink="../orders">
            <i class="material-icons">assignment</i>
            <span>View Orders</span>
          </button>
          <button class="action-card" routerLink="../drivers">
            <i class="material-icons">person</i>
            <span>Manage Drivers</span>
          </button>
          <button class="action-card" routerLink="../vehicles">
            <i class="material-icons">commute</i>
            <span>Fleet Management</span>
          </button>
        </div>
      </div>

      <!-- Recent Activity Placeholder -->
      <div class="recent-activity">
        <h3>Recent Activity</h3>
        <div class="activity-list">
          <div class="activity-item">
            <div class="activity-icon">
              <i class="material-icons">shopping_cart</i>
            </div>
            <div class="activity-content">
              <p>New order placed by Dealer ABC</p>
              <span class="activity-time">2 hours ago</span>
            </div>
          </div>
          <div class="activity-item">
            <div class="activity-icon">
              <i class="material-icons">inventory</i>
            </div>
            <div class="activity-content">
              <p>Product inventory updated</p>
              <span class="activity-time">4 hours ago</span>
            </div>
          </div>
          <div class="activity-item">
            <div class="activity-icon">
              <i class="material-icons">local_shipping</i>
            </div>
            <div class="activity-content">
              <p>New driver registered</p>
              <span class="activity-time">1 day ago</span>
            </div>
          </div>
        </div>
      </div>
    </div>
  `,
  styleUrls: ['./dealer-overview.component.scss']
})
export class DealerOverviewComponent implements OnInit {
  totalProducts: number = 0;
  totalOrders: number = 0;
  totalDrivers: number = 0;
  totalVehicles: number = 0;
  loading: boolean = true;

  constructor(private dealerService: DealerManagementService) {}

  ngOnInit(): void {
    this.loadStatistics();
  }

  loadStatistics(): void {
    this.loading = true;
    
    // Load all counts
    this.dealerService.getDealerProducts(1, 1).subscribe((response: PaginatedResponse<DealerProduct>) => {
      this.totalProducts = response.metadata.totalCount;
    });

    this.dealerService.getDealerOrders(1, 1).subscribe((response: PaginatedResponse<DealerOrder>) => {
      this.totalOrders = response.metadata.totalCount;
    });

    this.dealerService.getDealerDrivers(1, 1).subscribe((response: PaginatedResponse<DealerDriver>) => {
      this.totalDrivers = response.metadata.totalCount;
    });

    this.dealerService.getDealerVehicles(1, 1).subscribe((response: PaginatedResponse<DealerVehicle>) => {
      this.totalVehicles = response.metadata.totalCount;
      this.loading = false;
    });
  }
}