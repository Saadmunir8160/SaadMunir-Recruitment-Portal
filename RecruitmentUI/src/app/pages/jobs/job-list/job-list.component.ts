import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatChipsModule } from '@angular/material/chips';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { VacancyService } from '../../../core/services/vacancy.service';
import { VacancyDto, WorkTypeLabels, WorkLocationLabels } from '../../../shared/interfaces/models';

@Component({
  selector: 'app-job-list',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatFormFieldModule,
    MatInputModule,
    MatChipsModule,
    MatProgressSpinnerModule,
    MatPaginatorModule
  ],
  templateUrl: './job-list.component.html',
  styleUrl: './job-list.component.scss'
})
export class JobListComponent implements OnInit {
  jobs: VacancyDto[] = [];
  loading = false;
  searchTerm = '';
  pageNumber = 1;
  pageSize = 12;
  totalCount = 0;

  constructor(
    private vacancyService: VacancyService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.loadJobs();
  }

  loadJobs(): void {
    this.loading = true;
    this.vacancyService.getPublishedVacancies({
      pageNumber: this.pageNumber,
      pageSize: this.pageSize,
      search: this.searchTerm || undefined
    }).subscribe({
      next: (res) => {
        this.jobs = res.data || [];
        this.totalCount = res.metadata?.totalCount || 0;
        this.loading = false;
      },
      error: () => {
        this.loading = false;
      }
    });
  }

  searchJobs(): void {
    this.pageNumber = 1;
    this.loadJobs();
  }

  onPageChange(event: PageEvent): void {
    this.pageNumber = event.pageIndex + 1;
    this.pageSize = event.pageSize;
    this.loadJobs();
  }

  viewJob(id: number): void {
    this.router.navigate(['/jobs', id]);
  }

  getWorkType(type: number): string {
    return WorkTypeLabels[type] || 'Full Time';
  }

  getWorkLocation(type: number): string {
    return WorkLocationLabels[type] || 'On Site';
  }

  isClosingSoon(date: string): boolean {
    const closing = new Date(date);
    const now = new Date();
    const diffDays = (closing.getTime() - now.getTime()) / (1000 * 60 * 60 * 24);
    return diffDays <= 7 && diffDays >= 0;
  }
}
