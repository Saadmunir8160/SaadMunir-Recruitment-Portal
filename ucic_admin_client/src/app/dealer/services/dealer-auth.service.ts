import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, BehaviorSubject, tap } from 'rxjs';
import { Router } from '@angular/router';
import { environment } from '../../../environments/environment';

export interface DealerLoginRequest {
  email: string;
  password: string;
  rememberMe?: boolean;
}

export interface DealerLoginResponse {
  token: string;
  refreshToken: string;
  dealerInfo: DealerInfo;
  expiresAt: Date;
}

export interface DealerInfo {
  id: number;
  dealerCode: string;
  companyName: string;
  email: string;
  contactPerson: string;
  phone: string;
  status: 'Active' | 'Inactive' | 'Pending' | 'Suspended';
  role: string;
  permissions: string[];
  profileComplete: boolean;
  isVerified: boolean;
  lastLoginAt?: Date;
}

export interface PasswordChangeRequest {
  currentPassword: string;
  newPassword: string;
  confirmPassword: string;
}

export interface ForgotPasswordRequest {
  email: string;
}

export interface ResetPasswordRequest {
  token: string;
  email: string;
  newPassword: string;
  confirmPassword: string;
}

export interface DealerRegistrationRequest {
  companyName: string;
  email: string;
  password: string;
  confirmPassword: string;
  contactPerson: string;
  phone: string;
  businessType: string;
  gstNumber?: string;
  registrationNumber?: string;
  address: {
    street: string;
    city: string;
    state: string;
    pincode: string;
    country: string;
  };
  agreeToTerms: boolean;
}

@Injectable({
  providedIn: 'root'
})
export class DealerAuthService {
  private apiUrl = `${environment.apiUrl}/Dealer/Auth`;
  private currentDealerSubject = new BehaviorSubject<DealerInfo | null>(null);
  private tokenKey = 'dealer_token';
  private refreshTokenKey = 'dealer_refresh_token';
  private dealerInfoKey = 'dealer_info';

  public currentDealer$ = this.currentDealerSubject.asObservable();

  constructor(
    private http: HttpClient,
    private router: Router
  ) {
    this.loadDealerFromStorage();
  }

  // Login
  login(credentials: DealerLoginRequest): Observable<DealerLoginResponse> {
    return this.http.post<DealerLoginResponse>(`${this.apiUrl}/login`, credentials)
      .pipe(
        tap(response => {
          this.setSession(response);
        })
      );
  }

  // Register new dealer
  register(registrationData: DealerRegistrationRequest): Observable<{ message: string; dealerId: number }> {
    return this.http.post<{ message: string; dealerId: number }>(`${this.apiUrl}/register`, registrationData);
  }

  // Logout
  logout(): Observable<any> {
    const refreshToken = this.getRefreshToken();
    return this.http.post(`${this.apiUrl}/logout`, { refreshToken })
      .pipe(
        tap(() => {
          this.clearSession();
        })
      );
  }

  // Refresh token
  refreshToken(): Observable<DealerLoginResponse> {
    const refreshToken = this.getRefreshToken();
    return this.http.post<DealerLoginResponse>(`${this.apiUrl}/refresh-token`, { refreshToken })
      .pipe(
        tap(response => {
          this.setSession(response);
        })
      );
  }

  // Change password
  changePassword(passwordData: PasswordChangeRequest): Observable<{ message: string }> {
    return this.http.put<{ message: string }>(`${this.apiUrl}/change-password`, passwordData);
  }

  // Forgot password
  forgotPassword(request: ForgotPasswordRequest): Observable<{ message: string }> {
    return this.http.post<{ message: string }>(`${this.apiUrl}/forgot-password`, request);
  }

  // Reset password
  resetPassword(request: ResetPasswordRequest): Observable<{ message: string }> {
    return this.http.post<{ message: string }>(`${this.apiUrl}/reset-password`, request);
  }

  // Verify email
  verifyEmail(token: string): Observable<{ message: string }> {
    return this.http.post<{ message: string }>(`${this.apiUrl}/verify-email`, { token });
  }

  // Resend verification email
  resendVerificationEmail(): Observable<{ message: string }> {
    return this.http.post<{ message: string }>(`${this.apiUrl}/resend-verification`, {});
  }

  // Check if dealer is authenticated
  isAuthenticated(): boolean {
    const token = this.getToken();
    if (!token) return false;
    
    // Check if token is expired
    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      const expiry = payload.exp * 1000;
      return Date.now() < expiry;
    } catch {
      return false;
    }
  }

  // Get current dealer info
  getCurrentDealer(): DealerInfo | null {
    return this.currentDealerSubject.value;
  }

  // Get authentication token
  getToken(): string | null {
    return localStorage.getItem(this.tokenKey);
  }

  // Get refresh token
  getRefreshToken(): string | null {
    return localStorage.getItem(this.refreshTokenKey);
  }

  // Check if dealer has specific permission
  hasPermission(permission: string): boolean {
    const dealer = this.getCurrentDealer();
    return dealer?.permissions?.includes(permission) || false;
  }

  // Check if dealer profile is complete
  isProfileComplete(): boolean {
    const dealer = this.getCurrentDealer();
    return dealer?.profileComplete || false;
  }

  // Check if dealer is verified
  isVerified(): boolean {
    const dealer = this.getCurrentDealer();
    return dealer?.isVerified || false;
  }

  // Update dealer info in memory (after profile update)
  updateDealerInfo(dealerInfo: Partial<DealerInfo>): void {
    const current = this.getCurrentDealer();
    if (current) {
      const updated = { ...current, ...dealerInfo };
      this.currentDealerSubject.next(updated);
      localStorage.setItem(this.dealerInfoKey, JSON.stringify(updated));
    }
  }

  // Private methods
  private setSession(authResult: DealerLoginResponse): void {
    localStorage.setItem(this.tokenKey, authResult.token);
    localStorage.setItem(this.refreshTokenKey, authResult.refreshToken);
    localStorage.setItem(this.dealerInfoKey, JSON.stringify(authResult.dealerInfo));
    this.currentDealerSubject.next(authResult.dealerInfo);
  }

  private clearSession(): void {
    localStorage.removeItem(this.tokenKey);
    localStorage.removeItem(this.refreshTokenKey);
    localStorage.removeItem(this.dealerInfoKey);
    this.currentDealerSubject.next(null);
    this.router.navigate(['/auth/login']);
  }

  private loadDealerFromStorage(): void {
    const dealerInfo = localStorage.getItem(this.dealerInfoKey);
    if (dealerInfo && this.isAuthenticated()) {
      try {
        const dealer = JSON.parse(dealerInfo);
        this.currentDealerSubject.next(dealer);
      } catch (error) {
        this.clearSession();
      }
    } else {
      this.clearSession();
    }
  }

  // Session management
  extendSession(): void {
    if (this.isAuthenticated()) {
      this.refreshToken().subscribe({
        error: () => this.clearSession()
      });
    }
  }

  // Auto logout on token expiry
  startSessionTimer(): void {
    const token = this.getToken();
    if (token) {
      try {
        const payload = JSON.parse(atob(token.split('.')[1]));
        const expiry = payload.exp * 1000;
        const timeout = expiry - Date.now() - 60000; // Refresh 1 minute before expiry
        
        if (timeout > 0) {
          setTimeout(() => {
            this.refreshToken().subscribe({
              error: () => this.clearSession()
            });
          }, timeout);
        }
      } catch (error) {
        this.clearSession();
      }
    }
  }
}