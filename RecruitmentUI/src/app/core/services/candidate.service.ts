import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  ApiResponse,
  CandidateDetailDto,
  CreateCandidateDto,
  UpdateCandidateDto,
  CvParseResult
} from '../../shared/interfaces/models';

@Injectable({ providedIn: 'root' })
export class CandidateService {
  private apiUrl = environment.recruitmentApiUrl;

  constructor(private http: HttpClient) {}

  getProfile(): Observable<ApiResponse<CandidateDetailDto>> {
    return this.http.get<ApiResponse<CandidateDetailDto>>(`${this.apiUrl}/candidates/profile`);
  }

  getCandidateById(id: number): Observable<ApiResponse<CandidateDetailDto>> {
    return this.http.get<ApiResponse<CandidateDetailDto>>(`${this.apiUrl}/candidates/${id}`);
  }

  createCandidate(data: CreateCandidateDto): Observable<ApiResponse<number>> {
    return this.http.post<ApiResponse<number>>(`${this.apiUrl}/candidates`, data);
  }

  updateCandidate(id: number, data: UpdateCandidateDto): Observable<ApiResponse<string>> {
    return this.http.put<ApiResponse<string>>(`${this.apiUrl}/candidates/${id}`, data);
  }

  submitProfile(id: number): Observable<ApiResponse<string>> {
    return this.http.post<ApiResponse<string>>(`${this.apiUrl}/candidates/${id}/submit`, {});
  }

  uploadDocument(candidateId: number, file: File, documentType: number): Observable<ApiResponse<number>> {
    const formData = new FormData();
    formData.append('file', file);
    formData.append('documentType', documentType.toString());
    return this.http.post<ApiResponse<number>>(`${this.apiUrl}/candidates/${candidateId}/documents`, formData);
  }

  deleteDocument(candidateId: number, documentId: number): Observable<ApiResponse<string>> {
    return this.http.delete<ApiResponse<string>>(`${this.apiUrl}/candidates/${candidateId}/documents/${documentId}`);
  }

  parseCvPreview(file: File): Observable<{ success: boolean; data: CvParseResult }> {
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post<{ success: boolean; data: CvParseResult }>(`${this.apiUrl}/documents/parse-cv-preview`, formData);
  }
}
