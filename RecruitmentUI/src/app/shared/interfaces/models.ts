// Shared interfaces for RecruitmentUI

export interface ApiResponse<T> {
  success: boolean;
  message: string;
  data: T;
}

export interface PaginatedResponse<T> {
  data: T[];
  message?: string;
  metadata: PaginationMetadata;
  success?: boolean;
}

export interface PaginationMetadata {
  currentPage: number;
  hasNext: boolean;
  hasPrevious: boolean;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

export interface PaginationParams {
  pageNumber: number;
  pageSize: number;
}

// Auth interfaces
export interface LoginRequest {
  email: string;
  password: string;
}

export interface LoginResponse {
  token: string;
  fullName: string;
  userId: string;
  role: string;
}

export interface RegisterRequest {
  fullName: string;
  email: string;
  password: string;
  confirmPassword: string;
  phoneNumber: string;
}

// Candidate interfaces
export interface CandidateDto {
  candidateId: number;
  userId: string;
  fullName: string;
  nationalId: string | null;
  idType: number;
  gender: number | null;
  dateOfBirth: string | null;
  nationality: string | null;
  mobileNumber: string;
  email: string;
  nationalAddress: string | null;
  residenceCity: string | null;
  residenceLatitude: number | null;
  residenceLongitude: number | null;
  profileStatus: number;
  profileStatusName: string;
  isProfileLocked: boolean;
  profilePhotoPath: string | null;
  cvSummary: string | null;
}

export interface CandidateDetailDto extends CandidateDto {
  educations: CandidateEducationDto[];
  experiences: CandidateExperienceDto[];
  documents: CandidateDocumentDto[];
}

export interface CandidateEducationDto {
  candidateEducationId: number;
  candidateId: number;
  qualification: string;
  major: string | null;
  institution: string | null;
  graduationYear: number | null;
  gradeOrGPA: string | null;
  country: string | null;
  dataSource: number;
}

export interface CandidateExperienceDto {
  candidateExperienceId: number;
  candidateId: number;
  employer: string | null;
  jobTitle: string | null;
  startDate: string | null;
  endDate: string | null;
  isCurrent: boolean;
  salary: number | null;
  currency: string;
  description: string | null;
  country: string | null;
  dataSource: number;
}

export interface CandidateDocumentDto {
  candidateDocumentId: number;
  candidateId: number;
  documentType: number;
  documentTypeName: string;
  fileName: string | null;
  filePath: string;
  fileSize: number | null;
  contentType: string | null;
  aiProcessingStatus: number;
  aiConfidenceScore: number | null;
  uploadedDate: string;
}

export interface CreateCandidateDto {
  fullName: string;
  nationalId?: string;
  idType?: number;
  gender?: number;
  dateOfBirth?: string;
  nationality?: string;
  mobileNumber: string;
  email: string;
  nationalAddress?: string;
  residenceCity?: string;
  educations?: CreateEducationDto[];
  experiences?: CreateExperienceDto[];
}

export interface UpdateCandidateDto extends CreateCandidateDto {
  residenceLatitude?: number;
  residenceLongitude?: number;
}

export interface CreateEducationDto {
  qualification: string;
  major?: string;
  institution?: string;
  graduationYear?: number;
  gradeOrGPA?: string;
  country?: string;
}

export interface CreateExperienceDto {
  employer?: string;
  jobTitle?: string;
  startDate?: string;
  endDate?: string;
  isCurrent?: boolean;
  salary?: number;
  currency?: string;
  description?: string;
  country?: string;
}

// Vacancy interfaces
export interface VacancyDto {
  vacancyId: number;
  requisitionNumber: string;
  jobTitle: string;
  departmentName: string | null;
  location: string | null;
  jobGrade: string | null;
  numberOfPositions: number;
  workType: number;
  workTypeName: string;
  workLocation: number;
  workLocationName: string;
  publishStatus: number;
  publishStatusName: string;
  publishedDate: string | null;
  closingDate: string | null;
  salaryRangeMin: number | null;
  salaryRangeMax: number | null;
  currency: string;
}

export interface VacancyDetailDto extends VacancyDto {
  jobDescription: string | null;
  qualifications: string | null;
  requirements: string | null;
  requiredDocuments: string | null;
  requiredExperienceMin: number | null;
  requiredExperienceMax: number | null;
  requiredNationality: string | null;
  requiredSpecialization: string | null;
  requiredQualification: string | null;
  requiredCertifications: string | null;
  requiredSkills: string | null;
  recruiters: VacancyRecruiterDto[];
}

export interface VacancyRecruiterDto {
  vacancyRecruiterId: number;
  recruiterUserId: string;
  recruiterName: string;
  isPrimary: boolean;
}

// Application interfaces
export interface ApplicationDto {
  applicationId: number;
  candidateId: number;
  candidateName: string;
  vacancyId: number;
  vacancyTitle: string;
  applicationDate: string;
  status: number;
  statusName: string;
  matchScore: number | null;
  isAutoMatched: boolean;
  assignedRecruiterName: string | null;
}

export interface ApplicationDetailDto extends ApplicationDto {
  rejectionReason: string | null;
  rejectionPhase: string | null;
  matchResult: MatchResultDto | null;
  screeningTasks: ScreeningTaskDto[];
  interviews: InterviewDto[];
}

export interface MatchResultDto {
  matchResultId: number;
  specializationScore: number | null;
  experienceScore: number | null;
  qualificationScore: number | null;
  nationalityScore: number | null;
  locationScore: number | null;
  certificationScore: number | null;
  overallScore: number;
  isMatch: boolean;
  matchExplanation: string | null;
}

export interface ScreeningTaskDto {
  screeningTaskId: number;
  assignedToName: string | null;
  taskDescription: string | null;
  deadline: string | null;
  status: number;
  statusName: string;
}

export interface InterviewDto {
  interviewId: number;
  interviewType: number;
  interviewTypeName: string;
  interviewMode: number;
  interviewModeName: string;
  scheduledDate: string;
  location: string | null;
  interviewerName: string | null;
  status: number;
  statusName: string;
  candidateConfirmed: boolean;
}

export interface CreateApplicationDto {
  vacancyId: number;
}

// Enums as const objects for display
export const ProfileStatus: Record<number, string> = {
  0: 'Incomplete',
  1: 'Submitted',
  2: 'Under Review',
  3: 'Approved',
  4: 'Correction Required',
  5: 'Rejected'
};

export const ApplicationStatusLabels: Record<number, string> = {
  0: 'Applied',
  1: 'Matched',
  2: 'Not Matched',
  3: 'Screening',
  4: 'Verified',
  5: 'Shared with Department',
  6: 'Shortlisted',
  7: 'Interview Scheduled',
  8: 'Interview Completed',
  9: 'HR Interview Scheduled',
  10: 'HR Interview Completed',
  11: 'HR Approved',
  12: 'HR Rejected',
  13: 'Medical Pending',
  14: 'Medical Fit',
  15: 'Medical Unfit',
  16: 'Medical Conditional',
  17: 'Offer Draft',
  18: 'Offer Pending Approval',
  19: 'Offer Approved',
  20: 'Offer Sent',
  21: 'Offer Accepted',
  22: 'Offer Rejected',
  23: 'Onboarding',
  24: 'Hired',
  25: 'Rejected',
  26: 'Withdrawn'
};

export const WorkTypeLabels: Record<number, string> = {
  0: 'Full Time',
  1: 'Part Time',
  2: 'Contract'
};

export const WorkLocationLabels: Record<number, string> = {
  0: 'On Site',
  1: 'Remote',
  2: 'Hybrid'
};

export const DocumentTypeLabels: Record<number, string> = {
  0: 'CV',
  1: 'National ID',
  2: 'Passport',
  3: 'Degree',
  4: 'Certificate',
  5: 'License',
  6: 'Other'
};

// CV Parse Result interfaces (returned by /documents/parse-cv-preview)
export interface CvPersonalInfo {
  fullName: string | null;
  email: string | null;
  phone: string | null;
  nationality: string | null;
  dateOfBirth: string | null;
  nationalId: string | null;
  address: string | null;
  city: string | null;
}

export interface CvEducationItem {
  qualification: string | null;
  major: string | null;
  institution: string | null;
  graduationYear: number | null;
  grade: string | null;
  country: string | null;
}

export interface CvExperienceItem {
  employer: string | null;
  jobTitle: string | null;
  startDate: string | null;
  endDate: string | null;
  isCurrent: boolean;
  salary: number | null;
  currency: string | null;
  description: string | null;
  country: string | null;
}

export interface CvParseResult {
  personalInfo: CvPersonalInfo | null;
  education: CvEducationItem[];
  experience: CvExperienceItem[];
  skills: string[];
  summary: string | null;
  totalYearsOfExperience: number | null;
  overallConfidence: number;
}
