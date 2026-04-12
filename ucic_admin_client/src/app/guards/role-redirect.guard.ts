import { Injectable, Inject, PLATFORM_ID } from '@angular/core';
import { CanActivate, Router } from '@angular/router';
import { RoleService, UserRole } from '../services/role.service';
import { isPlatformBrowser } from '@angular/common';

@Injectable({
  providedIn: 'root'
})
export class RoleRedirectGuard implements CanActivate {

  constructor(
    private roleService: RoleService,
    private router: Router,
    @Inject(PLATFORM_ID) private platformId: Object
  ) {}

  canActivate(): boolean {
    if (!isPlatformBrowser(this.platformId)) {
      return true;
    }

    const userRole = this.roleService.getRole();
    
    // Redirect based on user role
    switch (userRole) {
      case UserRole.DEALER:
        this.router.navigate(['/dealer/dashboard']);
        return false;
      case UserRole.HR:
      case UserRole.HR_MANAGER:
      case UserRole.HR_SECTION_HEAD:
      case UserRole.HR_SUPERVISOR:
        this.router.navigate(['/admin/recruiter-management']);
        return false;
      case UserRole.ADMIN:
      case UserRole.FINANCE:
      case UserRole.PURCHASE:
      case UserRole.SALE:
      default:
        this.router.navigate(['/admin/dashboard']);
        return false;
    }
  }
}