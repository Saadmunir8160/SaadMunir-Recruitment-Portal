import { Component, OnInit, CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import { HttpClient, HttpClientModule } from '@angular/common/http';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { JobApplicationsService, JobApplication } from '../services/job-applications.service';
import { environment } from '../../environments/environment';
import { PaginationComponent } from '../shared/components/pagination/pagination.component';
import { PaginatedResponse } from '../shared/interfaces/paginated-response.interface';

@Component({
  selector: 'app-job-applications-list',
  templateUrl: './job-applications-list.component.html',
  styleUrls: ['./job-applications-list.component.scss'],
  standalone: true,
  imports: [CommonModule, HttpClientModule, FormsModule, PaginationComponent],
  schemas: [CUSTOM_ELEMENTS_SCHEMA]
})
export class JobApplicationsListComponent implements OnInit {
  allJobApplications: JobApplication[] = [];
  filteredJobApplications: JobApplication[] = [];
  searchTerm: string = '';
  loading = false;
  error = '';

  // Pagination state
  currentPage: number = 1;
  pageSize: number = 10;
  pageSizeOptions: (number | string)[] = [5, 10, 15, 20, 30, 50, 'All'];
  totalCount: number = 0;
  totalPages: number = 0;
  isServerSidePagination: boolean = false;

  constructor(private jobApplicationsService: JobApplicationsService) {}

  ngOnInit(): void {
    this.fetchJobApplications();
  }

  getFullPath(relativePath: string): string {
    if (!relativePath) return '';
  
    // Normalize all slashes to forward slashes
    let normalizedPath = relativePath.replace(/\\/g, '/');
  
    // Find the last occurrence of 'wwwroot'
    const lastWwwrootIndex = normalizedPath.toLowerCase().lastIndexOf('wwwroot');
  
    if (lastWwwrootIndex === -1) {
      return `${environment.apiUrl}/${normalizedPath.replace(/^[/]+/, '')}`;
    }
  
    // Extract path after the last 'wwwroot'
    let cleanPath = normalizedPath.substring(lastWwwrootIndex + 'wwwroot'.length);
  
    // Remove any leading slashes
    cleanPath = cleanPath.replace(/^[/]+/, '');
  
    return `${environment.apiUrl.replace('/api', '')}/static/${cleanPath}`;
  }

  fetchJobApplications() {
    this.loading = true;
    const pageSizeToSend = this.pageSize === 0 ? 1000000 : this.pageSize;
    this.jobApplicationsService.getAll(this.currentPage, pageSizeToSend).subscribe({
      next: (data) => {
        // Handle both array and paginated response
        if (Array.isArray(data)) {
          this.allJobApplications = data;
          this.isServerSidePagination = false;
          this.totalCount = data.length;
          this.totalPages = this.pageSize === 0 ? 1 : Math.ceil(this.totalCount / this.pageSize);
          this.applyFiltersAndPaginate();
        } else {
          const response = data as PaginatedResponse<JobApplication>;
          this.allJobApplications = response.data;
          this.isServerSidePagination = true;
          this.currentPage = response.metadata.currentPage;
          this.totalCount = response.metadata.totalCount;
          this.totalPages = response.metadata.totalPages;
          this.filteredJobApplications = response.data; // Use the data directly from server
        }
        this.loading = false;
      },
      error: (err) => {
        this.error = 'Failed to load job applications.';
        this.loading = false;
      }
    });
  }

  applyFiltersAndPaginate(): void {
    if (this.isServerSidePagination) {
      // For server-side pagination, we need to fetch data from server
      this.fetchJobApplications();
      return;
    }

    // Client-side pagination logic
    let filtered = this.allJobApplications;
    if (this.searchTerm) {
      const searchTermLower = this.searchTerm.toLowerCase();
      filtered = filtered.filter(app =>
        app.fullName?.toLowerCase().includes(searchTermLower) ||
        app.email?.toLowerCase().includes(searchTermLower) ||
        app.phone?.toLowerCase().includes(searchTermLower) ||
        app.jobTitle?.toLowerCase().includes(searchTermLower)
      );
    }
    
    this.totalCount = filtered.length;
    this.totalPages = this.pageSize === 0 ? 1 : Math.ceil(this.totalCount / this.pageSize);
    
    if (this.pageSize === 0) {
      this.filteredJobApplications = filtered;
    } else {
      const start = (this.currentPage - 1) * this.pageSize;
      this.filteredJobApplications = filtered.slice(start, start + this.pageSize);
    }
  }

  onSearch(): void {
    if (this.isServerSidePagination) {
      // For server-side pagination, we need to implement search on server
      // For now, just reset to first page
      this.currentPage = 1;
      this.fetchJobApplications();
    } else {
      this.currentPage = 1;
      this.applyFiltersAndPaginate();
    }
  }

  goToPage(page: number): void {
    this.currentPage = page;
    if (this.isServerSidePagination) {
      this.fetchJobApplications();
    } else {
      this.applyFiltersAndPaginate();
    }
  }

  onPageSizeChange(size: number): void {
    this.pageSize = size;
    this.currentPage = 1;
    if (this.isServerSidePagination) {
      this.fetchJobApplications();
    } else {
      this.applyFiltersAndPaginate();
    }
  }

  downloadResume(id: number) {
    this.jobApplicationsService.downloadResume(id).subscribe(blob => {
      const url = window.URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = `resume_${id}.pdf`;
      a.click();
      window.URL.revokeObjectURL(url);
    });
  }

  trackByJobApplicationId(index: number, application: JobApplication): number {
    return application.jobApplicationsId;
  }
} 