import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, catchError, map, of } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse, LookupItemDto } from '../../shared/interfaces/models';

@Injectable({ providedIn: 'root' })
export class LookupService {
  private readonly base = `${environment.recruitmentApiUrl}/lookups`;

  constructor(private http: HttpClient) {}

  private normalizeResponse(response: ApiResponse<LookupItemDto[]> | LookupItemDto[] | any): ApiResponse<LookupItemDto[]> {
    if (Array.isArray(response)) {
      return { success: true, message: '', data: response };
    }

    const data = Array.isArray(response?.data)
      ? response.data
      : Array.isArray(response?.result)
        ? response.result
        : [];

    return {
      success: response?.success ?? true,
      message: response?.message ?? '',
      data
    };
  }

  private getLookup(url: string): Observable<ApiResponse<LookupItemDto[]>> {
    return this.http.get<ApiResponse<LookupItemDto[]> | LookupItemDto[]>(url).pipe(
      map((res) => this.normalizeResponse(res)),
      catchError((error) => {
        console.error('[LookupService] lookup request failed:', url, error);
        return of({
          success: false,
          message: error?.error?.message ?? 'Lookup request failed.',
          data: []
        });
      })
    );
  }

  getNationalities(): Observable<ApiResponse<LookupItemDto[]>> {
    return this.getLookup(`${this.base}/nationalities`);
  }

  getCountries(): Observable<ApiResponse<LookupItemDto[]>> {
    return this.getLookup(`${this.base}/countries`);
  }

  getRegions(countryId?: number): Observable<ApiResponse<LookupItemDto[]>> {
    const url = countryId
      ? `${this.base}/regions?countryId=${countryId}`
      : `${this.base}/regions`;
    return this.getLookup(url);
  }

  getCities(countryId?: number): Observable<ApiResponse<LookupItemDto[]>> {
    const url = countryId
      ? `${this.base}/cities?countryId=${countryId}`
      : `${this.base}/cities`;
    return this.getLookup(url);
  }

  getDistricts(cityId?: number): Observable<ApiResponse<LookupItemDto[]>> {
    const url = cityId
      ? `${this.base}/districts?cityId=${cityId}`
      : `${this.base}/districts`;
    return this.getLookup(url);
  }

  getDistrictCodes(districtId?: number): Observable<ApiResponse<LookupItemDto[]>> {
    const url = districtId
      ? `${this.base}/district-codes?districtId=${districtId}`
      : `${this.base}/district-codes`;
    return this.getLookup(url);
  }

  getDegrees(): Observable<ApiResponse<LookupItemDto[]>> {
    return this.getLookup(`${this.base}/degrees`);
  }

  getCertificates(): Observable<ApiResponse<LookupItemDto[]>> {
    return this.getLookup(`${this.base}/certificates`);
  }

  getMajors(): Observable<ApiResponse<LookupItemDto[]>> {
    return this.getLookup(`${this.base}/majors`);
  }

  getInstitutions(countryId?: number): Observable<ApiResponse<LookupItemDto[]>> {
    const url = countryId
      ? `${this.base}/institutions?countryId=${countryId}`
      : `${this.base}/institutions`;
    return this.getLookup(url);
  }

  getUniversities(countryId?: number): Observable<ApiResponse<LookupItemDto[]>> {
    const url = countryId
      ? `${this.base}/universities?countryId=${countryId}`
      : `${this.base}/universities`;
    return this.getLookup(url);
  }

  getCurrencies(): Observable<ApiResponse<LookupItemDto[]>> {
    return this.getLookup(`${this.base}/currencies`);
  }

  getQualificationTypes(): Observable<ApiResponse<LookupItemDto[]>> {
    return this.getLookup(`${this.base}/qualification-types`);
  }
}
