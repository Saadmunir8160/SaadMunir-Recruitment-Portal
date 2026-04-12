import { Component, OnInit } from '@angular/core';
import { NewsService } from '../services/news.service';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import { News } from '../news.model';
import { PaginationComponent } from '../../shared/components/pagination/pagination.component';
import { PaginatedResponse } from '../../shared/interfaces/paginated-response.interface';

@Component({
  selector: 'app-news-list',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule, PaginationComponent],
  templateUrl: './news-list.component.html',
  styleUrls: ['./news-list.component.scss'],
  schemas: [CUSTOM_ELEMENTS_SCHEMA]
})
export class NewsListComponent implements OnInit {
  allNewsList: News[] = [];
  filteredNews: News[] = [];
  loading = false;
  searchTerm: string = '';

  // Pagination state
  currentPage: number = 1;
  pageSize: number = 10;
  pageSizeOptions: (number | string)[] = [5, 10, 15, 20, 30, 50, 'All'];
  totalCount: number = 0;
  totalPages: number = 0;
  isServerSidePagination: boolean = false;

  constructor(
    private newsService: NewsService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.getAllNews();
  }

  getAllNews(): void {
    this.loading = true;
    const pageSizeToSend = this.pageSize === 0 ? 1000000 : this.pageSize;
    this.newsService.getNews(this.currentPage, pageSizeToSend).subscribe({
      next: (news) => {
        // Handle both array and paginated response
        if (Array.isArray(news)) {
          this.allNewsList = news;
          this.isServerSidePagination = false;
          this.currentPage = 1;
          this.totalCount = news.length;
          this.totalPages = this.pageSize === 0 ? 1 : Math.ceil(this.totalCount / this.pageSize);
          this.applyFiltersAndPaginate();
        } else {
          const response = news as PaginatedResponse<News>;
          this.allNewsList = response.data;
          this.isServerSidePagination = true;
          this.currentPage = response.metadata.currentPage;
          this.totalCount = response.metadata.totalCount;
          this.totalPages = response.metadata.totalPages;
          this.filteredNews = response.data; // Use the data directly from server
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
      this.getAllNews();
      return;
    }

    // Client-side pagination logic
    let filtered = this.allNewsList;
    if (this.searchTerm) {
      const searchTermLower = this.searchTerm.toLowerCase();
      filtered = filtered.filter(news =>
        news.title.toLowerCase().includes(searchTermLower) ||
        news.content.toLowerCase().includes(searchTermLower)
      );
    }
    
    this.totalCount = filtered.length;
    this.totalPages = this.pageSize === 0 ? 1 : Math.ceil(this.totalCount / this.pageSize);
    
    if (this.pageSize === 0) {
      this.filteredNews = filtered;
    } else {
      const start = (this.currentPage - 1) * this.pageSize;
      this.filteredNews = filtered.slice(start, start + this.pageSize);
    }
  }

  onSearch(): void {
    if (this.isServerSidePagination) {
      // For server-side pagination, we need to implement search on server
      // For now, just reset to first page
      this.currentPage = 1;
      this.getAllNews();
    } else {
      this.currentPage = 1;
      this.applyFiltersAndPaginate();
    }
  }

  goToPage(page: number): void {
    this.currentPage = page;
    if (this.isServerSidePagination) {
      this.getAllNews();
    } else {
      this.applyFiltersAndPaginate();
    }
  }

  onPageSizeChange(size: number): void {
    this.pageSize = size;
    this.currentPage = 1;
    if (this.isServerSidePagination) {
      this.getAllNews();
    } else {
      this.applyFiltersAndPaginate();
    }
  }

  editNews(id: string): void {
    this.router.navigate(['/admin/news/edit', id]);
  }

  viewNews(id: string): void {
    this.router.navigate(['/admin/news/view', id]);
  }

  deleteNews(id: string): void {
    if (confirm('Are you sure you want to delete this news?')) {
      this.newsService.deleteNews(Number(id)).subscribe({
        next: () => {
          // Refresh the current page after deletion
          this.getAllNews();
        },
        error: (error) => {
        }
      });
    }
  }

  trackByNewsId(index: number, news: News): string {
    return news.id;
  }
}
