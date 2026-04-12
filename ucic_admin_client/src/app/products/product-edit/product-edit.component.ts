import { Component, OnInit, CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { UserService, UpdateUserDTO } from '../../services/user.service';
import { RoleService } from '../../services/role.service';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { ProductsService } from '../services/products.service';
import { CoverageAreaService } from '../../services/coverage-area.service';
import { MatSnackBar } from '@angular/material/snack-bar';
import { SuccessSnackbarComponent } from '../../shared/components/success-snackbar/success-snackbar.component';
import { environment } from '../../../environments/environment';

@Component({
  selector: 'app-product-edit',
  templateUrl: './product-edit.component.html',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  schemas: [CUSTOM_ELEMENTS_SCHEMA]
})
export class ProductEditComponent implements OnInit {
  productForm!: FormGroup;
  selectedFile: File | null = null;
  loading = false;
  productId!: number;
  coverageAreasList: any[] = [];
  existingImageUrl: string | null = null;

  constructor(
    private fb: FormBuilder,
    private productService: ProductsService,
    private coverageAreaService: CoverageAreaService,
    private route: ActivatedRoute,
    private router: Router,
    private snackBar: MatSnackBar
  ) { }

  ngOnInit(): void {
    this.productForm = this.fb.group({
      name: ['', Validators.required],
      coverageAreaId: [null, Validators.required],
      price: [null, Validators.required],
      vatPercentage: [null, Validators.required],
      discountPercentage: [null, Validators.required],
      shipingCostPercentage: [null, Validators.required],
      productCode: ['', Validators.required],
      sku: ['', Validators.required],
      type: ['', Validators.required],
      description: ['', Validators.required],
      arabicName: [''],
      arabicDescription: [''],
      productFilePath: [null]
    });

    this.productId = +this.route.snapshot.paramMap.get('id')!;
    this.loadCoverageAreas();
    this.loadProductData();
  }
    loadCoverageAreas() {
    this.coverageAreaService.getCoverageAreas().subscribe({
      next: (res: any) => {
        this.coverageAreasList = res.data;
      },
      error: (err) => console.error(err)
    });
  }

    loadProductData() {
    this.productService.getProductById(this.productId).subscribe({
      next: (res: any) => {
        const data = res;
        this.productForm.patchValue({
 name: data.name,
  coverageAreaId: data.coverageAreaId,
  price: parseFloat(data.price),                      // ensure it's a number
  vatPercentage: parseFloat(data.vatPercentage),
  discountPercentage: parseFloat(data.discountPercentage),
  shipingCostPercentage: parseFloat(data.shipingCostPercentage),
  productCode: data.productCode,
  sku: data.sku,
  type: data.type,
  description: data.description,
  arabicName: data.arabicName,
  arabicDescription: data.arabicDescription
});
this.existingImageUrl = this.getFullImageUrl(data.productFilePath);

this.productForm.updateValueAndValidity();
        // Set a dummy file value for validation bypass if needed
        this.productForm.get('productFilePath')?.setValidators([]);
      },
      error: (err) => console.error('Error fetching product:', err)
    });
  }
getFullImageUrl(relativePath: string): string {
  // Adjust this based on your actual hosting setup
  //return `${window.location.origin}/${relativePath}`;
    return `${environment.apiUrl.replace('/api', '')}/static/Product/${relativePath}`
}
  onSubmit(): void {
    this.loading = true;

    if (this.productForm.valid) {
      const formValue = this.productForm.value;
      const formData = new FormData();

      Object.keys(formValue).forEach(key => {
        if (key !== 'productFilePath') {
          formData.append(key, formValue[key]);
        }
      });

      if (this.selectedFile) {
        formData.append('productFilePath', this.selectedFile);
      }

      this.productService.updateProduct(this.productId, formData).subscribe({
        next: () => {
          this.snackBar.openFromComponent(SuccessSnackbarComponent, {
            data: { message: 'Product updated successfully!' },
            duration: 5000,
            horizontalPosition: 'center',
            verticalPosition: 'top',
            panelClass: ['custom-snackbar']
          });
          this.router.navigate(['/admin/products']);
        },
        error: (err) => {
          this.loading = false;
        }
      });
    }
  }

    onProductFileSelected(event: any) {
    const file = event.target.files[0];
    if (file) {
      const allowedTypes = ['image/png', 'image/jpeg', 'image/jpg', 'image/gif'];
      if (!allowedTypes.includes(file.type)) {
        alert('Invalid file type.');
        return;
      }
      if (file.size > 5 * 1024 * 1024) {
        alert('File size must be under 5MB.');
        return;
      }
      this.selectedFile = file;
      this.productForm.get('productFilePath')?.setValue(file);
    }
  }

  onCancel(): void {
    this.router.navigate(['/admin/products']);
  }
} 