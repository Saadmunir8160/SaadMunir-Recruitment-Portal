import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router, ActivatedRoute } from '@angular/router';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';
import { UpdateDriverRequest, DriverService, Driver } from '../../../services/driver.service';

@Component({
  selector: 'app-driver-edit',
  standalone: true,
  imports: [CommonModule, RouterModule, ReactiveFormsModule, TranslateModule],
  template: `
    <div class="driver-edit-container">
      <div class="page-header">
        <div class="header-content">
          <h1 class="page-title">
            <i class="material-icons">edit</i>
            {{ 'DEALER.DRIVERS.EDIT.TITLE' | translate }}
          </h1>
          <p class="page-subtitle">{{ 'DEALER.DRIVERS.EDIT.SUBTITLE' | translate }}</p>
        </div>
        <div class="header-actions">
          <button 
            class="btn btn-secondary"
            routerLink="/dealer/drivers">
            <i class="material-icons">arrow_back</i>
            {{ 'DEALER.DRIVERS.EDIT.BACK_TO_DRIVERS' | translate }}
          </button>
        </div>
      </div>

      <div class="content-section" *ngIf="!loading">
        <div class="driver-form-card">
          <form [formGroup]="driverForm" (ngSubmit)="onSubmit()">
            
            <div class="form-section">
              <h3>{{ 'DEALER.DRIVERS.EDIT.DRIVER_INFORMATION' | translate }}</h3>
              
              <div class="form-row">
                <div class="form-group">
                  <label for="fullName">{{ 'DEALER.DRIVERS.EDIT.FORM.FULL_NAME' | translate }} *</label>
                  <input 
                    type="text" 
                    id="fullName"
                    class="form-control"
                    formControlName="fullName"
                    [placeholder]="'DEALER.DRIVERS.EDIT.FORM.PLACEHOLDERS.FULL_NAME' | translate">
                  <div *ngIf="driverForm.get('fullName')?.invalid && driverForm.get('fullName')?.touched" class="form-error">
                    {{ 'DEALER.DRIVERS.EDIT.FORM.ERRORS.FULL_NAME_REQUIRED' | translate }}
                  </div>
                </div>

                <div class="form-group">
                  <label for="email">{{ 'DEALER.DRIVERS.EDIT.FORM.EMAIL' | translate }} *</label>
                  <input 
                    type="email" 
                    id="email"
                    class="form-control"
                    formControlName="email"
                    [placeholder]="'DEALER.DRIVERS.EDIT.FORM.PLACEHOLDERS.EMAIL' | translate">
                  <div *ngIf="driverForm.get('email')?.invalid && driverForm.get('email')?.touched" class="form-error">
                    <div *ngIf="driverForm.get('email')?.errors?.['required']">{{ 'DEALER.DRIVERS.EDIT.FORM.ERRORS.EMAIL_REQUIRED' | translate }}</div>
                    <div *ngIf="driverForm.get('email')?.errors?.['email']">{{ 'DEALER.DRIVERS.EDIT.FORM.ERRORS.EMAIL_INVALID' | translate }}</div>
                  </div>
                </div>
              </div>

              <div class="form-row">
                <div class="form-group">
                  <label for="phoneNumber">{{ 'DEALER.DRIVERS.EDIT.FORM.PHONE_NUMBER' | translate }} *</label>
                  <input 
                    type="text" 
                    id="phoneNumber"
                    class="form-control"
                    formControlName="phoneNumber"
                    [placeholder]="'DEALER.DRIVERS.EDIT.FORM.PLACEHOLDERS.PHONE_NUMBER' | translate">
                  <div *ngIf="driverForm.get('phoneNumber')?.invalid && driverForm.get('phoneNumber')?.touched" class="form-error">
                    {{ 'DEALER.DRIVERS.EDIT.FORM.ERRORS.PHONE_REQUIRED' | translate }}
                  </div>
                </div>

                <div class="form-group">
                  <label for="iqamaNumber">{{ 'DEALER.DRIVERS.EDIT.FORM.IQAMA_NUMBER' | translate }}</label>
                  <input 
                    type="text" 
                    id="iqamaNumber"
                    class="form-control"
                    formControlName="iqamaNumber"
                    [placeholder]="'DEALER.DRIVERS.EDIT.FORM.PLACEHOLDERS.IQAMA_NUMBER' | translate">
                </div>

                <div class="form-group">
                  <label for="ln_ID">{{ 'DEALER.DRIVERS.EDIT.FORM.LICENSE_NUMBER' | translate }}</label>
                  <input 
                    type="text" 
                    id="ln_ID"
                    class="form-control"
                    formControlName="ln_ID"
                    [placeholder]="'DEALER.DRIVERS.EDIT.FORM.PLACEHOLDERS.LICENSE_NUMBER' | translate">
                </div>
              </div>

              <div class="form-row">
                <div class="form-group">
                  <label for="isActive">{{ 'DEALER.DRIVERS.EDIT.FORM.STATUS' | translate }}</label>
                  <select 
                    id="isActive"
                    class="form-control"
                    formControlName="isActive">
                    <option [ngValue]="true">{{ 'DEALER.DRIVERS.EDIT.FORM.STATUS_OPTIONS.ACTIVE' | translate }}</option>
                    <option [ngValue]="false">{{ 'DEALER.DRIVERS.EDIT.FORM.STATUS_OPTIONS.INACTIVE' | translate }}</option>
                  </select>
                </div>
              </div>
            </div>

            <div class="form-actions">
              <button 
                type="button" 
                class="btn btn-secondary"
                routerLink="/dealer/drivers">
                {{ 'DEALER.DRIVERS.EDIT.FORM.CANCEL' | translate }}
              </button>
              <button 
                type="submit" 
                class="btn btn-primary"
                [disabled]="driverForm.invalid || isSubmitting">
                <span *ngIf="isSubmitting" class="spinner-sm"></span>
                <i class="material-icons" *ngIf="!isSubmitting">save</i>
                {{ isSubmitting ? ('DEALER.DRIVERS.EDIT.FORM.UPDATING' | translate) : ('DEALER.DRIVERS.EDIT.FORM.UPDATE_DRIVER' | translate) }}
              </button>
            </div>
          </form>
        </div>
      </div>

      <!-- Loading State -->
      <div class="loading-state" *ngIf="loading">
        <div class="spinner"></div>
        <p>{{ 'DEALER.DRIVERS.EDIT.LOADING.MESSAGE' | translate }}</p>
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
  styleUrls: ['./driver-edit.component.scss']
})
export class DriverEditComponent implements OnInit {
  driverForm: FormGroup;
  isSubmitting = false;
  loading = true;
  errorMessage = '';
  successMessage = '';
  driverId: number = 0;
  currentDriver: Driver | null = null;

  constructor(
    private fb: FormBuilder,
    private driverService: DriverService,
    private router: Router,
    private route: ActivatedRoute
  ) {
    this.driverForm = this.fb.group({
      fullName: ['', [Validators.required]],
      email: ['', [Validators.required, Validators.email]],
      phoneNumber: ['', [Validators.required]],
      iqamaNumber: [''],
      ln_ID: [''],
      isActive: [true, [Validators.required]]
    });
  }

  ngOnInit(): void {
    this.route.params.subscribe(params => {
      this.driverId = parseInt(params['id']);
      if (this.driverId && !isNaN(this.driverId)) {
        this.loadDriver();
      } else {
        this.errorMessage = 'Invalid driver ID provided.';
        this.loading = false;
        setTimeout(() => {
          this.router.navigate(['/dealer/drivers']);
        }, 2000);
      }
    });
  }

  private loadDriver(): void {
    this.loading = true;
    this.errorMessage = '';
    
    this.driverService.getDriver(this.driverId).subscribe({
      next: (driver) => {
        this.currentDriver = driver;
        this.driverForm.patchValue({
          fullName: driver.fullName || driver.userName || driver.name || '',
          email: driver.email || '',
          phoneNumber: driver.phoneNumber || driver.phone || '',
          iqamaNumber: driver.iqamaNumber || '',
          ln_ID: driver.ln_ID || '',
          isActive: driver.isActive
        });
        this.loading = false;
      },
      error: (error) => {
        if (error.status === 404) {
          this.errorMessage = 'Driver not found. Please check the driver ID.';
        } else if (error.status === 403) {
          this.errorMessage = 'You do not have permission to view this driver.';
        } else {
          this.errorMessage = 'Failed to load driver information. Please try again.';
        }
        this.loading = false;
      }
    });
  }

  onSubmit(): void {
    if (this.driverForm.valid && this.currentDriver) {
      this.isSubmitting = true;
      this.errorMessage = '';
      this.successMessage = '';

      const formValue = this.driverForm.value;
      const driverData: UpdateDriverRequest = {
        driverId: this.driverId,
        userId: this.currentDriver.userId, // Use the userId from the loaded driver data
        dealerID: this.currentDriver.dealerID,
        fullName: formValue.fullName,
        email: formValue.email,
        phoneNumber: formValue.phoneNumber,
        ln_ID: formValue.ln_ID || null,
        iqamaNumber: formValue.iqamaNumber || null,
        isActive: formValue.isActive === true || formValue.isActive === 'true' // Ensure boolean
      };


      this.driverService.updateDriver(driverData).subscribe({
        next: (result) => {
          this.isSubmitting = false;
          this.successMessage = 'Driver updated successfully!';
          
          // Navigate back to drivers list after a short delay
          setTimeout(() => {
            this.router.navigate(['/dealer/drivers']);
          }, 1500);
        },
        error: (error) => {
          this.isSubmitting = false;
          
          if (error.status === 400) {
            this.errorMessage = error.error?.message || 'Invalid data provided. Please check your input.';
          } else if (error.status === 403) {
            this.errorMessage = 'You do not have permission to update this driver.';
          } else if (error.status === 404) {
            this.errorMessage = 'Driver not found. It may have been deleted.';
          } else {
            this.errorMessage = error.error?.message || 'Failed to update driver. Please try again.';
          }
        }
      });
    } else {
      // Mark all fields as touched to show validation errors
      Object.keys(this.driverForm.controls).forEach(key => {
        this.driverForm.get(key)?.markAsTouched();
      });
      
      if (!this.currentDriver) {
        this.errorMessage = 'Driver data not loaded. Please refresh the page and try again.';
      }
    }
  }
}