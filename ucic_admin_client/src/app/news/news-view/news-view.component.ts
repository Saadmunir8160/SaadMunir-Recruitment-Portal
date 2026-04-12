import { Component, OnInit, CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { NewsService } from '../services/news.service';
import { News } from '../news.model';
import { CommonModule } from '@angular/common';
import { environment } from '../../../environments/environment';

@Component({
  selector: 'app-news-view',
  standalone: true,
  imports: [CommonModule],
  schemas: [CUSTOM_ELEMENTS_SCHEMA],
  template: `
    <div class="container-fluid">
      <div class="row">
        <div class="col-12">
          <div class="card">
            <div class="card-header">
              <h4 class="card-title">News Details</h4>
            </div>
            <div class="card-body">
              <div class="row" *ngIf="news">
                <div class="col-md-12">
                  <div class="mb-3">
                    <h2>{{ news.title }}</h2>
                    <p class="text-muted">
                      Created: {{ news.createdAt | date:'medium' }}
                      <span *ngIf="news.updatedAt"> | Updated: {{ news.updatedAt | date:'medium' }}</span>
                    </p>
                  </div>
                  <div class="mb-3" *ngIf="news.imageUrl">
                    <img [src]="getImageUrl(news.imageUrl)" class="img-fluid rounded" alt="News image">
                  </div>
                  <div class="mb-3">
                    <p class="content">{{ news.content }}</p>
                  </div>
                  <div class="mb-3">
                    <button class="btn btn-primary me-2" (click)="onEdit()">
                      <iconify-icon icon="solar:pen-bold-duotone" class="me-1"></iconify-icon>
                      Edit
                    </button>
                    <button class="btn btn-light" (click)="onBack()">
                      <iconify-icon icon="solar:arrow-left-bold-duotone" class="me-1"></iconify-icon>
                      Back
                    </button>
                  </div>
                </div>
              </div>
              <div class="row" *ngIf="!news && !loading">
                <div class="col-12">
                  <div class="alert alert-danger">News not found</div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .content {
      white-space: pre-wrap;
      line-height: 1.6;
    }
  `]
})
export class NewsViewComponent implements OnInit {
  news: News | null = null;
  loading = true;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private newsService: NewsService
  ) {}

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.loadNews(Number(id));
    }
  }

  loadNews(id: number): void {
    this.loading = true;
    this.newsService.getNewsById(id).subscribe({
      next: (data) => {
        this.news = data;
        this.loading = false;
      },
      error: () => {
        this.loading = false;
      }
    });
  }

  
  getImageUrl(relativePath: string): string {
    
    if (!relativePath) return '';
    
    // Extract just the relative path part after 'wwwroot'
    const wwwrootIndex = relativePath.toLowerCase().indexOf('wwwroot');
    let cleanPath = '';
    
    if (wwwrootIndex !== -1) {
      // If path contains 'wwwroot', take everything after it
      cleanPath = relativePath.substring(wwwrootIndex + 'wwwroot'.length);
    } else {
      // If no 'wwwroot' in path, use the path as is
      cleanPath = relativePath;
    }
    
    // Remove any leading slashes or backslashes
    cleanPath = cleanPath.replace(/^[/\\]+/, '');
    
    // Replace any backslashes with forward slashes
    cleanPath = cleanPath.replace(/\\/g, '/');
    
     return `${environment.apiUrl.replace('/api', '')}/static/News/${cleanPath}`
    // Construct the final URL
  }
  onEdit(): void {
    if (this.news) {
      this.router.navigate(['/admin/news/edit', this.news.id]);
    }
  }

  onBack(): void {
    this.router.navigate(['/admin/news']);
  }
} 