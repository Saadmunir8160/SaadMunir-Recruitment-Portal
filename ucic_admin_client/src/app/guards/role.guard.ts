import { Injectable } from '@angular/core';
import { CanActivate, ActivatedRouteSnapshot, RouterStateSnapshot, Router } from '@angular/router';
import { RoleService, UserRole } from '../services/role.service';

@Injectable({
  providedIn: 'root'
})
export class RoleGuard implements CanActivate {
  constructor(private roleService: RoleService, private router: Router) {}

  canActivate(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): boolean {
    const requiredRoles = route.data['roles'] as UserRole[];
    const currentRole = this.roleService.getRole();

    // If current role is null (not loaded yet), redirect to login
    if (!currentRole) {
      this.router.navigate(['/login']);
      return false;
    }

    // If no roles are required, allow access
    if (!requiredRoles || requiredRoles.length === 0) {
      return true;
    }

    // Admin has access to everything
    if (currentRole === UserRole.ADMIN) {
      return true;
    }

    // Check if user's role is in the required roles
    if (requiredRoles.includes(currentRole)) {
      return true;
    }

    // If not authorized, redirect based on user role
    if (currentRole === UserRole.DEALER) {
      this.router.navigate(['/dealer/dashboard']);
    } else {
      this.router.navigate(['/admin/dashboard']);
    }
    return false;
  }
} 