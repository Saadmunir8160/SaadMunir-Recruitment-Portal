import { Component, OnInit } from '@angular/core';
import { RoleService } from '../../services/role.service';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';

@Component({
  selector: 'app-role-list',
  templateUrl: './role-list.component.html',
  styleUrls: ['./role-list.component.scss'],
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  schemas: [CUSTOM_ELEMENTS_SCHEMA]
})
export class RoleListComponent implements OnInit {
  roles: any[] = [];
  filteredRoles: any[] = [];
  loading = false;
  searchTerm: string = '';

  constructor(
    private roleService: RoleService,
    private router: Router
  ) { }

  ngOnInit(): void {
    this.loadRoles();
  }

  loadRoles(): void {
    this.loading = true;
    this.roleService.getAllRoles().subscribe({
      next: (response) => {
        this.roles = response;
        this.filteredRoles = response;
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading roles:', error);
        this.loading = false;
      }
    });
  }

  onSearch(): void {
    if (!this.searchTerm) {
      this.filteredRoles = this.roles;
      return;
    }

    this.filteredRoles = this.roles.filter(role =>
      role.roleName.toLowerCase().includes(this.searchTerm.toLowerCase())
    );
  }

  createNewRole(): void {
    this.router.navigate(['/admin/roles/create']);
  }

  editRole(role: any): void {
    this.router.navigate(['/admin/roles/edit', role.id]);
  }

  deleteRole(id: string): void {
    if (confirm('Are you sure you want to delete this role?')) {
      this.roleService.deleteRole(id).subscribe({
        next: () => {
          this.loadRoles();
        },
        error: (error) => {
          console.error('Error deleting role:', error);
        }
      });
    }
  }
} 