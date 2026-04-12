import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { MatDividerModule } from '@angular/material/divider';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { VacancyService } from '../../../core/services/vacancy.service';
import { ApplicationService } from '../../../core/services/application.service';
import { AuthService } from '../../../core/services/auth.service';
import { VacancyDetailDto, WorkTypeLabels, WorkLocationLabels } from '../../../shared/interfaces/models';

@Component({
  selector: 'app-job-detail',
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
    MatSnackBarModule,
    MatDialogModule
  ],
  templateUrl: './job-detail.component.html',
  styleUrl: './job-detail.component.scss'
})
export class JobDetailComponent implements OnInit {
  vacancy: VacancyDetailDto | null = null;
  loading = false;
  applying = false;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private vacancyService: VacancyService,
    private applicationService: ApplicationService,
    public authService: AuthService,
    private snackBar: MatSnackBar
  ) {}

  ngOnInit(): void {
    const id = Number(this.route.snapshot.paramMap.get('id'));
    if (id) {
      this.loadVacancy(id);
    }
  }

  loadVacancy(id: number): void {
    this.loading = true;
    this.vacancyService.getVacancyById(id).subscribe({
      next: (res) => {
        this.vacancy = res.data;
        this.loading = false;
      },
      error: () => {
        this.loading = false;
      }
    });
  }

  applyToJob(): void {
    if (!this.vacancy) return;

    if (!this.authService.isLoggedIn()) {
      this.router.navigate(['/login']);
      return;
    }

    if (this.authService.getUserRole() !== 'candidate') {
      this.snackBar.open('Only candidates can apply for jobs.', 'Close', { duration: 4000 });
      return;
    }

    this.applying = true;
    this.applicationService.applyToVacancy(this.vacancy.vacancyId).subscribe({
      next: () => {
        this.applying = false;
        this.snackBar.open('Application submitted successfully!', 'Close', { duration: 5000 });
        this.router.navigate(['/my-applications']);
      },
      error: (err) => {
        this.applying = false;
        const msg = err.error?.message || 'Failed to submit application. Please complete your profile first.';
        this.snackBar.open(msg, 'Close', { duration: 5000 });
      }
    });
  }

  goBack(): void {
    this.router.navigate(['/jobs']);
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

  parseJsonArray(json: string): string[] {
    try {
      return JSON.parse(json);
    } catch {
      return json.split(',').map(s => s.trim());
    }
  }
}
