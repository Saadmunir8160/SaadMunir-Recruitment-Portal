import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { RouterModule, Router, ActivatedRoute } from '@angular/router';
import { DealerManagementService } from '../../../services/dealer-management.service';

export interface CreateDealerProductRequest {
  productName: string;
  product_LnCode?: string;
  description?: string;
  unit: string;
}

export interface UpdateDealerProductRequest {
  productName: string;
  product_LnCode?: string;
  description?: string;
  unit: string;
}

@Component({
  selector: 'app-product-form',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule, RouterModule],
  template: `
    <div class="product-form-container">
      <button class="btn btn-outline-secondary mb-3" (click)="goBack()">
        <i class="material-icons">arrow_back</i>
        Back to Products
      </button>
      <!-- Header Section -->
      <div class="form-header">
        <div class="header-content">

          
          <h2 class="form-title">
            <i class="material-icons">inventory</i>
            {{ isEditMode ? 'Edit' : 'Create' }} Dealer Product
          </h2>
        </div>
      </div>

      <!-- Form Section -->
      <div class="form-section">
        <div class="card">
          <div class="card-body">
            <form [formGroup]="productForm" (ngSubmit)="saveProduct()">
              
              <!-- Product Name -->
              <div class="row">
                <div class="col-md-12">
                  <div class="form-group">
                    <label for="productName" class="form-label">Product Name *</label>
                    <input type="text" 
                           id="productName" 
                           formControlName="productName" 
                           class="form-control"
                           maxlength="200"
                           placeholder="Enter product name"
                           [class.is-invalid]="productForm.get('productName')?.invalid && productForm.get('productName')?.touched">
                    <div *ngIf="productForm.get('productName')?.invalid && productForm.get('productName')?.touched" 
                         class="invalid-feedback">
                      <div *ngIf="productForm.get('productName')?.errors?.['required']">
                        Product name is required
                      </div>
                      <div *ngIf="productForm.get('productName')?.errors?.['maxlength']">
                        Product name cannot exceed 200 characters
                      </div>
                    </div>
                  </div>
                </div>
              </div>

              <!-- Product Line Code (Optional) -->
              <div class="row">
                <div class="col-md-6">
                  <div class="form-group">
                    <label for="product_LnCode" class="form-label">Product LN-Code</label>
                    <input type="text" 
                           id="product_LnCode" 
                           formControlName="product_LnCode" 
                           class="form-control"
                           maxlength="100"
                           placeholder="Enter product line code (optional)"
                           [class.is-invalid]="productForm.get('product_LnCode')?.invalid && productForm.get('product_LnCode')?.touched">
                    <div *ngIf="productForm.get('product_LnCode')?.invalid && productForm.get('product_LnCode')?.touched" 
                         class="invalid-feedback">
                      Product line code cannot exceed 100 characters
                    </div>
                    <small class="form-text text-muted">
                      Optional code for internal tracking and categorization
                    </small>
                  </div>
                </div>

                <div class="col-md-6">
                  <div class="form-group">
                    <label for="unit" class="form-label">Unit of Measurement *</label>
                    <select id="unit" 
                            formControlName="unit" 
                            class="form-select"
                            [class.is-invalid]="productForm.get('unit')?.invalid && productForm.get('unit')?.touched">
                      <option value="">Select unit...</option>
                      <option value="BAG">BAG - Bags</option>
                      <option value="TON">TON - Tons</option>
                      <option value="EA">EA - Each</option>
                      <option value="TRK">TRK - Truck</option>
                      <option value="PCS">PCS - Pieces</option>
                    </select>
                    <div *ngIf="productForm.get('unit')?.invalid && productForm.get('unit')?.touched" 
                         class="invalid-feedback">
                      Please select a unit of measurement
                    </div>
                    <small class="form-text text-muted">
                      Choose the unit for quantity measurement (default: BAG)
                    </small>
                  </div>
                </div>
              </div>

              <!-- Description -->
              <div class="row">
                <div class="col-md-12">
                  <div class="form-group">
                    <label for="description" class="form-label">Description</label>
                    <textarea id="description" 
                              formControlName="description" 
                              class="form-control"
                              rows="4"
                              maxlength="500"
                              placeholder="Enter product description (optional)"
                              [class.is-invalid]="productForm.get('description')?.invalid && productForm.get('description')?.touched"></textarea>
                    <div *ngIf="productForm.get('description')?.invalid && productForm.get('description')?.touched" 
                         class="invalid-feedback">
                      Description cannot exceed 500 characters
                    </div>
                    <small class="form-text text-muted">
                      Provide detailed information about the product (up to 500 characters)
                    </small>
                  </div>
                </div>
              </div>

              <!-- Status (Only in Edit Mode) -->
              <div class=\"row\" *ngIf=\"isEditMode\">
                <div class=\"col-md-12\">
                  <div class=\"form-group\">
                    <label class=\"form-label\">Status</label>
                    <div class=\"form-check form-switch\">
                      <input class=\"form-check-input\" type=\"checkbox\" id=\"isActive\" formControlName=\"isActive\">
                      <label class=\"form-check-label\" for=\"isActive\">
                        {{ productForm.get('isActive')?.value ? 'Active' : 'Inactive' }}
                      </label>
                    </div>
                    <small class=\"form-text text-muted\">
                      Toggle to activate or deactivate this product
                    </small>
                  </div>
                </div>
              </div>

              <!-- Error Message -->
              <div *ngIf=\"errorMessage\" class=\"alert alert-danger\">
                {{ errorMessage }}
              </div>

              <!-- Success Message -->
              <div *ngIf=\"successMessage\" class=\"alert alert-success\">
                {{ successMessage }}
              </div>

              <!-- Form Actions -->
              <div class="form-actions">
                <button type="button" 
                        class="btn btn-secondary" 
                        (click)="goBack()">
                  Cancel
                </button>
                <button type="submit" 
                        class="btn btn-primary" 
                        [disabled]="productForm.invalid || saving">
                  <span *ngIf="saving" class="spinner-border spinner-border-sm me-2"></span>
                  {{ isEditMode ? 'Update' : 'Create' }} Product
                </button>
              </div>
            </form>
          </div>
        </div>
      </div>
    </div>
  `,
  styleUrls: ['./product-form.component.scss']
})
export class ProductFormComponent implements OnInit {
  productForm!: FormGroup;
  isEditMode = false;
  productId: number | null = null;
  loading = false;
  saving = false;
  errorMessage = '';
  successMessage = '';

  constructor(
    private fb: FormBuilder,
    private dealerService: DealerManagementService,
    private router: Router,
    private route: ActivatedRoute
  ) {
    this.initForm();
  }

  ngOnInit(): void {
    this.route.params.subscribe(params => {
      if (params['id']) {
        this.isEditMode = true;
        this.productId = +params['id'];
        this.loadProduct();
      }
    });
  }

  loadProduct(): void {
    if (!this.productId) return;

    this.loading = true;
    this.dealerService.getProductById(this.productId).subscribe({
      next: (response) => {
        this.loading = false;
        console.log('Product data loaded:', response);
        if (response) {
          // Extract unit value - could be 'UnitOfMeasure', 'unit', or 'Unit'
          const unitValue = response.UnitOfMeasure || response.unitOfMeasure || response.Unit || 'bags';
          const descValue = response.ProductDescription || response.productDescription || response.Description || '';
          
          this.productForm.patchValue({
            productName: response.ProductName || response.productName || '',
            product_LnCode: response.Product_LnCode || response.product_LnCode || '',
            description: descValue,
            unit: unitValue,
            isActive: response.IsActive !== undefined ? response.IsActive : (response.isActive !== undefined ? response.isActive : true)
          });
          
          console.log('Form values after patch:', this.productForm.value);
        }
      },
      error: (error) => {
        this.loading = false;
        console.error('Error loading product:', error);
        this.errorMessage = 'Failed to load product details';
      }
    });
  }

  initForm(): void {
    this.productForm = this.fb.group({
      productName: ['', [Validators.required, Validators.maxLength(200)]],
      product_LnCode: ['', [Validators.maxLength(100)]],
      description: ['', [Validators.maxLength(500)]],
      unit: ['BAG', Validators.required], // Default to 'BAG'
      isActive: [true]
    });
  }

  // Edit functionality disabled - new API structure focuses on create only

  saveProduct(): void {
    if (this.productForm.invalid) {
      // Mark all fields as touched to show validation errors
      Object.keys(this.productForm.controls).forEach(key => {
        this.productForm.get(key)?.markAsTouched();
      });
      return;
    }

    this.saving = true;
    this.errorMessage = '';
    this.successMessage = '';

    const formValue = this.productForm.value;

    if (this.isEditMode && this.productId) {
      // Update existing product
      const updateRequest: any = {
        productName: formValue.productName,
        product_LnCode: formValue.product_LnCode || undefined,
        description: formValue.description || undefined,
        unit: formValue.unit,
        isActive: formValue.isActive
      };

      this.dealerService.updateProduct(this.productId, updateRequest).subscribe({
        next: (response) => {
          this.saving = false;
          if (response.success) {
            this.successMessage = 'Product updated successfully!';
            setTimeout(() => this.goBack(), 2000);
          } else {
            this.errorMessage = response.message || 'Failed to update product';
          }
        },
        error: (error) => {
          this.saving = false;
          console.error('Update error:', error);
          this.errorMessage = error.error?.message || 'An error occurred while updating the product';
        }
      });
    } else {
      // Create new product
      const createRequest: CreateDealerProductRequest = {
        productName: formValue.productName,
        product_LnCode: formValue.product_LnCode || undefined,
        description: formValue.description || undefined,
        unit: formValue.unit
      };

      this.dealerService.createDealerProduct(createRequest).subscribe({
        next: (response) => {
          this.saving = false;
          if (response.success) {
            this.successMessage = 'Product created successfully!';
            setTimeout(() => this.goBack(), 2000);
          } else {
            this.errorMessage = response.message || 'Failed to create product';
          }
        },
        error: (error) => {
          this.saving = false;
          console.error('Create error:', error);
          this.errorMessage = error.error?.message || 'An error occurred while creating the product';
        }
      });
    }
  }

  goBack(): void {
    this.router.navigate(['/admin/dealer-management/products']);
  }
}