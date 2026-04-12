import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { RoleService } from '../../services/role.service';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';

@Component({
  selector: 'app-role-create',
  templateUrl: './role-create.component.html',
  styleUrls: ['./role-create.component.scss'],
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  schemas: [CUSTOM_ELEMENTS_SCHEMA]
})
export class RoleCreateComponent {
  roleForm: FormGroup;
  loading = false;

  constructor(
    private router: Router,
    private roleService: RoleService,
    private fb: FormBuilder
  ) {
    this.roleForm = this.fb.group({
      roleName: ['', [Validators.required, Validators.minLength(3)]]
    });
  }

  onSubmit(): void {
    if (this.roleForm.valid) {
      this.loading = true;
      this.roleService.createRole(this.roleForm.value.roleName).subscribe({
        next: () => {
          this.router.navigate(['/admin/roles']);
        },
        error: (error) => {
          console.error('Error creating role:', error);
          this.loading = false;
        }
      });
    }
  }

  onCancel(): void {
    this.router.navigate(['/admin/roles']);
  }
} 