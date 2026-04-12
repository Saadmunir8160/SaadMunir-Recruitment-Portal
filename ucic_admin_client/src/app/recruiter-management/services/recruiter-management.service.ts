import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import { PaginatedResponse } from '../../shared/interfaces/paginated-response.interface';
import {
  CandidateListDto,
  CandidateDetailDto,
  VacancyListDto,
  VacancyDetailDto,
  CreateVacancyDto,
  ApplicationListDto,
  ApplicationDetailDto,
  MatchResultDto,
  InterviewListDto,
  ScreeningTaskDto,
  OcrVerificationResultDto
} from '../models/recruitment.models';

interface ApiResponse<T> {
  success: boolean;
  data: T;
  message: string;
}

@Injectable({ providedIn: 'root' })
export class RecruiterManagementService {
  private baseUrl = environment.recruitmentApiUrl;
  private readonly publishStatusMap: Record<string, number> = {
    draft: 0,
    pendingapproval: 1,
    published: 2,
    paused: 3,
    closed: 4,
    cancelled: 5
  };
  private readonly workTypeMap: Record<string, number> = {
    fulltime: 0,
    parttime: 1,
    contract: 2,
    internship: 3
  };
  private readonly workLocationMap: Record<string, number> = {
    onsite: 0,
    remote: 1,
    hybrid: 2
  };

  constructor(private http: HttpClient) { }

  // ========================
  // CANDIDATES
  // ========================

  getCandidates(pageNumber: number = 1, pageSize: number = 10): Observable<PaginatedResponse<CandidateListDto>> {
    const params = new HttpParams()
      .set('pageNumber', pageNumber)
      .set('pageSize', pageSize);
    return this.http.get<PaginatedResponse<CandidateListDto>>(`${this.baseUrl}/Candidates`, { params });
  }

  getCandidateById(id: number): Observable<CandidateDetailDto> {
    return this.http.get<ApiResponse<CandidateDetailDto>>(`${this.baseUrl}/Candidates/${id}`)
      .pipe(map(r => r.data));
  }

  updateCandidateStatus(id: number, body: { status: number; reason?: string }): Observable<any> {
    return this.http.put(`${this.baseUrl}/Candidates/${id}/status`, { newStatus: body.status, reason: body.reason });
  }

  // ========================
  // DOCUMENTS
  // ========================

  getCandidateDocuments(candidateId: number): Observable<any> {
    return this.http.get(`${this.baseUrl}/Documents/candidates/${candidateId}`);
  }

  downloadDocument(documentId: number): Observable<Blob> {
    return this.http.get(`${this.baseUrl}/Documents/${documentId}/download`, { responseType: 'blob' });
  }

  // ========================
  // VACANCIES
  // ========================

  getVacancies(pageNumber: number = 1, pageSize: number = 10, status?: number): Observable<PaginatedResponse<VacancyListDto>> {
    let params = new HttpParams()
      .set('pageNumber', pageNumber)
      .set('pageSize', pageSize);
    if (status !== undefined && status !== null) {
      params = params.set('status', status);
    }
    return this.http.get<PaginatedResponse<VacancyListDto>>(`${this.baseUrl}/Vacancies`, { params })
      .pipe(
        map((res) => ({
          ...res,
          data: (res.data || []).map((v: any) => ({
            ...v,
            publishStatus: this.normalizeEnumValue(v.publishStatus, this.publishStatusMap)
          }))
        }))
      );
  }

  getVacancyById(id: number): Observable<VacancyDetailDto> {
    return this.http.get<ApiResponse<VacancyDetailDto>>(`${this.baseUrl}/Vacancies/${id}`)
      .pipe(
        map((r: any) => {
          const d = r.data || {};
          return {
            ...d,
            publishStatus: this.normalizeEnumValue(d.publishStatus, this.publishStatusMap),
            workType: this.normalizeEnumValue(d.workType, this.workTypeMap),
            workLocation: this.normalizeEnumValue(d.workLocation, this.workLocationMap)
          } as VacancyDetailDto;
        })
      );
  }

  createVacancy(vacancy: CreateVacancyDto): Observable<any> {
    return this.http.post(`${this.baseUrl}/Vacancies`, vacancy);
  }

  updateVacancy(id: number, vacancy: Partial<CreateVacancyDto>): Observable<any> {
    return this.http.put(`${this.baseUrl}/Vacancies/${id}`, vacancy);
  }

  // ========================
  // VACANCY APPROVAL WORKFLOW
  // ========================

  submitForApproval(vacancyId: number): Observable<any> {
    return this.http.post(`${this.baseUrl}/Vacancies/${vacancyId}/submit-for-approval`, {});
  }

  approveVacancy(vacancyId: number, comments?: string): Observable<any> {
    return this.http.post(`${this.baseUrl}/Vacancies/${vacancyId}/approve`, { comments });
  }

  rejectVacancy(vacancyId: number, comments?: string): Observable<any> {
    return this.http.post(`${this.baseUrl}/Vacancies/${vacancyId}/reject`, { comments });
  }

  // ========================
  // APPLICATIONS
  // ========================

  getApplicationsByVacancy(vacancyId: number, pageNumber: number = 1, pageSize: number = 10): Observable<PaginatedResponse<ApplicationListDto>> {
    const params = new HttpParams()
      .set('pageNumber', pageNumber)
      .set('pageSize', pageSize);
    return this.http.get<PaginatedResponse<ApplicationListDto>>(
      `${this.baseUrl}/Applications/vacancy/${vacancyId}`, { params }
    );
  }

  getApplicationById(id: number): Observable<ApplicationDetailDto> {
    return this.http.get<ApiResponse<ApplicationDetailDto>>(`${this.baseUrl}/Applications/${id}`)
      .pipe(map(r => r.data));
  }

  updateApplicationStatus(id: number, body: { status: number; reason?: string }): Observable<any> {
    return this.http.put(`${this.baseUrl}/Applications/${id}/status`, { newStatus: body.status, reason: body.reason });
  }

  getMatchResult(applicationId: number): Observable<MatchResultDto> {
    return this.http.get<ApiResponse<MatchResultDto>>(`${this.baseUrl}/Applications/${applicationId}/match-result`)
      .pipe(map(r => r.data));
  }

  // ========================
  // INTERVIEWS
  // ========================

  getInterviews(pageNumber: number = 1, pageSize: number = 10): Observable<PaginatedResponse<InterviewListDto>> {
    const params = new HttpParams()
      .set('pageNumber', pageNumber)
      .set('pageSize', pageSize);
    return this.http.get<PaginatedResponse<InterviewListDto>>(`${this.baseUrl}/Interviews`, { params });
  }

  scheduleInterview(body: any): Observable<any> {
    return this.http.post(`${this.baseUrl}/Interviews`, body);
  }

  // ========================
  // SCREENING TASKS
  // ========================

  getScreeningTasks(applicationId: number): Observable<ScreeningTaskDto[]> {
    return this.http.get<ApiResponse<ScreeningTaskDto[]>>(`${this.baseUrl}/ScreeningTasks/application/${applicationId}`)
      .pipe(map(r => r.data || []));
  }

  getInterviewsByApplication(applicationId: number): Observable<InterviewListDto[]> {
    return this.http.get<ApiResponse<InterviewListDto[]>>(`${this.baseUrl}/Interviews/application/${applicationId}`)
      .pipe(map(r => r.data || []));
  }

  createScreeningTask(body: any): Observable<any> {
    return this.http.post(`${this.baseUrl}/ScreeningTasks`, body);
  }

  updateScreeningTask(id: number, body: any): Observable<any> {
    return this.http.put(`${this.baseUrl}/ScreeningTasks/${id}`, body);
  }

  // ========================
  // OCR VERIFICATION
  // ========================

  getOcrResults(candidateId: number): Observable<OcrVerificationResultDto[]> {
    return this.http.get<ApiResponse<OcrVerificationResultDto[]>>(
      `${this.baseUrl}/Documents/candidates/${candidateId}/ocr-results`
    ).pipe(map(r => r.data || []));
  }

  triggerOcrVerification(documentId: number): Observable<any> {
    return this.http.post(`${this.baseUrl}/Documents/${documentId}/verify-ocr`, {});
  }

  markOcrVerified(candidateId: number): Observable<any> {
    return this.http.post(`${this.baseUrl}/Candidates/${candidateId}/mark-ocr-verified`, {});
  }

  private normalizeEnumValue(
    value: number | string | undefined | null,
    mapDef: Record<string, number>
  ): number {
    if (typeof value === 'number') {
      return value;
    }
    if (typeof value === 'string') {
      const key = value.replace(/\s|_/g, '').toLowerCase();
      if (Object.prototype.hasOwnProperty.call(mapDef, key)) {
        return mapDef[key];
      }
      const parsed = Number(value);
      if (!Number.isNaN(parsed)) {
        return parsed;
      }
    }
    return 0;
  }
}
