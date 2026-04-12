import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule, AbstractControl, ValidationErrors } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';
import { CreateDriverRequest, DriverService } from '../../../services/driver.service';

@Component({
  selector: 'app-driver-create',
  standalone: true,
  imports: [CommonModule, RouterModule, ReactiveFormsModule, TranslateModule],
  template: `
    <div class="driver-create-container">
      <div class="page-header">
        <div class="header-content">
          <h1 class="page-title">
            <i class="material-icons">person_add</i>
            {{ 'DEALER.DRIVERS.CREATE.TITLE' | translate }}
          </h1>
          <p class="page-subtitle">{{ 'DEALER.DRIVERS.CREATE.SUBTITLE' | translate }}</p>
        </div>
        <div class="header-actions">
          <button 
            class="btn btn-secondary"
            routerLink="/dealer/drivers">
            <i class="material-icons">arrow_back</i>
            {{ 'DEALER.DRIVERS.CREATE.BACK_TO_DRIVERS' | translate }}
          </button>
        </div>
      </div>

      <div class="content-section">
        <div class="driver-form-card">
          <form [formGroup]="driverForm" (ngSubmit)="onSubmit()">
            
            <div class="form-section">
              <h3>{{ 'DEALER.DRIVERS.CREATE.USER_INFORMATION' | translate }}</h3>
              
              <div class="form-row">
                <div class="form-group">
                  <label for="fullName">{{ 'DEALER.DRIVERS.CREATE.FORM.FULL_NAME' | translate }} *</label>
                  <input 
                    type="text" 
                    id="fullName"
                    class="form-control"
                    formControlName="fullName"
                    [placeholder]="'DEALER.DRIVERS.CREATE.FORM.PLACEHOLDERS.FULL_NAME' | translate">
                  <div class="error-message" 
                       *ngIf="driverForm.get('fullName')?.invalid && driverForm.get('fullName')?.touched">
                    {{ 'DEALER.DRIVERS.CREATE.FORM.ERRORS.FULL_NAME_REQUIRED' | translate }}
                  </div>
                </div>

                <div class="form-group">
                  <label for="userName">{{ 'DEALER.DRIVERS.CREATE.FORM.USERNAME' | translate }} *</label>
                  <input 
                    type="text" 
                    id="userName"
                    class="form-control"
                    formControlName="userName"
                    [placeholder]="'DEALER.DRIVERS.CREATE.FORM.PLACEHOLDERS.USERNAME' | translate">
                  <div class="error-message" 
                       *ngIf="driverForm.get('userName')?.invalid && driverForm.get('userName')?.touched">
                    {{ 'DEALER.DRIVERS.CREATE.FORM.ERRORS.USERNAME_REQUIRED' | translate }}
                  </div>
                </div>
              </div>

              <div class="form-row">
                <div class="form-group">
                  <label for="email">{{ 'DEALER.DRIVERS.CREATE.FORM.EMAIL' | translate }} *</label>
                  <input 
                    type="email" 
                    id="email"
                    class="form-control"
                    formControlName="email"
                    [placeholder]="'DEALER.DRIVERS.CREATE.FORM.PLACEHOLDERS.EMAIL' | translate">
                  <div class="error-message" 
                       *ngIf="driverForm.get('email')?.invalid && driverForm.get('email')?.touched">
                    <span *ngIf="driverForm.get('email')?.errors?.['required']">{{ 'DEALER.DRIVERS.CREATE.FORM.ERRORS.EMAIL_REQUIRED' | translate }}</span>
                    <span *ngIf="driverForm.get('email')?.errors?.['email']">{{ 'DEALER.DRIVERS.CREATE.FORM.ERRORS.EMAIL_INVALID' | translate }}</span>
                  </div>
                </div>

                <div class="form-group">
                  <label for="phone">{{ 'DEALER.DRIVERS.CREATE.FORM.PHONE_NUMBER' | translate }} *</label>
                  <input 
                    type="tel" 
                    id="phone"
                    class="form-control"
                    formControlName="phone"
                    [placeholder]="'DEALER.DRIVERS.CREATE.FORM.PLACEHOLDERS.PHONE_NUMBER' | translate">
                  <div class="error-message" 
                       *ngIf="driverForm.get('phone')?.invalid && driverForm.get('phone')?.touched">
                    {{ 'DEALER.DRIVERS.CREATE.FORM.ERRORS.PHONE_REQUIRED' | translate }}
                  </div>
                </div>
              </div>

              <div class="form-row">
                <div class="form-group">
                  <label for="password">{{ 'DEALER.DRIVERS.CREATE.FORM.PASSWORD' | translate }} *</label>
                  <input 
                    type="password" 
                    id="password"
                    class="form-control"
                    formControlName="password"
                    [placeholder]="'DEALER.DRIVERS.CREATE.FORM.PLACEHOLDERS.PASSWORD' | translate">
                  <div class="error-message" 
                       *ngIf="driverForm.get('password')?.invalid && driverForm.get('password')?.touched">
                    <span *ngIf="driverForm.get('password')?.errors?.['required']">{{ 'DEALER.DRIVERS.CREATE.FORM.ERRORS.PASSWORD_REQUIRED' | translate }}</span>
                    <span *ngIf="driverForm.get('password')?.errors?.['minlength']">{{ 'DEALER.DRIVERS.CREATE.FORM.ERRORS.PASSWORD_MIN_LENGTH' | translate }}</span>
                  </div>
                </div>

                <div class="form-group">
                  <label for="confirmationPassword">{{ 'DEALER.DRIVERS.CREATE.FORM.CONFIRM_PASSWORD' | translate }} *</label>
                  <input 
                    type="password" 
                    id="confirmationPassword"
                    class="form-control"
                    formControlName="confirmationPassword"
                    [placeholder]="'DEALER.DRIVERS.CREATE.FORM.PLACEHOLDERS.CONFIRM_PASSWORD' | translate">
                  <div class="error-message" 
                       *ngIf="driverForm.get('confirmationPassword')?.invalid && driverForm.get('confirmationPassword')?.touched">
                    <span *ngIf="driverForm.get('confirmationPassword')?.errors?.['required']">{{ 'DEALER.DRIVERS.CREATE.FORM.ERRORS.CONFIRM_PASSWORD_REQUIRED' | translate }}</span>
                    <span *ngIf="driverForm.get('confirmationPassword')?.errors?.['mismatch']">{{ 'DEALER.DRIVERS.CREATE.FORM.ERRORS.PASSWORD_MISMATCH' | translate }}</span>
                  </div>
                </div>
              </div>
            </div>

            <div class="form-section">
              <h3>{{ 'DEALER.DRIVERS.CREATE.DRIVER_INFORMATION' | translate }}</h3>
              
              <div class="form-row">
                <div class="form-group">
                  <label for="iqamaNumber">{{ 'DEALER.DRIVERS.CREATE.FORM.IQAMA_NUMBER' | translate }}</label>
                  <input 
                    type="text" 
                    id="iqamaNumber"
                    class="form-control"
                    formControlName="iqamaNumber"
                    [placeholder]="'DEALER.DRIVERS.CREATE.FORM.PLACEHOLDERS.IQAMA_NUMBER' | translate">
                </div>

                <div class="form-group">
                  <label for="ln_ID">{{ 'DEALER.DRIVERS.CREATE.FORM.LICENSE_NUMBER' | translate }}</label>
                  <input 
                    type="text" 
                    id="ln_ID"
                    class="form-control"
                    formControlName="ln_ID"
                    [placeholder]="'DEALER.DRIVERS.CREATE.FORM.PLACEHOLDERS.LICENSE_NUMBER' | translate">
                </div>
              </div>
            </div>

            <div class="form-actions">
              <button 
                type="button" 
                class="btn btn-secondary"
                routerLink="/dealer/drivers">
                {{ 'DEALER.DRIVERS.CREATE.FORM.CANCEL' | translate }}
              </button>
              <button 
                type="submit" 
                class="btn btn-primary"
                [disabled]="driverForm.invalid || isSubmitting">
                <span *ngIf="isSubmitting" class="spinner-sm"></span>
                <i class="material-icons" *ngIf="!isSubmitting">save</i>
                {{ isSubmitting ? ('DEALER.DRIVERS.CREATE.FORM.ADDING' | translate) : ('DEALER.DRIVERS.CREATE.FORM.ADD_DRIVER' | translate) }}
              </button>
            </div>
          </form>
        </div>
      </div>

      <!-- Error/Success Messages -->
      <div class="alert alert-danger" *ngIf="errorMessage">
        {{ errorMessage }}
      </div>
      <div class="alert alert-success" *ngIf="successMessage">
        {{ successMessage }}
      </div>
    </div>
  `,
  styleUrls: ['./driver-create.component.scss']
})
export class DriverCreateComponent implements OnInit {
  driverForm: FormGroup;
  isSubmitting = false;
  errorMessage = '';
  successMessage = '';

  constructor(
    private fb: FormBuilder,
    private driverService: DriverService,
    private router: Router
  ) {
    this.driverForm = this.fb.group({
      fullName: ['', [Validators.required]],
      userName: ['', [Validators.required]],
      email: ['', [Validators.required, Validators.email]],
      phone: ['', [Validators.required]],
      password: ['', [Validators.required, Validators.minLength(6)]],
      confirmationPassword: ['', [Validators.required]],
      iqamaNumber: [''],
      ln_ID: ['']
    }, { validators: this.passwordMatchValidator });
  }

  // Custom validator to check if passwords match
  passwordMatchValidator(form: AbstractControl): ValidationErrors | null {
    const password = form.get('password');
    const confirmPassword = form.get('confirmationPassword');
    
    if (password && confirmPassword && password.value !== confirmPassword.value) {
      confirmPassword.setErrors({ mismatch: true });
      return { mismatch: true };
    } else {
      if (confirmPassword?.errors?.['mismatch']) {
        delete confirmPassword.errors['mismatch'];
        if (Object.keys(confirmPassword.errors).length === 0) {
          confirmPassword.setErrors(null);
        }
      }
    }
    return null;
  }

  ngOnInit(): void {
    // No need to load dealer info as it will be handled by the backend
  }

  onSubmit(): void {
    if (this.driverForm.valid) {
      this.isSubmitting = true;
      this.errorMessage = '';
      this.successMessage = '';

      const driverData: CreateDriverRequest = {
        fullName: this.driverForm.value.fullName,
        userName: this.driverForm.value.userName,
        email: this.driverForm.value.email,
        phone: this.driverForm.value.phone,
        password: this.driverForm.value.password,
        confirmationPassword: this.driverForm.value.confirmationPassword,
        ln_ID: this.driverForm.value.ln_ID,
        iqamaNumber: this.driverForm.value.iqamaNumber
      };

      this.driverService.createDriver(driverData).subscribe({
        next: (result) => {
          this.isSubmitting = false;
          this.successMessage = 'DEALER.DRIVERS.CREATE.SUCCESS_MESSAGE';
          
          // Navigate back to drivers list after a short delay
          setTimeout(() => {
            this.router.navigate(['/dealer/drivers']);
          }, 1500);
        },
        error: (error) => {
          this.isSubmitting = false;
          this.errorMessage = error.error?.message || 'DEALER.DRIVERS.CREATE.ERROR_MESSAGE';
        }
      });
    } else {
      // Mark all fields as touched to show validation errors
      Object.keys(this.driverForm.controls).forEach(key => {
        this.driverForm.get(key)?.markAsTouched();
      });
    }
  }
}