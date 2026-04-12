import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatDividerModule } from '@angular/material/divider';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatTabsModule } from '@angular/material/tabs';
import { MatTableModule } from '@angular/material/table';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { FormsModule } from '@angular/forms';
import { RecruiterManagementService } from '../../services/recruiter-management.service';
import {
  ApplicationDetailDto,
  ApplicationStatus,
  ApplicationStatusLabels,
  InterviewListDto,
  MatchResultDto,
  ScreeningTaskDto
} from '../../models/recruitment.models';

@Component({
  selector: 'app-application-detail',
  standalone: true,
  imports: [
    CommonModule, FormsModule, MatCardModule, MatButtonModule, MatIconModule,
    MatChipsModule, MatProgressSpinnerModule, MatProgressBarModule,
    MatDividerModule, MatTooltipModule, MatTabsModule, MatTableModule,
    MatSnackBarModule, MatFormFieldModule, MatInputModule, MatSelectModule
  ],
  templateUrl: './application-detail.component.html',
  styleUrl: './application-detail.component.scss'
})
export class ApplicationDetailComponent implements OnInit {
  application: ApplicationDetailDto | null = null;
  matchResult: MatchResultDto | null = null;
  screeningTasks: ScreeningTaskDto[] = [];
  interviews: InterviewListDto[] = [];
  isLoading = true;
  isUpdating = false;
  newStatus: number | null = null;
  statusReason = '';

  screeningColumns = ['taskDescription', 'assignedToName', 'deadline', 'status', 'notes'];
  interviewColumns = ['interviewType', 'interviewMode', 'scheduledDate', 'location', 'interviewerName', 'status', 'candidateConfirmed'];

  matchBreakdownItems: { label: string; value: number }[] = [];

  availableStatuses = Object.entries(ApplicationStatusLabels).map(([value, label]) => ({
    value: +value, label
  }));

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private service: RecruiterManagementService,
    private snackBar: MatSnackBar
  ) {}

  ngOnInit(): void {
    const id = Number(this.route.snapshot.paramMap.get('id'));
    if (id) {
      this.service.getApplicationById(id).subscribe({
        next: (app) => {
          this.application = app;
          this.isLoading = false;
          this.loadMatchResult();
          this.loadScreeningTasks();
          this.loadInterviews();
        },
        error: () => { this.isLoading = false; }
      });
    }
  }

  loadMatchResult(): void {
    if (!this.application) return;
    this.service.getMatchResult(this.application.applicationId).subscribe({
      next: (mr) => {
        this.matchResult = mr;
        this.matchBreakdownItems = [
          { label: 'Specialization', value: mr.specializationScore },
          { label: 'Experience', value: mr.experienceScore },
          { label: 'Qualification', value: mr.qualificationScore },
          { label: 'Nationality', value: mr.nationalityScore },
          { label: 'Location', value: mr.locationScore },
          { label: 'Certifications', value: mr.certificationScore }
        ];
      }
    });
  }

  loadScreeningTasks(): void {
    if (!this.application) return;
    this.service.getScreeningTasks(this.application.applicationId).subscribe({
      next: (tasks) => { this.screeningTasks = tasks; }
    });
  }

  loadInterviews(): void {
    if (!this.application) return;
    this.service.getInterviewsByApplication(this.application.applicationId).subscribe({
      next: (interviews) => { this.interviews = interviews; }
    });
  }

  updateStatus(): void {
    if (this.newStatus === null || !this.application) return;
    this.isUpdating = true;
    this.service.updateApplicationStatus(this.application.applicationId, {
      status: this.newStatus,
      reason: this.statusReason || undefined
    }).subscribe({
      next: () => {
        this.application!.status = this.newStatus!;
        this.application!.statusName = ApplicationStatusLabels[this.newStatus!];
        this.snackBar.open('Status updated successfully', 'Close', { duration: 3000 });
        this.isUpdating = false;
        this.newStatus = null;
        this.statusReason = '';
      },
      error: () => {
        this.snackBar.open('Failed to update status', 'Close', { duration: 3000 });
        this.isUpdating = false;
      }
    });
  }

  getStatusLabel(status: number): string { return ApplicationStatusLabels[status] || 'Unknown'; }

  getStatusClass(status: number): string {
    if ([ApplicationStatus.Hired, ApplicationStatus.OfferAccepted, ApplicationStatus.HRApproved, ApplicationStatus.MedicalFit].includes(status))
      return 'status-success';
    if ([ApplicationStatus.Rejected, ApplicationStatus.HRRejected, ApplicationStatus.MedicalUnfit, ApplicationStatus.OfferRejected, ApplicationStatus.Withdrawn].includes(status))
      return 'status-danger';
    if ([ApplicationStatus.InterviewScheduled, ApplicationStatus.HRInterviewScheduled, ApplicationStatus.MedicalPending, ApplicationStatus.Screening].includes(status))
      return 'status-warning';
    return 'status-default';
  }

  getScoreColor(score: number): string {
    return score >= 70 ? 'primary' : score >= 40 ? 'accent' : 'warn';
  }

  overrideToScreening(): void {
    if (!this.application) return;
    this.isUpdating = true;
    this.service.updateApplicationStatus(this.application.applicationId, {
      status: ApplicationStatus.Screening,
      reason: 'Manual recruiter override from NotMatched'
    }).subscribe({
      next: () => {
        this.application!.status = ApplicationStatus.Screening;
        this.application!.statusName = ApplicationStatusLabels[ApplicationStatus.Screening];
        this.snackBar.open('Application moved to Screening', 'Close', { duration: 3000 });
        this.isUpdating = false;
      },
      error: () => {
        this.snackBar.open('Failed to override status', 'Close', { duration: 3000 });
        this.isUpdating = false;
      }
    });
  }

  getTaskStatusLabel(status: number): string {
    const labels: Record<number, string> = { 0: 'Pending', 1: 'In Progress', 2: 'Completed' };
    return labels[status] || 'Unknown';
  }

  getInterviewTypeLabel(type: number): string {
    const labels: Record<number, string> = { 0: 'Technical', 1: 'Managerial', 2: 'HR', 3: 'Final' };
    return labels[type] || 'Unknown';
  }

  getInterviewModeLabel(mode: number): string {
    return mode === 0 ? 'On-Site' : 'Online';
  }

  getInterviewStatusLabel(status: number): string {
    const labels: Record<number, string> = { 0: 'Scheduled', 1: 'Confirmed', 2: 'Rescheduled', 3: 'Completed', 4: 'Cancelled', 5: 'No Show' };
    return labels[status] || 'Unknown';
  }

  getInterviewStatusClass(status: number): string {
    switch (status) {
      case 0: return 'status-info';
      case 1: return 'status-primary';
      case 2: return 'status-warning';
      case 3: return 'status-success';
      case 4: return 'status-danger';
      case 5: return 'status-danger';
      default: return 'status-default';
    }
  }

  goBack(): void { this.router.navigate(['/admin/recruiter-management/applications']); }
  viewCandidate(): void { this.router.navigate(['/admin/recruiter-management/candidates', this.application!.candidateId]); }
  viewVacancy(): void { this.router.navigate(['/admin/recruiter-management/vacancies', this.application!.vacancyId]); }
}
