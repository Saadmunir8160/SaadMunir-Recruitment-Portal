import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';
import { DealerProfileService, DealerProfile, DealerUserInfo, ApiResponse } from '../services/dealer-profile.service';

@Component({
  selector: 'app-dealer-profile',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, TranslateModule],
  template: `
    <div>
      <div class="page-header">
        <div class="header-content">
          <h1 class="page-title">
            <span class="material-icons">account_circle</span>
            {{ 'DEALER.PROFILE.TITLE' | translate }}
          </h1>
          <p class="page-subtitle">{{ 'DEALER.PROFILE.SUBTITLE' | translate }}</p>
        </div>
      </div>

      <!-- Profile Information -->
      <div class="profile-section" *ngIf="dealerProfile">
        <div class="section-header">
          <h3>
            <span class="material-icons">business</span>
            {{ 'DEALER.PROFILE.DEALER_INFORMATION' | translate }}
          </h3>
          <button class="btn-change-password" (click)="openChangePasswordDialog()">
            <span class="material-icons">lock</span>
            {{ 'DEALER.PROFILE.CHANGE_PASSWORD' | translate }}
          </button>
        </div>
        
        <div class="profile-grid">
          <!-- Dealer Basic Info -->
          <div class="info-card">
            <h4>{{ 'DEALER.PROFILE.BASIC_INFORMATION' | translate }}</h4>
            <div class="info-row">
              <span class="label">{{ 'DEALER.PROFILE.DEALER_NAME' | translate }}:</span>
              <span class="value">{{ dealerProfile.dealerName || ('DEALER.PROFILE.NOT_AVAILABLE' | translate) }}</span>
            </div>
            <div class="info-row">
              <span class="label">{{ 'DEALER.PROFILE.DEALER_CODE' | translate }}:</span>
              <span class="value">{{ dealerProfile.dealerCode || ('DEALER.PROFILE.NOT_AVAILABLE' | translate) }}</span>
            </div>
            <div class="info-row">
              <span class="label">{{ 'DEALER.PROFILE.LN_ID' | translate }}:</span>
              <span class="value">{{ dealerProfile.ln_ID || ('DEALER.PROFILE.NOT_AVAILABLE' | translate) }}</span>
            </div>
            <div class="info-row">
              <span class="label">{{ 'DEALER.PROFILE.STATUS' | translate }}:</span>
              <span class="value" [class.verified]="dealerProfile.isVerified">
                {{ dealerProfile.isVerified ? ('DEALER.PROFILE.VERIFIED' | translate) : ('DEALER.PROFILE.NOT_VERIFIED' | translate) }}
              </span>
            </div>
          </div>

          <!-- Financial Info -->
          <div class="info-card">
            <h4>{{ 'DEALER.PROFILE.FINANCIAL_INFORMATION' | translate }}</h4>
            <!-- <div class="info-row">
              <span class="label">{{ 'DEALER.PROFILE.CREDIT_LIMIT' | translate }}:</span>
              <span class="value">₹{{ dealerProfile.creditLimit ? (dealerProfile.creditLimit | number:'1.2-2') : ('DEALER.PROFILE.NOT_AVAILABLE' | translate) }}</span>
            </div>
            <div class="info-row">
              <span class="label">{{ 'DEALER.PROFILE.CURRENT_BALANCE' | translate }}:</span>
              <span class="value">₹{{ dealerProfile.currentBalance ? (dealerProfile.currentBalance | number:'1.2-2') : ('DEALER.PROFILE.NOT_AVAILABLE' | translate) }}</span>
            </div> -->
            <div class="info-row">
              <span class="label">{{ 'DEALER.PROFILE.AVAILABLE_CREDIT' | translate }}:</span>
              <span class="value available-credit d-flex gap-1">
                <img width="15" height="15" src="assets/images/currencyLogo.png" alt="">
                <span *ngIf="loadingCredit">...</span>
                <span *ngIf="!loadingCredit">{{ getDisplayCredit() | number:'1.2-2' }}</span>
              </span>
            </div>
          </div>

          <!-- User Information -->
          <div class="info-card">
            <h4>{{ 'DEALER.PROFILE.USER_ACCOUNT_DETAILS' | translate }}</h4>
            <div class="info-row">
              <span class="label">{{ 'DEALER.PROFILE.FULL_NAME' | translate }}:</span>
              <span class="value">{{ dealerProfile.userInfo.fullName || ('DEALER.PROFILE.NOT_AVAILABLE' | translate) }}</span>
            </div>
            <div class="info-row">
              <span class="label">{{ 'DEALER.PROFILE.EMAIL' | translate }}:</span>
              <span class="value">{{ dealerProfile.userInfo.email || ('DEALER.PROFILE.NOT_AVAILABLE' | translate) }}</span>
            </div>
            <div class="info-row">
              <span class="label">{{ 'DEALER.PROFILE.PHONE_NUMBER' | translate }}:</span>
              <span class="value">{{ dealerProfile.userInfo.phoneNumber || ('DEALER.PROFILE.NOT_AVAILABLE' | translate) }}</span>
            </div>
            <div class="info-row">
              <span class="label">{{ 'DEALER.PROFILE.USERNAME' | translate }}:</span>
              <span class="value">{{ dealerProfile.userInfo.userName || ('DEALER.PROFILE.NOT_AVAILABLE' | translate) }}</span>
            </div>
            <div class="info-row">
              <span class="label">{{ 'DEALER.PROFILE.ROLES' | translate }}:</span>
              <span class="value">{{ dealerProfile.userInfo.roles.join(', ') || ('DEALER.PROFILE.NOT_AVAILABLE' | translate) }}</span>
            </div>
          </div>

          <!-- Account Dates -->
          <div class="info-card">
            <h4>{{ 'DEALER.PROFILE.ACCOUNT_INFORMATION' | translate }}</h4>
            <div class="info-row">
              <span class="label">{{ 'DEALER.PROFILE.CREATED_AT' | translate }}:</span>
              <span class="value">{{ dealerProfile.createdAt | date:'medium' }}</span>
            </div>
            <div class="info-row" *ngIf="dealerProfile.updatedAt">
              <span class="label">{{ 'DEALER.PROFILE.LAST_UPDATED' | translate }}:</span>
              <span class="value">{{ dealerProfile.updatedAt | date:'medium' }}</span>
            </div>
          </div>
        </div>
      </div>

      <!-- Loading State -->
      <div class="loading-section" *ngIf="loading">
        <div class="spinner"></div>
        <p>{{ 'DEALER.PROFILE.LOADING.MESSAGE' | translate }}</p>
      </div>

      <!-- Error State -->
      <div class="error-section" *ngIf="error && !loading">
        <div class="error-message">
          <span class="material-icons">error</span>
          <p>{{ error }}</p>
          <button class="btn-retry" (click)="loadProfile()">
            <span class="material-icons">refresh</span>
            {{ 'DEALER.PROFILE.RETRY' | translate }}
          </button>
        </div>
      </div>
    </div>

    <!-- Change Password Dialog -->
    <div class="dialog-overlay" *ngIf="showChangePasswordDialog" (click)="closeChangePasswordDialog()">
      <div class="dialog" (click)="$event.stopPropagation()">
        <div class="dialog-header">
          <h3>{{ 'DEALER.PROFILE.CHANGE_PASSWORD_DIALOG.TITLE' | translate }}</h3>
          <button class="dialog-close" (click)="closeChangePasswordDialog()">
            <span class="material-icons">close</span>
          </button>
        </div>
        
        <form [formGroup]="passwordForm" (ngSubmit)="onChangePassword()" class="password-form">
          <div class="form-group">
            <label for="currentPassword">{{ 'DEALER.PROFILE.CHANGE_PASSWORD_DIALOG.CURRENT_PASSWORD' | translate }}</label>
            <input 
              type="password" 
              id="currentPassword"
              formControlName="currentPassword"
              [placeholder]="'DEALER.PROFILE.CHANGE_PASSWORD_DIALOG.PLACEHOLDERS.CURRENT_PASSWORD' | translate">
            <div class="error-message" *ngIf="passwordForm.get('currentPassword')?.invalid && passwordForm.get('currentPassword')?.touched">
              {{ 'DEALER.PROFILE.CHANGE_PASSWORD_DIALOG.ERRORS.CURRENT_PASSWORD_REQUIRED' | translate }}
            </div>
          </div>

          <div class="form-group">
            <label for="newPassword">{{ 'DEALER.PROFILE.CHANGE_PASSWORD_DIALOG.NEW_PASSWORD' | translate }}</label>
            <input 
              type="password" 
              id="newPassword"
              formControlName="newPassword"
              [placeholder]="'DEALER.PROFILE.CHANGE_PASSWORD_DIALOG.PLACEHOLDERS.NEW_PASSWORD' | translate">
            <div class="error-message" *ngIf="passwordForm.get('newPassword')?.invalid && passwordForm.get('newPassword')?.touched">
              {{ 'DEALER.PROFILE.CHANGE_PASSWORD_DIALOG.ERRORS.NEW_PASSWORD_MIN_LENGTH' | translate }}
            </div>
          </div>

          <div class="form-group">
            <label for="confirmPassword">{{ 'DEALER.PROFILE.CHANGE_PASSWORD_DIALOG.CONFIRM_PASSWORD' | translate }}</label>
            <input 
              type="password" 
              id="confirmPassword"
              formControlName="confirmPassword"
              [placeholder]="'DEALER.PROFILE.CHANGE_PASSWORD_DIALOG.PLACEHOLDERS.CONFIRM_PASSWORD' | translate">
            <div class="error-message" *ngIf="passwordForm.get('confirmPassword')?.invalid && passwordForm.get('confirmPassword')?.touched">
              {{ 'DEALER.PROFILE.CHANGE_PASSWORD_DIALOG.ERRORS.PASSWORDS_MISMATCH' | translate }}
            </div>
          </div>

          <div class="dialog-actions">
            <button type="button" class="btn-cancel" (click)="closeChangePasswordDialog()">
              {{ 'DEALER.PROFILE.CHANGE_PASSWORD_DIALOG.CANCEL' | translate }}
            </button>
            <button type="submit" class="btn-save" [disabled]="passwordForm.invalid || changingPassword">
              <span *ngIf="changingPassword" class="spinner-small"></span>
              {{ changingPassword ? ('DEALER.PROFILE.CHANGE_PASSWORD_DIALOG.CHANGING' | translate) : ('DEALER.PROFILE.CHANGE_PASSWORD_DIALOG.CHANGE_PASSWORD' | translate) }}
            </button>
          </div>
        </form>
      </div>
    </div>
  `,
  styleUrls: ['./dealer-profile.component.scss']
})
export class DealerProfileComponent implements OnInit {
  dealerProfile: DealerProfile | null = null;
  loading = false;
  error: string | null = null;
  availableCredit: number | null = null;
  loadingCredit = false;
  
  showChangePasswordDialog = false;
  changingPassword = false;
  passwordForm: FormGroup;

  constructor(
    private profileService: DealerProfileService,
    private fb: FormBuilder
  ) {
    this.passwordForm = this.fb.group({
      currentPassword: ['', Validators.required],
      newPassword: ['', [Validators.required, Validators.minLength(8)]],
      confirmPassword: ['', Validators.required]
    }, { validators: this.passwordMatchValidator });
  }

  ngOnInit() {
    this.loadProfile();
    this.loadCreditLimit();
  }

  loadProfile() {
    this.loading = true;
    this.error = null;
    
    this.profileService.getProfile().subscribe({
      next: (response: ApiResponse<DealerProfile>) => {
        this.loading = false;
        if (response.success && response.data) {
          this.dealerProfile = response.data;
        } else {
          this.error = response.message || 'Failed to load profile';
        }
      },
      error: (error) => {
        this.loading = false;
        this.error = error.error?.message || 'Failed to load dealer profile';
        console.error('Profile load error:', error);
      }
    });
  }

  openChangePasswordDialog() {
    this.showChangePasswordDialog = true;
    this.passwordForm.reset();
  }

  closeChangePasswordDialog() {
    this.showChangePasswordDialog = false;
    this.passwordForm.reset();
  }

  onChangePassword() {
    if (this.passwordForm.valid) {
      this.changingPassword = true;
      
      const passwordData = {
        currentPassword: this.passwordForm.value.currentPassword,
        newPassword: this.passwordForm.value.newPassword,
        confirmPassword: this.passwordForm.value.confirmPassword
      };

      this.profileService.changePassword(passwordData).subscribe({
        next: (response) => {
          this.changingPassword = false;
          if (response.success) {
            alert('Password changed successfully!');
            this.closeChangePasswordDialog();
          } else {
            alert(response.message || 'Failed to change password');
          }
        },
        error: (error) => {
          this.changingPassword = false;
          alert(error.error?.message || 'Failed to change password');
          console.error('Password change error:', error);
        }
      });
    }
  }

  loadCreditLimit() {
    this.loadingCredit = true;
    this.profileService.getCreditLimit().subscribe({
      next: (response: ApiResponse<any>) => {
        this.loadingCredit = false;
        if (response.success && response.data) {
          // Handle both camelCase and PascalCase response
          this.availableCredit = response.data.availableCredit || response.data.AvailableCredit || 0;
        } else {
          // Fallback to calculated value if API fails
          this.availableCredit = null;
          console.warn('Failed to load credit limit from API:', response.message);
        }
      },
      error: (error) => {
        this.loadingCredit = false;
        // Fallback to calculated value if API fails
        this.availableCredit = null;
        console.error('Credit limit API error:', error);
      }
    });
  }

  getDisplayCredit(): number {
    // Use API value if available, otherwise fallback to calculated value
    if (this.availableCredit !== null) {
      return this.availableCredit;
    }
    // Fallback to calculated value from profile
    return (this.dealerProfile?.creditLimit || 0) - (this.dealerProfile?.currentBalance || 0);
  }

  passwordMatchValidator(form: FormGroup) {
    const newPassword = form.get('newPassword')?.value;
    const confirmPassword = form.get('confirmPassword')?.value;
    
    if (newPassword && confirmPassword && newPassword !== confirmPassword) {
      form.get('confirmPassword')?.setErrors({ mismatch: true });
      return { mismatch: true };
    }
    return null;
  }
}