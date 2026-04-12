import { Component, OnInit } from '@angular/core';
import { JobsService } from '../services/jobs.service';
import { DepartmentsService } from '../services/departments.service';
import { Router, ActivatedRoute } from '@angular/router';
import { FormBuilder, FormGroup, Validators, FormsModule, ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import { JobType } from '../models/job-type.enum';

@Component({
  selector: 'app-jobs-edit',
  standalone: true,
  templateUrl: './jobs-edit.component.html',
  styleUrl: './jobs-edit.component.scss',
  imports: [CommonModule, FormsModule, ReactiveFormsModule, RouterModule],
  schemas: [CUSTOM_ELEMENTS_SCHEMA]
})
export class JobsEditComponent implements OnInit {
  JobsForm!: FormGroup;
  submitted = false;
  jobId!: number;
  departments: any[] = [];
  jobTypes = Object.values(JobType);
  locationTypes = ['Remote', 'OnSite'];

  constructor(
    private jobsService: JobsService,
    private departmentsService: DepartmentsService,
    private router: Router,
    private route: ActivatedRoute,
    private fb: FormBuilder
  ) {}

  ngOnInit(): void {
    this.jobId = Number(this.route.snapshot.paramMap.get('id'));
    this.loadDepartmentsAndJob();
    this.JobsForm = this.fb.group({
      title: ['', [Validators.required, Validators.minLength(5)]],
      description: ['', [Validators.required]],
      location: ['', [Validators.required]],
      locationType: ['', [Validators.required]],
      department: ['', [Validators.required]],
      jobType: ['', [Validators.required]],
      salary: ['', [Validators.required, Validators.pattern('^[0-9]*$')]],
      isArabic: [false]
    });
  }

  loadDepartmentsAndJob() {
    this.departmentsService.getDepartments().subscribe({
      next: (departments: any) => {
        this.departments = Array.isArray(departments) ? departments : departments.data;
        this.loadJobData();
      },
      error: (error) => {
      }
    });
  }

  loadJobData() {
    this.jobsService.getJobById(this.jobId).subscribe({
      next: (job) => {
        this.JobsForm.patchValue({
          title: job.title,
          description: job.description,
          location: job.location,
          locationType: job.workLocation,
          department: Number(job.departmentId),
          jobType: job.workType,
          salary: job.salary,
          isArabic: job.isArabic
        });
      },
      error: (error) => {
        this.router.navigate(['/jobs']);
      }
    });
  }

  get f() {
    return this.JobsForm.controls;
  }

  submitJobs() {
    this.submitted = true;
    
    if (this.JobsForm.invalid) {
      return;
    }

    const formData = new FormData();
    formData.append('title', this.JobsForm.get('title')?.value);
    formData.append('description', this.JobsForm.get('description')?.value);
    formData.append('location', this.JobsForm.get('location')?.value);
    formData.append('locationType', this.JobsForm.get('locationType')?.value);
    formData.append('department', this.JobsForm.get('department')?.value);
    formData.append('departmentId', this.JobsForm.get('department')?.value);
    formData.append('jobType', this.JobsForm.get('jobType')?.value);
    formData.append('salary', this.JobsForm.get('salary')?.value);
    formData.append('isArabic', this.JobsForm.get('isArabic')?.value);

    this.jobsService.updateJobs(this.jobId, formData).subscribe({
      next: () => {
        this.router.navigate(['/admin/jobs']);
      },
      error: (error) => {
      }
    });
  }

  back() {
    this.router.navigate(['/admin/jobs']);
  }
} 