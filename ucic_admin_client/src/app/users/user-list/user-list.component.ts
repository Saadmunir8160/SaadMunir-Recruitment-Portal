import { Component, OnInit, CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import { UserService, UserResponseDTO } from '../../services/user.service';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { PaginationComponent } from '../../shared/components/pagination/pagination.component';
import { PaginatedResponse } from '../../shared/interfaces/paginated-response.interface';

@Component({
  selector: 'app-user-list',
  templateUrl: './user-list.component.html',
  styleUrls: ['./user-list.component.scss'],
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    FormsModule,
    PaginationComponent
  ],
  schemas: [CUSTOM_ELEMENTS_SCHEMA]
})
export class UserListComponent implements OnInit {
  allUsers: UserResponseDTO[] = [];
  filteredUsers: UserResponseDTO[] = [];
  searchTerm = '';
  loading = false;
  errorMessage = '';

  currentPage = 1;
  pageSize = 10;
  pageSizeOptions: (number | string)[] = [5, 10, 15, 20, 30, 50, 'All'];
  totalCount = 0;
  totalPages = 0;
  isServerSidePagination = false;

  constructor(private userService: UserService, private router: Router) {}

  ngOnInit(): void {
    this.loadUsers();
  }

  loadUsers(): void {
    this.loading = true;
    const pageSizeToSend = this.pageSize === 0 ? 1000000 : this.pageSize;
    this.userService.getAllUsers(this.currentPage, pageSizeToSend).subscribe({
      next: (response) => {
        if (Array.isArray(response)) {
          this.allUsers = response;
          this.isServerSidePagination = false;
          this.totalCount = response.length;
          this.totalPages = this.pageSize === 0 ? 1 : Math.ceil(this.totalCount / this.pageSize);
          this.applyFiltersAndPaginate();
        } else {
          const res = response as PaginatedResponse<UserResponseDTO>;
          this.allUsers = res.data;
          this.isServerSidePagination = true;
          this.currentPage = res.metadata.currentPage;
          this.totalCount = res.metadata.totalCount;
          this.totalPages = res.metadata.totalPages;
          this.filteredUsers = res.data;
        }
        this.loading = false;
      },
      error: () => {
        this.errorMessage = 'Failed to load users. Please try again.';
        this.loading = false;
      }
    });
  }

  applyFiltersAndPaginate(): void {
    if (this.isServerSidePagination) {
      this.loadUsers();
      return;
    }

    let filtered = this.allUsers;
    if (this.searchTerm) {
      const term = this.searchTerm.toLowerCase();
      filtered = this.allUsers.filter(u =>
        u.fullName?.toLowerCase().includes(term) ||
        u.userName?.toLowerCase().includes(term) ||
        u.email?.toLowerCase().includes(term) ||
        u.phone?.toLowerCase().includes(term)
      );
    }

    this.totalCount = filtered.length;
    this.totalPages = this.pageSize === 0 ? 1 : Math.ceil(this.totalCount / this.pageSize);
    if (this.pageSize === 0) {
      this.filteredUsers = filtered;
    } else {
      const start = (this.currentPage - 1) * this.pageSize;
      this.filteredUsers = filtered.slice(start, start + this.pageSize);
    }
  }

  onSearch(): void {
    this.currentPage = 1;
    if (this.isServerSidePagination) {
      this.loadUsers();
    } else {
      this.applyFiltersAndPaginate();
    }
  }

  onPageSizeChange(size: number): void {
    this.pageSize = size;
    this.currentPage = 1;
    this.loadUsers();
  }

  goToPage(page: number): void {
    this.currentPage = page;
    if (this.isServerSidePagination) {
      this.loadUsers();
    } else {
      this.applyFiltersAndPaginate();
    }
  }

  editUser(user: UserResponseDTO): void {
    this.router.navigate(['/admin/users/edit', user.id]);
  }

  deleteUser(id: string): void {
    if (confirm('Are you sure?')) {
      this.userService.deleteUser(id).subscribe({
        next: () => this.loadUsers(),
        error: () => (this.errorMessage = 'Failed to delete user.')
      });
    }
  }

  createUser(): void {
    this.router.navigate(['/admin/users/create']);
  }

  trackByUserId(index: number, user: UserResponseDTO): string {
    return user.id;
  }
}