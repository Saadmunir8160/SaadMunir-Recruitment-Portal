import { Injectable, Inject, PLATFORM_ID } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { isPlatformBrowser } from '@angular/common';

export enum UserRole {
  ADMIN = 'admin',
  HR = 'hr',
  HR_MANAGER = 'HR Manager',
  /** Legacy / senior HR role used elsewhere in RMS APIs */
  HR_SUPERVISOR = 'HRSupervisor',
  /** Approves step 2 of vacancy publishing */
  HR_SECTION_HEAD = 'HR Section Head',
  FINANCE = 'finance',
  PURCHASE = 'purchase',
  SALE = 'sale',
  DEALER = 'dealer'
}

@Injectable({
  providedIn: 'root'
})
export class RoleService {
  private apiUrl = `${environment.apiUrl}/Role`;
  private currentRole: UserRole | null = null;

  constructor(
    private http: HttpClient,
    @Inject(PLATFORM_ID) private platformId: Object
  ) {
    // Initialize role from localStorage if available
    if (isPlatformBrowser(this.platformId)) {
      const storedRole = localStorage.getItem('userRole');
      if (storedRole) {
        this.currentRole = storedRole as UserRole;
      }
    }
  }

  getAllRoles(): Observable<any> {
    return this.http.get(`${this.apiUrl}/GetAll`);
  }

  getRoleById(id: string): Observable<any> {
    return this.http.get(`${this.apiUrl}/${id}`);
  }

  createRole(roleName: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/Create`, { roleName });
  }

  updateRole(id: string, roleName: string): Observable<any> {
    return this.http.put(`${this.apiUrl}/Edit/${id}`, { id, roleName });
  }

  deleteRole(id: string): Observable<any> {
    return this.http.delete(`${this.apiUrl}/Delete/${id}`);
  }

  setRole(role: UserRole) {
    this.currentRole = role;
    if (isPlatformBrowser(this.platformId)) {
      localStorage.setItem('userRole', role);
    }
  }

  /** Case- and spacing-insensitive role name match. */
  private normalizeRoleName(s: string): string {
    return s.toLowerCase().replace(/\s+/g, ' ').trim();
  }

  /**
   * Every role assigned to the signed-in user (UCIC `user.roles` / `user.role` + JWT `role` claim(s) + stored primary).
   * Use this for vacancy approval so HR Manager + HR Section Head on the same account both work.
   */
  getAllAssignedRoles(): string[] {
    if (!isPlatformBrowser(this.platformId)) return [];
    const out = new Set<string>();
    const add = (r: string | null | undefined) => {
      if (r != null && String(r).trim()) out.add(String(r).trim());
    };
    try {
      const userStr = localStorage.getItem('user');
      if (userStr) {
        const u = JSON.parse(userStr) as { roles?: unknown; role?: string };
        if (Array.isArray(u.roles)) {
          u.roles.forEach((x) => add(typeof x === 'string' ? x : String(x)));
        }
        add(u.role);
      }
    } catch {
      /* ignore */
    }
    for (const r of this.parseJwtRoleClaims()) {
      add(r);
    }
    const primary = this.getRole();
    if (primary) add(primary);
    return Array.from(out);
  }

  /** True if any assigned role matches one of the given names (e.g. "HR Manager", "Admin"). */
  hasAnyRole(...roleNames: string[]): boolean {
    if (roleNames.length === 0) return false;
    const assigned = new Set(this.getAllAssignedRoles().map((r) => this.normalizeRoleName(r)));
    return roleNames.some((n) => assigned.has(this.normalizeRoleName(n)));
  }

  private parseJwtRoleClaims(): string[] {
    if (!isPlatformBrowser(this.platformId)) return [];
    const token = localStorage.getItem('token');
    if (!token) return [];
    const parts = token.split('.');
    if (parts.length < 2) return [];
    try {
      const base64 = parts[1].replace(/-/g, '+').replace(/_/g, '/');
      const padded = base64.padEnd(base64.length + (4 - (base64.length % 4)) % 4, '=');
      const json = JSON.parse(atob(padded)) as Record<string, unknown>;
      const claimUri = 'http://schemas.microsoft.com/ws/2008/06/identity/claims/role';
      const raw = json['role'] ?? json[claimUri];
      if (Array.isArray(raw)) return raw.map((x) => String(x));
      if (typeof raw === 'string') return [raw];
      return [];
    } catch {
      return [];
    }
  }

  getRole(): UserRole | null {
    // Always re-read from localStorage to ensure we have the latest role
    if (isPlatformBrowser(this.platformId)) {
      const storedRole = localStorage.getItem('userRole');
      if (storedRole) {
        this.currentRole = storedRole as UserRole;
        return this.currentRole;
      }
    }

    // If no role is found and we have a cached role, return it
    if (this.currentRole) {
      return this.currentRole;
    }

    // As a last resort, return null instead of defaulting to ADMIN
    return null;
  }

  hasAccess(requiredRole: UserRole): boolean {
    const currentRole = this.getRole();

    // If no role, no access
    if (!currentRole) {
      return false;
    }

    // Admin has access to everything
    if (currentRole === UserRole.ADMIN) {
      return true;
    }

    // Check if current role matches required role
    return currentRole === requiredRole;
  }

  getMenuItems(): any[] {
    const role = this.getRole();

    // If no role, return empty menu
    if (!role) {
      return [];
    }

    // Special menu for dealers
    if (role === UserRole.DEALER) {
      return [
        { title: 'Shop', icon: 'solar:shop-bold-duotone', route: '/dealer/shop' },
        { title: 'Dashboard', icon: 'solar:widget-5-bold-duotone', route: '/dealer/dashboard' },
        { title: 'My Orders', icon: 'solar:bag-smile-bold-duotone', route: '/dealer/orders' }
      ];
    }

    // For vacancy-approval roles, keep sidebar minimal as requested.
    if (
      role === UserRole.HR ||
      role === UserRole.HR_MANAGER ||
      role === UserRole.HR_SECTION_HEAD ||
      role === UserRole.HR_SUPERVISOR
    ) {
      return [
        {
          title: 'Recruiter Portal',
          icon: 'solar:case-minimalistic-bold-duotone',
          route: '/admin/recruiter-management'
        }
      ];
    }

    const allMenuItems = [
      { title: 'Dashboard', icon: 'solar:widget-5-bold-duotone', route: '/admin/dashboard', roles: [UserRole.ADMIN, UserRole.FINANCE, UserRole.SALE] },
      { title: 'Orders', icon: 'solar:shop-bold-duotone', route: '/admin/orders', roles: [UserRole.ADMIN, UserRole.FINANCE, UserRole.SALE] },
      { title: 'Products', icon: 'solar:bag-smile-bold-duotone', route: '/admin/products', roles: [UserRole.ADMIN, UserRole.SALE] },
      { title: 'Promotion', icon: 'solar:gift-bold-duotone', route: '/admin/promotion', roles: [UserRole.ADMIN, UserRole.SALE] },
      { title: 'Coverage Area', icon: 'solar:clipboard-list-bold-duotone', route: '/admin/coverage-area', roles: [UserRole.ADMIN, UserRole.SALE] },
      { title: 'Vendors', icon: 'solar:shop-bold-duotone', route: '/admin/vendors/list', roles: [UserRole.ADMIN, UserRole.PURCHASE] },
      { title: 'Users', icon: 'solar:users-group-two-rounded-bold-duotone', route: '/admin/users', roles: [UserRole.ADMIN] },
      //{ title: 'Roles', icon: 'solar:user-speak-rounded-bold-duotone', route: '/admin/roles', roles: [UserRole.ADMIN] },
      { title: 'News', icon: 'solar:chat-square-like-bold-duotone', route: '/admin/news', roles: [UserRole.ADMIN, UserRole.HR, UserRole.HR_MANAGER, UserRole.HR_SUPERVISOR, UserRole.HR_SECTION_HEAD] },
      { title: 'Career', icon: 'solar:case-minimalistic-bold-duotone', route: '/admin/jobs', roles: [UserRole.ADMIN, UserRole.HR, UserRole.HR_MANAGER, UserRole.HR_SUPERVISOR, UserRole.HR_SECTION_HEAD] },
      { title: 'Job Applications', icon: 'solar:document-bold-duotone', route: '/admin/job-applications/list', roles: [UserRole.ADMIN, UserRole.HR, UserRole.HR_MANAGER, UserRole.HR_SUPERVISOR, UserRole.HR_SECTION_HEAD] },
      { title: 'Customer Portal', icon: 'solar:buildings-2-bold-duotone', route: '/admin/dealer-management', roles: [UserRole.ADMIN] },
      { title: 'Recruiter Portal', icon: 'solar:case-minimalistic-bold-duotone', route: '/admin/recruiter-management', roles: [UserRole.ADMIN, UserRole.HR, UserRole.HR_MANAGER, UserRole.HR_SUPERVISOR, UserRole.HR_SECTION_HEAD] },
      { title: 'Email Recipient', icon: 'solar:bag-smile-bold-duotone', route: '/admin/email', roles: [UserRole.ADMIN] },
      { title: 'Settings', icon: 'solar:settings-bold-duotone', route: '/admin/settings', roles: [UserRole.ADMIN] },
    ];

    return allMenuItems.filter(item => {
      if (role === UserRole.ADMIN) return true;
      return item.roles.includes(role);
    });
  }
} 