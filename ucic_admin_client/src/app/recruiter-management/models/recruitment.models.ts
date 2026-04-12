// ---- Enums ----

export enum CandidateProfileStatus {
  Incomplete = 0,
  Submitted = 1,
  UnderReview = 2,
  Approved = 3,
  CorrectionRequired = 4,
  Rejected = 5
}

export const CandidateProfileStatusLabels: Record<number, string> = {
  0: 'Incomplete',
  1: 'Submitted',
  2: 'Under Review',
  3: 'Approved',
  4: 'Correction Required',
  5: 'Rejected'
};

export enum VacancyPublishStatus {
  Draft = 0,
  PendingApproval = 1,
  Published = 2,
  Paused = 3,
  Closed = 4,
  Cancelled = 5
}

export const VacancyPublishStatusLabels: Record<number, string> = {
  0: 'Draft',
  1: 'Pending Approval',
  2: 'Published',
  3: 'Paused',
  4: 'Closed',
  5: 'Cancelled'
};

export enum ApplicationStatus {
  Applied = 0,
  Matched = 1,
  NotMatched = 2,
  Screening = 3,
  Verified = 4,
  SharedWithDepartment = 5,
  Shortlisted = 6,
  InterviewScheduled = 7,
  InterviewCompleted = 8,
  HRInterviewScheduled = 9,
  HRInterviewCompleted = 10,
  HRApproved = 11,
  HRRejected = 12,
  MedicalPending = 13,
  MedicalFit = 14,
  MedicalUnfit = 15,
  MedicalConditional = 16,
  OfferDraft = 17,
  OfferPendingApproval = 18,
  OfferApproved = 19,
  OfferSent = 20,
  OfferAccepted = 21,
  OfferRejected = 22,
  Onboarding = 23,
  Hired = 24,
  Rejected = 25,
  Withdrawn = 26
}

export const ApplicationStatusLabels: Record<number, string> = {
  0: 'Applied',
  1: 'Matched',
  2: 'Not Matched',
  3: 'Screening',
  4: 'Verified',
  5: 'Shared With Dept',
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

export enum DocumentType {
  CV = 0,
  NationalId = 1,
  Passport = 2,
  Degree = 3,
  Certificate = 4,
  License = 5,
  Other = 6
}

export const DocumentTypeLabels: Record<number, string> = {
  0: 'CV',
  1: 'National ID',
  2: 'Passport',
  3: 'Degree',
  4: 'Certificate',
  5: 'License',
  6: 'Other'
};

// ---- DTOs ----

export interface CandidateListDto {
  candidateId: number;
  userId: string;
  fullName: string;
  nationalId: string;
  email: string;
  mobileNumber: string;
  nationality: string;
  profileStatus: number;
  profileStatusName: string;
  createdDate: string;
}

export interface CandidateDetailDto {
  candidateId: number;
  userId: string;
  fullName: string;
  nationalId: string;
  idType: number;
  gender: number;
  dateOfBirth: string;
  nationality: string;
  mobileNumber: string;
  email: string;
  nationalAddress: string;
  residenceCity: string;
  profileStatus: number;
  isProfileLocked: boolean;
  rejectionReason: string;
  ocrVerificationStatus: number;
  cvSummary: string;
  parsedCvJson: string;
  createdDate: string;
  educations: CandidateEducationDto[];
  experiences: CandidateExperienceDto[];
  documents: CandidateDocumentDto[];
}

export interface CandidateEducationDto {
  candidateEducationId: number;
  qualification: string;
  major: string;
  institution: string;
  graduationYear: number;
  gradeOrGPA: string;
  country: string;
  dataSource: number;
}

export interface CandidateExperienceDto {
  candidateExperienceId: number;
  employer: string;
  jobTitle: string;
  startDate: string;
  endDate: string;
  isCurrent: boolean;
  salary: number;
  currency: string;
  description: string;
  country: string;
  dataSource: number;
}

export interface CandidateDocumentDto {
  candidateDocumentId: number;
  documentType: number;
  fileName: string;
  filePath: string;
  fileSize: number;
  contentType: string;
  aiProcessingStatus: number;
  aiConfidenceScore: number;
  uploadedDate: string;
}

export interface VacancyListDto {
  vacancyId: number;
  requisitionNumber: string;
  jobTitle: string;
  departmentName: string;
  location: string;
  numberOfPositions: number;
  filledPositions: number;
  publishStatus: number;
  publishStatusName: string;
  closingDate: string;
  createdDate: string;
  applicationsCount?: number;
}

export interface VacancyDetailDto {
  vacancyId: number;
  requisitionNumber: string;
  jobTitle: string;
  departmentName: string;
  location: string;
  jobGrade: string;
  numberOfPositions: number;
  filledPositions: number;
  jobDescription: string;
  qualifications: string;
  requirements: string;
  salaryRangeMin: number;
  salaryRangeMax: number;
  currency: string;
  workType: number;
  workLocation: number;
  requiredExperienceMin: number;
  requiredExperienceMax: number;
  requiredNationality: string;
  requiredSpecialization: string;
  requiredQualification: string;
  publishStatus: number;
  publishedDate: string;
  closingDate: string;
  createdDate: string;
  weightSpecialization: number;
  weightExperience: number;
  weightQualification: number;
  weightNationality: number;
  weightLocation: number;
  weightCertifications: number;
  matchThreshold: number;
  /** 1 = HR Manager, 2 = HR Section Head, when publishStatus is PendingApproval */
  pendingApprovalStep?: number | null;
  pendingApprovalStepName?: string | null;
}

export interface CreateVacancyDto {
  jobTitle: string;
  departmentName: string;
  location: string;
  jobGrade: string;
  numberOfPositions: number;
  jobDescription: string;
  qualifications: string;
  requirements: string;
  salaryRangeMin: number;
  salaryRangeMax: number;
  currency: string;
  workType: number;
  workLocation: number;
  requiredExperienceMin: number;
  requiredExperienceMax: number;
  requiredNationality: string;
  requiredSpecialization: string;
  requiredQualification: string;
  closingDate: string;
  weightSpecialization: number;
  weightExperience: number;
  weightQualification: number;
  weightNationality: number;
  weightLocation: number;
  weightCertifications: number;
  matchThreshold: number;
}

export interface ApplicationListDto {
  applicationId: number;
  candidateId: number;
  candidateName: string;
  vacancyId: number;
  vacancyTitle: string;
  status: number;
  statusName: string;
  matchScore: number;
  applicationDate: string;
  assignedRecruiterName: string;
}

export interface ApplicationDetailDto {
  applicationId: number;
  candidateId: number;
  candidateName: string;
  vacancyId: number;
  vacancyTitle: string;
  status: number;
  statusName: string;
  matchScore: number;
  isAutoMatched: boolean;
  applicationDate: string;
  assignedRecruiterId: string;
  assignedRecruiterName: string;
  rejectionReason: string;
  rejectionPhase: string;
}

export interface MatchResultDto {
  matchResultId: number;
  applicationId: number;
  specializationScore: number;
  experienceScore: number;
  qualificationScore: number;
  nationalityScore: number;
  locationScore: number;
  certificationScore: number;
  overallScore: number;
  isMatch: boolean;
  matchExplanation: string;
}

export interface InterviewListDto {
  interviewId: number;
  applicationId: number;
  candidateName: string;
  interviewType: number;
  interviewMode: number;
  scheduledDate: string;
  location: string;
  interviewerName: string;
  status: number;
  candidateConfirmed: boolean;
}

export interface ScreeningTaskDto {
  screeningTaskId: number;
  applicationId: number;
  assignedToName: string;
  taskDescription: string;
  deadline: string;
  status: number;
  notes: string;
}

export interface OcrVerificationResultDto {
  ocrVerificationId: number;
  candidateDocumentId: number;
  candidateId: number;
  fieldName: string;
  extractedValue: string | null;
  enteredValue: string | null;
  isMatch: boolean | null;
  confidenceScore: number | null;
  mismatchSeverity: number;
  processedDate: string;
}

export const MismatchSeverityLabels: Record<number, string> = {
  0: 'None',
  1: 'Minor',
  2: 'Major',
  3: 'Critical'
};
