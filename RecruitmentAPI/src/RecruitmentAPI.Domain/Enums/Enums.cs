namespace RecruitmentAPI.Domain.Enums;

public enum CandidateProfileStatus : byte
{
    Incomplete = 0,
    Submitted = 1,
    UnderReview = 2,
    Approved = 3,
    CorrectionRequired = 4,
    Rejected = 5
}

public enum IdType : byte
{
    NationalId = 0,
    Passport = 1
}

public enum Gender : byte
{
    Male = 0,
    Female = 1
}

public enum OcrVerificationStatus : byte
{
    Pending = 0,
    Verified = 1,
    Mismatch = 2
}

public enum DocumentType : byte
{
    CV = 0,
    NationalId = 1,
    Passport = 2,
    Degree = 3,
    Certificate = 4,
    License = 5,
    Other = 6
}

public enum AiProcessingStatus : byte
{
    Pending = 0,
    Processing = 1,
    Completed = 2,
    Failed = 3
}

public enum DataSource : byte
{
    ManualEntry = 0,
    CvParsed = 1,
    OcrExtracted = 2
}

public enum MismatchSeverity : byte
{
    None = 0,
    Minor = 1,
    Major = 2,
    Critical = 3
}

public enum VacancyPublishStatus : byte
{
    Draft = 0,
    PendingApproval = 1,
    Published = 2,
    Paused = 3,
    Closed = 4,
    Cancelled = 5
}

public enum WorkType : byte
{
    FullTime = 0,
    PartTime = 1,
    Contract = 2
}

public enum WorkLocation : byte
{
    OnSite = 0,
    Remote = 1,
    Hybrid = 2
}

public enum ApprovalStatus : byte
{
    Pending = 0,
    Approved = 1,
    Rejected = 2,
    ReturnedForRevision = 3
}

public enum ApplicationStatus : byte
{
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

public enum ScreeningTaskStatus : byte
{
    Pending = 0,
    InProgress = 1,
    Completed = 2
}

public enum InterviewType : byte
{
    Technical = 0,
    Managerial = 1,
    HR = 2,
    Final = 3
}

public enum InterviewMode : byte
{
    OnSite = 0,
    Online = 1
}

public enum InterviewStatus : byte
{
    Scheduled = 0,
    Confirmed = 1,
    Rescheduled = 2,
    Completed = 3,
    Cancelled = 4,
    NoShow = 5
}

public enum EvaluationType : byte
{
    Technical = 0,
    Managerial = 1,
    HR = 2
}

public enum Recommendation : byte
{
    StrongReject = 0,
    Reject = 1,
    Neutral = 2,
    Recommend = 3,
    StronglyRecommend = 4
}

public enum EvaluationDecision : byte
{
    Pending = 0,
    Accepted = 1,
    Rejected = 2
}

public enum ClearanceStatus : byte
{
    Pending = 0,
    Fit = 1,
    Unfit = 2,
    Conditional = 3
}

public enum OfferStatus : byte
{
    Draft = 0,
    PendingApproval = 1,
    Approved = 2,
    Sent = 3,
    Accepted = 4,
    Rejected = 5,
    Expired = 6,
    Revoked = 7
}

public enum SentVia : byte
{
    Email = 0,
    WhatsApp = 1,
    Both = 2
}

public enum OnboardingStatus : byte
{
    Pending = 0,
    InProgress = 1,
    Completed = 2,
    Cancelled = 3
}

public enum EmployeeStatus : byte
{
    Pending = 0,
    Active = 1
}

public enum HcmSyncStatus : byte
{
    Pending = 0,
    Synced = 1,
    Failed = 2
}

public enum OnboardingTaskStatus : byte
{
    Pending = 0,
    InProgress = 1,
    Completed = 2,
    Skipped = 3
}

public enum NotificationChannel : byte
{
    Email = 0,
    WhatsApp = 1,
    InApp = 2,
    SMS = 3
}

public enum NotificationStatus : byte
{
    Pending = 0,
    Sent = 1,
    Failed = 2,
    Read = 3
}
