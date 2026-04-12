import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { PromotionService } from '../../services/promotion.service';
import { CoverageAreaService, CoverageArea } from '../../services/coverage-area.service';
import { HttpErrorResponse } from '@angular/common/http';

@Component({
  selector: 'app-promotion-create',
  templateUrl: './promotion-create.component.html',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule]
})
export class PromotionCreateComponent implements OnInit {
  promotionForm: FormGroup;
  loading = false;
  errorMessage = '';
  coverageAreas: CoverageArea[] = [];

  constructor(
    private fb: FormBuilder,
    private promotionService: PromotionService,
    private coverageAreaService: CoverageAreaService,
    private router: Router
  ) {
    this.promotionForm = this.fb.group({
      code: ['', [Validators.required, Validators.maxLength(50)]],
      coverageAreaId: ['', Validators.required],
      discountPercentage: ['', [Validators.required, Validators.min(0), Validators.max(100)]],
      validFrom: ['', Validators.required],
      validTo: ['', Validators.required]
    });
  }

  ngOnInit(): void {
    this.loadCoverageAreas();
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
        this.errorMessage = 'Failed to load coverage areas. Please try again.';
      }
    });
  }

  onSubmit(): void {
    if (this.promotionForm.valid) {
      this.loading = true;
      this.errorMessage = '';
      const formValue = this.promotionForm.value;
      
      // Convert datetime-local inputs to ISO string
      formValue.validFrom = new Date(formValue.validFrom).toISOString();
      formValue.validTo = new Date(formValue.validTo).toISOString();

      this.promotionService.createPromotion(formValue).subscribe({
        next: (response) => {
          this.loading = false;
          this.router.navigate(['/admin/promotion/list']);
        },
        error: (error: HttpErrorResponse) => {
          this.loading = false;
          console.error('Error creating promotion:', error);
          if (error.error && typeof error.error === 'string') {
            this.errorMessage = error.error;
          } else if (error.error && error.error.message) {
            this.errorMessage = error.error.message;
          } else {
            this.errorMessage = 'An error occurred while creating the promotion. Please try again.';
          }
        }
      });
    }
  }

  onCancel(): void {
    this.router.navigate(['/admin/promotion/list']);
  }
} 