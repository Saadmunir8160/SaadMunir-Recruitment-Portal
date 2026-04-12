import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { MatDialog } from '@angular/material/dialog';
import { 
  DealerManagementService, 
  DealerArea
} from '../services/dealer-management.service';
import { PaginatedResponse } from '../../shared/interfaces/paginated-response.interface';
import { 
  AreaViewDialogComponent,
  AreaEditDialogComponent,
  AreaCreateDialogComponent
} from './index';

@Component({
  selector: 'app-dealer-areas',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './dealer-areas.component.html',
  styleUrls: ['./dealer-areas.component.scss']
})
export class DealerAreasComponent implements OnInit {
  areas: DealerArea[] = [];
  loading = false;
  error: string | null = null;

  // Pagination
  currentPage = 1;
  pageSize = 10;
  totalPages = 0;
  totalRecords = 0;

  // Filtering
  searchTerm = '';
  selectedStatus: boolean | null = null;

  // Status options
  statusOptions = [
    { value: null, label: 'All Status' },
    { value: true, label: 'Active' },
    { value: false, label: 'Inactive' }
  ];

  constructor(
    private dealerService: DealerManagementService,
    private router: Router,
    private dialog: MatDialog
  ) {}

  ngOnInit(): void {
    this.loadAreas();
  }

  loadAreas(): void {
    this.loading = true;
    this.error = null;

    this.dealerService.getDealerAreas(
      this.currentPage,
      this.pageSize,
      this.searchTerm || undefined,
      this.selectedStatus ?? undefined
    ).subscribe({
      next: (response: PaginatedResponse<DealerArea>) => {
        this.areas = response.data;
        this.totalRecords = response.metadata.totalCount;
        this.totalPages = response.metadata.totalPages;
        this.loading = false;
      },
      error: (error) => {
        this.error = 'Failed to load dealer areas';
        console.error('Error loading areas:', error);
        this.loading = false;
      }
    });
  }

  onSearch(): void {
    this.currentPage = 1;
    this.loadAreas();
  }

  onClearFilters(): void {
    this.searchTerm = '';
    this.selectedStatus = null;
    this.currentPage = 1;
    this.loadAreas();
  }

  onPageChange(page: number): void {
    this.currentPage = page;
    this.loadAreas();
  }

  viewAreaDetails(area: DealerArea): void {
    this.dialog.open(AreaViewDialogComponent, {
      width: '600px',
      data: area
    });
  }

  createArea(): void {
    const dialogRef = this.dialog.open(AreaCreateDialogComponent, {
      width: '500px'
    });

    dialogRef.afterClosed().subscribe((success: boolean) => {
      if (success) {
        this.loadAreas();
      }
    });
  }

  editArea(area: DealerArea): void {
    const dialogRef = this.dialog.open(AreaEditDialogComponent, {
      width: '500px',
      data: area
    });

    dialogRef.afterClosed().subscribe((success: boolean) => {
      if (success) {
        this.loadAreas();
      }
    });
  }

  deleteArea(area: DealerArea): void {
    if (confirm(`Are you sure you want to delete area "${area.areaName}"?`)) {
      this.dealerService.deleteDealerArea(area.areaID).subscribe({
        next: (response) => {
          if (response.success) {
            alert('Area deleted successfully!');
            this.loadAreas();
          } else {
            alert(`Failed to delete area: ${response.message}`);
          }
        },
        error: (error) => {
          console.error('Error deleting area:', error);
          alert('An error occurred while deleting the area.');
        }
      });
    }
  }

  getStatusClass(isActive: boolean): string {
    return isActive ? 'badge-success' : 'badge-danger';
  }

  getStatusText(isActive: boolean): string {
    return isActive ? 'Active' : 'Inactive';
  }

  formatDate(date: Date): string {
    return new Date(date).toLocaleDateString();
  }
}
