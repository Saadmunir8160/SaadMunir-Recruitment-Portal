import { Component, OnInit } from '@angular/core';
import { CoverageAreaService, CoverageArea } from '../../services/coverage-area.service';
import { Router, RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import { PaginationComponent } from '../../shared/components/pagination/pagination.component';
import { PaginatedResponse } from '../../shared/interfaces/paginated-response.interface';

@Component({
  selector: 'app-coverage-area-list',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule, PaginationComponent],
  templateUrl: './coverage-area-list.component.html',
  styleUrls: ['./coverage-area-list.component.scss'],
  schemas: [CUSTOM_ELEMENTS_SCHEMA]
})
export class CoverageAreaListComponent implements OnInit {
  allCoverageAreas: CoverageArea[] = [];
  filteredCoverageAreas: CoverageArea[] = [];
  searchTerm: string = '';

  // Pagination state
  currentPage: number = 1;
  pageSize: number = 10;
  pageSizeOptions: (number | string)[] = [5, 10, 15, 20, 30, 50, 'All'];
  totalCount: number = 0;
  totalPages: number = 0;
  loading: boolean = false;
  isServerSidePagination: boolean = false;

  constructor(
    private coverageAreaService: CoverageAreaService,
    private router: Router
  ) { 
  }

  ngOnInit(): void {
    this.loadCoverageAreas();
  }

  loadCoverageAreas(): void {
    this.loading = true;
    const pageSizeToSend = this.pageSize === 0 ? 1000000 : this.pageSize;
    this.coverageAreaService.getAllCoverageAreas(this.currentPage, pageSizeToSend).subscribe({
      next: (data) => {
        
        // Handle both array and paginated response
        if (Array.isArray(data)) {
          data.forEach((item, index) => {
          });
          this.allCoverageAreas = data;
          this.isServerSidePagination = false;
          this.totalCount = data.length;
          this.totalPages = this.pageSize === 0 ? 1 : Math.ceil(this.totalCount / this.pageSize);
          this.applyFiltersAndPaginate();
        } else {
          const response = data as PaginatedResponse<CoverageArea>;
          this.allCoverageAreas = response.data;
          this.isServerSidePagination = true;
          this.currentPage = response.metadata.currentPage;
          this.totalCount = response.metadata.totalCount;
          this.totalPages = response.metadata.totalPages;
          this.filteredCoverageAreas = response.data; // Use the data directly from server
        }
        
        this.loading = false;
      },
      error: (error) => {
        this.loading = false;
      }
    });
  }

  applyFiltersAndPaginate(): void {
    if (this.isServerSidePagination) {
      // For server-side pagination, we need to fetch data from server
      this.loadCoverageAreas();
      return;
    }

    // Client-side pagination logic
    let filtered = this.allCoverageAreas;
    if (this.searchTerm) {
      const searchTermLower = this.searchTerm.toLowerCase();
      filtered = filtered.filter(area =>
        area.name.toLowerCase().includes(searchTermLower) ||
        (area.arabicName && area.arabicName.toLowerCase().includes(searchTermLower))
      );
    }
    
    this.totalCount = filtered.length;
    this.totalPages = this.pageSize === 0 ? 1 : Math.ceil(this.totalCount / this.pageSize);
    
    if (this.pageSize === 0) {
      this.filteredCoverageAreas = filtered;
    } else {
      const start = (this.currentPage - 1) * this.pageSize;
      this.filteredCoverageAreas = filtered.slice(start, start + this.pageSize);
    }
  }

  onSearch(): void {
    if (this.isServerSidePagination) {
      // For server-side pagination, we need to implement search on server
      // For now, just reset to first page
      this.currentPage = 1;
      this.loadCoverageAreas();
    } else {
      this.currentPage = 1;
      this.applyFiltersAndPaginate();
    }
  }

  goToPage(page: number): void {
    this.currentPage = page;
    if (this.isServerSidePagination) {
      this.loadCoverageAreas();
    } else {
      this.applyFiltersAndPaginate();
    }
  }

  onPageSizeChange(size: number): void {
    this.pageSize = size;
    this.currentPage = 1;
    if (this.isServerSidePagination) {
      this.loadCoverageAreas();
    } else {
      this.applyFiltersAndPaginate();
    }
  }

  editCoverageArea(id: number): void {
    this.router.navigate(['/admin/coverage-area/edit', id]);
  }

  createCoverageArea(): void {
    this.router.navigate(['/admin/coverage-area/create']);
  }

  deleteCoverageArea(id: number): void {
    if (confirm('Are you sure you want to delete this coverage area?')) {
      this.coverageAreaService.deleteCoverageArea(id).subscribe({
        next: () => {
          // Refresh the current page after deletion
          this.loadCoverageAreas();
        },
        error: (error) => {
        }
      });
    }
  }

  trackByCoverageAreaId(index: number, area: CoverageArea): number {
    return area.coverageAreaId;
  }
} 