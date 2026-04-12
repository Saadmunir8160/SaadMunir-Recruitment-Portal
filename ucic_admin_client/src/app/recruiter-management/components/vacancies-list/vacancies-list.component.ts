import { Component, OnInit, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { MatTableModule, MatTableDataSource } from '@angular/material/table';
import { MatPaginator, MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatSortModule, MatSort } from '@angular/material/sort';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatInputModule } from '@angular/material/input';
import { MatChipsModule } from '@angular/material/chips';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatMenuModule } from '@angular/material/menu';
import { RecruiterManagementService } from '../../services/recruiter-management.service';
import {
  VacancyListDto,
  VacancyPublishStatus,
  VacancyPublishStatusLabels
} from '../../models/recruitment.models';

@Component({
  selector: 'app-vacancies-list',
  standalone: true,
  imports: [
    CommonModule, MatTableModule, MatPaginatorModule, MatSortModule,
    MatCardModule, MatButtonModule, MatIconModule, MatFormFieldModule,
    MatSelectModule, MatInputModule, MatChipsModule, MatProgressSpinnerModule,
    MatTooltipModule, MatMenuModule
  ],
  templateUrl: './vacancies-list.component.html',
  styleUrl: './vacancies-list.component.scss'
})
export class VacanciesListComponent implements OnInit {
  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;

  displayedColumns = ['index', 'requisitionNumber', 'jobTitle', 'departmentName', 'location', 'positions', 'publishStatus', 'closingDate', 'actions'];
  dataSource = new MatTableDataSource<VacancyListDto>();
  isLoading = true;
  totalCount = 0;
  currentPage = 0;
  pageSize = 10;
  selectedStatus = -1;

  statusOptions = Object.entries(VacancyPublishStatusLabels).map(([value, label]) => ({
    value: +value, label
  }));

  constructor(
    private service: RecruiterManagementService,
    private router: Router,
    private route: ActivatedRoute
  ) { }

  ngOnInit(): void {
    const statusParam = this.route.snapshot.queryParamMap.get('status');
    if (statusParam !== null && statusParam !== '') {
      const n = parseInt(statusParam, 10);
      if (!Number.isNaN(n)) {
        this.selectedStatus = n;
      }
    }
    this.loadVacancies();
  }

  loadVacancies(): void {
    this.isLoading = true;
    const status = this.selectedStatus >= 0 ? this.selectedStatus : undefined;
    this.service.getVacancies(this.currentPage + 1, this.pageSize, status).subscribe({
      next: (res) => {
        this.dataSource.data = res.data;
        this.totalCount = res.metadata.totalCount;
        this.isLoading = false;
      },
      error: () => { this.isLoading = false; }
    });
  }

  onPageChange(event: PageEvent): void {
    this.currentPage = event.pageIndex;
    this.pageSize = event.pageSize;
    this.loadVacancies();
  }

  onStatusChange(): void {
    this.currentPage = 0;
    this.loadVacancies();
  }

  applyFilter(event: Event): void {
    const value = (event.target as HTMLInputElement).value;
    this.dataSource.filter = value.trim().toLowerCase();
  }

  getStatusLabel(status: number): string {
    return VacancyPublishStatusLabels[status] || 'Unknown';
  }

  getStatusClass(status: number): string {
    switch (status) {
      case VacancyPublishStatus.Draft: return 'status-default';
      case VacancyPublishStatus.PendingApproval: return 'status-warning';
      case VacancyPublishStatus.Published: return 'status-success';
      case VacancyPublishStatus.Paused: return 'status-info';
      case VacancyPublishStatus.Closed: return 'status-primary';
      case VacancyPublishStatus.Cancelled: return 'status-danger';
      default: return 'status-default';
    }
  }

  createVacancy(): void { this.router.navigate(['/admin/recruiter-management/vacancies/create']); }
  viewVacancy(id: number): void { this.router.navigate(['/admin/recruiter-management/vacancies', id]); }
  editVacancy(id: number): void { this.router.navigate(['/admin/recruiter-management/vacancies/edit', id]); }
  viewApplications(vacancyId: number): void { this.router.navigate(['/admin/recruiter-management/applications'], { queryParams: { vacancyId } }); }
}
