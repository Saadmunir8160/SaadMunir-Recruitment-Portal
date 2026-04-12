import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router, ActivatedRoute, NavigationEnd } from '@angular/router';
import { MatTabsModule } from '@angular/material/tabs';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatBadgeModule } from '@angular/material/badge';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatTooltipModule } from '@angular/material/tooltip';
import { RecruiterManagementService } from './services/recruiter-management.service';
import { filter, Subscription } from 'rxjs';

@Component({
  selector: 'app-recruiter-management',
  standalone: true,
  imports: [
    CommonModule, RouterModule,
    MatTabsModule, MatIconModule, MatButtonModule, MatBadgeModule, MatToolbarModule, MatTooltipModule
  ],
  templateUrl: './recruiter-management.component.html',
  styleUrls: ['./recruiter-management.component.scss']
})
export class RecruiterManagementComponent implements OnInit, OnDestroy {
  activeTab = 'overview';
  tabs = [
    { key: 'overview', label: 'Overview', icon: 'dashboard', count: 0 },
    { key: 'candidates', label: 'Candidates', icon: 'people', count: 0 },
    { key: 'vacancies', label: 'Vacancies', icon: 'work', count: 0 },
    { key: 'applications', label: 'Applications', icon: 'description', count: 0 },
    { key: 'interviews', label: 'Interviews', icon: 'videocam', count: 0 }
  ];
  private routeSub!: Subscription;

  constructor(
    private router: Router,
    private route: ActivatedRoute,
    private recruiterService: RecruiterManagementService
  ) {}

  ngOnInit(): void {
    this.syncActiveTab();
    this.loadCounts();
    this.routeSub = this.router.events
      .pipe(filter(e => e instanceof NavigationEnd))
      .subscribe(() => this.syncActiveTab());
  }

  private syncActiveTab(): void {
    const urlParts = this.router.url.split('/');
    const last = urlParts[urlParts.length - 1]?.split('?')[0];
    const tabKey = this.tabs.find(t => t.key === last)?.key;
    this.activeTab = tabKey || 'overview';
  }

  navigateToTab(tab: string): void {
    this.activeTab = tab;
    this.router.navigate([tab], { relativeTo: this.route });
  }

  loadCounts(): void {
    this.recruiterService.getCandidates(1, 1).subscribe({
      next: (r) => this.tabs[1].count = r.metadata?.totalCount || 0,
      error: () => {}
    });
    this.recruiterService.getVacancies(1, 1).subscribe({
      next: (r) => this.tabs[2].count = r.metadata?.totalCount || 0,
      error: () => {}
    });
    this.recruiterService.getInterviews(1, 1).subscribe({
      next: (r) => this.tabs[4].count = r.metadata?.totalCount || 0,
      error: () => {}
    });
  }

  refreshData(): void {
    this.loadCounts();
  }

  ngOnDestroy(): void {
    this.routeSub?.unsubscribe();
  }
}
