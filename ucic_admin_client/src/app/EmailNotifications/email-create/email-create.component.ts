import { Component, OnInit, CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { EmailService } from '../services/email.service';
import { SuccessSnackbarComponent } from '../../shared/components/success-snackbar/success-snackbar.component';
import { MatSnackBar } from '@angular/material/snack-bar';

@Component({
  selector: 'app-email-create',
  templateUrl: './email-create.component.html',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  schemas: [CUSTOM_ELEMENTS_SCHEMA]
})
export class EmailCreateComponent implements OnInit {
  productForm!: FormGroup;
  selectedFile: File | null = null;
  productFile: File | null = null;
  availableRoles: any[] = [];
  loading = false;
  errorMessage: string = '';
  departmentList: any[] = [];

  constructor(
    private fb: FormBuilder,
     private snackBar: MatSnackBar,
    private router: Router,
    private emailService: EmailService
  ) {
  }

  ngOnInit(): void {
    this.productForm = this.fb.group({
      name: ['', Validators.required],
      departmentId: [null, Validators.required],
      emailAddress: ['', Validators.required],
      phoneNo: ['']
    });
    this.emailService.getallDepartments().subscribe({
      next: (res : any) => {
        this.departmentList = res.data;
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
      formData.append('name', formValue.name);
      formData.append('emailAddress', formValue.emailAddress);
      formData.append('phoneNo', formValue.phoneNo ?? '');
      formData.append('departmentId', formValue.departmentId);
      // Resume file is mandatory
      this.emailService.createItem(formData).subscribe({
        next: (response) => {
          if (!response.success) {
            alert('An unexpected error occurred');
          } else {
            this.loading = false;
          this.snackBar.openFromComponent(SuccessSnackbarComponent, {
            data: {
              message: 'Email Recipient created successfully!'
            },
            duration: 5000,
            horizontalPosition: 'center',
            verticalPosition: 'top',
            panelClass: ['custom-snackbar']
          });
          this.productForm.reset();
          this.router.navigate(['/admin/email']);
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

  onCancel(): void {
    this.router.navigate(['/admin/email']);
  }
} 