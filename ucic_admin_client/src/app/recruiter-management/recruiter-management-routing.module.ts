import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { RecruiterManagementComponent } from './recruiter-management.component';

const routes: Routes = [
  {
    path: '',
    component: RecruiterManagementComponent,
    children: [
      { path: '', redirectTo: 'overview', pathMatch: 'full' },
      { path: 'overview', loadComponent: () => import('./components/recruitment-overview/recruitment-overview.component').then(c => c.RecruitmentOverviewComponent) },
      { path: 'candidates', loadComponent: () => import('./components/candidates-list/candidates-list.component').then(c => c.CandidatesListComponent) },
      { path: 'candidates/:id', loadComponent: () => import('./components/candidate-detail/candidate-detail.component').then(c => c.CandidateDetailComponent) },
      { path: 'vacancies', loadComponent: () => import('./components/vacancies-list/vacancies-list.component').then(c => c.VacanciesListComponent) },
      { path: 'vacancies/create', loadComponent: () => import('./components/vacancy-form/vacancy-form.component').then(c => c.VacancyFormComponent) },
      { path: 'vacancies/edit/:id', loadComponent: () => import('./components/vacancy-form/vacancy-form.component').then(c => c.VacancyFormComponent) },
      { path: 'vacancies/:id', loadComponent: () => import('./components/vacancy-detail/vacancy-detail.component').then(c => c.VacancyDetailComponent) },
      { path: 'applications', loadComponent: () => import('./components/applications-list/applications-list.component').then(c => c.ApplicationsListComponent) },
      { path: 'applications/:id', loadComponent: () => import('./components/application-detail/application-detail.component').then(c => c.ApplicationDetailComponent) },
      { path: 'interviews', loadComponent: () => import('./components/interviews-list/interviews-list.component').then(c => c.InterviewsListComponent) }
    ]
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class RecruiterManagementRoutingModule { }
