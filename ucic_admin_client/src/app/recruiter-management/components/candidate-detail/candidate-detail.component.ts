import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTabsModule } from '@angular/material/tabs';
import { MatTableModule } from '@angular/material/table';
import { MatChipsModule } from '@angular/material/chips';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDialogModule, MatDialog } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { RecruiterManagementService } from '../../services/recruiter-management.service';
import {
  CandidateDetailDto,
  CandidateProfileStatusLabels,
  DocumentTypeLabels,
  OcrVerificationResultDto,
  MismatchSeverityLabels
} from '../../models/recruitment.models';

@Component({
  selector: 'app-candidate-detail',
  standalone: true,
  imports: [
    CommonModule, FormsModule, RouterModule,
    MatCardModule, MatButtonModule, MatIconModule, MatTabsModule,
    MatTableModule, MatChipsModule, MatProgressSpinnerModule,
    MatDialogModule, MatFormFieldModule, MatInputModule, MatTooltipModule,
    MatSnackBarModule
  ],
  templateUrl: './candidate-detail.component.html',
  styleUrls: ['./candidate-detail.component.scss']
})
export class CandidateDetailComponent implements OnInit {
  candidate: CandidateDetailDto | null = null;
  loading = true;
  showModal: 'rejection' | 'correction' | null = null;
  modalReason = '';

  ocrResults: OcrVerificationResultDto[] = [];
  isOcrLoading = false;
  isMarkingVerified = false;

  eduColumns = ['qualification', 'major', 'institution', 'graduationYear', 'gradeOrGPA', 'country'];
  expColumns = ['employer', 'jobTitle', 'period', 'salary', 'country'];
  docColumns = ['documentType', 'fileName', 'fileSize', 'aiStatus', 'confidence', 'uploadedDate', 'actions'];
  ocrColumns = ['fieldName', 'extractedValue', 'enteredValue', 'isMatch', 'severity', 'processedDate'];

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private recruiterService: RecruiterManagementService,
    private snackBar: MatSnackBar
  ) {}

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) this.loadCandidate(+id);
  }

  loadCandidate(id: number): void {
    this.loading = true;
    this.recruiterService.getCandidateById(id).subscribe({
      next: (c) => {
        this.candidate = c;
        this.loading = false;
        this.loadOcrResults(id);
      },
      error: () => { this.loading = false; }
    });
  }

  loadOcrResults(candidateId: number): void {
    this.isOcrLoading = true;
    this.recruiterService.getOcrResults(candidateId).subscribe({
      next: (results) => { this.ocrResults = results; this.isOcrLoading = false; },
      error: () => { this.isOcrLoading = false; }
    });
  }

  triggerVerification(docId: number): void {
    this.recruiterService.triggerOcrVerification(docId).subscribe({
      next: () => {
        this.snackBar.open('OCR verification triggered. Results will update shortly.', 'Dismiss', { duration: 4000 });
        if (this.candidate) {
          setTimeout(() => this.loadOcrResults(this.candidate!.candidateId), 5000);
        }
      },
      error: () => this.snackBar.open('Failed to trigger OCR verification.', 'Dismiss', { duration: 3000 })
    });
  }

  markAsVerified(): void {
    if (!this.candidate) return;
    this.isMarkingVerified = true;
    this.recruiterService.markOcrVerified(this.candidate.candidateId).subscribe({
      next: () => {
        this.candidate!.ocrVerificationStatus = 1; // Verified
        this.snackBar.open('Candidate marked as OCR verified.', 'Dismiss', { duration: 4000 });
        this.isMarkingVerified = false;
      },
      error: () => {
        this.snackBar.open('Failed to mark as verified.', 'Dismiss', { duration: 3000 });
        this.isMarkingVerified = false;
      }
    });
  }

  goBack(): void {
    this.router.navigate(['/admin/recruiter-management/candidates']);
  }

  updateStatus(status: number, reason?: string): void {
    if (!this.candidate) return;
    this.recruiterService.updateCandidateStatus(this.candidate.candidateId, { status, reason }).subscribe({
      next: () => {
        this.showModal = null;
        this.modalReason = '';
        this.loadCandidate(this.candidate!.candidateId);
      }
    });
  }

  downloadDocument(docId: number): void {
    this.recruiterService.downloadDocument(docId).subscribe({
      next: (blob) => {
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = 'document';
        a.click();
        window.URL.revokeObjectURL(url);
      }
    });
  }

  getStatusLabel(s: number): string { return CandidateProfileStatusLabels[s] || 'Unknown'; }
  getStatusClass(s: number): string {
    return ({ 0: 'status-warning', 1: 'status-info', 2: 'status-primary', 3: 'status-success', 4: 'status-warning', 5: 'status-danger' } as any)[s] || 'status-default';
  }
  getDocTypeLabel(t: number): string { return DocumentTypeLabels[t] || 'Other'; }
  getSeverityLabel(s: number): string { return MismatchSeverityLabels[s] || 'None'; }
  getSeverityClass(s: number): string {
    return ({ 0: 'severity-none', 1: 'severity-minor', 2: 'severity-major', 3: 'severity-critical' } as any)[s] || 'severity-none';
  }
}
