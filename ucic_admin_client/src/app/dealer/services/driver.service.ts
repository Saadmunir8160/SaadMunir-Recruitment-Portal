import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface Driver {
  driverID: number;
  userId: string;
  dealerID: number;
  ln_ID?: string;
  iqamaNumber?: string;
  isActive: boolean;
  // User information from ApplicationUser
  userName: string;
  fullName: string;
  email: string;
  phoneNumber: string;
  createdDate: Date;
  updatedDate: Date;
  
  // Legacy fields for compatibility (can be removed later)
  id?: number;
  name?: string;
  licenseNumber?: string;
  licenseExpiryDate?: Date;
  phone?: string;
  createdAt?: Date;
  updatedAt?: Date;
}

export interface CreateDriverRequest {
  fullName: string;
  userName: string;
  email: string;
  phone: string;
  password: string;
  confirmationPassword: string;
  ln_ID?: string;
  iqamaNumber?: string;
}

export interface UpdateDriverRequest {
  driverId: number;
  userId: string;
  dealerID: number;
  fullName: string;
  email: string;
  phoneNumber: string;
  ln_ID?: string;
  iqamaNumber?: string;
  isActive?: boolean;
}

export interface PaginatedResponse<T> {
  data: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
}

@Injectable({
  providedIn: 'root'
})
export class DriverService {
  // General (system-wide) driver endpoints
  private generalApiUrl = `${environment.apiUrl}/driver`;
  // Dealer-scoped endpoints for create/update/delete operations
  private dealerApiUrl = `${environment.apiUrl}/dealer/drivers`;
  private dealerUserApiUrl = `${environment.apiUrl}/DealerUser`;

  constructor(private http: HttpClient) {}

  // Return dealer-specific drivers (reads DealerDriver table)
  getDrivers(): Observable<Driver[]> {
    return this.http.get<any>(`${this.dealerApiUrl}?pageNumber=1&pageSize=1000`).pipe(
      map(response => {
        const items = response.data || [];
        return (items || []).map((driver: any) => ({
          driverID: driver.driverID,
          userId: driver.userId || '',
          dealerID: driver.dealerID,
          ln_ID: driver.ln_ID,
          iqamaNumber: driver.iqamaNumber,
          isActive: driver.isActive !== undefined ? driver.isActive : true,
          userName: driver.userName || driver.userId || '',
          fullName: driver.fullName || driver.userName || driver.userId || 'Driver',
          email: driver.email || 'Not available',
          phoneNumber: driver.phoneNumber || 'Not available',
          createdDate: new Date(driver.createdDate || Date.now()),
          updatedDate: new Date(driver.updatedDate || Date.now()),
          id: driver.driverID,
          name: driver.fullName || driver.userName || driver.userId || 'Unknown',
          licenseNumber: driver.ln_ID || '',
          licenseExpiryDate: driver.licenseExpiryDate ? new Date(driver.licenseExpiryDate) : new Date(),
          phone: driver.phoneNumber || 'Not available',
          createdAt: new Date(driver.createdDate || Date.now()),
          updatedAt: new Date(driver.updatedDate || Date.now())
        }));
      })
    );
  }

  // Get a single driver from dealer endpoint
  getDriver(id: number): Observable<Driver> {
    return this.http.get<any>(`${this.dealerApiUrl}/${id}`).pipe(
      map(response => {
        const driver = response.data || response;
        return {
          driverID: driver.driverID,
          userId: driver.userId || '',
          dealerID: driver.dealerID,
          ln_ID: driver.ln_ID,
          iqamaNumber: driver.iqamaNumber,
          isActive: driver.isActive !== undefined ? driver.isActive : true,
          userName: driver.userName || driver.userId || '',
          fullName: driver.fullName || driver.userName || driver.userId || 'Driver',
          email: driver.email || 'Not available',
          phoneNumber: driver.phoneNumber || 'Not available',
          createdDate: new Date(driver.createdDate || Date.now()),
          updatedDate: new Date(driver.updatedDate || Date.now()),
          id: driver.driverID,
          name: driver.fullName || driver.userName || driver.userId || 'Unknown',
          licenseNumber: driver.ln_ID || '',
          licenseExpiryDate: driver.licenseExpiryDate ? new Date(driver.licenseExpiryDate) : new Date(),
          phone: driver.phoneNumber || 'Not available',
          createdAt: new Date(driver.createdDate || Date.now()),
          updatedAt: new Date(driver.updatedDate || Date.now())
        };
      })
    );
  }

  // Create/update/delete/activate/deactivate remain dealer-scoped
  createDriver(driver: CreateDriverRequest): Observable<Driver> {
    return this.http.post<any>(`${this.dealerUserApiUrl}/CreateDriver`, driver).pipe(
      map(response => response.data)
    );
  }

  updateDriver(driver: UpdateDriverRequest): Observable<Driver> {
    const updateData = {
      driverID: driver.driverId,
      userId: driver.userId,
      dealerID: driver.dealerID,
      fullName: driver.fullName,
      email: driver.email,
      phoneNumber: driver.phoneNumber,
      ln_ID: driver.ln_ID,
      iqamaNumber: driver.iqamaNumber,
      isActive: driver.isActive
    };
    
    return this.http.put<any>(`${this.dealerApiUrl}/${driver.driverId}`, updateData).pipe(
      map(response => response.data)
    );
  }

  deleteDriver(id: number): Observable<void> {
    return this.http.delete<void>(`${this.dealerApiUrl}/${id}`);
  }

  activateDriver(id: number): Observable<Driver> {
    return this.http.patch<any>(`${this.dealerApiUrl}/${id}/activate`, {}).pipe(
      map(response => response.data)
    );
  }

  deactivateDriver(id: number): Observable<Driver> {
    return this.http.patch<any>(`${this.dealerApiUrl}/${id}/deactivate`, {}).pipe(
      map(response => response.data)
    );
  }
}