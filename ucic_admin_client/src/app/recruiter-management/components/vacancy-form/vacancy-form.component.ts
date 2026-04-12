import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatDividerModule } from '@angular/material/divider';
import { MatSliderModule } from '@angular/material/slider';
import { MatTooltipModule } from '@angular/material/tooltip';
import { RecruiterManagementService } from '../../services/recruiter-management.service';
import { CreateVacancyDto } from '../../models/recruitment.models';

@Component({
  selector: 'app-vacancy-form',
  standalone: true,
  imports: [
    CommonModule, ReactiveFormsModule, MatCardModule, MatButtonModule,
    MatIconModule, MatFormFieldModule, MatInputModule, MatSelectModule,
    MatDatepickerModule, MatNativeDateModule, MatProgressSpinnerModule,
    MatSnackBarModule, MatDividerModule, MatSliderModule, MatTooltipModule
  ],
  templateUrl: './vacancy-form.component.html',
  styleUrl: './vacancy-form.component.scss'
})
export class VacancyFormComponent implements OnInit {
  form!: FormGroup;
  isEdit = false;
  isLoading = false;
  isSaving = false;
  vacancyId: number | null = null;

  constructor(
    private fb: FormBuilder,
    private route: ActivatedRoute,
    private router: Router,
    private service: RecruiterManagementService,
    private snackBar: MatSnackBar
  ) {}

  ngOnInit(): void {
    this.initForm();
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.isEdit = true;
      this.vacancyId = +id;
      this.loadVacancy(this.vacancyId);
    }
  }

  private initForm(): void {
    this.form = this.fb.group({
      jobTitle: ['', Validators.required],
      departmentName: ['', Validators.required],
      location: ['', Validators.required],
      jobGrade: [''],
      numberOfPositions: [1, [Validators.required, Validators.min(1)]],
      jobDescription: ['', Validators.required],
      qualifications: [''],
      requirements: [''],
      salaryRangeMin: [0],
      salaryRangeMax: [0],
      currency: ['SAR'],
      workType: [0],
      workLocation: [0],
      requiredExperienceMin: [0],
      requiredExperienceMax: [0],
      requiredNationality: [''],
      requiredSpecialization: [''],
      requiredQualification: [''],
      closingDate: ['', Validators.required],
      weightSpecialization: [25],
      weightExperience: [25],
      weightQualification: [20],
      weightNationality: [10],
      weightLocation: [10],
      weightCertifications: [10],
      matchThreshold: [60]
    });
  }

  private loadVacancy(id: number): void {
    this.isLoading = true;
    this.service.getVacancyById(id).subscribe({
      next: (v) => {
        const workTypeMap: Record<string, number> = { FullTime: 0, PartTime: 1, Contract: 2 };
        const workLocationMap: Record<string, number> = { OnSite: 0, Remote: 1, Hybrid: 2 };
        this.form.patchValue({
          ...v,
          workType: typeof v.workType === 'string' ? (workTypeMap[v.workType as string] ?? v.workType) : v.workType,
          workLocation: typeof v.workLocation === 'string' ? (workLocationMap[v.workLocation as string] ?? v.workLocation) : v.workLocation,
          closingDate: v.closingDate ? new Date(v.closingDate) : ''
        });
        this.isLoading = false;
      },
      error: () => {
        this.snackBar.open('Failed to load vacancy', 'Close', { duration: 3000 });
        this.isLoading = false;
      }
    });
  }

  getTotalWeight(): number {
    const v = this.form.value;
    return (v.weightSpecialization || 0) + (v.weightExperience || 0) + (v.weightQualification || 0)
      + (v.weightNationality || 0) + (v.weightLocation || 0) + (v.weightCertifications || 0);
  }

  onSubmit(): void {
    if (this.form.invalid) return;
    this.isSaving = true;

    const dto: CreateVacancyDto = {
      ...this.form.value,
      closingDate: this.form.value.closingDate instanceof Date
        ? this.form.value.closingDate.toISOString()
        : this.form.value.closingDate
    };

    const req$ = this.isEdit
      ? this.service.updateVacancy(this.vacancyId!, dto)
      : this.service.createVacancy(dto);

    req$.subscribe({
      next: () => {
        this.snackBar.open(
          this.isEdit ? 'Vacancy updated successfully' : 'Vacancy created successfully',
          'Close', { duration: 3000 }
        );
        this.router.navigate(['/admin/recruiter-management/vacancies']);
      },
      error: () => {
        this.snackBar.open('Failed to save vacancy', 'Close', { duration: 3000 });
        this.isSaving = false;
      }
    });
  }

  goBack(): void { this.router.navigate(['/admin/recruiter-management/vacancies']); }
}
