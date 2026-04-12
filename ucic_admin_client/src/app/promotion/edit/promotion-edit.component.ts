import { Component, OnInit, CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { CommonModule } from '@angular/common';
import { PromotionService } from '../../services/promotion.service';
import { CoverageAreaService, CoverageArea } from '../../services/coverage-area.service';

@Component({
  selector: 'app-promotion-edit',
  templateUrl: './promotion-edit.component.html',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  schemas: [CUSTOM_ELEMENTS_SCHEMA]
})
export class PromotionEditComponent implements OnInit {
  promotionForm: FormGroup;
  loading = false;
  promotionId!: number;
  coverageAreas: CoverageArea[] = [];

  constructor(
    private fb: FormBuilder,
    private promotionService: PromotionService,
    private coverageAreaService: CoverageAreaService,
    private router: Router,
    private route: ActivatedRoute
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
    this.promotionId = Number(this.route.snapshot.paramMap.get('id'));
    this.loadCoverageAreas();
    this.loadPromotion();
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

  loadPromotion(): void {
    this.loading = true;
    this.promotionService.getAllPromotions().subscribe({
      next: (response) => {
        const promotions = response.data || response;
        const promotion = promotions.find((p: any) => p.promotionId === this.promotionId);
        if (promotion) {
          // Convert ISO dates to datetime-local format
          const validFrom = new Date(promotion.validFrom);
          const validTo = new Date(promotion.validTo);
          
          this.promotionForm.patchValue({
            code: promotion.code,
            coverageAreaId: promotion.coverageAreaId,
            discountPercentage: promotion.discountPercentage,
            validFrom: validFrom.toISOString().slice(0, 16),
            validTo: validTo.toISOString().slice(0, 16)
          });
        }
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading promotion:', error);
        this.loading = false;
      }
    });
  }

  onSubmit(): void {
    if (this.promotionForm.valid) {
      this.loading = true;
      const formValue = this.promotionForm.value;
      
      // Convert datetime-local inputs to ISO string
      formValue.validFrom = new Date(formValue.validFrom).toISOString();
      formValue.validTo = new Date(formValue.validTo).toISOString();
      formValue.promotionId = this.promotionId;

      this.promotionService.updatePromotion(formValue).subscribe({
        next: () => {
          this.loading = false;
          this.router.navigate(['/admin/promotion/list']);
        },
        error: (error) => {
          console.error('Error updating promotion:', error);
          this.loading = false;
        }
      });
    }
  }

  onCancel(): void {
    this.router.navigate(['/admin/promotion/list']);
  }
} 