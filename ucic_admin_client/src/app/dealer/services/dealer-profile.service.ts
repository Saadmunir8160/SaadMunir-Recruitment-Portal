import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface DealerProfile {
  id: number;
  dealerCode?: string;
  dealerName?: string;
  creditLimit?: number;
  currentBalance?: number;
  ln_ID?: string;
  isVerified: boolean;
  userInfo: DealerUserInfo;
  createdAt: Date;
  updatedAt?: Date;
}

export interface DealerUserInfo {
  userId?: string;
  fullName?: string;
  email?: string;
  phoneNumber?: string;
  userName?: string;
  roles: string[];
}

export interface ApiResponse<T> {
  success: boolean;
  data?: T;
  message: string;
}



@Injectable({
  providedIn: 'root'
})
export class DealerProfileService {
  private apiUrl = `${environment.apiUrl}/DealerPortal`;

  constructor(private http: HttpClient) {}

  // Get current dealer profile
  getProfile(): Observable<ApiResponse<DealerProfile>> {
    return this.http.get<ApiResponse<DealerProfile>>(`${this.apiUrl}/Profile`);
  }

  // Change password
  changePassword(changePasswordRequest: { currentPassword: string; newPassword: string; confirmPassword: string }): Observable<ApiResponse<any>> {
    return this.http.post<ApiResponse<any>>(`${this.apiUrl}/Profile/change-password`, changePasswordRequest);
  }

  // Get credit limit from LN API
  getCreditLimit(): Observable<ApiResponse<CreditLimitResponse>> {
    return this.http.get<ApiResponse<CreditLimitResponse>>(`${environment.apiUrl}/CreditLimit`);
  }
}

export interface CreditLimitResponse {
  availableCredit: number;
}