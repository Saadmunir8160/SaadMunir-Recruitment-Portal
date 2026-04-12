import { Component, OnInit, ViewChild, AfterViewInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule, Router } from '@angular/router';
import { MatTableModule, MatTableDataSource } from '@angular/material/table';
import { MatPaginatorModule, MatPaginator, PageEvent } from '@angular/material/paginator';
import { MatSortModule, MatSort } from '@angular/material/sort';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { MatCardModule } from '@angular/material/card';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTooltipModule } from '@angular/material/tooltip';
import { RecruiterManagementService } from '../../services/recruiter-management.service';
import { CandidateListDto, CandidateProfileStatusLabels } from '../../models/recruitment.models';

@Component({
  selector: 'app-candidates-list',
  standalone: true,
  imports: [
    CommonModule, FormsModule, RouterModule,
    MatTableModule, MatPaginatorModule, MatSortModule,
    MatFormFieldModule, MatInputModule, MatSelectModule,
    MatButtonModule, MatIconModule, MatChipsModule,
    MatCardModule, MatProgressSpinnerModule, MatTooltipModule
  ],
  templateUrl: './candidates-list.component.html',
  styleUrls: ['./candidates-list.component.scss']
})
export class CandidatesListComponent implements OnInit {
  displayedColumns = ['index', 'fullName', 'email', 'mobileNumber', 'nationality', 'profileStatus', 'createdDate', 'actions'];
  dataSource = new MatTableDataSource<CandidateListDto>([]);
  loading = true;
  searchTerm = '';
  statusFilter = '';
  currentPage = 0;
  pageSize = 10;
  totalRecords = 0;

  constructor(
    private recruiterService: RecruiterManagementService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.loadCandidates();
  }

  loadCandidates(): void {
    this.loading = true;
    this.recruiterService.getCandidates(this.currentPage + 1, this.pageSize).subscribe({
      next: (res) => {
        this.dataSource.data = res.data || [];
        this.totalRecords = res.metadata?.totalCount || 0;
        this.loading = false;
      },
      error: () => {
        this.dataSource.data = [];
        this.loading = false;
      }
    });
  }

  applyFilters(): void {
    this.currentPage = 0;
    this.loadCandidates();
  }

  onPageChange(event: PageEvent): void {
    this.currentPage = event.pageIndex;
    this.pageSize = event.pageSize;
    this.loadCandidates();
  }

  viewCandidate(row: CandidateListDto): void {
    this.router.navigate(['/admin/recruiter-management/candidates', row.candidateId]);
  }

  getStatusLabel(status: number): string {
    return CandidateProfileStatusLabels[status] || 'Unknown';
  }

  getStatusClass(status: number): string {
    return ({
      0: 'status-warning', 1: 'status-info', 2: 'status-primary',
      3: 'status-success', 4: 'status-warning', 5: 'status-danger'
    } as Record<number, string>)[status] || 'status-default';
  }
}
