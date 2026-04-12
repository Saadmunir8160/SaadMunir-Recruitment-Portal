import { Component, OnInit, CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import { Router } from '@angular/router';
import { PromotionService } from '../../services/promotion.service';
import { CoverageAreaService, CoverageArea } from '../../services/coverage-area.service';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { PaginationComponent } from '../../shared/components/pagination/pagination.component';
import { PaginatedResponse } from '../../shared/interfaces/paginated-response.interface';

@Component({
  selector: 'app-promotion-list',
  templateUrl: './promotion-list.component.html',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule, PaginationComponent],
  schemas: [CUSTOM_ELEMENTS_SCHEMA]
})
export class PromotionListComponent implements OnInit {
  allPromotions: any[] = [];
  filteredPromotions: any[] = [];
  searchTerm: string = '';
  coverageAreas: CoverageArea[] = [];

  // Pagination state
  currentPage: number = 1;
  pageSize: number = 10;
  pageSizeOptions: (number | string)[] = [5, 10, 15, 20, 30, 50, 'All'];
  totalCount: number = 0;
  totalPages: number = 0;
  loading = false;
  isServerSidePagination: boolean = false;

  constructor(
    private promotionService: PromotionService,
    private coverageAreaService: CoverageAreaService,
    private router: Router
  ) { }

  ngOnInit(): void {
    this.loadCoverageAreas();
    this.loadPromotions();
  }

  loadCoverageAreas(): void {
    this.coverageAreaService.getAllCoverageAreas().subscribe({
      next: (areas) => {
        // Handle both array and paginated response
        if (Array.isArray(areas)) {
          this.coverageAreas = areas;
        } else {
          this.coverageAreas = areas.data;
        }
      },
      error: (error) => {
        console.error('Error loading coverage areas:', error);
      }
    });
  }

  getCoverageAreaName(id: number): string {
    const area = this.coverageAreas.find(a => a.coverageAreaId === id);
    return area ? area.name : id.toString();
  }

  loadPromotions(): void {
    this.loading = true;
    const pageSizeToSend = this.pageSize === 0 ? 1000000 : this.pageSize;
    this.promotionService.getAllPromotions(this.currentPage, pageSizeToSend).subscribe({
      next: (response) => {
        // Handle both array and paginated response
        if (Array.isArray(response)) {
          this.allPromotions = response;
          this.isServerSidePagination = false;
          this.totalCount = response.length;
          this.totalPages = this.pageSize === 0 ? 1 : Math.ceil(this.totalCount / this.pageSize);
          this.applyFiltersAndPaginate();
        } else {
          const paginatedResponse = response as PaginatedResponse<any>;
          this.allPromotions = paginatedResponse.data || response.data || response;
          this.isServerSidePagination = true;
          this.currentPage = paginatedResponse.metadata.currentPage || 1;
          this.totalCount = paginatedResponse.metadata.totalCount || this.allPromotions.length;
          this.totalPages = paginatedResponse.metadata.totalPages || 1;
          this.filteredPromotions = paginatedResponse.data; // Use the data directly from server
        }
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading promotions:', error);
        this.loading = false;
      }
    });
  }

  applyFiltersAndPaginate(): void {
    if (this.isServerSidePagination) {
      // For server-side pagination, we need to fetch data from server
      this.loadPromotions();
      return;
    }

    // Client-side pagination logic
    let filtered = this.allPromotions;
    if (this.searchTerm) {
      const searchTermLower = this.searchTerm.toLowerCase();
      filtered = filtered.filter(promotion =>
        promotion.code?.toLowerCase().includes(searchTermLower) ||
        this.getCoverageAreaName(promotion.coverageAreaId).toLowerCase().includes(searchTermLower) ||
        promotion.discountPercentage?.toString().includes(searchTermLower)
      );
    }
    
    this.totalCount = filtered.length;
    this.totalPages = this.pageSize === 0 ? 1 : Math.ceil(this.totalCount / this.pageSize);
    
    if (this.pageSize === 0) {
      this.filteredPromotions = filtered;
    } else {
      const start = (this.currentPage - 1) * this.pageSize;
      this.filteredPromotions = filtered.slice(start, start + this.pageSize);
    }
  }

  onSearch(): void {
    if (this.isServerSidePagination) {
      // For server-side pagination, we need to implement search on server
      // For now, just reset to first page
      this.currentPage = 1;
      this.loadPromotions();
    } else {
      this.currentPage = 1;
      this.applyFiltersAndPaginate();
    }
  }

  goToPage(page: number): void {
    this.currentPage = page;
    if (this.isServerSidePagination) {
      this.loadPromotions();
    } else {
      this.applyFiltersAndPaginate();
    }
  }

  onPageSizeChange(size: number | string): void {
    if (size === 'All') {
      this.pageSize = 0; // 0 means show all
    } else {
      this.pageSize = Number(size);
    }
    this.currentPage = 1;
    if (this.isServerSidePagination) {
      this.loadPromotions();
    } else {
      this.applyFiltersAndPaginate();
    }
  }

  createNew(): void {
    this.router.navigate(['/admin/promotion/create']);
  }

  editPromotion(id: number): void {
    this.router.navigate(['/admin/promotion/edit', id]);
  }

  deletePromotion(id: number): void {
    if (confirm('Are you sure you want to delete this promotion?')) {
      this.promotionService.deletePromotion(id).subscribe({
        next: () => {
          // Refresh the current page after deletion
          this.loadPromotions();
        },
        error: (error) => {
          console.error('Error deleting promotion:', error);
        }
      });
    }
  }

  formatDate(date: Date): string {
    return new Date(date).toLocaleString();
  }

  trackByPromotionId(index: number, promotion: any): number {
    return promotion.promotionId;
  }
} 