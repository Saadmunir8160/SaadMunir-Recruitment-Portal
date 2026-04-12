import { Component, OnInit, CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { UserService, CreateUserDTO } from '../../services/user.service';
import { RoleService } from '../../services/role.service';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-user-create',
  templateUrl: './user-create.component.html',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  schemas: [CUSTOM_ELEMENTS_SCHEMA]
})
export class UserCreateComponent implements OnInit {
  userForm: FormGroup;
  availableRoles: any[] = [];
  loading = false;
  errorMessage: string = '';

  constructor(
    private fb: FormBuilder,
    private router: Router,
    private userService: UserService,
    private roleService: RoleService
  ) {
    this.userForm = this.fb.group({
      fullName: ['', [Validators.required, Validators.maxLength(100)]],
      userName: ['', [Validators.required, Validators.minLength(3), Validators.maxLength(50)]],
      email: ['', [Validators.required, Validators.email]],
      phone: ['', [Validators.required, Validators.pattern('^[0-9+()-]{10,15}$')]],
      password: ['', [Validators.required, Validators.minLength(6)]],
      confirmationPassword: ['', [Validators.required]],
      roles: ['', [Validators.required]],
      cr_No: [''],
      vat_ID: [''],
      contactPerson: [''],
      location: ['']
    });
  }

  ngOnInit(): void {
    this.loadRoles();
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
      if (this.userForm.value.password !== this.userForm.value.confirmationPassword) {
        this.errorMessage = 'Passwords do not match';
        return;
      }

      this.loading = true;
      this.errorMessage = '';

      const userData: CreateUserDTO = {
        fullName: this.userForm.value.fullName,
        userName: this.userForm.value.userName,
        email: this.userForm.value.email,
        phone: this.userForm.value.phone,
        password: this.userForm.value.password,
        confirmationPassword: this.userForm.value.confirmationPassword,
        roles: [this.userForm.value.roles],
        cr_No: this.userForm.value.cr_No,
        vat_ID: this.userForm.value.vat_ID,
        contactPerson: this.userForm.value.contactPerson,
        location: this.userForm.value.location
      };

      this.userService.createUser(userData).subscribe({
        next: (response) => {
          this.router.navigate(['/admin/users']);
        },
        error: (error) => {
          console.error('Error creating user:', error);
          this.loading = false;
          
          if (error.error?.errors) {
            // Handle validation errors
            const validationErrors = error.error.errors;
            this.errorMessage = Object.values(validationErrors).join('\n');
          } else if (error.error?.message) {
            // Handle API error message
            this.errorMessage = error.error.message;
          } else {
            // Handle generic error
            this.errorMessage = 'Failed to create user. Please try again.';
          }
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