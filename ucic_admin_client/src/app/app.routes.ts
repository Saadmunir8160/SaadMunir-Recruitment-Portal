import { Routes } from '@angular/router';
import { LoginComponent } from './auth/login/login.component';
import { MainLayoutComponent } from './layout/main-layout/main-layout.component';
import { authGuard } from './auth/authguard/auth.guard';
import { RoleGuard } from './guards/role.guard';
import { RoleRedirectGuard } from './guards/role-redirect.guard';
import { UserRole } from './services/role.service';

export const routes: Routes = [
  { path: 'login', component: LoginComponent },
  {
    path: '',
    canActivate: [authGuard, RoleRedirectGuard],
    children: []
  },
  {
    path: 'admin',
    component: MainLayoutComponent,
    canActivate: [authGuard],
    children: [
      {
        path: 'dashboard',
        loadChildren: () =>
          import('./dashboard/dashboard.module').then((m) => m.DashboardModule),
        canActivate: [RoleGuard],
        data: { roles: [UserRole.ADMIN, UserRole.HR, UserRole.HR_MANAGER, UserRole.HR_SECTION_HEAD, UserRole.FINANCE, UserRole.PURCHASE, UserRole.SALE] }
      },
      {
        path: 'users',
        loadChildren: () =>
          import('./users/users.module').then((m) => m.UsersModule),
        canActivate: [RoleGuard],
        data: { roles: [UserRole.ADMIN] }
      },
      {
        path: 'news',
        loadChildren: () =>
          import('./news/news.module').then((m) => m.NewsModule),
        canActivate: [RoleGuard],
        data: { roles: [UserRole.ADMIN, UserRole.HR, UserRole.HR_MANAGER, UserRole.HR_SUPERVISOR, UserRole.HR_SECTION_HEAD] }
      },
      {
        path: 'jobs',
        loadChildren: () =>
          import('./jobs/jobs.module').then((m) => m.JobsModule),
        canActivate: [RoleGuard],
        data: { roles: [UserRole.ADMIN, UserRole.HR, UserRole.HR_MANAGER, UserRole.HR_SUPERVISOR, UserRole.HR_SECTION_HEAD] }
      },
      {
        path: 'vendors',
        loadChildren: () =>
          import('./vendors/vendors.module').then((m) => m.VendorsModule),
        canActivate: [RoleGuard],
        data: { roles: [UserRole.ADMIN, UserRole.PURCHASE] }
      },
      {
        path: 'orders',
        loadChildren: () =>
          import('./orders/orders.module').then((m) => m.OrdersModule),
        canActivate: [RoleGuard],
        data: { roles: [UserRole.ADMIN, UserRole.SALE, UserRole.FINANCE] }
      },
      {
        path: 'products',
        loadChildren: () =>
          import('./products/products.module').then((m) => m.ProductsModule),
        canActivate: [RoleGuard],
        data: { roles: [UserRole.ADMIN, UserRole.SALE] }
      },
      {
        path: 'email',
        loadChildren: () =>
          import('./EmailNotifications/email.module').then((m) => m.EmailModule),
        canActivate: [RoleGuard],
        data: { roles: [UserRole.ADMIN] }
      },
      {
        path: 'roles',
        loadChildren: () =>
          import('./roles/roles.module').then((m) => m.RolesModule),
        canActivate: [RoleGuard],
        data: { roles: [UserRole.ADMIN] }
      },
      {
        path: 'coverage-area',
        loadChildren: () =>
          import('./coverage-area/coverage-area.module').then((m) => m.CoverageAreaModule),
        canActivate: [RoleGuard],
        data: { roles: [UserRole.ADMIN, UserRole.SALE] }
      },
      // {
      //   path: 'dealers',
      //   children: [
      //     {
      //       path: '',
      //       loadComponent: () =>
      //         import('./AdminPanal/components/dealers/admin-dealers-list/admin-dealers-list.component').then((m) => m.AdminDealersListComponent),
      //       canActivate: [RoleGuard],
      //       data: { roles: [UserRole.ADMIN] }
      //     },
      //     {
      //       path: ':id',
      //       loadComponent: () =>
      //         import('./AdminPanal/components/dealers/admin-dealer-details/admin-dealer-details.component').then((m) => m.AdminDealerDetailsComponent),
      //       canActivate: [RoleGuard],
      //       data: { roles: [UserRole.ADMIN] }
      //     }
      //   ]
      // },
      {
        path: 'promotion',
        loadChildren: () =>
          import('./promotion/promotion.module').then((m) => m.PromotionModule),
        canActivate: [RoleGuard],
        data: { roles: [UserRole.ADMIN, UserRole.SALE] }
      },
      {
        path: 'job-applications',
        loadChildren: () =>
          import('./job-applications/job-applications.module').then((m) => m.JobApplicationsModule),
        canActivate: [RoleGuard],
        data: { roles: [UserRole.ADMIN, UserRole.HR, UserRole.HR_MANAGER, UserRole.HR_SUPERVISOR, UserRole.HR_SECTION_HEAD] }
      },
      {
        path: 'dealer-management',
        loadChildren: () =>
          import('./dealer-management/dealer-management.module').then((m) => m.DealerManagementModule),
        canActivate: [RoleGuard],
        data: { roles: [UserRole.ADMIN] }
      },
      {
        path: 'recruiter-management',
        loadChildren: () =>
          import('./recruiter-management/recruiter-management.module').then((m) => m.RecruiterManagementModule),
        canActivate: [RoleGuard],
        data: { roles: [UserRole.ADMIN, UserRole.HR, UserRole.HR_MANAGER, UserRole.HR_SUPERVISOR, UserRole.HR_SECTION_HEAD] }
      },
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
    ],
  },

  // Dealer routes
  {
    path: 'dealer',
    loadChildren: () => import('./dealer/dealer.module').then(m => m.DealerModule),
    canActivate: [authGuard, RoleGuard],
    data: { roles: [UserRole.DEALER] }
  },

  // Fallback route - use role redirect guard to determine where to go
  {
    path: '**',
    canActivate: [authGuard, RoleRedirectGuard],
    children: []
  },
];
