import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { ApplicationService } from '../../../core/services/application.service';
import { ApplicationDto, ApplicationStatusLabels } from '../../../shared/interfaces/models';

@Component({
  selector: 'app-my-applications',
  standalone: true,
  imports: [
    CommonModule,
    RouterLink,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatChipsModule,
    MatPaginatorModule,
    MatProgressSpinnerModule
  ],
  templateUrl: './my-applications.component.html',
  styleUrl: './my-applications.component.scss'
})
export class MyApplicationsComponent implements OnInit {
  applications: ApplicationDto[] = [];
  loading = false;
  totalCount = 0;
  pageSize = 10;
  pageIndex = 0;

  constructor(private applicationService: ApplicationService) {}

  ngOnInit(): void {
    this.loadApplications();
  }

  loadApplications(): void {
    this.loading = true;
    this.applicationService.getMyApplications({
      pageNumber: this.pageIndex + 1,
      pageSize: this.pageSize
    }).subscribe({
      next: (res) => {
        this.applications = res.data;
        this.totalCount = res.metadata?.totalCount || 0;
        this.loading = false;
      },
      error: () => {
        this.loading = false;
      }
    });
  }

  onPageChange(event: PageEvent): void {
    this.pageIndex = event.pageIndex;
    this.pageSize = event.pageSize;
    this.loadApplications();
  }

  getStatusLabel(status: number): string {
    return ApplicationStatusLabels[status] || 'Unknown';
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
