import { Component, Inject, PLATFORM_ID } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../auth.service';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AuthRoutingModule } from '../auth-routing.module';
import { RoleService, UserRole } from '../../services/role.service';
import { isPlatformBrowser } from '@angular/common';

@Component({
  selector: 'app-login',
  imports: [CommonModule, FormsModule, AuthRoutingModule],
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss'
})
export class LoginComponent {
  email = '';
  password = '';
  errorMessage = '';
  isLoading = false;

  constructor(
    private authService: AuthService,
    private router: Router,
    private roleService: RoleService,
    @Inject(PLATFORM_ID) private platformId: Object
  ) { }

  login() {
    this.isLoading = true;
    this.errorMessage = '';

    this.authService.login(this.email, this.password).subscribe({
      next: (response: any) => {
        if (response.token) {
          if (isPlatformBrowser(this.platformId)) {
            localStorage.setItem('token', response.token);
            const resolved = this.pickRoleStringFromLoginResponse(response);
            const userRole = this.getUserRole(resolved || this.email);
            localStorage.setItem('userRole', userRole);
            this.roleService.setRole(userRole);
          }
          // Redirect to role-based default route
          const resolved = this.pickRoleStringFromLoginResponse(response);
          const userRole = this.getUserRole(resolved || this.email);
          const defaultRoute = this.getDefaultRouteForRole(userRole);
          this.router.navigate([defaultRoute]);
        }
      },
      error: (error) => {
        this.errorMessage = error.error?.message || 'Invalid email or password';
        this.isLoading = false;
      },
      complete: () => {
        this.isLoading = false;
      }
    });
  }

  /**
   * UCIC may return multiple roles; JWT has all claims. Prefer recruitment roles for admin UI.
   */
  private pickRoleStringFromLoginResponse(response: { role?: string; roles?: string[] }): string {
    const priority = [
      'HR Section Head', 'HR Manager', 'HR', 'hr', 'Recruiter', 'HRSupervisor',
      'HiringManager', 'Admin', 'Finance', 'Purchase', 'Sale', 'User'
    ];
    const list: string[] = Array.isArray(response.roles) && response.roles.length > 0
      ? response.roles
      : [response.role].filter((x): x is string => !!x && x.length > 0);
    if (list.length === 0) {
      return '';
    }
    const lower = (s: string) => s.toLowerCase().trim();
    const hit = priority.find((p) => list.some((r) => lower(r) === lower(p)));
    return hit ?? list[0];
  }

  private getUserRole(roleOrEmail: string): UserRole {
    // First try to match the role directly
    const roleMap: { [key: string]: UserRole } = {
      'admin': UserRole.ADMIN,
      'hr': UserRole.HR,
      'recruiter': UserRole.HR,
      'hr manager': UserRole.HR_MANAGER,
      'hrsupervisor': UserRole.HR_SUPERVISOR,
      'hr section head': UserRole.HR_SECTION_HEAD,
      'finance': UserRole.FINANCE,
      'purchase': UserRole.PURCHASE,
      'sale': UserRole.SALE,
      'dealer': UserRole.DEALER
    };

    const directRole = roleMap[roleOrEmail.toLowerCase()];
    if (directRole) {
      return directRole;
    }

    // If no direct match, try to extract from email
    const emailPrefix = roleOrEmail.toLowerCase().split('@')[0];
    return roleMap[emailPrefix] || UserRole.ADMIN;
  }

  private getDefaultRouteForRole(role: UserRole): string {
    switch (role) {
      case UserRole.HR:
      case UserRole.HR_MANAGER:
      case UserRole.HR_SECTION_HEAD:
        return '/admin/recruiter-management/overview';
      case UserRole.HR_SUPERVISOR:
        return '/admin/recruiter-management/overview';
      case UserRole.PURCHASE:
        return '/admin/vendors/list';
      case UserRole.FINANCE:
        return '/admin/dashboard';
      case UserRole.SALE:
        return '/admin/dashboard';
      case UserRole.ADMIN:
        return '/admin/dashboard';
      case UserRole.DEALER:
        return '/dealer/dashboard'; // Changed from shop to dashboard
      default:
        return '/admin/dashboard';
    }
  }
}
