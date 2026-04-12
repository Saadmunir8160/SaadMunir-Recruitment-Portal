import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDividerModule } from '@angular/material/divider';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { RecruiterManagementService } from '../../services/recruiter-management.service';
import { RoleService } from '../../../services/role.service';
import {
  VacancyDetailDto,
  VacancyPublishStatus,
  VacancyPublishStatusLabels
} from '../../models/recruitment.models';

@Component({
  selector: 'app-vacancy-detail',
  standalone: true,
  imports: [
    CommonModule, MatCardModule, MatButtonModule, MatIconModule,
    MatChipsModule, MatProgressSpinnerModule, MatDividerModule, MatTooltipModule,
    MatSnackBarModule
  ],
  templateUrl: './vacancy-detail.component.html',
  styleUrl: './vacancy-detail.component.scss'
})
export class VacancyDetailComponent implements OnInit {
  vacancy: VacancyDetailDto | null = null;
  isLoading = true;
  isProcessing = false;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private service: RecruiterManagementService,
    private roleService: RoleService,
    private snackBar: MatSnackBar
  ) { }

  ngOnInit(): void {
    const id = Number(this.route.snapshot.paramMap.get('id'));
    if (id) {
      this.service.getVacancyById(id).subscribe({
        next: (v) => { this.vacancy = v; this.isLoading = false; },
        error: () => { this.isLoading = false; }
      });
    }
  }

  // ========================
  // ROLE-BASED VISIBILITY
  // ========================

  /** Creators (Recruiter / HR) submit; Admin may also submit. Approvers do not. */
  get canSubmitForApproval(): boolean {
    if (!this.vacancy || this.vacancy.publishStatus !== VacancyPublishStatus.Draft) return false;
    const rs = this.roleService;
    return rs.hasAnyRole('Admin', 'Recruiter', 'HR', 'hr');
  }

  /** Step 1 → HR Manager; step 2 → HR Section Head (legacy HRSupervisor = step 2 only); Admin on any step. */
  get canApproveOrReject(): boolean {
    if (!this.vacancy || this.vacancy.publishStatus !== VacancyPublishStatus.PendingApproval) return false;
    const step = this.vacancy.pendingApprovalStep;
    if (step == null) return false;
    const rs = this.roleService;
    if (rs.hasAnyRole('Admin')) return true;
    if (step === 1 && rs.hasAnyRole('HR Manager')) return true;
    if (step === 2 && rs.hasAnyRole('HR Section Head', 'HRSupervisor')) return true;
    return false;
  }

  // ========================
  // APPROVAL ACTIONS
  // ========================

  submitForApproval(): void {
    if (!this.vacancy || !this.canSubmitForApproval) return;
    this.isProcessing = true;
    this.service.submitForApproval(this.vacancy.vacancyId).subscribe({
      next: (res) => {
        this.snackBar.open(res?.message || 'Submitted for approval!', 'Close', { duration: 3000 });
        this.vacancy!.publishStatus = VacancyPublishStatus.PendingApproval;
        this.isProcessing = false;
      },
      error: (err) => {
        const body = err?.error;
        const msg =
          (typeof body === 'object' && body?.message)
            ? body.message
            : typeof body === 'string'
              ? body
              : err?.status === 403
                ? 'Not allowed for your role (403).'
                : 'Failed to submit.';
        this.snackBar.open(msg, 'Close', { duration: 5000 });
        this.isProcessing = false;
      }
    });
  }

  approveVacancy(): void {
    if (!this.vacancy || !this.canApproveOrReject) return;
    this.isProcessing = true;
    this.service.approveVacancy(this.vacancy.vacancyId).subscribe({
      next: (res) => {
        this.snackBar.open(res?.message || 'Approved!', 'Close', { duration: 3000 });
        // Reload to get the updated status
        this.reloadVacancy();
      },
      error: (err) => {
        this.snackBar.open(err?.error?.message || 'Failed to approve.', 'Close', { duration: 4000 });
        this.isProcessing = false;
      }
    });
  }

  rejectVacancy(): void {
    if (!this.vacancy || !this.canApproveOrReject) return;
    const reason = prompt('Enter rejection reason (optional):');
    this.isProcessing = true;
    this.service.rejectVacancy(this.vacancy.vacancyId, reason || undefined).subscribe({
      next: (res) => {
        this.snackBar.open(res?.message || 'Rejected.', 'Close', { duration: 3000 });
        this.vacancy!.publishStatus = VacancyPublishStatus.Draft;
        this.isProcessing = false;
      },
      error: (err) => {
        this.snackBar.open(err?.error?.message || 'Failed to reject.', 'Close', { duration: 4000 });
        this.isProcessing = false;
      }
    });
  }

  private reloadVacancy(): void {
    this.service.getVacancyById(this.vacancy!.vacancyId).subscribe({
      next: (v) => { this.vacancy = v; this.isProcessing = false; },
      error: () => { this.isProcessing = false; }
    });
  }

  // ========================
  // HELPERS
  // ========================

  getStatusLabel(status: number): string { return VacancyPublishStatusLabels[status] || 'Unknown'; }

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

  getWorkTypeLabel(type: number): string {
    const labels: Record<number, string> = { 0: 'Full-time', 1: 'Part-time', 2: 'Contract', 3: 'Internship' };
    return labels[type] || 'Unknown';
  }

  getWorkLocationLabel(loc: number): string {
    const labels: Record<number, string> = { 0: 'On-site', 1: 'Remote', 2: 'Hybrid' };
    return labels[loc] || 'Unknown';
  }

  goBack(): void { this.router.navigate(['/admin/recruiter-management/vacancies']); }
  editVacancy(): void { this.router.navigate(['/admin/recruiter-management/vacancies/edit', this.vacancy!.vacancyId]); }
  viewApplications(): void { this.router.navigate(['/admin/recruiter-management/applications'], { queryParams: { vacancyId: this.vacancy!.vacancyId } }); }
}
