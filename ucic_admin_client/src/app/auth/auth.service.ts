import { Injectable, Inject, PLATFORM_ID } from '@angular/core';
import { Router } from '@angular/router';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { environment } from '../../environments/environment';
import { BehaviorSubject, Observable, throwError } from 'rxjs';
import { catchError, tap } from 'rxjs/operators';
import { isPlatformBrowser } from '@angular/common';

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private isAuthenticated = new BehaviorSubject<boolean>(false);
  private currentUser = new BehaviorSubject<any>(null);

  constructor(
    private router: Router,
    private http: HttpClient,
    @Inject(PLATFORM_ID) private platformId: Object
  ) {
    this.checkAuthStatus();
  }

  getToken(): string | null {
    if (isPlatformBrowser(this.platformId)) {
      const token = localStorage.getItem('token');
      return token;
    }
    return null;
  }

  private handleError(error: HttpErrorResponse) {
    let errorMessage = 'An error occurred';
    if (error.status === 0) {
      errorMessage = 'Network error: Please check your internet connection and try again.';
    } else if (error.status === 401) {
      errorMessage = 'Unauthorized: Please check your credentials.';
    } else if (error.status === 403) {
      errorMessage = 'Forbidden: You do not have permission to access this resource.';
    } else {
      errorMessage = `Server error: ${error.status} - ${error.error?.message || error.message}`;
    }
    return throwError(() => new Error(errorMessage));
  }

  login(email: string, password: string): Observable<any> {
    return this.http.post(`${environment.apiUrl}/Auth/Login`, { email, password }, {
      headers: {
        'Content-Type': 'application/json',
        'Accept': 'application/json'
      },
      withCredentials: false
    }).pipe(
      tap((response: any) => {
        if (response && response.token) {
          if (isPlatformBrowser(this.platformId)) {
            localStorage.setItem('token', response.token);
            localStorage.setItem('FullName', response.name);

            // Persist user object (roles array when API sends it — UCIC login)
            localStorage.setItem('user', JSON.stringify({
              userId: response.userId,
              name: response.name,
              role: response.role,
              roles: response.roles ?? null
            }));

            // userRole is set by LoginComponent + RoleService from Role / Roles (do not overwrite here)
          }
          this.isAuthenticated.next(true);
          this.currentUser.next(response);
        }
      }),
      catchError(this.handleError)
    );
  }

  logout(): void {
    this.http.post(`${environment.apiUrl}/Auth/logout`, {}, {
      withCredentials: false
    }).subscribe({
      next: () => {
        this.clearAuthData();
        this.router.navigate(['/login']);
      },
      error: (error) => {
        this.clearAuthData();
        this.router.navigate(['/login']);
      }
    });
  }

  clearAuthData(): void {
    if (isPlatformBrowser(this.platformId)) {
      localStorage.removeItem('token');
      localStorage.removeItem('user');
      localStorage.removeItem('userRole');
    }
    this.isAuthenticated.next(false);
    this.currentUser.next(null);
  }

  checkAuthStatus(): void {
    const token = this.getToken();
    let user = null;
    
    if (isPlatformBrowser(this.platformId)) {
      user = localStorage.getItem('user');
    }
    
    if (token && user) {
      try {
        const userData = JSON.parse(user);
        this.isAuthenticated.next(true);
        this.currentUser.next(userData);
      } catch (e) {
        this.clearAuthData();
        if (isPlatformBrowser(this.platformId)) {
          this.router.navigate(['/login']);
        }
      }
    } else {
      this.clearAuthData();
      if (isPlatformBrowser(this.platformId)) {
        this.router.navigate(['/login']);
      }
    }
  }

  isLoggedIn(): Observable<boolean> {
    return this.isAuthenticated.asObservable();
  }

  getCurrentUser(): Observable<any> {
    return this.currentUser.asObservable();
  }

  getCurrentUserId(): string | null {
    if (isPlatformBrowser(this.platformId)) {
      const userStr = localStorage.getItem('user');
      if (userStr) {
        try {
          const user = JSON.parse(userStr);
          return user.userId || null;
        } catch (e) {
          return null;
        }
      }
    }
    return null;
  }
}
