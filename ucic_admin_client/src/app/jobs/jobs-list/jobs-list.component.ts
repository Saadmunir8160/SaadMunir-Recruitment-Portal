import { Component, OnInit } from '@angular/core';
import { JobsService } from '../services/jobs.service';
import { DepartmentsService } from '../services/departments.service';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule, Router } from '@angular/router';
import { CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import { PaginationComponent } from '../../shared/components/pagination/pagination.component';
import { PaginatedResponse } from '../../shared/interfaces/paginated-response.interface';

@Component({
  selector: 'app-jobs-list',
  standalone: true,
  templateUrl: './jobs-list.component.html',
  styleUrl: './jobs-list.component.scss',
  imports: [CommonModule, FormsModule, RouterModule, PaginationComponent],
  schemas: [CUSTOM_ELEMENTS_SCHEMA]
})
export class JobsListComponent implements OnInit {
  allJobsList: any[] = [];
  filteredJobs: any[] = [];
  searchText: string = '';
  departments: any[] = [];

  // Pagination state
  currentPage: number = 1;
  pageSize: number = 10;
  pageSizeOptions: (number | string)[] = [5, 10, 15, 20, 30, 50, 'All'];
  totalCount: number = 0;
  totalPages: number = 0;
  loading: boolean = false;
  isServerSidePagination: boolean = false;

  constructor(
    private jobsService: JobsService,
    private departmentsService: DepartmentsService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.getDepartments();
    this.getAllJobs();
  }

  getDepartments(): void {
    this.departmentsService.getDepartments().subscribe({
      next: (res: any) => {
        this.departments = Array.isArray(res) ? res : res.data;
      },
      error: (err) => {
      }
    });
  }

  getDepartmentName(departmentId: number): string {
    return this.departments.find(d => d.departmentId === departmentId)?.name || 'N/A';
  }

  createJob(): void {
    this.router.navigate(['/admin/jobs/create']);
  }

  getAllJobs(): void {
    this.loading = true;
    const pageSizeToSend = this.pageSize === 0 ? 1000000 : this.pageSize;
    this.jobsService.getJobs(this.currentPage, pageSizeToSend).subscribe({
      next: (res) => {
        // Handle both array and paginated response
        if (Array.isArray(res)) {
          this.allJobsList = res;
          this.isServerSidePagination = false;
          this.totalCount = res.length;
          this.totalPages = this.pageSize === 0 ? 1 : Math.ceil(this.totalCount / this.pageSize);
          this.applyFiltersAndPaginate();
        } else {
          const response = res as PaginatedResponse<any>;
          this.allJobsList = response.data;
          this.isServerSidePagination = true;
          this.currentPage = response.metadata.currentPage;
          this.totalCount = response.metadata.totalCount;
          this.totalPages = response.metadata.totalPages;
          this.filteredJobs = response.data; // Use the data directly from server
        }
        this.loading = false;
      },
      error: (err) => {
        this.loading = false;
      }
    });
  }

  applyFiltersAndPaginate(): void {
    if (this.isServerSidePagination) {
      // For server-side pagination, we need to fetch data from server
      this.getAllJobs();
      return;
    }

    // Client-side pagination logic
    let filtered = this.allJobsList;
    if (this.searchText) {
      const searchLower = this.searchText.toLowerCase();
      filtered = filtered.filter(job => 
        job.title.toLowerCase().includes(searchLower) ||
        this.getDepartmentName(job.departmentId).toLowerCase().includes(searchLower) ||
        job.location.toLowerCase().includes(searchLower) ||
        job.workType.toLowerCase().includes(searchLower) ||
        job.salary.toString().includes(searchLower)
      );
    }
    
    this.totalCount = filtered.length;
    this.totalPages = this.pageSize === 0 ? 1 : Math.ceil(this.totalCount / this.pageSize);
    
    if (this.pageSize === 0) {
      this.filteredJobs = filtered;
    } else {
      const start = (this.currentPage - 1) * this.pageSize;
      this.filteredJobs = filtered.slice(start, start + this.pageSize);
    }
  }

  filterJobs(): void {
    if (this.isServerSidePagination) {
      this.currentPage = 1;
      this.getAllJobs();
    } else {
      this.currentPage = 1;
      this.applyFiltersAndPaginate();
    }
  }

  onSearch(): void {
    if (this.isServerSidePagination) {
      // For server-side pagination, we need to implement search on server
      // For now, just reset to first page
      this.currentPage = 1;
      this.getAllJobs();
    } else {
      this.currentPage = 1;
      this.applyFiltersAndPaginate();
    }
  }

  goToPage(page: number): void {
    this.currentPage = page;
    if (this.isServerSidePagination) {
      this.getAllJobs();
    } else {
      this.applyFiltersAndPaginate();
    }
  }

  onPageSizeChange(size: number): void {
    this.pageSize = size;
    this.currentPage = 1;
    if (this.isServerSidePagination) {
      this.getAllJobs();
    } else {
      this.applyFiltersAndPaginate();
    }
  }

  deleteJob(jobId: number): void {
    if (confirm('Are you sure you want to delete this job?')) {
      this.jobsService.deleteJobs(jobId).subscribe({
        next: () => {
          // Refresh the current page after deletion
          this.getAllJobs();
        },
        error: (err) => {
        }
      });
    }
  }

  trackByJobId(index: number, job: any): number {
    return job.jobsId;
  }
}
