import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router, ActivatedRoute } from '@angular/router';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';
import { VehicleService, UpdateVehicleRequest, Vehicle } from '../../../services/vehicle.service';

@Component({
  selector: 'app-vehicle-edit',
  standalone: true,
  imports: [CommonModule, RouterModule, ReactiveFormsModule, TranslateModule],
  template: `
    <div class="vehicle-edit-container">
      <div class="page-header">
        <div class="header-content">
          <h1 class="page-title">
            <i class="material-icons">edit</i>
            {{ 'DEALER.VEHICLES.EDIT.TITLE' | translate }}
          </h1>
          <p class="page-subtitle" *ngIf="currentVehicle">
            {{ 'DEALER.VEHICLES.EDIT.SUBTITLE' | translate }}: {{ currentVehicle.plateNumber }}
          </p>
        </div>
        <div class="header-actions">
          <button 
            class="btn btn-secondary"
            routerLink="/dealer/vehicles">
            <i class="material-icons">arrow_back</i>
            {{ 'DEALER.VEHICLES.EDIT.BACK_TO_VEHICLES' | translate }}
          </button>
        </div>
      </div>

      <div *ngIf="loading" class="loading-state">
        <div class="spinner"></div>
        <p>{{ 'DEALER.VEHICLES.EDIT.LOADING.MESSAGE' | translate }}</p>
      </div>

      <div *ngIf="!loading && !currentVehicle" class="error-state">
        <i class="material-icons">error</i>
        <h3>{{ 'DEALER.VEHICLES.EMPTY_STATE.NO_VEHICLES_TITLE' | translate }}</h3>
        <p>{{ 'DEALER.VEHICLES.EMPTY_STATE.NO_VEHICLES_MESSAGE' | translate }}</p>
        <button class="btn btn-primary" routerLink="/dealer/vehicles">
          {{ 'DEALER.VEHICLES.EDIT.BACK_TO_VEHICLES' | translate }}
        </button>
      </div>

      <div class="form-container" *ngIf="!loading && currentVehicle">
        <form [formGroup]="vehicleForm" (ngSubmit)="onSubmit()">
          <div class="form-grid">
            <!-- Basic Information -->
            <div class="form-section">
              <h3>Basic Information</h3>
              
              <div class="form-group">
                <label for="plateNumber">Plate Number *</label>
                <input 
                  type="text" 
                  id="plateNumber"
                  formControlName="plateNumber"
                  class="form-control"
                  placeholder="e.g., ABC-123">
                <div class="error-message" *ngIf="vehicleForm.get('plateNumber')?.invalid && vehicleForm.get('plateNumber')?.touched">
                  <small *ngIf="vehicleForm.get('plateNumber')?.errors?.['required']">Plate number is required</small>
                  <small *ngIf="vehicleForm.get('plateNumber')?.errors?.['maxlength']">Plate number cannot exceed 50 characters</small>
                </div>
              </div>

              <div class="form-group">
                <label for="type">Vehicle Type *</label>
                <select 
                  id="type"
                  formControlName="type"
                  class="form-control">
                  <option value="">Select vehicle type</option>
                  <option value="truck">Truck</option>
                  <option value="van">Van</option>
                  <option value="pickup">Pickup</option>
                  <option value="trailer">Trailer</option>
                </select>
                <div class="error-message" *ngIf="vehicleForm.get('type')?.invalid && vehicleForm.get('type')?.touched">
                  <small *ngIf="vehicleForm.get('type')?.errors?.['required']">Vehicle type is required</small>
                </div>
              </div>

              <div class="form-group">
                <label for="capacity">Capacity (tons) *</label>
                <input 
                  type="number" 
                  id="capacity"
                  formControlName="capacity"
                  class="form-control"
                  placeholder="e.g., 5.5"
                  step="0.1"
                  min="0.1"
                  max="100">
                <div class="error-message" *ngIf="vehicleForm.get('capacity')?.invalid && vehicleForm.get('capacity')?.touched">
                  <small *ngIf="vehicleForm.get('capacity')?.errors?.['required']">Capacity is required</small>
                  <small *ngIf="vehicleForm.get('capacity')?.errors?.['min']">Capacity must be at least 0.1 tons</small>
                  <small *ngIf="vehicleForm.get('capacity')?.errors?.['max']">Capacity cannot exceed 100 tons</small>
                </div>
              </div>
            </div>

            <!-- Dates -->
            <div class="form-section">
              <h3>Important Dates</h3>
              
              <div class="form-group">
                <label for="registrationDate">Registration Date *</label>
                <input 
                  type="date" 
                  id="registrationDate"
                  formControlName="registrationDate"
                  class="form-control">
                <div class="error-message" *ngIf="vehicleForm.get('registrationDate')?.invalid && vehicleForm.get('registrationDate')?.touched">
                  <small *ngIf="vehicleForm.get('registrationDate')?.errors?.['required']">Registration date is required</small>
                </div>
              </div>

              <div class="form-group">
                <label for="registrationExpiryDate">Registration Expiry Date *</label>
                <input 
                  type="date" 
                  id="registrationExpiryDate"
                  formControlName="registrationExpiryDate"
                  class="form-control">
                <div class="error-message" *ngIf="vehicleForm.get('registrationExpiryDate')?.invalid && vehicleForm.get('registrationExpiryDate')?.touched">
                  <small *ngIf="vehicleForm.get('registrationExpiryDate')?.errors?.['required']">Registration expiry date is required</small>
                </div>
              </div>

              <div class="form-group">
                <label for="insuranceExpiryDate">Insurance Expiry Date *</label>
                <input 
                  type="date" 
                  id="insuranceExpiryDate"
                  formControlName="insuranceExpiryDate"
                  class="form-control">
                <div class="error-message" *ngIf="vehicleForm.get('insuranceExpiryDate')?.invalid && vehicleForm.get('insuranceExpiryDate')?.touched">
                  <small *ngIf="vehicleForm.get('insuranceExpiryDate')?.errors?.['required']">Insurance expiry date is required</small>
                </div>
              </div>
            </div>

            <!-- Optional Information -->
            <div class="form-section">
              <h3>Additional Information</h3>
              
              <div class="form-group">
                <label for="ln_ID">License ID</label>
                <input 
                  type="text" 
                  id="ln_ID"
                  formControlName="ln_ID"
                  class="form-control"
                  placeholder="Optional license identifier">
                <div class="error-message" *ngIf="vehicleForm.get('ln_ID')?.invalid && vehicleForm.get('ln_ID')?.touched">
                  <small *ngIf="vehicleForm.get('ln_ID')?.errors?.['maxlength']">License ID cannot exceed 50 characters</small>
                </div>
              </div>

              <div class="form-group">
                <label for="registrationNumber">Registration Number</label>
                <input 
                  type="text" 
                  id="registrationNumber"
                  formControlName="registrationNumber"
                  class="form-control"
                  placeholder="Optional registration number">
                <div class="error-message" *ngIf="vehicleForm.get('registrationNumber')?.invalid && vehicleForm.get('registrationNumber')?.touched">
                  <small *ngIf="vehicleForm.get('registrationNumber')?.errors?.['maxlength']">Registration number cannot exceed 50 characters</small>
                </div>
              </div>

              <div class="form-group">
                <label class="checkbox-label">
                  <input 
                    type="checkbox" 
                    formControlName="isActive">
                  <span class="checkmark"></span>
                  Vehicle is active
                </label>
              </div>
            </div>
          </div>

          <div class="form-actions">
            <button 
              type="button" 
              class="btn btn-secondary"
              routerLink="/dealer/vehicles">
              Cancel
            </button>
            <button 
              type="submit" 
              class="btn btn-primary"
              [disabled]="vehicleForm.invalid || submitting">
              <span *ngIf="submitting" class="spinner-sm"></span>
              <i class="material-icons" *ngIf="!submitting">save</i>
              {{ submitting ? 'Updating...' : 'Update Vehicle' }}
            </button>
          </div>
        </form>
      </div>
    </div>
  `,
  styleUrls: ['./vehicle-edit.component.scss']
})
export class VehicleEditComponent implements OnInit {
  vehicleForm: FormGroup;
  currentVehicle: Vehicle | null = null;
  loading = true;
  submitting = false;
  vehicleId: number;

  constructor(
    private fb: FormBuilder,
    private vehicleService: VehicleService,
    private router: Router,
    private route: ActivatedRoute
  ) {
    this.vehicleForm = this.createForm();
    this.vehicleId = +this.route.snapshot.params['id'];
  }

  ngOnInit(): void {
    this.loadVehicle();
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

  private loadVehicle(): void {
    this.loading = true;
    
    this.vehicleService.getVehicle(this.vehicleId).subscribe({
      next: (vehicle) => {
        this.currentVehicle = vehicle;
        this.populateForm(vehicle);
        this.loading = false;
      },
      error: (error) => {
        this.loading = false;
      }
    });
  }

  private populateForm(vehicle: Vehicle): void {
    const registrationDate = new Date(vehicle.registrationDate).toISOString().split('T')[0];
    const registrationExpiryDate = new Date(vehicle.registrationExpiryDate).toISOString().split('T')[0];
    const insuranceExpiryDate = new Date(vehicle.insuranceExpiryDate).toISOString().split('T')[0];

    this.vehicleForm.patchValue({
      plateNumber: vehicle.plateNumber,
      type: vehicle.type,
      capacity: vehicle.capacity,
      registrationDate: registrationDate,
      registrationExpiryDate: registrationExpiryDate,
      insuranceExpiryDate: insuranceExpiryDate,
      ln_ID: vehicle.ln_ID || '',
      registrationNumber: vehicle.registrationNumber || '',
      isActive: vehicle.isActive
    });
  }

  onSubmit(): void {
    if (this.vehicleForm.valid && this.currentVehicle) {
      this.submitting = true;
      
      const formValue = this.vehicleForm.value;
      const updateRequest: UpdateVehicleRequest = {
        vehicleID: this.currentVehicle.vehicleID,
        dealerID: this.currentVehicle.dealerID,
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

      this.vehicleService.updateVehicle(updateRequest).subscribe({
        next: (success) => {
          if (success) {
            this.router.navigate(['/dealer/vehicles']);
          }
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