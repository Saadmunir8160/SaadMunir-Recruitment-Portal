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
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { RecruiterManagementService } from '../../services/recruiter-management.service';
import {
  ApplicationListDto,
  ApplicationStatus,
  ApplicationStatusLabels,
  VacancyListDto
} from '../../models/recruitment.models';

@Component({
  selector: 'app-applications-list',
  standalone: true,
  imports: [
    CommonModule, MatTableModule, MatPaginatorModule, MatSortModule,
    MatCardModule, MatButtonModule, MatIconModule, MatFormFieldModule,
    MatSelectModule, MatInputModule, MatChipsModule, MatProgressSpinnerModule,
    MatTooltipModule, MatProgressBarModule
  ],
  templateUrl: './applications-list.component.html',
  styleUrl: './applications-list.component.scss'
})
export class ApplicationsListComponent implements OnInit {
  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;

  displayedColumns = ['index', 'candidateName', 'matchScore', 'status', 'applicationDate', 'assignedRecruiterName', 'actions'];
  dataSource = new MatTableDataSource<ApplicationListDto>();
  vacancies: VacancyListDto[] = [];
  selectedVacancyId: number | null = null;
  isLoading = false;
  totalCount = 0;
  currentPage = 0;
  pageSize = 10;

  constructor(
    private service: RecruiterManagementService,
    private route: ActivatedRoute,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.loadVacancies();
  }

  private loadVacancies(): void {
    this.service.getVacancies(1, 100).subscribe({
      next: (res) => {
        this.vacancies = res.data;
        const qp = this.route.snapshot.queryParamMap.get('vacancyId');
        if (qp) {
          this.selectedVacancyId = +qp;
          this.loadApplications();
        }
      }
    });
  }

  loadApplications(): void {
    if (!this.selectedVacancyId) return;
    this.isLoading = true;
    this.service.getApplicationsByVacancy(this.selectedVacancyId, this.currentPage + 1, this.pageSize).subscribe({
      next: (res) => {
        this.dataSource.data = res.data;
        this.totalCount = res.metadata.totalCount;
        this.isLoading = false;
      },
      error: () => { this.isLoading = false; }
    });
  }

  onVacancyChange(): void {
    this.currentPage = 0;
    this.loadApplications();
  }

  onPageChange(event: PageEvent): void {
    this.currentPage = event.pageIndex;
    this.pageSize = event.pageSize;
    this.loadApplications();
  }

  applyFilter(event: Event): void {
    const value = (event.target as HTMLInputElement).value;
    this.dataSource.filter = value.trim().toLowerCase();
  }

  getStatusLabel(status: number): string { return ApplicationStatusLabels[status] || 'Unknown'; }

  getStatusClass(status: number): string {
    if ([ApplicationStatus.Hired, ApplicationStatus.OfferAccepted, ApplicationStatus.HRApproved, ApplicationStatus.MedicalFit].includes(status))
      return 'status-success';
    if ([ApplicationStatus.Rejected, ApplicationStatus.HRRejected, ApplicationStatus.MedicalUnfit, ApplicationStatus.OfferRejected, ApplicationStatus.Withdrawn].includes(status))
      return 'status-danger';
    if ([ApplicationStatus.InterviewScheduled, ApplicationStatus.HRInterviewScheduled, ApplicationStatus.MedicalPending, ApplicationStatus.Screening].includes(status))
      return 'status-warning';
    if ([ApplicationStatus.Matched, ApplicationStatus.Shortlisted, ApplicationStatus.Verified].includes(status))
      return 'status-info';
    return 'status-default';
  }

  getScoreColor(score: number): string {
    return score >= 70 ? 'primary' : score >= 40 ? 'accent' : 'warn';
  }

  viewApplication(id: number): void {
    this.router.navigate(['/admin/recruiter-management/applications', id]);
  }
}
