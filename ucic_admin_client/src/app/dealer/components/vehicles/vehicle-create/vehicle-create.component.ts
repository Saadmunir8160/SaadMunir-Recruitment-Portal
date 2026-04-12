import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';
import { VehicleService, CreateVehicleRequest } from '../../../services/vehicle.service';

@Component({
  selector: 'app-vehicle-create',
  standalone: true,
  imports: [CommonModule, RouterModule, ReactiveFormsModule, TranslateModule],
  template: `
    <div>
      <div class="page-header">
        <div class="header-content">
          <h1 class="page-title">
            <i class="material-icons">add</i>
            {{ 'DEALER.VEHICLES.CREATE.TITLE' | translate }}
          </h1>
          <p class="page-subtitle">{{ 'DEALER.VEHICLES.CREATE.SUBTITLE' | translate }}</p>
        </div>
        <div class="header-actions">
          <button 
            class="btn btn-secondary"
            routerLink="/dealer/vehicles">
            <i class="material-icons">arrow_back</i>
            {{ 'DEALER.VEHICLES.CREATE.BACK_TO_VEHICLES' | translate }}
          </button>
        </div>
      </div>

      <div class="form-container">
        <form [formGroup]="vehicleForm" (ngSubmit)="onSubmit()">
          <div class="form-grid">
            <!-- Basic Information -->
            <div class="form-section">
              <h3>{{ 'DEALER.VEHICLES.CREATE.BASIC_INFORMATION' | translate }}</h3>
              
              <div class="form-group">
                <label for="plateNumber">{{ 'DEALER.VEHICLES.CREATE.FORM.PLATE_NUMBER' | translate }} *</label>
                <input 
                  type="text" 
                  id="plateNumber"
                  formControlName="plateNumber"
                  class="form-control"
                  [placeholder]="'DEALER.VEHICLES.CREATE.FORM.PLACEHOLDERS.PLATE_NUMBER' | translate">
                <div class="error-message" *ngIf="vehicleForm.get('plateNumber')?.invalid && vehicleForm.get('plateNumber')?.touched">
                  <small *ngIf="vehicleForm.get('plateNumber')?.errors?.['required']">{{ 'DEALER.VEHICLES.CREATE.FORM.ERRORS.PLATE_NUMBER_REQUIRED' | translate }}</small>
                  <small *ngIf="vehicleForm.get('plateNumber')?.errors?.['maxlength']">{{ 'DEALER.VEHICLES.CREATE.FORM.ERRORS.PLATE_NUMBER_MAX_LENGTH' | translate }}</small>
                </div>
              </div>

              <div class="form-group">
                <label for="type">{{ 'DEALER.VEHICLES.CREATE.FORM.VEHICLE_TYPE' | translate }} *</label>
                <select 
                  id="type"
                  formControlName="type"
                  class="form-control">
                  <option value="">{{ 'DEALER.VEHICLES.CREATE.FORM.SELECT_VEHICLE_TYPE' | translate }}</option>
                  <option value="truck">{{ 'DEALER.VEHICLES.CREATE.FORM.TYPES.TRUCK' | translate }}</option>
                  <option value="van">{{ 'DEALER.VEHICLES.CREATE.FORM.TYPES.VAN' | translate }}</option>
                  <option value="pickup">{{ 'DEALER.VEHICLES.CREATE.FORM.TYPES.PICKUP' | translate }}</option>
                  <option value="trailer">{{ 'DEALER.VEHICLES.CREATE.FORM.TYPES.TRAILER' | translate }}</option>
                </select>
                <div class="error-message" *ngIf="vehicleForm.get('type')?.invalid && vehicleForm.get('type')?.touched">
                  <small *ngIf="vehicleForm.get('type')?.errors?.['required']">{{ 'DEALER.VEHICLES.CREATE.FORM.ERRORS.VEHICLE_TYPE_REQUIRED' | translate }}</small>
                </div>
              </div>

              <div class="form-group">
                <label for="capacity">{{ 'DEALER.VEHICLES.CREATE.FORM.CAPACITY' | translate }} *</label>
                <input 
                  type="number" 
                  id="capacity"
                  formControlName="capacity"
                  class="form-control"
                  [placeholder]="'DEALER.VEHICLES.CREATE.FORM.PLACEHOLDERS.CAPACITY' | translate"
                  step="0.1"
                  min="0.1"
                  max="100">
                <div class="error-message" *ngIf="vehicleForm.get('capacity')?.invalid && vehicleForm.get('capacity')?.touched">
                  <small *ngIf="vehicleForm.get('capacity')?.errors?.['required']">{{ 'DEALER.VEHICLES.CREATE.FORM.ERRORS.CAPACITY_REQUIRED' | translate }}</small>
                  <small *ngIf="vehicleForm.get('capacity')?.errors?.['min']">{{ 'DEALER.VEHICLES.CREATE.FORM.ERRORS.CAPACITY_MIN' | translate }}</small>
                  <small *ngIf="vehicleForm.get('capacity')?.errors?.['max']">{{ 'DEALER.VEHICLES.CREATE.FORM.ERRORS.CAPACITY_MAX' | translate }}</small>
                </div>
              </div>
            </div>

            <!-- Dates -->
            <div class="form-section">
              <h3>{{ 'DEALER.VEHICLES.CREATE.IMPORTANT_DATES' | translate }}</h3>
              
              <div class="form-group">
                <label for="registrationDate">{{ 'DEALER.VEHICLES.CREATE.FORM.REGISTRATION_DATE' | translate }} *</label>
                <input 
                  type="date" 
                  id="registrationDate"
                  formControlName="registrationDate"
                  class="form-control">
                <div class="error-message" *ngIf="vehicleForm.get('registrationDate')?.invalid && vehicleForm.get('registrationDate')?.touched">
                  <small *ngIf="vehicleForm.get('registrationDate')?.errors?.['required']">{{ 'DEALER.VEHICLES.CREATE.FORM.ERRORS.REGISTRATION_DATE_REQUIRED' | translate }}</small>
                </div>
              </div>

              <div class="form-group">
                <label for="registrationExpiryDate">{{ 'DEALER.VEHICLES.CREATE.FORM.REGISTRATION_EXPIRY_DATE' | translate }} *</label>
                <input 
                  type="date" 
                  id="registrationExpiryDate"
                  formControlName="registrationExpiryDate"
                  class="form-control">
                <div class="error-message" *ngIf="vehicleForm.get('registrationExpiryDate')?.invalid && vehicleForm.get('registrationExpiryDate')?.touched">
                  <small *ngIf="vehicleForm.get('registrationExpiryDate')?.errors?.['required']">{{ 'DEALER.VEHICLES.CREATE.FORM.ERRORS.REGISTRATION_EXPIRY_DATE_REQUIRED' | translate }}</small>
                </div>
              </div>

              <div class="form-group">
                <label for="insuranceExpiryDate">{{ 'DEALER.VEHICLES.CREATE.FORM.INSURANCE_EXPIRY_DATE' | translate }} *</label>
                <input 
                  type="date" 
                  id="insuranceExpiryDate"
                  formControlName="insuranceExpiryDate"
                  class="form-control">
                <div class="error-message" *ngIf="vehicleForm.get('insuranceExpiryDate')?.invalid && vehicleForm.get('insuranceExpiryDate')?.touched">
                  <small *ngIf="vehicleForm.get('insuranceExpiryDate')?.errors?.['required']">{{ 'DEALER.VEHICLES.CREATE.FORM.ERRORS.INSURANCE_EXPIRY_DATE_REQUIRED' | translate }}</small>
                </div>
              </div>
            </div>

            <!-- Optional Information -->
            <div class="form-section">
              <h3>{{ 'DEALER.VEHICLES.CREATE.ADDITIONAL_INFORMATION' | translate }}</h3>
              
              <div class="form-group">
                <label for="ln_ID">{{ 'DEALER.VEHICLES.CREATE.FORM.LN_ID' | translate }}</label>
                <input 
                  type="text" 
                  id="ln_ID"
                  formControlName="ln_ID"
                  class="form-control"
                  [placeholder]="'DEALER.VEHICLES.CREATE.FORM.PLACEHOLDERS.LN_ID' | translate">
                <div class="error-message" *ngIf="vehicleForm.get('ln_ID')?.invalid && vehicleForm.get('ln_ID')?.touched">
                  <small *ngIf="vehicleForm.get('ln_ID')?.errors?.['maxlength']">{{ 'DEALER.VEHICLES.CREATE.FORM.ERRORS.LICENSE_ID_MAX_LENGTH' | translate }}</small>
                </div>
              </div>

              <div class="form-group">
                <label for="registrationNumber">{{ 'DEALER.VEHICLES.CREATE.FORM.REGISTRATION_NUMBER' | translate }}</label>
                <input 
                  type="text" 
                  id="registrationNumber"
                  formControlName="registrationNumber"
                  class="form-control"
                  [placeholder]="'DEALER.VEHICLES.CREATE.FORM.PLACEHOLDERS.REGISTRATION_NUMBER' | translate">
                <div class="error-message" *ngIf="vehicleForm.get('registrationNumber')?.invalid && vehicleForm.get('registrationNumber')?.touched">
                  <small *ngIf="vehicleForm.get('registrationNumber')?.errors?.['maxlength']">{{ 'DEALER.VEHICLES.CREATE.FORM.ERRORS.REGISTRATION_NUMBER_MAX_LENGTH' | translate }}</small>
                </div>
              </div>

              <div class="form-group">
                <label class="checkbox-label">
                  <input 
                    type="checkbox" 
                    formControlName="isActive">
                  <span class="checkmark"></span>
                  {{ 'DEALER.VEHICLES.CREATE.FORM.VEHICLE_IS_ACTIVE' | translate }}
                </label>
              </div>
            </div>
          </div>

          <div class="form-actions">
            <button 
              type="button" 
              class="btn btn-secondary"
              routerLink="/dealer/vehicles">
              {{ 'DEALER.VEHICLES.CREATE.FORM.CANCEL' | translate }}
            </button>
            <button 
              type="submit" 
              class="btn btn-primary"
              [disabled]="vehicleForm.invalid || submitting">
              <span *ngIf="submitting" class="spinner-sm"></span>
              <i class="material-icons" *ngIf="!submitting">save</i>
              {{ submitting ? ('DEALER.VEHICLES.CREATE.FORM.CREATING' | translate) : ('DEALER.VEHICLES.CREATE.FORM.CREATE_VEHICLE' | translate) }}
            </button>
          </div>
        </form>
      </div>
    </div>
  `,
  styleUrls: ['./vehicle-create.component.scss']
})
export class VehicleCreateComponent implements OnInit {
  vehicleForm: FormGroup;
  submitting = false;

  constructor(
    private fb: FormBuilder,
    private vehicleService: VehicleService,
    private router: Router
  ) {
    this.vehicleForm = this.createForm();
  }

  ngOnInit(): void {
    // Set default registration date to today
    this.vehicleForm.patchValue({
      registrationDate: new Date().toISOString().split('T')[0]
    });
  }

  private createForm(): FormGroup {
    return this.fb.group({
      plateNumber: ['', [Validators.required, Validators.maxLength(50)]],
      type: ['', [Validators.required, Validators.maxLength(20)]],
      capacity: ['', [Validators.required, Validators.min(0.1), Validators.max(100)]],
      registrationDate: ['', Validators.required],
      registrationExpiryDate: ['', Validators.required],
      insuranceExpiryDate: ['', Validators.required],
      ln_ID: ['', [Validators.maxLength(50)]],
      registrationNumber: ['', [Validators.maxLength(50)]],
      isActive: [true]
    });
  }

  onSubmit(): void {
    if (this.vehicleForm.valid) {
      this.submitting = true;
      
      const formValue = this.vehicleForm.value;
      const createRequest: CreateVehicleRequest = {
        dealerID: 0, // This will be set by the backend based on current user
        plateNumber: formValue.plateNumber,
        type: formValue.type,
        capacity: formValue.capacity,
        registrationDate: new Date(formValue.registrationDate),
        registrationExpiryDate: new Date(formValue.registrationExpiryDate),
        insuranceExpiryDate: new Date(formValue.insuranceExpiryDate),
        ln_ID: formValue.ln_ID || undefined,
        registrationNumber: formValue.registrationNumber || undefined,
        isActive: formValue.isActive
      };

      this.vehicleService.createVehicle(createRequest).subscribe({
        next: (vehicleId) => {
          this.router.navigate(['/dealer/vehicles']);
        },
        error: (error) => {
          this.submitting = false;
        }
      });
    } else {
      // Mark all fields as touched to show validation errors
      Object.keys(this.vehicleForm.controls).forEach(key => {
        this.vehicleForm.get(key)?.markAsTouched();
      });
    }
  }
}