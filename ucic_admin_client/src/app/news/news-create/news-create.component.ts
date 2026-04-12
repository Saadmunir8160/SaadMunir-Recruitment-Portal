import { Component, OnInit, CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import { NewsService } from '../services/news.service';
import { Router } from '@angular/router';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-news-create',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './news-create.component.html',
  styleUrls: ['./news-create.component.scss'],
  schemas: [CUSTOM_ELEMENTS_SCHEMA]
})
export class NewsCreateComponent implements OnInit {
  newsForm: FormGroup;
  loading = false;
  selectedImage: File | null = null;
  apiError: string | null = null;

  constructor(
    private newsService: NewsService, 
    private router: Router, 
    private fb: FormBuilder
  ) {
    this.newsForm = this.fb.group({
      title: ['', [Validators.required, Validators.minLength(5)]],
      content: ['', [Validators.required, Validators.minLength(20)]],
      createdAt: [new Date(), [Validators.required]],
      isArabic: [true]
    });
  }

  ngOnInit(): void {}

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

  onSubmit() {
    if (this.newsForm.invalid) {
      this.newsForm.markAllAsTouched();
      return;
    }

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

    this.newsService.addNews(formData).subscribe({
      next: () => {
        this.loading = false;
        this.router.navigate(['/admin/news']);
      },
      error: (error) => {
        this.apiError = 'Error creating news: ' + (error?.error?.message || 'Unknown error');
        this.loading = false;
      }
    });
  }

  onBack(): void {
      this.router.navigate(['/admin/news']);
  }
}
