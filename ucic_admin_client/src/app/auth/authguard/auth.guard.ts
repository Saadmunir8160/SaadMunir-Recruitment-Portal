import { Injectable, Inject, PLATFORM_ID } from '@angular/core';
import { CanActivate, Router, ActivatedRouteSnapshot, RouterStateSnapshot } from '@angular/router';
import { RoleService, UserRole } from '../../services/role.service';
import { isPlatformBrowser } from '@angular/common';

@Injectable({
  providedIn: 'root'
})
export class authGuard implements CanActivate {
  constructor(
    private router: Router, 
    private roleService: RoleService,
    @Inject(PLATFORM_ID) private platformId: Object
  ) {}

  canActivate(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): boolean {
    if (!isPlatformBrowser(this.platformId)) {
      return false;
    }

    const token = localStorage.getItem('token');
    const userRole = localStorage.getItem('userRole') as UserRole;

    if (!token) {
      this.router.navigate(['/login']);
      return false;
    }

    // If no role is set, redirect to login
    if (!userRole) {
      this.router.navigate(['/login']);
      return false;
    }

    // Set the role in the service
    this.roleService.setRole(userRole);

    return true;
  }
}

