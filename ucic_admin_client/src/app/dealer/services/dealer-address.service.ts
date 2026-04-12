import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from '../../../environments/environment';

export interface DealerAddress {
  id: string;
  dealerId: string;
  addressLine1: string;
  addressLine2?: string;
  city: string;
  state?: string;
  postalCode?: string;
  country?: string;
  isActive: boolean;
  createdAt?: Date;
  // Backend DTO mapping
  addressID?: number;
  dealerID?: number;
}

export interface CreateAddressRequest {
  addressLine1: string;
  addressLine2?: string;
  city: string;
  state?: string;
  postalCode?: string;
  country?: string;
  isActive?: boolean;
}

@Injectable({
  providedIn: 'root'
})
export class DealerAddressService {
  private apiUrl = `${environment.apiUrl}/DealerShippingAddress`;

  constructor(private http: HttpClient) {}

  // Get all addresses for current dealer
  getAddresses(): Observable<DealerAddress[]> {
    // Add cache-busting headers to ensure fresh data
    const headers = {
      'Cache-Control': 'no-cache, no-store, must-revalidate',
      'Pragma': 'no-cache',
      'Expires': '0'
    };
    
    return this.http.get<any>(`${this.apiUrl}/GetMyAddresses`, { headers }).pipe(
      map(response => {
        if (response.success) {
          const mappedAddresses = response.data.map((dto: any) => this.mapDtoToAddress(dto));
          return mappedAddresses;
        }
        throw new Error(response.message || 'Failed to load addresses');
      })
    );
  }

  // Get address by ID
  getAddressById(id: string): Observable<DealerAddress> {
    return this.http.get<any>(`${this.apiUrl}/GetById/${id}`).pipe(
      map(response => {
        if (response.success) {
          return this.mapDtoToAddress(response.data);
        }
        throw new Error(response.message || 'Failed to load address');
      })
    );
  }

  // Create new address
  createAddress(address: CreateAddressRequest): Observable<DealerAddress> {
    const backendDto = this.mapToBackendDto(address);
    return this.http.post<any>(`${this.apiUrl}/Create`, backendDto).pipe(
      map(response => {
        if (response.success) {
          return this.mapDtoToAddress(response.data);
        }
        throw new Error(response.message || 'Failed to create address');
      })
    );
  }

  // Update existing address
  updateAddress(id: string, address: CreateAddressRequest): Observable<DealerAddress> {
    const backendDto = {
      ...this.mapToBackendDto(address),
      addressID: parseInt(id) // Use addressID to match backend DTO
    };
    return this.http.put<any>(`${this.apiUrl}/Update/${id}`, backendDto).pipe(
      map(response => {
        if (response.success) {
          return this.mapDtoToAddress(response.data);
        }
        throw new Error(response.message || 'Failed to update address');
      })
    );
  }

  // Delete address
  deleteAddress(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/Delete/${id}`);
  }

  // Set default address - Not implemented in backend
  setDefaultAddress(id: string): Observable<void> {
    throw new Error('Default address functionality not implemented in backend');
  }

  // Create dealer profile for current user
  createDealerProfile(profileData?: { companyName?: string; dealerCode?: string }): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/CreateMyDealerProfile`, profileData || {});
  }

  // Map backend DTO to frontend interface
  private mapDtoToAddress(dto: any): DealerAddress {
    const mapped = {
      id: dto.addressID?.toString() || dto.AddressID?.toString() || '',
      dealerId: dto.dealerID?.toString() || dto.DealerID?.toString() || '',
      addressLine1: dto.addressLine1 || dto.AddressLine1 || '',
      addressLine2: dto.addressLine2 || dto.AddressLine2 || '',
      city: dto.city || dto.City || '',
      state: dto.state || dto.State || '',
      postalCode: dto.postalCode || dto.PostalCode || '',
      country: dto.country || dto.Country || '',
      isActive: dto.isActive !== undefined ? dto.isActive : (dto.IsActive !== undefined ? dto.IsActive : true),
      createdAt: dto.createdAt ? new Date(dto.createdAt) : (dto.CreatedAt ? new Date(dto.CreatedAt) : new Date()),
      // Keep original DTO fields for backward compatibility
      addressID: dto.addressID || dto.AddressID,
      dealerID: dto.dealerID || dto.DealerID
    };
    return mapped;
  }

  // Map frontend request to backend DTO
  private mapToBackendDto(address: CreateAddressRequest): any {
    return {
      // DealerID will be set by the backend based on current user
      addressLine1: address.addressLine1,
      addressLine2: address.addressLine2 || '',
      city: address.city,
      state: address.state || '',
      country: address.country || 'Saudi Arabia',
      postalCode: address.postalCode || '',
      isActive: address.isActive !== undefined ? address.isActive : true
    };
  }
}