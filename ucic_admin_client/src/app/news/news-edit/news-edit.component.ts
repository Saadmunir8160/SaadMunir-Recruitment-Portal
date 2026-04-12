import { Component, OnInit, CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import { NewsService } from '../services/news.service';
import { Router, ActivatedRoute } from '@angular/router';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { News } from '../news.model';
import { environment } from '../../../environments/environment';

@Component({
  selector: 'app-news-edit',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  schemas: [CUSTOM_ELEMENTS_SCHEMA],
  templateUrl: './news-edit.component.html'
})
export class NewsEditComponent implements OnInit {
  newsForm: FormGroup;
  loading = false;
  selectedImage: File | null = null;
  currentImageUrl: string | null = null;
  apiError: string | null = null;
  newsId: number | null = null;

  constructor(
    private newsService: NewsService,
    private router: Router,
    private route: ActivatedRoute,
    private fb: FormBuilder
  ) {
    this.newsForm = this.fb.group({
      title: ['', [Validators.required, Validators.minLength(5)]],
      content: ['', [Validators.required, Validators.minLength(20)]],
      createdAt: [new Date(), [Validators.required]],
      isArabic: [true]
    });
  }

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.newsId = Number(id);
      this.loadNews(this.newsId);
    }
  }

  // Helper function to format date for datetime-local input
  private formatDateForInput(date: string | Date): string {
    const d = new Date(date);
    const pad = (n: number) => n < 10 ? '0' + n : n;
    return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}T${pad(d.getHours())}:${pad(d.getMinutes())}`;
  }

  loadNews(id: number): void {
    this.loading = true;
    this.newsService.getNewsById(id).subscribe({
      next: (news) => {
        this.newsForm.patchValue({
          title: news.title,
          content: news.content,
          createdAt: this.formatDateForInput(news.createdAt ? news.createdAt : new Date()),
          isArabic: ('isArabic' in news) ? news.isArabic : false
        });
        if (news.imageUrl) {
          this.currentImageUrl = news.imageUrl;
        }
        this.loading = false;
      },
      error: () => {
        this.loading = false;
        this.router.navigate(['/admin/news']);
      }
    });
  }

  isFieldInvalid(fieldName: string): boolean {
    const field = this.newsForm.get(fieldName);
    return !!field && field.invalid && (field.dirty || field.touched);
  }

  onFileChange(event: any) {
    const file = event.target.files[0];
    if (file && file.type.startsWith('image/')) {
      this.selectedImage = file;
      this.apiError = null;
    } else {
      this.selectedImage = null;
      this.apiError = 'Invalid file type selected. Please select an image.';
      event.target.value = '';
    }
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
    
    // Construct the final URL
   return `${environment.apiUrl.replace('/api', '')}/static/News/${cleanPath}`
  }

  onSubmit() {
    this.apiError = null;
    if (this.newsForm.invalid || !this.newsId) {
      this.newsForm.markAllAsTouched();
      return;
    }

    // Debug logs

    this.loading = true;
    const formValue = this.newsForm.value;
    const formData = new FormData();

    formData.append('Title', formValue.title);
    formData.append('Content', formValue.content);
    formData.append('IsArabic', formValue.isArabic);
    
    // Convert the datetime-local input value to a proper Date object
    const selectedDate = new Date(formValue.createdAt);
    formData.append('CreatedDate', selectedDate.toISOString());

    if (this.selectedImage) {
      formData.append('File', this.selectedImage, this.selectedImage.name);
    }

    // // Debug: log FormData
    // console.log('FormData contents:');
    // for (let pair of formData.entries()) {
    //   console.log('FormData:', pair[0], pair[1]);
    // }

    this.newsService.updateNews(this.newsId, formData).subscribe({
      next: () => {
        this.loading = false;
        this.router.navigate(['/admin/news/view', this.newsId]);
      },
      error: (error) => {
        this.apiError = 'Error updating news: ' + (error?.error?.message || 'Unknown error');
        this.loading = false;
      }
    });
  }

  onBack(): void {
    if (this.newsId) {
      this.router.navigate(['/admin/news']);
    } else {
      this.router.navigate(['/admin/news']);
    }
  }
} 