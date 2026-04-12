import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, catchError, map, of } from 'rxjs';
import { environment } from '../../environments/environment';
import { PaginatedResponse } from '../shared/interfaces/paginated-response.interface';

export interface UserResponseDTO {
  id: string;
  fullName: string;
  userName: string;
  email: string;
  phone: string;
  roles: string[];
}

export interface UserDetailsResponseDTO extends UserResponseDTO {
  crNo?: string;
  vatId?: string;
  contactPerson?: string;
  location?: string;
}

export interface CreateUserDTO {
  fullName: string;
  userName: string;
  email: string;
  phone: string;
  password: string;
  confirmationPassword: string;
  roles?: string[];
  cr_No?: string;
  vat_ID?: string;
  contactPerson?: string;
  location?: string;
}

export interface UpdateUserDTO {
  id: string;
  fullName: string;
  email: string;
  phone: string;
  password?: string;
  roles?: string[];
  cr_No?: string;
  vat_ID?: string;
  contactPerson?: string;
  location?: string;
}

@Injectable({
  providedIn: 'root'
})
export class UserService {
  private apiUrl = `${environment.apiUrl}/User`;

  constructor(private http: HttpClient) { }

  getAllUsers(pageNumber?: number, pageSize?: number): Observable<UserResponseDTO[] | PaginatedResponse<UserResponseDTO>> {
    // Always send pagination parameters to the API. If caller omits them,
    // default to first page and a very large pageSize so server returns all rows.
    const pn = pageNumber ?? 1;
    const ps = pageSize === undefined ? 1000000 : (pageSize === 0 ? 1000000 : pageSize);
    const url = `${this.apiUrl}/GetAll?pageNumber=${pn}&pageSize=${ps}`;

    return this.http.get<PaginatedResponse<any>>(url).pipe(
      map(response => ({
        ...response,
        data: response.data.map((user: any) => ({
          id: user.id,
          fullName: user.fullName,
          userName: user.userName,
          email: user.email,
          phone: user.phone,
          roles: user.roles || []
        }))
      })),
      catchError(error => {
        console.error('Error fetching users:', error);
        throw error;
      })
    );
  }

  getUserDetails(userId: string): Observable<UserDetailsResponseDTO> {
    return this.http.get<any>(`${this.apiUrl}/GetUserDetails/${userId}`).pipe(
      map(response => {
        const user = response.data || response;
        return {
          id: user.id,
          fullName: user.fullName,
          userName: user.userName,
          email: user.email,
          phone: user.phone,
          roles: user.roles || [],
          crNo: user.crNo,
          vatId: user.vatId,
          contactPerson: user.contactPerson,
          location: user.location
        };
      }),
      catchError(error => {
        console.error('Error fetching user details:', error);
        throw error;
      })
    );
  }

  createUser(userData: CreateUserDTO): Observable<any> {
    const createUserCommand = {
      FullName: userData.fullName,
      UserName: userData.userName,
      Email: userData.email,
      Phone: userData.phone,
      Password: userData.password,
      ConfirmationPassword: userData.confirmationPassword,
      Roles: userData.roles || [],
      CR_No: userData.cr_No || null,
      VAT_ID: userData.vat_ID || null,
      ContactPerson: userData.contactPerson || null,
      Location: userData.location || null
    };

    return this.http.post(`${this.apiUrl}/Create`, createUserCommand).pipe(
      catchError(error => {
        console.error('Error creating user:', error);
        throw error;
      })
    );
  }

  updateUserProfile(id: string, userData: UpdateUserDTO): Observable<any> {
    return this.http.put(`${this.apiUrl}/EditUserProfile/${id}`, userData).pipe(
      catchError(error => {
        console.error('Error updating user profile:', error);
        throw error;
      })
    );
  }

  deleteUser(userId: string): Observable<any> {
    return this.http.delete(`${this.apiUrl}/Delete/${userId}`).pipe(
      catchError(error => {
        console.error('Error deleting user:', error);
        throw error;
      })
    );
  }

  updateUserRoles(userName: string, roles: string[]): Observable<any> {
    return this.http.put(`${this.apiUrl}/EditUserRoles`, { userName, roles }).pipe(
      catchError(error => {
        console.error('Error updating user roles:', error);
        throw error;
      })
    );
  }

  changePassword(userId: string, currentPassword: string, newPassword: string, confirmPassword: string): Observable<any> {
    const changePasswordCommand = {
      UserId: userId,
      CurrentPassword: currentPassword,
      NewPassword: newPassword,
      ConfirmPassword: confirmPassword
    };

    return this.http.post(`${this.apiUrl}/ChangePassword`, changePasswordCommand).pipe(
      catchError(error => {
        console.error('Error changing password:', error);
        throw error;
      })
    );
  }
} 