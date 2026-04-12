import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { RecruiterManagementService } from '../../services/recruiter-management.service';
import {
  VacancyListDto,
  VacancyPublishStatus,
  VacancyPublishStatusLabels
} from '../../models/recruitment.models';

@Component({
  selector: 'app-recruitment-overview',
  standalone: true,
  imports: [
    CommonModule, RouterModule,
    MatIconModule, MatProgressSpinnerModule
  ],
  templateUrl: './recruitment-overview.component.html',
  styleUrls: ['./recruitment-overview.component.scss']
})
export class RecruitmentOverviewComponent implements OnInit {
  /** Expose enums / labels to template. */
  readonly VacancyPublishStatus = VacancyPublishStatus;
  readonly VacancyPublishStatusLabels = VacancyPublishStatusLabels;

  totalCandidates = 0;
  totalVacancies = 0;
  totalInterviews = 0;
  pendingApprovals = 0;
  /** Preview rows for pending-approval vacancies (full count in <code>pendingApprovals</code>). */
  pendingVacancyRows: VacancyListDto[] = [];
  loading = true;

  stats: { icon: string; label: string; value: number; route: string; gradient: string; queryParams?: any }[] = [];

  constructor(private recruiterService: RecruiterManagementService) { }

  ngOnInit(): void {
    this.loadStatistics();
  }

  loadStatistics(): void {
    this.loading = true;
    let loaded = 0;
    const total = 4;
    const checkDone = () => {
      if (++loaded >= total) {
        this.buildStats();
        this.loading = false;
      }
    };

    this.recruiterService.getCandidates(1, 1).subscribe({
      next: (r) => { this.totalCandidates = r.metadata?.totalCount || 0; checkDone(); },
      error: () => checkDone()
    });
    this.recruiterService.getVacancies(1, 1).subscribe({
      next: (r) => { this.totalVacancies = r.metadata?.totalCount || 0; checkDone(); },
      error: () => checkDone()
    });
    this.recruiterService.getInterviews(1, 1).subscribe({
      next: (r) => { this.totalInterviews = r.metadata?.totalCount || 0; checkDone(); },
      error: () => checkDone()
    });

    this.recruiterService.getVacancies(1, 15, VacancyPublishStatus.PendingApproval).subscribe({
      next: (r) => {
        this.pendingApprovals = r.metadata?.totalCount || 0;
        this.pendingVacancyRows = r.data || [];
        checkDone();
      },
      error: () => checkDone()
    });
  }

  private buildStats(): void {
    // Order matches reference: Candidates, Vacancies, Pending Approvals, Applications, Interviews
    this.stats = [
      {
        icon: 'people',
        label: 'Total Candidates',
        value: this.totalCandidates,
        route: '../candidates',
        gradient: 'linear-gradient(145deg, #7c4dff, #5c6bc0)'
      },
      {
        icon: 'work',
        label: 'Active Vacancies',
        value: this.totalVacancies,
        route: '../vacancies',
        gradient: 'linear-gradient(145deg, #ff6b9d, #ec407a)'
      },
      {
        icon: 'pending_actions',
        label: 'Pending Approvals',
        value: this.pendingApprovals,
        route: '../vacancies',
        queryParams: { status: 1 },
        gradient: 'linear-gradient(145deg, #ffb74d, #ff9800)'
      },
      {
        icon: 'description',
        label: 'Applications',
        value: 0,
        route: '../applications',
        gradient: 'linear-gradient(145deg, #4fc3f7, #29b6f6)'
      },
      {
        icon: 'videocam',
        label: 'Interviews',
        value: this.totalInterviews,
        route: '../interviews',
        gradient: 'linear-gradient(145deg, #66bb6a, #26a69a)'
      }
    ];
  }
}

