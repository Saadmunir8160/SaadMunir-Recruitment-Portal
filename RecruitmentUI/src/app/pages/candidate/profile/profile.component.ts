import { Component, OnInit, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormArray, FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSelectModule } from '@angular/material/select';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatExpansionModule } from '@angular/material/expansion';
import { MatChipsModule } from '@angular/material/chips';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatDividerModule } from '@angular/material/divider';
import { MatTabsModule } from '@angular/material/tabs';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatStepper, MatStepperModule } from '@angular/material/stepper';
import { StepperSelectionEvent } from '@angular/cdk/stepper';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { forkJoin } from 'rxjs';
import { CandidateService } from '../../../core/services/candidate.service';
import {
  CandidateDetailDto,
  CreateCandidateDto,
  UpdateCandidateDto,
  CreateEducationDto,
  CreateExperienceDto,
  CandidateDocumentDto,
  CvParseResult,
  ProfileStatus,
  DocumentTypeLabels
} from '../../../shared/interfaces/models';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatSelectModule,
    MatDatepickerModule,
    MatNativeDateModule,
    MatCheckboxModule,
    MatExpansionModule,
    MatChipsModule,
    MatProgressSpinnerModule,
    MatSnackBarModule,
    MatDividerModule,
    MatTabsModule,
    MatTooltipModule,
    MatStepperModule,
    MatProgressBarModule
  ],
  templateUrl: './profile.component.html',
  styleUrl: './profile.component.scss'
})
export class ProfileComponent implements OnInit {
  @ViewChild('stepper') stepper!: MatStepper;

  // --- Form groups (shared between wizard and edit mode) ---
  personalForm!: FormGroup;
  educationForm!: FormGroup;
  experienceForm!: FormGroup;
  cvStepForm!: FormGroup;

  // --- State ---
  candidate: CandidateDetailDto | null = null;
  documents: CandidateDocumentDto[] = [];
  isNew = false;
  loading = false;
  saving = false;
  submitting = false;

  // --- CV Wizard state ---
  cvFile: File | null = null;
  autoFill = true;
  parsing = false;
  cvAnalyzed = false;
  parseFailed = false;
  pendingDocuments: { file: File; type: number }[] = [];
  wizardDocType = 1;

  // --- Document upload (edit mode) ---
  uploading = false;
  showUploadForm = false;
  selectedFile: File | null = null;
  selectedDocType = 0;

  docTypes = Object.entries(DocumentTypeLabels).map(([value, label]) => ({
    value: Number(value), label
  }));

  /** All doc types except CV (type 0) — CV is handled in Step 1 */
  get wizardDocTypes() {
    return this.docTypes.filter(t => t.value !== 0);
  }

  get isLocked(): boolean {
    return this.candidate?.isProfileLocked ?? false;
  }

  get educations(): FormArray {
    return this.educationForm.get('educations') as FormArray;
  }

  get experiences(): FormArray {
    return this.experienceForm.get('experiences') as FormArray;
  }

  constructor(
    private fb: FormBuilder,
    private candidateService: CandidateService,
    private snackBar: MatSnackBar
  ) {}

  ngOnInit(): void {
    this.buildForms();
    this.loadProfile();
  }

  buildForms(): void {
    this.cvStepForm = this.fb.group({ cv: [null, Validators.required] });
    this.personalForm = this.fb.group({
      fullName: ['', Validators.required],
      email: ['', [Validators.required, Validators.email]],
      mobileNumber: ['', Validators.required],
      nationalId: [''],
      idType: [0],
      gender: [null],
      dateOfBirth: [null],
      nationality: [''],
      nationalAddress: [''],
      residenceCity: ['']
    });
    this.educationForm = this.fb.group({ educations: this.fb.array([]) });
    this.experienceForm = this.fb.group({ experiences: this.fb.array([]) });
  }

  loadProfile(): void {
    this.loading = true;
    this.candidateService.getProfile().subscribe({
      next: (res) => {
        this.candidate = res.data;
        this.documents = res.data.documents || [];
        this.patchForms(res.data);
        this.loading = false;
        if (this.isLocked) {
          this.personalForm.disable();
          this.educationForm.disable();
          this.experienceForm.disable();
        }
      },
      error: (err) => {
        this.loading = false;
        if (err.status === 404) this.candidate = null;
      }
    });
  }

  patchForms(data: CandidateDetailDto): void {
    this.personalForm.patchValue({
      fullName: data.fullName,
      email: data.email,
      mobileNumber: data.mobileNumber,
      nationalId: data.nationalId,
      idType: data.idType,
      gender: data.gender,
      dateOfBirth: data.dateOfBirth ? new Date(data.dateOfBirth) : null,
      nationality: data.nationality,
      nationalAddress: data.nationalAddress,
      residenceCity: data.residenceCity
    });

    this.educations.clear();
    (data.educations || []).forEach(edu => {
      this.educations.push(this.fb.group({
        qualification: [edu.qualification, Validators.required],
        major: [edu.major],
        institution: [edu.institution],
        graduationYear: [edu.graduationYear],
        gradeOrGPA: [edu.gradeOrGPA],
        country: [edu.country]
      }));
    });

    this.experiences.clear();
    (data.experiences || []).forEach(exp => {
      this.experiences.push(this.fb.group({
        jobTitle: [exp.jobTitle],
        employer: [exp.employer],
        startDate: [exp.startDate ? new Date(exp.startDate) : null],
        endDate: [exp.endDate ? new Date(exp.endDate) : null],
        isCurrent: [exp.isCurrent],
        salary: [exp.salary],
        currency: [exp.currency || 'SAR'],
        country: [exp.country],
        description: [exp.description]
      }));
    });
  }

  // ── Wizard ──────────────────────────────────────────────────────────────────

  startCreating(): void {
    this.isNew = true;
    this.buildForms();
    this.cvFile = null;
    this.cvAnalyzed = false;
    this.autoFill = true;
    this.pendingDocuments = [];
    this.wizardDocType = 1;
  }

  onCvFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files?.length) {
      this.cvFile = input.files[0];
      this.cvAnalyzed = false;
      this.parseFailed = false;
      this.cvStepForm.get('cv')?.setValue(this.cvFile.name);
    }
  }

  toggleAutoFill(checked: boolean): void {
    this.autoFill = checked;
    if (checked) {
      this.parseFailed = false;
    }
  }

  onStepChange(event: StepperSelectionEvent): void {
    // If the user clicked a header tab to jump away from step 0 while
    // auto-fill is enabled but parsing hasn't run yet, snap back and parse first.
    // Do NOT re-trigger if parsing already failed — that would cause an infinite loop.
    if (
      event.previouslySelectedIndex === 0 &&
      this.cvFile &&
      this.autoFill &&
      !this.cvAnalyzed &&
      !this.parsing &&
      !this.parseFailed
    ) {
      setTimeout(() => { this.stepper.selectedIndex = 0; }, 0);
      this.analyzeAndContinue();
    }
  }

  analyzeAndContinue(): void {
    if (!this.cvFile || !this.autoFill) {
      this.stepper.next();
      return;
    }
    this.parsing = true;
    this.candidateService.parseCvPreview(this.cvFile).subscribe({
      next: (res) => {
        this.parsing = false;
        if (res?.data) {
          this.applyParseResult(res.data);
          this.cvAnalyzed = true;
          this.snackBar.open('CV analyzed! Form has been pre-filled.', 'Close', { duration: 4000 });
        }
        this.stepper.next();
      },
      error: () => {
        this.parsing = false;
        this.parseFailed = true;
        this.snackBar.open(
          'CV analysis failed. Uncheck "Auto-fill form from CV" to continue manually, or upload a different file.',
          'Dismiss',
          { duration: 8000 }
        );
      }
    });
  }

  applyParseResult(result: CvParseResult): void {
    const info = result.personalInfo;
    if (info) {
      this.personalForm.patchValue({
        fullName: info.fullName || '',
        email: info.email || '',
        mobileNumber: info.phone || '',
        nationalId: info.nationalId || '',
        nationality: info.nationality || '',
        nationalAddress: info.address || '',
        residenceCity: info.city || '',
        dateOfBirth: info.dateOfBirth ? new Date(info.dateOfBirth) : null
      });
    }

    this.educations.clear();
    (result.education || []).forEach(edu => {
      this.educations.push(this.fb.group({
        qualification: [edu.qualification || '', Validators.required],
        major: [edu.major],
        institution: [edu.institution],
        graduationYear: [edu.graduationYear],
        gradeOrGPA: [edu.grade],
        country: [edu.country]
      }));
    });

    this.experiences.clear();
    (result.experience || []).forEach(exp => {
      this.experiences.push(this.fb.group({
        jobTitle: [exp.jobTitle],
        employer: [exp.employer],
        startDate: [exp.startDate ? new Date(exp.startDate) : null],
        endDate: [exp.endDate ? new Date(exp.endDate) : null],
        isCurrent: [exp.isCurrent || false],
        salary: [exp.salary],
        currency: [exp.currency || 'SAR'],
        country: [exp.country],
        description: [exp.description]
      }));
    });
  }

  submitWizard(): void {
    if (this.personalForm.invalid) {
      this.personalForm.markAllAsTouched();
      this.snackBar.open('Please fill in all required fields on the Personal Info step.', 'Close', { duration: 3000 });
      return;
    }

    const payload = this.buildCandidatePayload();
    this.submitting = true;

    this.candidateService.createCandidate(payload).subscribe({
      next: (res) => {
        const candidateId = res.data;
        const uploads = [
          ...(this.cvFile ? [this.candidateService.uploadDocument(candidateId, this.cvFile, 0)] : []),
          ...this.pendingDocuments.map(d => this.candidateService.uploadDocument(candidateId, d.file, d.type))
        ];

        if (uploads.length === 0) {
          this.submitting = false;
          this.isNew = false;
          this.snackBar.open('Profile created successfully!', 'Close', { duration: 4000 });
          this.loadProfile();
          return;
        }

        forkJoin(uploads).subscribe({
          next: () => {
            this.submitting = false;
            this.isNew = false;
            this.snackBar.open('Profile created and documents uploaded!', 'Close', { duration: 4000 });
            this.loadProfile();
          },
          error: () => {
            this.submitting = false;
            this.isNew = false;
            this.snackBar.open('Profile created! Some documents failed to upload — check the Documents tab.', 'Close', { duration: 6000 });
            this.loadProfile();
          }
        });
      },
      error: (err) => {
        this.submitting = false;
        this.snackBar.open(err.error?.message || 'Failed to create profile.', 'Close', { duration: 5000 });
      }
    });
  }

  // ── Edit mode ────────────────────────────────────────────────────────────────

  saveEdit(): void {
    if (this.personalForm.invalid) {
      this.personalForm.markAllAsTouched();
      this.snackBar.open('Please fill in all required fields.', 'Close', { duration: 3000 });
      return;
    }
    this.saving = true;
    this.candidateService.updateCandidate(this.candidate!.candidateId, this.buildCandidatePayload() as UpdateCandidateDto).subscribe({
      next: () => {
        this.saving = false;
        this.snackBar.open('Profile updated successfully!', 'Close', { duration: 4000 });
        this.loadProfile();
      },
      error: (err) => {
        this.saving = false;
        this.snackBar.open(err.error?.message || 'Failed to update profile.', 'Close', { duration: 5000 });
      }
    });
  }

  submitProfile(): void {
    if (!this.candidate) return;
    this.submitting = true;
    this.candidateService.submitProfile(this.candidate.candidateId).subscribe({
      next: () => {
        this.submitting = false;
        this.snackBar.open('Profile submitted for review!', 'Close', { duration: 4000 });
        this.loadProfile();
      },
      error: (err) => {
        this.submitting = false;
        this.snackBar.open(err.error?.message || 'Failed to submit profile.', 'Close', { duration: 5000 });
      }
    });
  }

  private buildCandidatePayload(): CreateCandidateDto {
    const pv = this.personalForm.getRawValue();
    const eduVal: CreateEducationDto[] = this.educations.getRawValue().map((e: any) => ({
      ...e,
      graduationYear: e.graduationYear || undefined
    }));
    const expVal: CreateExperienceDto[] = this.experiences.getRawValue().map((e: any) => ({
      ...e,
      startDate: e.startDate ? new Date(e.startDate).toISOString().split('T')[0] : undefined,
      endDate: e.endDate ? new Date(e.endDate).toISOString().split('T')[0] : undefined,
      salary: e.salary || undefined
    }));
    return {
      fullName: pv.fullName,
      email: pv.email,
      mobileNumber: pv.mobileNumber,
      nationalId: pv.nationalId || undefined,
      idType: pv.idType,
      gender: pv.gender,
      dateOfBirth: pv.dateOfBirth ? new Date(pv.dateOfBirth).toISOString().split('T')[0] : undefined,
      nationality: pv.nationality || undefined,
      nationalAddress: pv.nationalAddress || undefined,
      residenceCity: pv.residenceCity || undefined,
      educations: eduVal,
      experiences: expVal
    };
  }

  // ── Education / Experience helpers ───────────────────────────────────────────

  addEducation(): void {
    this.educations.push(this.fb.group({
      qualification: ['', Validators.required],
      major: [''],
      institution: [''],
      graduationYear: [null],
      gradeOrGPA: [''],
      country: ['']
    }));
  }

  removeEducation(i: number): void { this.educations.removeAt(i); }

  addExperience(): void {
    this.experiences.push(this.fb.group({
      jobTitle: [''],
      employer: [''],
      startDate: [null],
      endDate: [null],
      isCurrent: [false],
      salary: [null],
      currency: ['SAR'],
      country: [''],
      description: ['']
    }));
  }

  removeExperience(i: number): void { this.experiences.removeAt(i); }

  getEducationGroup(i: number): FormGroup { return this.educations.at(i) as FormGroup; }
  getExperienceGroup(i: number): FormGroup { return this.experiences.at(i) as FormGroup; }

  // ── Document helpers (edit mode) ─────────────────────────────────────────────

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files?.length) {
      this.selectedFile = input.files[0];
      this.showUploadForm = true;
    }
  }

  uploadDocument(): void {
    if (!this.selectedFile || !this.candidate) return;
    this.uploading = true;
    this.candidateService.uploadDocument(this.candidate.candidateId, this.selectedFile, this.selectedDocType).subscribe({
      next: () => {
        this.uploading = false;
        this.showUploadForm = false;
        this.selectedFile = null;
        this.snackBar.open('Document uploaded successfully!', 'Close', { duration: 4000 });
        this.loadProfile();
      },
      error: (err) => {
        this.uploading = false;
        this.snackBar.open(err.error?.message || 'Failed to upload document.', 'Close', { duration: 5000 });
      }
    });
  }

  cancelUpload(): void {
    this.showUploadForm = false;
    this.selectedFile = null;
  }

  addPendingDoc(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files?.length) {
      this.pendingDocuments.push({ file: input.files[0], type: this.wizardDocType });
      input.value = '';
    }
  }

  removePendingDoc(i: number): void {
    this.pendingDocuments.splice(i, 1);
  }

  deleteDocument(docId: number): void {
    if (!this.candidate) return;
    this.candidateService.deleteDocument(this.candidate.candidateId, docId).subscribe({
      next: () => {
        this.snackBar.open('Document deleted.', 'Close', { duration: 3000 });
        this.loadProfile();
      },
      error: (err) => {
        this.snackBar.open(err.error?.message || 'Failed to delete document.', 'Close', { duration: 5000 });
      }
    });
  }

  // ── Utilities ────────────────────────────────────────────────────────────────

  getProfileStatus(status: number): string { return ProfileStatus[status] || 'Unknown'; }

  getStatusClass(status: number): string {
    const classes: Record<number, string> = {
      0: 'status-incomplete', 1: 'status-submitted', 2: 'status-under-review',
      3: 'status-approved', 4: 'status-correction', 5: 'status-rejected'
    };
    return classes[status] || '';
  }

  getDocTypeName(type: number): string { return DocumentTypeLabels[type] || 'Other'; }

  formatFileSize(bytes: number): string {
    if (bytes < 1024) return bytes + ' B';
    if (bytes < 1048576) return (bytes / 1024).toFixed(1) + ' KB';
    return (bytes / 1048576).toFixed(1) + ' MB';
  }
}
