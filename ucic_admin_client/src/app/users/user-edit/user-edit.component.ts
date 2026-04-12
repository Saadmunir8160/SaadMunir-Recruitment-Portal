import { Component, OnInit, CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule, AbstractControl, ValidationErrors } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { UserService, UpdateUserDTO } from '../../services/user.service';
import { RoleService } from '../../services/role.service';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-user-edit',
  templateUrl: './user-edit.component.html',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  schemas: [CUSTOM_ELEMENTS_SCHEMA]
})
export class UserEditComponent implements OnInit {
  userForm: FormGroup;
  userId: string = '';
  availableRoles: any[] = [];
  loading = false;
  errorMessage: string = '';

  constructor(
    private fb: FormBuilder,
    private route: ActivatedRoute,
    private router: Router,
    private userService: UserService,
    private roleService: RoleService
  ) {
    this.userForm = this.fb.group({
      fullName: ['', [Validators.required, Validators.maxLength(100)]],
      email: ['', [Validators.required, Validators.email]],
      phone: ['', [Validators.required]],
      roles: ['', [Validators.required]],
      password: ['', [Validators.minLength(6)]],
      confirmPassword: [''],
      cr_No: [''],
      vat_ID: [''],
      contactPerson: [''],
      location: ['']
    }, { validators: this.passwordMatchValidator });
  }

  // Custom validator to check if passwords match
  passwordMatchValidator(control: AbstractControl): ValidationErrors | null {
    const password = control.get('password');
    const confirmPassword = control.get('confirmPassword');

    if (password && confirmPassword && password.value !== confirmPassword.value) {
      return { passwordMismatch: true };
    }
    return null;
  }

  ngOnInit(): void {
    this.userId = this.route.snapshot.paramMap.get('id')!;
    this.loadUser();
    this.loadRoles();
  }

  loadUser(): void {
    this.loading = true;
    this.userService.getUserDetails(this.userId).subscribe({
      next: (user) => {
        this.userForm.patchValue({
          fullName: user.fullName,
          email: user.email,
          phone: user.phone,
          roles: user.roles?.[0] || '',
          password: '',
          confirmPassword: '',
          cr_No: user.crNo || '',
          vat_ID: user.vatId || '',
          contactPerson: user.contactPerson || '',
          location: user.location || ''
        });
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading user:', error);
        this.errorMessage = 'Failed to load user details. Please try again.';
        this.loading = false;
      }
    });
  }

  loadRoles(): void {
    this.roleService.getAllRoles().subscribe({
      next: (roles) => {
        this.availableRoles = roles;
      },
      error: (error) => {
        console.error('Error loading roles:', error);
        this.errorMessage = 'Failed to load roles. Please try again.';
      }
    });
  }

  onSubmit(): void {
    if (this.userForm.valid) {
      this.loading = true;
      this.errorMessage = '';

      const userData: UpdateUserDTO = {
        id: this.userId,
        fullName: this.userForm.value.fullName,
        email: this.userForm.value.email,
        roles: [this.userForm.value.roles],
        password: this.userForm.value.password || undefined,
        cr_No: this.userForm.value.cr_No,
        vat_ID: this.userForm.value.vat_ID,
        phone: this.userForm.value.phone,
        contactPerson: this.userForm.value.contactPerson,
        location: this.userForm.value.location
      };

      this.userService.updateUserProfile(this.userId, userData).subscribe({
        next: () => {
          this.router.navigate(['/admin/users']);
        },
        error: (error) => {
          console.error('Error updating user:', error);
          this.errorMessage = 'Failed to update user. Please try again.';
          this.loading = false;
        }
      });
    } else {
      this.errorMessage = 'Please fill in all required fields correctly.';
    }
  }

  onCancel(): void {
    this.router.navigate(['/admin/users']);
  }
} 