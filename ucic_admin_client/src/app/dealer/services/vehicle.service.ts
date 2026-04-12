import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from '../../../environments/environment';

// Backend response wrapper
export interface ApiResponse<T> {
  success: boolean;
  data: T;
  message: string;
}

// Updated interfaces to match backend DTOs
export interface Vehicle {
  vehicleID: number;
  dealerID: number;
  plateNumber: string;
  type: string;
  capacity: number;
  registrationDate: Date;
  registrationExpiryDate: Date;
  insuranceExpiryDate: Date;
  ln_ID?: string;
  registrationNumber?: string;
  vehicleType?: string; // Legacy field
  isActive: boolean;
  createdAt: Date;
  updatedAt: Date;
}

export interface CreateVehicleRequest {
  dealerID: number;
  plateNumber: string;
  type: string;
  capacity: number;
  registrationDate: Date;
  registrationExpiryDate: Date;
  insuranceExpiryDate: Date;
  ln_ID?: string;
  registrationNumber?: string;
  isActive?: boolean;
}

export interface UpdateVehicleRequest extends CreateVehicleRequest {
  vehicleID: number;
}

@Injectable({
  providedIn: 'root'
})
export class VehicleService {
  // General (system-wide) vehicle endpoints
  private generalApiUrl = `${environment.apiUrl}/vehicle`;
  // Dealer-scoped endpoints for create/update/delete operations
  private dealerApiUrl = `${environment.apiUrl}/dealer/vehicles`;

  constructor(private http: HttpClient) {}

  // Return dealer-specific vehicles (read DealerVehicle table)
  getVehicles(pageNumber: number = 1, pageSize: number = 10): Observable<Vehicle[]> {
    return this.http.get<any>(`${this.dealerApiUrl}?pageNumber=${pageNumber}&pageSize=${pageSize}`).pipe(
      map(response => response.data || [])
    );
  }

  // Get single vehicle from dealer endpoint
  getVehicle(id: number): Observable<Vehicle> {
    return this.http.get<any>(`${this.dealerApiUrl}/${id}`).pipe(
      map(response => response.data || response)
    );
  }

  // Create/update/delete remain dealer-scoped
  createVehicle(vehicle: CreateVehicleRequest): Observable<number> {
    return this.http.post<ApiResponse<number>>(this.dealerApiUrl, vehicle)
      .pipe(map(response => response.data));
  }

  updateVehicle(vehicle: UpdateVehicleRequest): Observable<boolean> {
    return this.http.put<ApiResponse<boolean>>(`${this.dealerApiUrl}/${vehicle.vehicleID}`, vehicle)
      .pipe(map(response => response.data));
  }

  deleteVehicle(id: number): Observable<boolean> {
    return this.http.delete<ApiResponse<boolean>>(`${this.dealerApiUrl}/${id}`)
      .pipe(map(response => response.data));
  }

  activateVehicle(id: number): Observable<boolean> {
    return this.http.patch<ApiResponse<boolean>>(`${this.dealerApiUrl}/${id}/activate`, {})
      .pipe(map(response => response.data));
  }

  deactivateVehicle(id: number): Observable<boolean> {
    return this.http.patch<ApiResponse<boolean>>(`${this.dealerApiUrl}/${id}/deactivate`, {})
      .pipe(map(response => response.data));
  }
}