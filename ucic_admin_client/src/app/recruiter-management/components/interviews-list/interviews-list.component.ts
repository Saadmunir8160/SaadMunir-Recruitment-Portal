import { Component, OnInit, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { MatTableModule, MatTableDataSource } from '@angular/material/table';
import { MatPaginator, MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatSortModule, MatSort } from '@angular/material/sort';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatChipsModule } from '@angular/material/chips';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTooltipModule } from '@angular/material/tooltip';
import { RecruiterManagementService } from '../../services/recruiter-management.service';
import { InterviewListDto } from '../../models/recruitment.models';

@Component({
  selector: 'app-interviews-list',
  standalone: true,
  imports: [
    CommonModule, MatTableModule, MatPaginatorModule, MatSortModule,
    MatCardModule, MatButtonModule, MatIconModule, MatFormFieldModule,
    MatInputModule, MatChipsModule, MatProgressSpinnerModule, MatTooltipModule
  ],
  templateUrl: './interviews-list.component.html',
  styleUrl: './interviews-list.component.scss'
})
export class InterviewsListComponent implements OnInit {
  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;

  displayedColumns = ['index', 'candidateName', 'interviewType', 'interviewMode', 'scheduledDate', 'location', 'interviewerName', 'status', 'candidateConfirmed', 'actions'];
  dataSource = new MatTableDataSource<InterviewListDto>();
  isLoading = true;
  totalCount = 0;
  currentPage = 0;
  pageSize = 10;

  constructor(
    private service: RecruiterManagementService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.loadInterviews();
  }

  loadInterviews(): void {
    this.isLoading = true;
    this.service.getInterviews(this.currentPage + 1, this.pageSize).subscribe({
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
    this.loadInterviews();
  }

  applyFilter(event: Event): void {
    const value = (event.target as HTMLInputElement).value;
    this.dataSource.filter = value.trim().toLowerCase();
  }

  getInterviewTypeLabel(type: number): string {
    const labels: Record<number, string> = { 0: 'Technical', 1: 'Managerial', 2: 'HR', 3: 'Final' };
    return labels[type] || 'Unknown';
  }

  getInterviewModeLabel(mode: number): string {
    const labels: Record<number, string> = { 0: 'On-Site', 1: 'Online' };
    return labels[mode] || 'Unknown';
  }

  getInterviewStatusLabel(status: number): string {
    const labels: Record<number, string> = { 0: 'Scheduled', 1: 'Confirmed', 2: 'Rescheduled', 3: 'Completed', 4: 'Cancelled', 5: 'No Show' };
    return labels[status] || 'Unknown';
  }

  getInterviewStatusClass(status: number): string {
    switch (status) {
      case 0: return 'status-info';     // Scheduled
      case 1: return 'status-primary';  // Confirmed
      case 2: return 'status-warning';  // Rescheduled
      case 3: return 'status-success';  // Completed
      case 4: return 'status-danger';   // Cancelled
      case 5: return 'status-danger';   // No Show
      default: return 'status-default';
    }
  }

  viewApplication(applicationId: number): void {
    this.router.navigate(['/admin/recruiter-management/applications', applicationId]);
  }
}
