import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule, Router } from '@angular/router';
import { DealerManagementService, DealerProduct, Dealer } from '../../services/dealer-management.service';
import { PaginatedResponse } from '../../../shared/interfaces/paginated-response.interface';

@Component({
  selector: 'app-dealer-products',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  template: `
    <div class="dealer-products-container">
      <!-- Header Section -->
      <div class="products-header">
        <!-- <div class="header-content">
          <h2 class="section-title">
            <i class="material-icons">inventory</i>
            Dealer Products Management
          </h2>
          <p class="section-subtitle">View and manage all products across customer network</p>
        </div> -->
        <div class="header-actions">
          <button class="btn btn-primary" (click)="createProduct()">
            <i class="material-icons">add</i>
            Create Product
          </button>
          <button class="btn btn-outline-secondary me-2" (click)="refreshData()">
            <i class="material-icons">refresh</i>
            Refresh
          </button>
        </div>
      </div>

      <!-- Filters Section -->
      <div class="filters-section">
        <div class="row g-3">
          <div class="col-md-3">
            <label class="form-label fw-semibold">Search</label>
            <input 
              type="text" 
              [(ngModel)]="searchTerm" 
              (input)="onSearch()"
              placeholder="Search by product name..."
              class="form-control">
          </div>
          
          <div class="col-md-2">
            <label class="form-label fw-semibold">Status</label>
            <select [(ngModel)]="statusFilter" (ngModelChange)="applyFilters()" class="form-select">
              <option value="">All Status</option>
              <option value="true">Active</option>
              <option value="false">Inactive</option>
            </select>
          </div>
          <div class="col-md-2">
            <label class="form-label fw-semibold">Items per Page</label>
            <select [(ngModel)]="pageSize" (ngModelChange)="changePageSize()" class="form-select">
              <option value="10">10</option>
              <option value="25">25</option>
              <option value="50">50</option>
              <option value="100">100</option>
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


      <!-- Loading State -->
      <div *ngIf="loading" class="loading-container">
        <div class="loading-spinner">
          <i class="material-icons spinning">refresh</i>
          <p>Loading products...</p>
        </div>
      </div>

      <!-- Data Table -->
      <div *ngIf="!loading" class="table-container">
        <div class="card">
          <div class="card-body">
            <table class="table data-table table-hover mb-0 border">
              <thead class="table-header">
                <tr>
              <th>Product Name</th>
              <th>LN Product Code</th>
              <th>Unit</th>
              <th>Status</th>
              <th>Last Modified</th>
              <th>Actions</th>
            </tr>
          </thead>
          <tbody>
            <tr *ngFor="let product of products">
              <td>
                <div class="product-info">
                  <strong>{{ product.productName }}</strong>
                  <small *ngIf="product.productDescription">{{ product.productDescription }}</small>
                </div>
              </td>
              <td>
                <span class="ln-code">{{ product.product_LnCode || 'N/A' }}</span>
              </td>
              
              <td>{{ product.unitOfMeasure || 'Unit' }}</td>
              <td>
                <span class="status-badge" [class]="getStatusClass(product.isActive)">
                  {{ product.isActive ? 'Active' : 'Inactive' }}
                </span>
              </td>
              <td>
                <span class="date-text">{{ product.modifiedDate | date:'short' }}</span>
              </td>
              <td>
                <div class="action-buttons">
                  <button class="btn btn-sm btn-outline-info" 
                          (click)="viewProduct(product)"
                          title="View Details">
                    <i class="material-icons">visibility</i>
                  </button>
                  <button class="btn btn-sm btn-outline-primary" 
                          (click)="editProduct(product)"
                          title="Edit Product">
                    <i class="material-icons">edit</i>
                  </button>
                  <button class="btn btn-sm btn-outline-danger" 
                          (click)="deleteProduct(product)"
                          title="Delete Product">
                    <i class="material-icons">delete</i>
                  </button>
                </div>
              </td>
            </tr>
            <tr *ngIf="products.length === 0">
              <td colspan="8" class="no-data">
                <div class="no-data-message">
                  <i class="material-icons">inventory_2</i>
                  <p>No products found</p>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
        </div>
      </div>
    </div>

      <!-- Pagination -->
      <div *ngIf="!loading && totalRecords > 0" class="pagination-container">
        <div class="pagination-info">
          Showing {{ (currentPage - 1) * pageSize + 1 }} to {{ Math.min(currentPage * pageSize, totalRecords) }} of {{ totalRecords }} results
        </div>
        <div class="pagination-controls">
          <button 
            class="btn-page" 
            [disabled]="currentPage === 1"
            (click)="goToPage(currentPage - 1)">
            <i class="material-icons">chevron_left</i>
          </button>
          
          <span class="page-info">{{ currentPage }} of {{ totalPages }}</span>
          
          <button 
            class="btn-page" 
            [disabled]="currentPage === totalPages"
            (click)="goToPage(currentPage + 1)">
            <i class="material-icons">chevron_right</i>
          </button>
        </div>
      </div>
    </div>

    <!-- View Product Modal -->
    <div class="modal fade" 
         [class.show]="showViewModal" 
         [style.display]="showViewModal ? 'block' : 'none'"
         tabindex="-1">
      <div class="modal-dialog modal-lg">
        <div class="modal-content">
          <div class="modal-header">
            <h5 class="modal-title">
              <i class="material-icons">visibility</i>
              View Product Details
            </h5>
            <button type="button" class="btn-close" (click)="closeViewModal()"></button>
          </div>
          
          <div class="modal-body" *ngIf="viewingProduct">
            <div class="row mb-3">
              <div class="col-md-6">
                <div class="detail-group">
                  <label class="detail-label">Product ID</label>
                  <p class="detail-value">{{ viewingProduct.dealerProductID }}</p>
                </div>
              </div>
              <div class="col-md-6">
                <div class="detail-group">
                  <label class="detail-label">Status</label>
                  <p class="detail-value">
                    <span class="badge" [class]="viewingProduct.isActive ? 'badge-success' : 'badge-danger'">
                      {{ viewingProduct.isActive ? 'Active' : 'Inactive' }}
                    </span>
                  </p>
                </div>
              </div>
            </div>

            <div class="row mb-3">
              <div class="col-md-12">
                <div class="detail-group">
                  <label class="detail-label">Product Name</label>
                  <p class="detail-value">{{ viewingProduct.productName }}</p>
                </div>
              </div>
            </div>

            <div class="row mb-3">
              <div class="col-md-6">
                <div class="detail-group">
                  <label class="detail-label">LN Product Code</label>
                  <p class="detail-value">{{ viewingProduct.product_LnCode || 'N/A' }}</p>
                </div>
              </div>
              <div class="col-md-6">
                <div class="detail-group">
                  <label class="detail-label">Dealer</label>
                  <p class="detail-value">{{ viewingProduct.dealerName || 'N/A' }}</p>
                </div>
              </div>
            </div>

            <div class="row mb-3">
              <div class="col-md-12">
                <div class="detail-group">
                  <label class="detail-label">Description</label>
                  <p class="detail-value">{{ viewingProduct.productDescription || 'N/A' }}</p>
                </div>
              </div>
            </div>

            <div class="row mb-3">
              <div class="col-md-4">
                <div class="detail-group">
                  <label class="detail-label">Price per Unit</label>
                  <p class="detail-value">{{ viewingProduct.pricePerUnit | currency:'USD':'symbol':'1.2-2' }}</p>
                </div>
              </div>
              <div class="col-md-4">
                <div class="detail-group">
                  <label class="detail-label">Available Quantity</label>
                  <p class="detail-value">{{ viewingProduct.availableQuantity }}</p>
                </div>
              </div>
              <div class="col-md-4">
                <div class="detail-group">
                  <label class="detail-label">Unit of Measure</label>
                  <p class="detail-value">{{ viewingProduct.unitOfMeasure || 'Unit' }}</p>
                </div>
              </div>
            </div>

            <div class="row mb-3">
              <div class="col-md-6">
                <div class="detail-group">
                  <label class="detail-label">Created Date</label>
                  <p class="detail-value">{{ viewingProduct.createdDate | date:'medium' }}</p>
                </div>
              </div>
              <div class="col-md-6">
                <div class="detail-group">
                  <label class="detail-label">Modified Date</label>
                  <p class="detail-value">{{ viewingProduct.modifiedDate | date:'medium' }}</p>
                </div>
              </div>
            </div>
          </div>
          
          <div class="modal-footer">
            <button type="button" class="btn btn-secondary" (click)="closeViewModal()">
              Close
            </button>
            <button type="button" class="btn btn-primary" (click)="editProduct(viewingProduct!); closeViewModal();">
              <i class="material-icons">edit</i>
              Edit Product
            </button>
          </div>
        </div>
      </div>
    </div>

    <!-- Modal backdrop -->
    <div class="modal-backdrop fade" 
         [class.show]="showViewModal" 
         *ngIf="showViewModal"
         (click)="closeViewModal()"></div>
  `,
  styleUrls: ['./dealer-products.component.scss']
})
export class DealerProductsComponent implements OnInit {
  products: DealerProduct[] = [];
  loading: boolean = true;
  searchTerm: string = '';
  statusFilter: string = '';
  selectedDealerId: number | undefined = undefined;
  dealers: any[] = [];
  currentPage: number = 1;
  pageSize: number = 10;
  totalRecords: number = 0;
  totalPages: number = 0;

  // Statistics
  activeProducts: number = 0;
  inactiveProducts: number = 0;

  // Modal
  showViewModal = false;
  viewingProduct: DealerProduct | null = null;

  // Expose Math to template
  Math = Math;

  constructor(
    private dealerService: DealerManagementService,
    private router: Router
  ) { }

  ngOnInit(): void {
    this.loadDealers();
    this.loadProducts();
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

  loadProducts(): void {
    this.loading = true;

    // Parse status filter
    let isActiveFilter: boolean | undefined = undefined;
    if (this.statusFilter === 'true') {
      isActiveFilter = true;
    } else if (this.statusFilter === 'false') {
      isActiveFilter = false;
    }

    this.dealerService.getDealerProducts(
      this.currentPage,
      this.pageSize,
      this.searchTerm || undefined,
      undefined, // dealerId
      isActiveFilter
    ).subscribe({
      next: (response: PaginatedResponse<DealerProduct>) => {
        this.products = response.data;
        this.totalRecords = response.metadata.totalCount;
        this.totalPages = response.metadata.totalPages;
        this.calculateStatistics();
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading products:', error);
        this.loading = false;
      }
    });
  }

  calculateStatistics(): void {
    this.activeProducts = this.products.filter(p => p.isActive).length;
    this.inactiveProducts = this.products.filter(p => !p.isActive).length;
  }

  onSearch(): void {
    this.currentPage = 1;
    this.loadProducts();
  }

  applyFilters(): void {
    this.currentPage = 1;
    this.loadProducts();
  }

  changePageSize(): void {
    this.currentPage = 1;
    this.loadProducts();
  }

  goToPage(page: number): void {
    if (page >= 1 && page <= this.totalPages) {
      this.currentPage = page;
      this.loadProducts();
    }
  }

  viewProduct(product: DealerProduct): void {
    this.viewingProduct = product;
    this.showViewModal = true;
  }

  closeViewModal(): void {
    this.showViewModal = false;
    this.viewingProduct = null;
  }

  editProduct(product: DealerProduct): void {
    this.router.navigate(['/admin/dealer-management/products/edit', product.dealerProductID]);
  }

  deleteProduct(product: DealerProduct): void {
    if (!confirm(`Are you sure you want to delete product "${product.productName}"? This action cannot be undone.`)) {
      return;
    }

    this.dealerService.deleteDealerProduct(product.dealerProductID).subscribe({
      next: (response) => {
        if (response.success) {
          this.loadProducts();
        } else {
          alert(response.message || 'Failed to delete product');
        }
      },
      error: (error) => {
        console.error('Error deleting product:', error);
        alert(error.error?.message || 'An error occurred while deleting product');
      }
    });
  }

  getStatusClass(isActive: boolean): string {
    return isActive ? 'active' : 'inactive';
  }

  refreshData(): void {
    this.loadProducts();
  }

  exportData(): void {
    // Implement export functionality
    console.log('Exporting products data...');
  }

  createProduct(): void {
    this.router.navigate(['/admin/dealer-management/products/create']);
  }
}