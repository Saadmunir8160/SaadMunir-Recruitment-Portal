import { Injectable, Inject, PLATFORM_ID } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { BehaviorSubject, Observable, tap } from 'rxjs';
import { isPlatformBrowser } from '@angular/common';
import { environment } from '../../../environments/environment';
import { LoginRequest, RegisterRequest, ApiResponse } from '../../shared/interfaces/models';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private apiUrl = environment.apiUrl;
  private isAuthenticatedSubject = new BehaviorSubject<boolean>(false);
  private currentUserSubject = new BehaviorSubject<any>(null);

  isAuthenticated$ = this.isAuthenticatedSubject.asObservable();
  currentUser$ = this.currentUserSubject.asObservable();

  constructor(
    private http: HttpClient,
    private router: Router,
    @Inject(PLATFORM_ID) private platformId: Object
  ) {
    if (isPlatformBrowser(this.platformId)) {
      const token = sessionStorage.getItem('token');
      const user = sessionStorage.getItem('user');
      if (token && user) {
        this.isAuthenticatedSubject.next(true);
        this.currentUserSubject.next(JSON.parse(user));
      }
    }
  }

  login(credentials: LoginRequest): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/Auth/Login`, credentials).pipe(
      tap(response => {
        if (isPlatformBrowser(this.platformId) && response) {
          // Extract token and user info from response
          const token = response.token || response.data?.token;
          const fullName = response.fullName || response.data?.fullName || '';
          const userId = response.userId || response.data?.userId || '';
          const role = response.role || response.data?.role || '';

          if (role.toLowerCase() !== 'candidate') {
            throw new Error('Access denied. Only candidates can access this portal.');
          }

          if (token) {
            sessionStorage.setItem('token', token);
            sessionStorage.setItem('FullName', fullName);
            sessionStorage.setItem('userRole', role.toLowerCase());
            const user = { userId, name: fullName, role: role.toLowerCase() };
            sessionStorage.setItem('user', JSON.stringify(user));
            this.isAuthenticatedSubject.next(true);
            this.currentUserSubject.next(user);
          }
        }
      })
    );
  }

  register(data: RegisterRequest): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/Auth/RegisterCandidate`, data).pipe(
      tap(response => {
        if (isPlatformBrowser(this.platformId) && response) {
          const token = response.token || response.data?.token;
          const fullName = response.fullName || response.name || response.data?.fullName || '';
          const userId = response.userId || response.data?.userId || '';
          const role = response.role || response.data?.role || '';

          if (token) {
            sessionStorage.setItem('token', token);
            sessionStorage.setItem('FullName', fullName);
            sessionStorage.setItem('userRole', role.toLowerCase());
            const user = { userId, name: fullName, role: role.toLowerCase() };
            sessionStorage.setItem('user', JSON.stringify(user));
            this.isAuthenticatedSubject.next(true);
            this.currentUserSubject.next(user);
          }
        }
      })
    );
  }

  changePassword(currentPassword: string, newPassword: string, confirmPassword: string): Observable<any> {
    const userId = this.getUserId();
    return this.http.post(`${this.apiUrl}/User/ChangePassword`, {
      userId,
      currentPassword,
      newPassword,
      confirmPassword
    });
  }

  logout(): void {
    if (isPlatformBrowser(this.platformId)) {
      this.http.post(`${this.apiUrl}/Auth/logout`, {}).subscribe({
        complete: () => this.clearAuth(),
        error: () => this.clearAuth()
      });
    }
  }

  private clearAuth(): void {
    if (isPlatformBrowser(this.platformId)) {
      sessionStorage.removeItem('token');
      sessionStorage.removeItem('FullName');
      sessionStorage.removeItem('user');
      sessionStorage.removeItem('userRole');
    }
    this.isAuthenticatedSubject.next(false);
    this.currentUserSubject.next(null);
    this.router.navigate(['/login']);
  }

  getToken(): string | null {
    if (isPlatformBrowser(this.platformId)) {
      return sessionStorage.getItem('token');
    }
    return null;
  }

  getUserRole(): string | null {
    if (isPlatformBrowser(this.platformId)) {
      return sessionStorage.getItem('userRole');
    }
    return null;
  }

  getUserId(): string | null {
    if (isPlatformBrowser(this.platformId)) {
      const user = sessionStorage.getItem('user');
      if (user) {
        return JSON.parse(user).userId;
      }
    }
    return null;
  }

  getFullName(): string {
    if (isPlatformBrowser(this.platformId)) {
      return sessionStorage.getItem('FullName') || '';
    }
    return '';
  }

  isLoggedIn(): boolean {
    return !!this.getToken();
  }
}
