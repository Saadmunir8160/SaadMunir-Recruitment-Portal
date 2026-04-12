import { Component, OnInit, CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule, AbstractControl, ValidationErrors } from '@angular/forms';
import { AuthService } from '../../auth/auth.service';
import { UserService } from '../../services/user.service';
import { ModalService } from '../../services/modal.service';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-change-password',
  templateUrl: './change-password.component.html',
  styleUrl: './change-password.component.scss',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  schemas: [CUSTOM_ELEMENTS_SCHEMA]
})
export class ChangePasswordComponent implements OnInit {
  changePasswordForm: FormGroup;
  loading = false;
  successMessage: string = '';
  errorMessage: string = '';
  currentUserId: string = '';

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private userService: UserService,
    private modalService: ModalService
  ) {
    this.changePasswordForm = this.fb.group({
      currentPassword: ['', [Validators.required]],
      newPassword: ['', [Validators.required, Validators.minLength(6)]],
      confirmPassword: ['', [Validators.required]],
    }, { validators: this.passwordMatchValidator });
  }

  // Custom validator to check if new password and confirm password match
  passwordMatchValidator(control: AbstractControl): ValidationErrors | null {
    const newPassword = control.get('newPassword');
    const confirmPassword = control.get('confirmPassword');

    if (newPassword && confirmPassword && newPassword.value !== confirmPassword.value) {
      return { passwordMismatch: true };
    }
    return null;
  }

  ngOnInit(): void {
    // Get current user ID from auth service or token
    const userId = this.authService.getCurrentUserId();
    
    if (!userId) {
      this.errorMessage = 'Unable to identify current user. Please log in again.';
      setTimeout(() => {
        this.modalService.closeChangePasswordModal();
      }, 2000);
      return;
    }
    
    this.currentUserId = userId;
  }

  onSubmit(): void {
    if (this.changePasswordForm.valid) {
      this.loading = true;
      this.errorMessage = '';
      this.successMessage = '';

      const { currentPassword, newPassword, confirmPassword } = this.changePasswordForm.value;

      this.userService.changePassword(
        this.currentUserId,
        currentPassword,
        newPassword,
        confirmPassword
      ).subscribe({
        next: (response) => {
          this.loading = false;
          this.successMessage = 'Password changed successfully!';
          this.changePasswordForm.reset();
          
          // Close modal after 2 seconds
          setTimeout(() => {
            this.modalService.closeChangePasswordModal();
          }, 2000);
        },
        error: (error) => {
          this.loading = false;
          console.error('Error changing password:', error);
          
          if (error.status === 401) {
            this.errorMessage = 'Current password is incorrect';
          } else if (error.error?.message) {
            this.errorMessage = error.error.message;
          } else if (error.error?.errors) {
            // Handle validation errors
            const validationErrors = error.error.errors;
            this.errorMessage = Object.values(validationErrors).join('\n');
          } else {
            this.errorMessage = 'Failed to change password. Please try again.';
          }
        }
      });
    } else {
      this.errorMessage = 'Please fill in all required fields correctly.';
    }
  }

  onCancel(): void {
    this.modalService.closeChangePasswordModal();
  }
}
