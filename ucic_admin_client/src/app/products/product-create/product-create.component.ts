import { Component, OnInit, CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { CoverageAreaService } from '../../services/coverage-area.service';
import { ProductsService } from '../services/products.service';
import { SuccessSnackbarComponent } from '../../shared/components/success-snackbar/success-snackbar.component';
import { MatSnackBar } from '@angular/material/snack-bar';

@Component({
  selector: 'app-product-create',
  templateUrl: './product-create.component.html',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  schemas: [CUSTOM_ELEMENTS_SCHEMA]
})
export class ProductCreateComponent implements OnInit {
  productForm!: FormGroup;
  selectedFile: File | null = null;
  productFile: File | null = null;
  availableRoles: any[] = [];
  loading = false;
  errorMessage: string = '';
  coverageAreasList: any[] = [];

  constructor(
    private fb: FormBuilder,
    private productservice: ProductsService,
     private snackBar: MatSnackBar,
    private router: Router,
    private coverageAreaService: CoverageAreaService
  ) {
  }

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
      productFilePath: [null, [this.fileValidator()]], // ✅ Use custom validator
    });
    this.coverageAreaService.getCoverageAreas().subscribe({
      next: (res : any) => {
        this.coverageAreasList = res.data;
      },
      error: (err: any) => {
      }
    });
  }

  fileValidator() {
    return (control: any) => {
      return control.value ? null : { required: true };
    };
  }
  isFieldInvalid(fieldName: string): boolean {
    const field = this.productForm.get(fieldName);
    return !!field && field.invalid && (field.dirty || field.touched);
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
      // Resume file is mandatory
      this.productservice.createProduct(formData).subscribe({
        next: (response) => {
          if (!response.success) {
            alert('An unexpected error occurred');
          } else {
            this.loading = false;
          this.snackBar.openFromComponent(SuccessSnackbarComponent, {
            data: {
              message: 'Product created successfully!'
            },
            duration: 5000,
            horizontalPosition: 'center',
            verticalPosition: 'top',
            panelClass: ['custom-snackbar']
          });
          this.productForm.reset();
          this.router.navigate(['/admin/products']);
          }

        },
        error: (err) => {
          this.loading = false;
        },
      });
    } else {
      this.errorMessage = 'Please fill in all required fields correctly.';
    }
  }

  onProductFileSelected(event: any) {
    //this.fileTouched = true;
    const vatCertificateFilePath = event.target.files[0];

    if (vatCertificateFilePath) {
      // Check if the file type is PDF
      const allowedFileTypes = ['image/png', 'image/jpeg', 'image/jpg', 'image/gif'];
      if (!allowedFileTypes.includes(vatCertificateFilePath.type)) {
        alert('Invalid file type. Allowed: .PDF .png, .jpeg, .jpg, .gif.');
        event.target.value = ''; // Clear the input field
        return;
      }

      // Check if the file size is within the 5MB limit
      const maxSize = 5 * 1024 * 1024; // 5MB in bytes
      if (vatCertificateFilePath.size > maxSize) {
        alert('File size must be less than 5MB.') ;
        event.target.value = ''; // Clear the input field
        return;
      }

      this.selectedFile = vatCertificateFilePath;
      this.productForm.get('productFilePath')?.setValue(vatCertificateFilePath);
this.productForm.get('productFilePath')?.markAsTouched();
this.productForm.get('productFilePath')?.updateValueAndValidity();

    }
  }
  onCancel(): void {
    this.router.navigate(['/admin/products']);
  }
} 