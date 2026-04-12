import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { RoleService } from '../../services/role.service';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';

@Component({
  selector: 'app-role-edit',
  templateUrl: './role-edit.component.html',
  styleUrls: ['./role-edit.component.scss'],
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  schemas: [CUSTOM_ELEMENTS_SCHEMA]
})
export class RoleEditComponent implements OnInit {
  roleForm: FormGroup;
  roleId: string = '';
  loading = false;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private roleService: RoleService,
    private fb: FormBuilder
  ) {
    this.roleForm = this.fb.group({
      roleName: ['', [Validators.required, Validators.minLength(3)]]
    });
  }

  ngOnInit(): void {
    this.roleId = this.route.snapshot.paramMap.get('id')!;
    this.loadRole();
  }

  loadRole(): void {
    this.loading = true;
    this.roleService.getRoleById(this.roleId).subscribe({
      next: (response) => {
        this.roleForm.patchValue({
          roleName: response.roleName
        });
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading role:', error);
        this.loading = false;
      }
    });
  }

  onSubmit(): void {
    if (this.roleForm.valid) {
      this.loading = true;
      this.roleService.updateRole(this.roleId, this.roleForm.value.roleName).subscribe({
        next: () => {
          this.router.navigate(['/admin/roles']);
        },
        error: (error) => {
          console.error('Error updating role:', error);
          this.loading = false;
        }
      });
    }
  }

  onCancel(): void {
    this.router.navigate(['/admin/roles']);
  }
} 