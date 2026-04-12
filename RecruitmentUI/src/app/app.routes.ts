import { Routes } from '@angular/router';
import { AuthGuard, RoleGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  {
    path: '',
    redirectTo: 'jobs',
    pathMatch: 'full'
  },
  {
    path: 'login',
    loadComponent: () => import('./pages/auth/login/login.component').then(c => c.LoginComponent)
  },
  {
    path: 'signup',
    loadComponent: () => import('./pages/auth/signup/signup.component').then(c => c.SignupComponent)
  },
  {
    path: 'jobs',
    loadComponent: () => import('./pages/jobs/job-list/job-list.component').then(c => c.JobListComponent)
  },
  {
    path: 'jobs/:id',
    loadComponent: () => import('./pages/jobs/job-detail/job-detail.component').then(c => c.JobDetailComponent)
  },
  {
    path: 'profile',
    canActivate: [AuthGuard, RoleGuard],
    loadComponent: () => import('./pages/candidate/profile/profile.component').then(c => c.ProfileComponent)
  },
  {
    path: 'my-applications',
    canActivate: [AuthGuard, RoleGuard],
    loadComponent: () => import('./pages/candidate/my-applications/my-applications.component').then(c => c.MyApplicationsComponent)
  },
  {
    path: 'my-applications/:id',
    canActivate: [AuthGuard, RoleGuard],
    loadComponent: () => import('./pages/candidate/application-detail/application-detail.component').then(c => c.ApplicationDetailComponent)
  },
  {
    path: '**',
    redirectTo: 'jobs'
  }
];
