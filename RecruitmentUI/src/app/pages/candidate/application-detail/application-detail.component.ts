import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { MatDividerModule } from '@angular/material/divider';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatStepperModule } from '@angular/material/stepper';
import { ApplicationService } from '../../../core/services/application.service';
import {
  ApplicationDetailDto,
  ApplicationStatusLabels,
  MatchResultDto
} from '../../../shared/interfaces/models';

@Component({
  selector: 'app-application-detail',
  standalone: true,
  imports: [
    CommonModule,
    RouterLink,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatChipsModule,
    MatDividerModule,
    MatProgressSpinnerModule,
    MatProgressBarModule,
    MatStepperModule
  ],
  templateUrl: './application-detail.component.html',
  styleUrl: './application-detail.component.scss'
})
export class ApplicationDetailComponent implements OnInit {
  application: ApplicationDetailDto | null = null;
  matchResult: MatchResultDto | null = null;
  loading = false;
  progressSteps: { label: string; completed: boolean; active: boolean }[] = [];
  matchBreakdown: { label: string; value: number }[] = [];

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private applicationService: ApplicationService
  ) {}

  ngOnInit(): void {
    const id = Number(this.route.snapshot.paramMap.get('id'));
    if (id) {
      this.loadApplication(id);
    }
  }

  loadApplication(id: number): void {
    this.loading = true;
    this.applicationService.getApplicationById(id).subscribe({
      next: (res) => {
        this.application = res.data;
        this.buildProgressSteps(res.data.status);
        if (res.data.matchResult) {
          this.matchResult = res.data.matchResult;
          this.matchBreakdown = [
            { label: 'Specialization', value: res.data.matchResult.specializationScore ?? 0 },
            { label: 'Experience',     value: res.data.matchResult.experienceScore ?? 0 },
            { label: 'Qualification', value: res.data.matchResult.qualificationScore ?? 0 },
            { label: 'Nationality',   value: res.data.matchResult.nationalityScore ?? 0 },
            { label: 'Location',      value: res.data.matchResult.locationScore ?? 0 },
            { label: 'Certifications', value: res.data.matchResult.certificationScore ?? 0 }
          ];
        }
        this.loading = false;
      },
      error: () => {
        this.loading = false;
      }
    });
  }

  buildProgressSteps(currentStatus: number): void {
    const steps = [
      { label: 'Applied', threshold: 0 },
      { label: 'AI Matching', threshold: 1 },
      { label: 'Screening', threshold: 3 },
      { label: 'Shortlisted', threshold: 6 },
      { label: 'Interview', threshold: 7 },
      { label: 'HR Review', threshold: 9 },
      { label: 'Medical', threshold: 13 },
      { label: 'Job Offer', threshold: 17 },
      { label: 'Hired', threshold: 24 }
    ];

    this.progressSteps = steps.map((step, index) => {
      const nextThreshold = steps[index + 1]?.threshold ?? Infinity;
      return {
        label: step.label,
        completed: currentStatus >= nextThreshold,
        active: currentStatus >= step.threshold && currentStatus < nextThreshold
      };
    });
  }

  goBack(): void {
    this.router.navigate(['/my-applications']);
  }

  getStatusLabel(status: number): string {
    return ApplicationStatusLabels[status] || 'Unknown';
  }

  getScoreColor(score: number): string {
    return score >= 70 ? 'primary' : score >= 40 ? 'accent' : 'warn';
  }

  getStatusClass(status: number): string {
    if (status === 0) return 'status-applied';
    if (status === 1) return 'status-matched';
    if (status === 2) return 'status-not-matched';
    if (status >= 3 && status <= 6) return 'status-screening';
    if (status >= 7 && status <= 12) return 'status-interview';
    if (status >= 13 && status <= 16) return 'status-screening';
    if (status >= 17 && status <= 21) return 'status-offer';
    if (status === 24) return 'status-hired';
    if (status === 25 || status === 22) return 'status-rejected';
    if (status === 26) return 'status-withdrawn';
    return 'status-default';
  }
}
