# Recruitment Management System (RMS)

## Database Design & AI Integration — Implementation Guide

> **Project:** UCIC Recruitment Management System  
> **Architecture:** Separate .NET 8 Web API (Clean Architecture + CQRS/MediatR)  
> **Database:** Separate SQL Server Database (`RecruitmentDB`)  
> **Auth:** Shared JWT from existing UCIC_API (same Key/Issuer/Audience)  
> **AI Engine:** Claude AI (Anthropic) for CV parsing, OCR verification, auto-matching, scoring & offer generation  
> **Reference Implementation:** [SwiftRecruit](https://recruitment.swiftrecruit.com) — Autofill CV parsing approach  

---

## Table of Contents

1. [Business Decisions (Q&A)](#1-business-decisions)
2. [Architecture Overview](#2-architecture-overview)
3. [Database Design](#3-database-design)
4. [AI Integration Plan](#4-ai-integration-plan)
5. [SwiftRecruit Analysis](#5-swiftrecruit-analysis)
6. [Claude AI Integration Details](#6-claude-ai-integration-details)
7. [API Endpoint Plan](#7-api-endpoint-plan)
8. [Implementation Roadmap](#8-implementation-roadmap)

---

## 1. Business Decisions (Q&A)

> All decisions below are confirmed by the client and override any conflicting assumptions in the original documents.

### Phase 1 — Candidate Registration

| Question | Decision | Impact |
|----------|----------|--------|
| Can one user have multiple roles? | **One role per user** | No multi-role assignment in UCIC_API; each user gets exactly one recruitment role |
| Can one candidate apply to multiple vacancies? | **Yes** | Applications table supports 1 candidate → N vacancies |
| Can candidates edit profile after submission? | **No** — locked after submission. Only editable if org refers back (CorrectionRequired) | ProfileStatus controls edit lock. API must check status before allowing PUT |
| Which documents require OCR verification? | **National ID and Passport** — OCR via Claude Vision | Claude Vision extracts fields from ID documents; compared against candidate-entered data in OcrVerificationResults |
| What happens if OCR fails or data mismatches? | **Flag for recruiter review** | OcrVerificationResults stores field-level mismatches; Critical mismatch → CorrectionRequired status |
| Are document expiry reminders required? | **Not important** | Skip expiry tracking for now |
| Should duplicate candidates be detected using National ID? | **Yes — National ID** | Add UNIQUE index on `Candidates.NationalId` in addition to email/mobile |

### Phase 2 — Recruiter Review

| Question | Decision | Impact |
|----------|----------|--------|
| Can recruiters edit candidate information? | **Yes, but log who modified** | AuditLogs must capture old/new values + modifier UserId for every candidate edit |
| Is "Correction Required" temporary or final? | **Temporary status** | Candidate can re-edit and re-submit; status cycles: Submitted → CorrectionRequired → Submitted |
| Can rejected candidates be reconsidered? | **Yes** — if previous/current experience matches | Add `ReactivatedFromRejection BIT` flag; allow status change Rejected → UnderReview |
| Should rejection reasons be visible to candidates? | **Internal only** | Frontend must NOT expose `RejectionReason` to candidate portal; only show generic status |

### Phase 3 — Vacancy Management

| Question | Decision | Impact |
|----------|----------|--------|
| Who can create vacancies? | **Both HR and Recruiter** | Authorize both roles on vacancy create endpoint |
| Can a vacancy be assigned to multiple recruiters? | **Yes** | **NEW TABLE: `VacancyRecruiters`** (junction table) |
| Can one vacancy have multiple locations or departments? | **No** | Single location/department per vacancy (current design is correct) |
| Is vacancy approval required before publishing? | **Yes — HR Manager AND HR Section Head** | **NEW TABLE: `VacancyApprovals`** with 2-step approval chain |
| Can vacancies be paused/closed/extended? | **Yes — by HR Manager and HR Section Head** | Add `Paused` status to `PublishStatus` enum (0=Draft, 1=PendingApproval, 2=Published, 3=Paused, 4=Closed, 5=Cancelled) |
| Should vacancies auto-close when positions filled? | **No — only when expired** | Auto-close logic based on `ClosingDate` only, not `FilledPositions` |

### Phase 4 — Auto-Matching

| Question | Decision | Impact |
|----------|----------|--------|
| Are matching rules configurable by admin? | **Yes** | Vacancy-level weight fields already in design (WeightSpecialization, etc.) |
| Can recruiters override auto-rejection? | **Yes** | Allow status change: NotMatched → Screening (manual override) |
| Can candidates reapply for same vacancy after rejection? | **No** | UNIQUE constraint on (CandidateId, VacancyId) stays; no re-application |
| Should auto-rejection reasons be visible to candidates? | **Yes** | Include match explanation in candidate-facing API (filtered view) |

### Phase 5 — Screening

| Question | Decision | Impact |
|----------|----------|--------|
| Can multiple recruiters be assigned screening tasks for same candidate? | **Yes** | Multiple ScreeningTasks per ApplicationId already supported |
| Are task deadlines mandatory? | **Optional** | `Deadline` column is already nullable |
| Who can mark candidate as "Verified"? | **Recruiters only** | Authorization check: only Recruiter role can set status = Verified |

### Phase 6 — Interviews

| Question | Decision | Impact |
|----------|----------|--------|
| Interviews conducted onsite, online, or both? | **Both** | `InterviewMode` enum already supports 0=OnSite, 1=Online |
| Who has final authority to shortlist? | **Both HR and Recruiter** | Both roles authorized for shortlisting endpoint |
| Is Hiring Manager a department manager/lead? | **Yes** | HiringManager role = department head |

### Phase 7 — Interview Confirmation

| Question | Decision | Impact |
|----------|----------|--------|
| Does "Factory Interview Security" refer to on-site access control? | **Will send them a report** | Generate security report/PDF; no system access for security personnel |
| Do security personnel need system access? | **No** | Security gets a downloadable report only, no user account needed |

### Phase 9 — HR Interview

| Question | Decision | Impact |
|----------|----------|--------|
| Is HR interview mandatory for all job roles? | **Yes** | Every application must pass through HRInterview status |
| Can HR override previous interview decisions? | **Yes** | HR can change InterviewEvaluation.Decision from Accepted to Rejected and vice versa |
| Is HR rejection final? | **Subject to further review** | HRRejected is NOT terminal; can be escalated/reconsidered |

### Phase 10 — Medical

| Question | Decision | Impact |
|----------|----------|--------|
| Medical providers: full portal or upload-only? | **Both** | MedicalProvider role gets portal access + upload capability |
| Can HR override medical results? | **Yes** | HR can change ClearanceStatus (e.g., Conditional → Fit) with audit log |

### Phase 12 — Offer

| Question | Decision | Impact |
|----------|----------|--------|
| Should offer templates vary by role or job grade? | **Both** | **Add `OfferTemplateType` and `JobGrade`** fields to JobOffers |
| Is e-signature external or internal? | **External provider (e.g., DocuSign)** | Integrate with external e-signature API; store signed doc URL |
| What if candidate doesn't respond to offer? | **Send reminder via Email AND WhatsApp** | Hangfire job: auto-reminder at day 3, 7, 10 if no response |
| Should offers auto-expire? After how many days? | **Yes — 12 days** | Default `OfferExpiryDays = 12`; auto-status → Expired via Hangfire |

### Phase 13 — Onboarding

| Question | Decision | Impact |
|----------|----------|--------|
| Is onboarding checklist same for all employees? | **Yes — same for all** | Standard checklist, no role/department variation |
| Should onboarding tasks be auto-assigned to departments? | **Yes** | **NEW TABLE: `OnboardingTasks`** with auto-assignment logic |
| Are HCM system integrations required? | **Yes** | Plan API integration point for HCM (future phase) |
| Who can deactivate employee after activation? | **Will handle via HCM** | Deactivation handled outside RMS; inform HCM system |

---

## 2. Architecture Overview

### System Context

```
┌─────────────────────────────────────────────────────────────────────┐
│                          FRONTENDS                                  │
│                                                                     │
│  ┌──────────────┐  ┌──────────────┐  ┌───────────────────────────┐ │
│  │ Admin Portal  │  │ Dealer Portal│  │ Recruitment Portal (NEW)  │ │
│  │ (Angular)     │  │ (Angular)    │  │ (Angular)                 │ │
│  └──────┬───────┘  └──────┬───────┘  └─────────────┬─────────────┘ │
│         │                 │                         │               │
│         │        LOGIN (all portals)                │               │
│         └────────────┬──────────────────────────────┘               │
│                      ▼                                              │
│         ┌─────────────────────────┐                                │
│         │   UCIC_API (Existing)   │                                │
│         │   POST /api/Auth/Login  │                                │
│         │   Returns JWT Token     │                                │
│         │   with roles & userId   │                                │
│         └─────────────────────────┘                                │
│                      │                                              │
│              JWT Token issued                                       │
│                      │                                              │
│         ┌────────────┴─────────────────┐                           │
│         ▼                              ▼                           │
│  ┌─────────────────┐     ┌──────────────────────────┐              │
│  │  UCIC_API        │     │  RECRUITMENT_API (NEW)   │             │
│  │  (Existing DB)   │     │  (Separate DB)           │             │
│  │  DealerTestDB    │     │  RecruitmentDB           │             │
│  │                  │     │                          │              │
│  │  • Shop APIs     │     │  • Jobs/Vacancies        │             │
│  │  • Dealer APIs   │     │  • Applications          │             │
│  │  • Admin APIs    │     │  • CV Processing (AI)    │             │
│  │  • HR basics     │     │  • Interviews            │             │
│  │  • Identity      │     │  • Offers/Onboarding     │             │
│  └─────────────────┘     └──────────────────────────┘              │
│                                    │                                │
│                           ┌────────┴────────┐                      │
│                           ▼                 ▼                      │
│                    ┌─────────────┐   ┌─────────────┐               │
│                    │ Claude AI   │   │ Azure Blob  │               │
│                    │ (Anthropic) │   │ Storage     │               │
│                    └─────────────┘   └─────────────┘               │
└─────────────────────────────────────────────────────────────────────┘
```

### Authentication Flow

```
1. ALL users login via UCIC_API → POST /api/Auth/Login
2. UCIC_API validates credentials against Identity DB (DealerTestDB → auth schema)
3. JWT token returned with claims: UserId, FullName, Roles
4. Frontend stores token in localStorage
5. Recruitment Portal sends token in Authorization header to Recruitment_API
6. Recruitment_API validates token using SAME JWT Key/Issuer/Audience
7. No database connection between the two APIs — only shared JWT config
```

### Token Claims (Already in UCIC_API TokenGenerator.cs)

```json
{
  "sub": "username",
  "jti": "user-id-guid",
  "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name": "Full Name",
  "UserId": "user-id-guid",
  "http://schemas.microsoft.com/ws/2008/06/identity/claims/role": "Recruiter",
  "exp": 1234567890,
  "iss": "jwt",
  "aud": "jwt"
}
```

### Roles to Add in UCIC_API (DataSeeder.cs — 1 line each)

> ⚠️ **Business Rule: ONE role per user.** Users cannot have multiple recruitment roles simultaneously.

| Role | Purpose |
|------|---------|
| `Recruiter` | HR staff managing recruitment — can edit candidates, verify, shortlist |
| `HRSupervisor` | Senior HR / HR Section Head — conducts HR interviews, prepares offers, approves vacancies |
| `HiringManager` | Department manager/lead — reviews CVs, conducts technical interviews, shortlists |
| `VPO` | VP Operations — offer approval chain |
| `MedicalProvider` | External — portal access + upload medical reports |
| `Candidate` | Job seekers — register, apply, view status (profile locked after submission) |

---

## 3. Database Design

### Database: `RecruitmentDB` (Separate from UCIC DealerTestDB)

> All tables follow the same `BaseEntity` pattern as UCIC_API:
> `IsActive`, `IsDeleted`, `CreatedDate`, `CreatedBy`, `ModifiedDate`, `ModifiedBy`

---

### 3.1 Candidates

**Purpose:** Stores candidate profile data (Phase 1 — Registration)  
**Linked to:** UCIC_API `AspNetUsers.Id` via `UserId` column (no FK constraint — cross-database)

> ⚠️ **Q&A Rules:**  
> - Profile is **locked after submission** (Status = Submitted). Candidate cannot edit.  
> - Editable again ONLY when org sets status to `CorrectionRequired`.  
> - Duplicate detection via **NationalId** (unique constraint).  
> - Rejected candidates **can be reconsidered** if experience matches (Rejected → UnderReview).  
> - Rejection reasons are **internal only** — not visible to candidates.

```sql
CREATE TABLE [recruitment].[Candidates] (
    CandidateId             BIGINT IDENTITY(1,1) PRIMARY KEY,
    UserId                  NVARCHAR(450) NOT NULL,         -- Maps to UCIC AspNetUsers.Id

    -- Personal Information (Phase 1, Section 3.2)
    FullName                NVARCHAR(200) NOT NULL,
    NationalId              NVARCHAR(50),                   -- National ID or Passport Number
    IdType                  TINYINT DEFAULT 0,              -- 0=NationalId, 1=Passport
    Gender                  TINYINT,                        -- 0=Male, 1=Female
    DateOfBirth             DATE,
    Nationality             NVARCHAR(100),
    MobileNumber            NVARCHAR(50) NOT NULL,
    Email                   NVARCHAR(150) NOT NULL,
    NationalAddress         NVARCHAR(500),

    -- Location (for proximity matching — Phase 4)
    ResidenceCity           NVARCHAR(100),
    ResidenceLatitude       DECIMAL(9,6),
    ResidenceLongitude      DECIMAL(9,6),

    -- Profile Status
    ProfileStatus           TINYINT DEFAULT 0,
    -- 0=Incomplete, 1=Submitted (LOCKED), 2=UnderReview, 3=Approved, 
    -- 4=CorrectionRequired (UNLOCKED), 5=Rejected (can be reconsidered)
    IsProfileLocked         BIT DEFAULT 0,                  -- TRUE when Submitted; FALSE when CorrectionRequired

    -- Rejection (internal only — NOT shown to candidate)
    RejectionReason         NVARCHAR(500),
    RejectionDate           DATETIME2,
    RejectedByUserId        NVARCHAR(450),
    ReactivatedFromRejection BIT DEFAULT 0,                 -- TRUE if previously rejected and reconsidered

    OcrVerificationStatus   TINYINT DEFAULT 0,              -- 0=Pending, 1=Verified, 2=Mismatch (FUTURE WORK)
    ProfilePhotoPath        NVARCHAR(500),
    CvSummary               NVARCHAR(MAX),                  -- AI-extracted summary text
    ParsedCvJson            NVARCHAR(MAX),                  -- Full structured CV data from Claude

    -- BaseEntity
    IsActive                BIT DEFAULT 1,
    IsDeleted               BIT DEFAULT 0,
    CreatedDate             DATETIME2 DEFAULT GETUTCDATE(),
    CreatedBy               NVARCHAR(100),
    ModifiedDate            DATETIME2,
    ModifiedBy              NVARCHAR(100)
);

CREATE UNIQUE INDEX IX_Candidates_UserId ON [recruitment].[Candidates](UserId);
CREATE UNIQUE INDEX IX_Candidates_NationalId ON [recruitment].[Candidates](NationalId) WHERE NationalId IS NOT NULL;
CREATE INDEX IX_Candidates_Email ON [recruitment].[Candidates](Email);
CREATE INDEX IX_Candidates_ProfileStatus ON [recruitment].[Candidates](ProfileStatus);
CREATE INDEX IX_Candidates_Nationality ON [recruitment].[Candidates](Nationality);
```

**ProfileStatus Enum:**

```csharp
public enum CandidateProfileStatus : byte
{
    Incomplete = 0,
    Submitted = 1,           // Profile LOCKED — candidate cannot edit
    UnderReview = 2,
    Approved = 3,
    CorrectionRequired = 4,  // Profile UNLOCKED — candidate can edit and re-submit
    Rejected = 5             // Can be reconsidered → moves to UnderReview
}
```

**Profile Lock Logic (API must enforce):**

```csharp
// Before allowing candidate PUT /profile:
if (candidate.ProfileStatus == CandidateProfileStatus.Submitted ||
    candidate.ProfileStatus == CandidateProfileStatus.UnderReview ||
    candidate.ProfileStatus == CandidateProfileStatus.Approved)
{
    throw new BadRequestException("Profile is locked. Contact HR for corrections.");
}
// Only Incomplete and CorrectionRequired allow edits
```

---

### 3.2 CandidateEducations

**Purpose:** Education details (Phase 1, Section 3.3) — Multiple per candidate

```sql
CREATE TABLE [recruitment].[CandidateEducations] (
    CandidateEducationId    BIGINT IDENTITY(1,1) PRIMARY KEY,
    CandidateId             BIGINT NOT NULL REFERENCES [recruitment].[Candidates](CandidateId),
    
    Qualification           NVARCHAR(200) NOT NULL,         -- e.g., Bachelor's, Master's, PhD
    Major                   NVARCHAR(200),                  -- Specialization
    Institution             NVARCHAR(300),                  -- University name
    GraduationYear          INT,
    GradeOrGPA              NVARCHAR(50),
    Country                 NVARCHAR(100),
    
    -- Source tracking (manual entry vs AI-extracted)
    DataSource              TINYINT DEFAULT 0,              -- 0=ManualEntry, 1=CvParsed, 2=OcrExtracted
    
    IsActive BIT DEFAULT 1, IsDeleted BIT DEFAULT 0,
    CreatedDate DATETIME2 DEFAULT GETUTCDATE(), CreatedBy NVARCHAR(100),
    ModifiedDate DATETIME2, ModifiedBy NVARCHAR(100)
);

CREATE INDEX IX_CandidateEducations_CandidateId ON [recruitment].[CandidateEducations](CandidateId);
CREATE INDEX IX_CandidateEducations_Major ON [recruitment].[CandidateEducations](Major);
```

---

### 3.3 CandidateExperiences

**Purpose:** Work experience (Phase 1, Section 3.3) — Multiple per candidate

```sql
CREATE TABLE [recruitment].[CandidateExperiences] (
    CandidateExperienceId   BIGINT IDENTITY(1,1) PRIMARY KEY,
    CandidateId             BIGINT NOT NULL REFERENCES [recruitment].[Candidates](CandidateId),
    
    Employer                NVARCHAR(300),
    JobTitle                NVARCHAR(200),
    StartDate               DATE,
    EndDate                 DATE,                           -- NULL if current
    IsCurrent               BIT DEFAULT 0,
    Salary                  DECIMAL(18,2),                  -- Previous salary (optional per doc)
    Currency                NVARCHAR(10) DEFAULT 'SAR',
    Description             NVARCHAR(MAX),
    Country                 NVARCHAR(100),
    
    DataSource              TINYINT DEFAULT 0,              -- 0=ManualEntry, 1=CvParsed
    
    IsActive BIT DEFAULT 1, IsDeleted BIT DEFAULT 0,
    CreatedDate DATETIME2 DEFAULT GETUTCDATE(), CreatedBy NVARCHAR(100),
    ModifiedDate DATETIME2, ModifiedBy NVARCHAR(100)
);

CREATE INDEX IX_CandidateExperiences_CandidateId ON [recruitment].[CandidateExperiences](CandidateId);
```

---

### 3.4 CandidateDocuments

**Purpose:** Uploaded files — CV, ID, certificates (Phase 1, Section 3.4)

```sql
CREATE TABLE [recruitment].[CandidateDocuments] (
    CandidateDocumentId     BIGINT IDENTITY(1,1) PRIMARY KEY,
    CandidateId             BIGINT NOT NULL REFERENCES [recruitment].[Candidates](CandidateId),
    
    DocumentType            TINYINT NOT NULL,
    -- 0=CV, 1=NationalId, 2=Passport, 3=Degree, 4=Certificate, 5=License, 6=Other
    
    FileName                NVARCHAR(300),
    FilePath                NVARCHAR(500) NOT NULL,         -- Azure Blob Storage path
    FileSize                BIGINT,                         -- Bytes
    ContentType             NVARCHAR(100),                  -- e.g., application/pdf
    
    -- AI Processing Status
    AiProcessingStatus      TINYINT DEFAULT 0,              -- 0=Pending, 1=Processing, 2=Completed, 3=Failed
    AiProcessedDate         DATETIME2,
    AiConfidenceScore       DECIMAL(5,2),                   -- Overall confidence 0-100
    AiRawResponseJson       NVARCHAR(MAX),                  -- Full Claude API response
    AiErrorMessage          NVARCHAR(500),
    
    UploadedDate            DATETIME2 DEFAULT GETUTCDATE(),
    
    IsActive BIT DEFAULT 1, IsDeleted BIT DEFAULT 0,
    CreatedDate DATETIME2 DEFAULT GETUTCDATE(), CreatedBy NVARCHAR(100),
    ModifiedDate DATETIME2, ModifiedBy NVARCHAR(100)
);

CREATE INDEX IX_CandidateDocuments_CandidateId ON [recruitment].[CandidateDocuments](CandidateId);
CREATE INDEX IX_CandidateDocuments_DocumentType ON [recruitment].[CandidateDocuments](DocumentType);
```

**DocumentType Enum:**

```csharp
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
```

---

### 3.5 OcrVerificationResults

**Purpose:** Field-by-field comparison of AI-extracted data vs candidate-entered data (Phase 1, Section 3.5)  
> ✅ **BUILD NOW** — Claude Vision will extract fields from National ID / Passport documents and compare them against candidate-entered profile data. Mismatches are stored per-field with confidence scores and severity levels.

```sql
CREATE TABLE [recruitment].[OcrVerificationResults] (
    OcrVerificationId       BIGINT IDENTITY(1,1) PRIMARY KEY,
    CandidateDocumentId     BIGINT NOT NULL REFERENCES [recruitment].[CandidateDocuments](CandidateDocumentId),
    CandidateId             BIGINT NOT NULL REFERENCES [recruitment].[Candidates](CandidateId),
    
    FieldName               NVARCHAR(100) NOT NULL,         -- e.g., "FullName", "NationalId", "DateOfBirth"
    ExtractedValue          NVARCHAR(500),                  -- What Claude AI extracted
    EnteredValue            NVARCHAR(500),                  -- What candidate typed in profile
    IsMatch                 BIT,
    ConfidenceScore         DECIMAL(5,2),                   -- Per-field confidence 0-100
    MismatchSeverity        TINYINT DEFAULT 0,              -- 0=None, 1=Minor, 2=Major, 3=Critical
    
    ProcessedDate           DATETIME2 DEFAULT GETUTCDATE(),
    
    IsActive BIT DEFAULT 1, IsDeleted BIT DEFAULT 0,
    CreatedDate DATETIME2 DEFAULT GETUTCDATE(), CreatedBy NVARCHAR(100),
    ModifiedDate DATETIME2, ModifiedBy NVARCHAR(100)
);
```

---

### 3.6 Vacancies

**Purpose:** Job postings/requisitions (Phase 3)

> ⚠️ **Q&A Rules:**  
> - Created by **both HR and Recruiter**.  
> - **Approval required before publishing** — HR Manager AND HR Section Head must approve.  
> - **Single location/department** per vacancy (no multi-location).  
> - **Multiple recruiters** can be assigned (see VacancyRecruiters table).  
> - Can be **paused/closed/extended** by HR Manager and HR Section Head.  
> - Auto-close **only when expired** (ClosingDate), NOT when positions filled.

```sql
CREATE TABLE [recruitment].[Vacancies] (
    VacancyId               BIGINT IDENTITY(1,1) PRIMARY KEY,
    RequisitionNumber       NVARCHAR(50) NOT NULL UNIQUE,   -- Auto: REQ-2025-0001

    -- Job Details
    JobTitle                NVARCHAR(200) NOT NULL,
    DepartmentId            INT,                            -- Can reference UCIC Department table
    DepartmentName          NVARCHAR(100),                  -- Denormalized for independence
    Location                NVARCHAR(200),
    LocationLatitude        DECIMAL(9,6),                   -- For proximity matching
    LocationLongitude       DECIMAL(9,6),
    JobGrade                NVARCHAR(50),
    NumberOfPositions       INT DEFAULT 1,
    FilledPositions         INT DEFAULT 0,

    -- Description
    JobDescription          NVARCHAR(MAX),
    Qualifications          NVARCHAR(MAX),
    Requirements            NVARCHAR(MAX),
    RequiredDocuments       NVARCHAR(MAX),                  -- JSON: ["CV","NationalId","Degree"]

    -- Compensation
    SalaryRangeMin          DECIMAL(18,2),
    SalaryRangeMax          DECIMAL(18,2),
    Currency                NVARCHAR(10) DEFAULT 'SAR',

    -- Work Type
    WorkType                TINYINT DEFAULT 0,              -- 0=FullTime, 1=PartTime, 2=Contract
    WorkLocation            TINYINT DEFAULT 0,              -- 0=OnSite, 1=Remote, 2=Hybrid

    -- Matching Criteria (used by AI Auto-Matching engine — configurable by admin)
    RequiredExperienceMin   INT,                            -- Minimum years
    RequiredExperienceMax   INT,                            -- Maximum years (optional)
    RequiredNationality     NVARCHAR(100),                  -- NULL = any nationality
    RequiredSpecialization  NVARCHAR(200),
    RequiredQualification   NVARCHAR(200),                  -- e.g., "Bachelor's", "Master's"
    RequiredCertifications  NVARCHAR(MAX),                  -- JSON: ["PMP","AWS"]
    RequiredSkills          NVARCHAR(MAX),                  -- JSON: ["C#",".NET","SQL"]

    -- Matching Weights (configurable per vacancy by system admin)
    WeightSpecialization    INT DEFAULT 25,
    WeightExperience        INT DEFAULT 20,
    WeightQualification     INT DEFAULT 20,
    WeightNationality       INT DEFAULT 10,
    WeightLocation          INT DEFAULT 15,
    WeightCertifications    INT DEFAULT 10,
    MatchThreshold          INT DEFAULT 60,                 -- Score >= this = "Match"

    -- Publishing (approval required before publishing)
    PublishStatus           TINYINT DEFAULT 0,
    -- 0=Draft, 1=PendingApproval, 2=Published, 3=Paused, 4=Closed, 5=Cancelled
    PublishedDate           DATETIME2,
    ClosingDate             DATETIME2,                      -- Auto-close when expired (NOT when filled)
    PausedDate              DATETIME2,
    PausedByUserId          NVARCHAR(450),

    -- Audit
    CreatedByUserId         NVARCHAR(450),

    IsActive BIT DEFAULT 1, IsDeleted BIT DEFAULT 0,
    CreatedDate DATETIME2 DEFAULT GETUTCDATE(), CreatedBy NVARCHAR(100),
    ModifiedDate DATETIME2, ModifiedBy NVARCHAR(100)
);

CREATE INDEX IX_Vacancies_PublishStatus ON [recruitment].[Vacancies](PublishStatus);
CREATE INDEX IX_Vacancies_DepartmentName ON [recruitment].[Vacancies](DepartmentName);
CREATE INDEX IX_Vacancies_ClosingDate ON [recruitment].[Vacancies](ClosingDate);
```

**PublishStatus Enum (UPDATED):**

```csharp
public enum VacancyPublishStatus : byte
{
    Draft = 0,
    PendingApproval = 1,     // Submitted for HR Manager + Section Head approval
    Published = 2,           // Approved and visible to candidates
    Paused = 3,              // Temporarily hidden (by HR Manager / Section Head)
    Closed = 4,              // Expired or manually closed
    Cancelled = 5
}
```

### 3.6a VacancyApprovals (NEW — Per Q&A)

**Purpose:** Vacancy must be approved by HR Manager AND HR Section Head before publishing

```sql
CREATE TABLE [recruitment].[VacancyApprovals] (
    VacancyApprovalId       BIGINT IDENTITY(1,1) PRIMARY KEY,
    VacancyId               BIGINT NOT NULL REFERENCES [recruitment].[Vacancies](VacancyId),

    ApprovalStep            TINYINT NOT NULL,
    -- 1=HRManager, 2=HRSectionHead

    ApprovalStepName        NVARCHAR(100),
    ApproverUserId          NVARCHAR(450),
    ApproverName            NVARCHAR(200),

    Status                  TINYINT DEFAULT 0,
    -- 0=Pending, 1=Approved, 2=Rejected, 3=ReturnedForRevision

    Comments                NVARCHAR(MAX),
    ActionDate              DATETIME2,

    IsActive BIT DEFAULT 1, IsDeleted BIT DEFAULT 0,
    CreatedDate DATETIME2 DEFAULT GETUTCDATE(), CreatedBy NVARCHAR(100),
    ModifiedDate DATETIME2, ModifiedBy NVARCHAR(100)
);

CREATE INDEX IX_VacancyApprovals_VacancyId ON [recruitment].[VacancyApprovals](VacancyId);
```

### 3.6b VacancyRecruiters (NEW — Per Q&A)

**Purpose:** Multiple recruiters can be assigned to a single vacancy

```sql
CREATE TABLE [recruitment].[VacancyRecruiters] (
    VacancyRecruiterId      BIGINT IDENTITY(1,1) PRIMARY KEY,
    VacancyId               BIGINT NOT NULL REFERENCES [recruitment].[Vacancies](VacancyId),
    RecruiterUserId         NVARCHAR(450) NOT NULL,         -- UCIC AspNetUsers.Id
    RecruiterName           NVARCHAR(200),
    IsPrimary               BIT DEFAULT 0,                  -- One primary recruiter per vacancy
    AssignedDate            DATETIME2 DEFAULT GETUTCDATE(),

    IsActive BIT DEFAULT 1, IsDeleted BIT DEFAULT 0,
    CreatedDate DATETIME2 DEFAULT GETUTCDATE(), CreatedBy NVARCHAR(100),
    ModifiedDate DATETIME2, ModifiedBy NVARCHAR(100),

    CONSTRAINT UQ_VacancyRecruiter UNIQUE (VacancyId, RecruiterUserId)
);

CREATE INDEX IX_VacancyRecruiters_VacancyId ON [recruitment].[VacancyRecruiters](VacancyId);
CREATE INDEX IX_VacancyRecruiters_RecruiterUserId ON [recruitment].[VacancyRecruiters](RecruiterUserId);
```

**RequisitionNumber auto-generation pattern:**

```csharp
// Format: REQ-{YEAR}-{SEQUENCE:0000}
// Example: REQ-2025-0001, REQ-2025-0042
var year = DateTime.UtcNow.Year;
var count = await _context.Vacancies.CountAsync(v => v.CreatedDate.Year == year) + 1;
var requisitionNumber = $"REQ-{year}-{count:D4}";
```

---

### 3.7 Applications

**Purpose:** Links candidates to vacancies — central tracking entity (Phase 4-13)

> ⚠️ **Q&A Rules:**  
> - Candidates **cannot reapply** for the same vacancy after rejection (unique constraint stays).  
> - Recruiters **can override auto-rejection** (NotMatched → Screening).  
> - Auto-rejection reasons **visible to candidates**.  
> - HR rejection is **NOT final** — subject to further review.

```sql
CREATE TABLE [recruitment].[Applications] (
    ApplicationId           BIGINT IDENTITY(1,1) PRIMARY KEY,
    CandidateId             BIGINT NOT NULL REFERENCES [recruitment].[Candidates](CandidateId),
    VacancyId               BIGINT NOT NULL REFERENCES [recruitment].[Vacancies](VacancyId),
    
    ApplicationDate         DATETIME2 DEFAULT GETUTCDATE(),
    
    -- Master Status (tracks entire lifecycle)
    Status                  TINYINT DEFAULT 0,
    -- 0  = Applied
    -- 1  = Matched (AI auto-match passed)
    -- 2  = NotMatched (AI auto-match failed — auto-rejected)
    -- 3  = Screening (recruiter pre-screening)
    -- 4  = Verified (screening completed)
    -- 5  = SharedWithDepartment
    -- 6  = Shortlisted (hiring manager selected)
    -- 7  = InterviewScheduled
    -- 8  = InterviewCompleted
    -- 9  = HRInterviewScheduled
    -- 10 = HRInterviewCompleted
    -- 11 = HRApproved
    -- 12 = HRRejected
    -- 13 = MedicalPending
    -- 14 = MedicalFit
    -- 15 = MedicalUnfit
    -- 16 = MedicalConditional
    -- 17 = OfferDraft
    -- 18 = OfferPendingApproval
    -- 19 = OfferApproved
    -- 20 = OfferSent
    -- 21 = OfferAccepted
    -- 22 = OfferRejected
    -- 23 = Onboarding
    -- 24 = Hired
    -- 25 = Rejected (general)
    -- 26 = Withdrawn (by candidate)
    
    RejectionReason         NVARCHAR(500),
    RejectionPhase          NVARCHAR(100),                  -- Which phase rejected in
    
    -- Assigned Recruiter
    AssignedRecruiterId     NVARCHAR(450),                  -- UCIC UserId
    AssignedRecruiterName   NVARCHAR(200),
    
    -- Match Score (populated by AI)
    MatchScore              DECIMAL(5,2),                   -- 0-100
    IsAutoMatched           BIT DEFAULT 0,
    
    IsActive BIT DEFAULT 1, IsDeleted BIT DEFAULT 0,
    CreatedDate DATETIME2 DEFAULT GETUTCDATE(), CreatedBy NVARCHAR(100),
    ModifiedDate DATETIME2, ModifiedBy NVARCHAR(100),
    
    -- Prevent duplicate applications
    CONSTRAINT UQ_Application_Candidate_Vacancy 
        UNIQUE (CandidateId, VacancyId)
);

CREATE INDEX IX_Applications_CandidateId ON [recruitment].[Applications](CandidateId);
CREATE INDEX IX_Applications_VacancyId ON [recruitment].[Applications](VacancyId);
CREATE INDEX IX_Applications_Status ON [recruitment].[Applications](Status);
CREATE INDEX IX_Applications_AssignedRecruiterId ON [recruitment].[Applications](AssignedRecruiterId);
```

**ApplicationStatus Enum:**

```csharp
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
    HRRejected = 12,        // NOT final — subject to further review
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
```

---

### 3.8 MatchResults

**Purpose:** AI auto-matching score breakdown (Phase 4)

```sql
CREATE TABLE [recruitment].[MatchResults] (
    MatchResultId           BIGINT IDENTITY(1,1) PRIMARY KEY,
    ApplicationId           BIGINT NOT NULL REFERENCES [recruitment].[Applications](ApplicationId),
    CandidateId             BIGINT NOT NULL,
    VacancyId               BIGINT NOT NULL,
    
    -- Individual Scores (each 0-100, weighted to final)
    SpecializationScore     DECIMAL(5,2),
    ExperienceScore         DECIMAL(5,2),
    QualificationScore      DECIMAL(5,2),
    NationalityScore        DECIMAL(5,2),
    LocationScore           DECIMAL(5,2),
    CertificationScore      DECIMAL(5,2),
    
    -- Weighted Final Score
    OverallScore            DECIMAL(5,2) NOT NULL,          -- 0-100
    IsMatch                 BIT NOT NULL,                   -- TRUE if >= vacancy threshold
    
    -- AI Reasoning
    MatchExplanation        NVARCHAR(MAX),                  -- Claude AI explanation of scoring
    MatchDetailsJson        NVARCHAR(MAX),                  -- Full JSON breakdown
    
    -- Processing Info
    ProcessedDate           DATETIME2 DEFAULT GETUTCDATE(),
    ProcessingTimeMs        INT,                            -- How long AI took
    AiModel                 NVARCHAR(50),                   -- e.g., "claude-sonnet-4-20250514"
    
    IsActive BIT DEFAULT 1, IsDeleted BIT DEFAULT 0,
    CreatedDate DATETIME2 DEFAULT GETUTCDATE(), CreatedBy NVARCHAR(100),
    ModifiedDate DATETIME2, ModifiedBy NVARCHAR(100)
);

CREATE INDEX IX_MatchResults_ApplicationId ON [recruitment].[MatchResults](ApplicationId);
CREATE INDEX IX_MatchResults_OverallScore ON [recruitment].[MatchResults](OverallScore DESC);
```

---

### 3.9 ScreeningTasks

**Purpose:** Pre-screening by recruiter (Phase 5)

> ⚠️ **Q&A Rules:**  
> - **Multiple recruiters** can be assigned screening tasks for the same candidate.  
> - Task deadlines are **optional** (nullable).  
> - Only **Recruiters** can mark a candidate as "Verified".

```sql
CREATE TABLE [recruitment].[ScreeningTasks] (
    ScreeningTaskId         BIGINT IDENTITY(1,1) PRIMARY KEY,
    ApplicationId           BIGINT NOT NULL REFERENCES [recruitment].[Applications](ApplicationId),
    
    AssignedToUserId        NVARCHAR(450),                  -- Recruiter
    AssignedToName          NVARCHAR(200),
    TaskDescription         NVARCHAR(MAX),
    Deadline                DATETIME2,
    
    -- Candidate Contact Data (Phase 5, Section 7.2)
    CandidateAvailability   NVARCHAR(200),
    ExpectedSalary          DECIMAL(18,2),
    SalaryCurrency          NVARCHAR(10) DEFAULT 'SAR',
    PreferredLocation       NVARCHAR(200),
    WillingnessToRelocate   BIT,
    NoticePeriodDays        INT,
    
    Status                  TINYINT DEFAULT 0,              -- 0=Pending, 1=InProgress, 2=Completed
    Notes                   NVARCHAR(MAX),
    CompletedDate           DATETIME2,
    
    IsActive BIT DEFAULT 1, IsDeleted BIT DEFAULT 0,
    CreatedDate DATETIME2 DEFAULT GETUTCDATE(), CreatedBy NVARCHAR(100),
    ModifiedDate DATETIME2, ModifiedBy NVARCHAR(100)
);
```

---

### 3.10 Interviews

**Purpose:** Interview scheduling and tracking (Phase 6-7)

> ⚠️ **Q&A Rules:**  
> - Interviews can be **both onsite and online**.  
> - **Both HR and Recruiter** have authority to shortlist.  
> - Hiring Manager = department manager/lead.  
> - Factory security gets a **report only** — no system access needed.  
> - HR interview is **mandatory for all job roles**.

```sql
CREATE TABLE [recruitment].[Interviews] (
    InterviewId             BIGINT IDENTITY(1,1) PRIMARY KEY,
    ApplicationId           BIGINT NOT NULL REFERENCES [recruitment].[Applications](ApplicationId),
    
    InterviewType           TINYINT NOT NULL,
    -- 0=Technical, 1=Managerial, 2=HR, 3=Final
    
    InterviewMode           TINYINT DEFAULT 0,              -- 0=OnSite, 1=Online
    ScheduledDate           DATETIME2 NOT NULL,
    ScheduledEndDate        DATETIME2,
    Location                NVARCHAR(300),                  -- Physical or meeting URL
    
    -- Interviewer
    InterviewerUserId       NVARCHAR(450),
    InterviewerName         NVARCHAR(200),
    
    -- Status
    Status                  TINYINT DEFAULT 0,
    -- 0=Scheduled, 1=Confirmed, 2=Rescheduled, 3=Completed, 4=Cancelled, 5=NoShow
    
    CandidateConfirmed      BIT DEFAULT 0,
    ConfirmedDate           DATETIME2,
    RescheduleReason        NVARCHAR(500),
    RescheduleCount         INT DEFAULT 0,
    
    -- Factory Security (Phase 7, Section 9.4)
    SecurityListGenerated   BIT DEFAULT 0,
    SecurityNotes           NVARCHAR(500),
    
    IsActive BIT DEFAULT 1, IsDeleted BIT DEFAULT 0,
    CreatedDate DATETIME2 DEFAULT GETUTCDATE(), CreatedBy NVARCHAR(100),
    ModifiedDate DATETIME2, ModifiedBy NVARCHAR(100)
);

CREATE INDEX IX_Interviews_ApplicationId ON [recruitment].[Interviews](ApplicationId);
CREATE INDEX IX_Interviews_ScheduledDate ON [recruitment].[Interviews](ScheduledDate);
CREATE INDEX IX_Interviews_InterviewerUserId ON [recruitment].[Interviews](InterviewerUserId);
```

---

### 3.11 InterviewEvaluations

**Purpose:** Evaluation forms — Technical, Managerial, HR (Phase 8-9)

> ⚠️ **Q&A Rules:**  
> - HR **can override** previous interview decisions (change Accepted ↔ Rejected).  
> - HR rejection is **subject to further review** (not terminal).

```sql
CREATE TABLE [recruitment].[InterviewEvaluations] (
    InterviewEvaluationId   BIGINT IDENTITY(1,1) PRIMARY KEY,
    InterviewId             BIGINT NOT NULL REFERENCES [recruitment].[Interviews](InterviewId),
    ApplicationId           BIGINT NOT NULL,
    
    EvaluationType          TINYINT NOT NULL,               -- 0=Technical, 1=Managerial, 2=HR
    EvaluatorUserId         NVARCHAR(450),
    EvaluatorName           NVARCHAR(200),
    
    -- Scoring (each 0-10)
    TechnicalScore          DECIMAL(4,2),
    CommunicationScore      DECIMAL(4,2),
    ProblemSolvingScore     DECIMAL(4,2),
    LeadershipScore         DECIMAL(4,2),
    CulturalFitScore        DECIMAL(4,2),
    OverallScore            DECIMAL(4,2),
    
    -- Assessment
    Strengths               NVARCHAR(MAX),
    Weaknesses              NVARCHAR(MAX),
    Notes                   NVARCHAR(MAX),
    
    Recommendation          TINYINT DEFAULT 2,
    -- 0=StrongReject, 1=Reject, 2=Neutral, 3=Recommend, 4=StronglyRecommend
    
    Decision                TINYINT DEFAULT 0,              -- 0=Pending, 1=Accepted, 2=Rejected
    RejectionReason         NVARCHAR(500),
    EvaluationDate          DATETIME2 DEFAULT GETUTCDATE(),
    
    IsActive BIT DEFAULT 1, IsDeleted BIT DEFAULT 0,
    CreatedDate DATETIME2 DEFAULT GETUTCDATE(), CreatedBy NVARCHAR(100),
    ModifiedDate DATETIME2, ModifiedBy NVARCHAR(100)
);
```

---

### 3.12 MedicalExaminations

**Purpose:** Medical exam requests and results (Phase 10)

> ⚠️ **Q&A Rules:**  
> - Medical providers get **both portal access and upload** capability.  
> - HR **can override** Conditional or Unfit results (e.g., Conditional → Fit) with audit log.

```sql
CREATE TABLE [recruitment].[MedicalExaminations] (
    MedicalExaminationId    BIGINT IDENTITY(1,1) PRIMARY KEY,
    ApplicationId           BIGINT NOT NULL REFERENCES [recruitment].[Applications](ApplicationId),
    CandidateId             BIGINT NOT NULL,
    
    HospitalName            NVARCHAR(300),
    ReferenceNumber         NVARCHAR(100),
    ContactDetails          NVARCHAR(300),
    
    RequestDate             DATETIME2 DEFAULT GETUTCDATE(),
    ExaminationDate         DATETIME2,
    
    -- Report
    ReportPath              NVARCHAR(500),                  -- Azure Blob path
    ReportUploadedByUserId  NVARCHAR(450),                  -- Medical provider userId
    ReportUploadedDate      DATETIME2,
    
    ClearanceStatus         TINYINT DEFAULT 0,
    -- 0=Pending, 1=Fit, 2=Unfit, 3=Conditional
    
    ConditionNotes          NVARCHAR(MAX),
    ReviewedByUserId        NVARCHAR(450),
    ReviewDate              DATETIME2,
    
    IsActive BIT DEFAULT 1, IsDeleted BIT DEFAULT 0,
    CreatedDate DATETIME2 DEFAULT GETUTCDATE(), CreatedBy NVARCHAR(100),
    ModifiedDate DATETIME2, ModifiedBy NVARCHAR(100)
);
```

---

### 3.13 JobOffers

**Purpose:** Offer details and tracking (Phase 11-12)

> ⚠️ **Q&A Rules:**  
> - Offer templates **vary by role AND job grade** (both).  
> - E-signature via **external provider** (e.g., DocuSign).  
> - If candidate doesn't respond: **send reminder via Email AND WhatsApp** (auto at day 3, 7, 10).  
> - Offers **auto-expire after 12 days**.

```sql
CREATE TABLE [recruitment].[JobOffers] (
    JobOfferId              BIGINT IDENTITY(1,1) PRIMARY KEY,
    ApplicationId           BIGINT NOT NULL REFERENCES [recruitment].[Applications](ApplicationId),
    CandidateId             BIGINT NOT NULL,
    VacancyId               BIGINT NOT NULL,

    -- Compensation
    ProposedSalary          DECIMAL(18,2),
    Currency                NVARCHAR(10) DEFAULT 'SAR',
    Benefits                NVARCHAR(MAX),                  -- JSON: housing, transport, etc.

    -- Template (varies by role + job grade per Q&A)
    OfferTemplateType       NVARCHAR(50),                   -- e.g., "Engineering", "Management", "General"
    JobGrade                NVARCHAR(50),                   -- Grade determines template variant

    -- Offer Document
    OfferLetterPath         NVARCHAR(500),                  -- Generated PDF

    -- E-Signature (external provider e.g., DocuSign)
    ESignatureProvider      NVARCHAR(50) DEFAULT 'DocuSign',
    ESignatureRequestId     NVARCHAR(200),                  -- External provider reference ID
    SignedOfferPath         NVARCHAR(500),                  -- Signed document URL from provider
    SignedDate              DATETIME2,

    -- Status
    Status                  TINYINT DEFAULT 0,
    -- 0=Draft, 1=PendingApproval, 2=Approved, 3=Sent, 
    -- 4=Accepted, 5=Rejected, 6=Expired, 7=Revoked

    CurrentApprovalStep     TINYINT DEFAULT 1,              -- Which step in approval chain

    -- Dates
    SentDate                DATETIME2,
    SentVia                 TINYINT,                        -- 0=Email, 1=WhatsApp, 2=Both
    ExpectedJoiningDate     DATE,
    OfferExpiryDate         DATE,                           -- Auto-set: SentDate + 12 days
    OfferExpiryDays         INT DEFAULT 12,                 -- Configurable, default 12 per Q&A

    -- Reminder tracking (auto-reminders at day 3, 7, 10)
    LastReminderSentDate    DATETIME2,
    ReminderCount           INT DEFAULT 0,

    IsActive BIT DEFAULT 1, IsDeleted BIT DEFAULT 0,
    CreatedDate DATETIME2 DEFAULT GETUTCDATE(), CreatedBy NVARCHAR(100),
    ModifiedDate DATETIME2, ModifiedBy NVARCHAR(100)
);
```

**Hangfire Jobs for Offer Reminders & Expiry:**

```csharp
// OfferReminderJob — runs daily
// 1. Find offers where Status = Sent AND not responded
// 2. If daysSinceSent == 3 or 7 or 10 → send Email + WhatsApp reminder
// 3. If daysSinceSent >= OfferExpiryDays (12) → Status = Expired
```

---

### 3.14 OfferApprovals

**Purpose:** Sequential approval chain — Recruitment → HR → VPO → CEO (Phase 11)

```sql
CREATE TABLE [recruitment].[OfferApprovals] (
    OfferApprovalId         BIGINT IDENTITY(1,1) PRIMARY KEY,
    JobOfferId              BIGINT NOT NULL REFERENCES [recruitment].[JobOffers](JobOfferId),
    
    ApprovalStep            TINYINT NOT NULL,
    -- 1=Recruitment, 2=HRSupervisor, 3=VPO, 4=CEO
    
    ApprovalStepName        NVARCHAR(100),
    ApproverUserId          NVARCHAR(450),
    ApproverName            NVARCHAR(200),
    
    Status                  TINYINT DEFAULT 0,
    -- 0=Pending, 1=Approved, 2=Rejected, 3=ReturnedForRevision
    
    Comments                NVARCHAR(MAX),
    ActionDate              DATETIME2,
    
    IsActive BIT DEFAULT 1, IsDeleted BIT DEFAULT 0,
    CreatedDate DATETIME2 DEFAULT GETUTCDATE(), CreatedBy NVARCHAR(100),
    ModifiedDate DATETIME2, ModifiedBy NVARCHAR(100)
);

CREATE INDEX IX_OfferApprovals_JobOfferId ON [recruitment].[OfferApprovals](JobOfferId);
```

---

### 3.15 Onboardings

**Purpose:** Post-offer onboarding tracking (Phase 13)

> ⚠️ **Q&A Rules:**  
> - Onboarding checklist is **same for all employees** (no role/department variation).  
> - Tasks are **auto-assigned to departments** when onboarding starts.  
> - **HCM system integration required** (future API integration point).  
> - Employee deactivation **handled via HCM** — outside RMS scope.

```sql
CREATE TABLE [recruitment].[Onboardings] (
    OnboardingId            BIGINT IDENTITY(1,1) PRIMARY KEY,
    ApplicationId           BIGINT NOT NULL REFERENCES [recruitment].[Applications](ApplicationId),
    CandidateId             BIGINT NOT NULL,
    JobOfferId              BIGINT NOT NULL REFERENCES [recruitment].[JobOffers](JobOfferId),

    -- Employee Setup
    EmployeeId              NVARCHAR(50),                   -- Auto-generated: EMP-2025-0001
    ContractPath            NVARCHAR(500),

    -- Insurance
    InsuranceEnrolled       BIT DEFAULT 0,
    InsuranceDetails        NVARCHAR(MAX),
    InsuranceEnrolledDate   DATETIME2,

    -- Orientation
    OrientationDate         DATETIME2,
    OrientationCompleted    BIT DEFAULT 0,

    -- Activation
    ActivationDate          DATETIME2,
    Status                  TINYINT DEFAULT 0,
    -- 0=Pending, 1=InProgress, 2=Completed, 3=Cancelled

    EmployeeStatus          TINYINT DEFAULT 0,              -- 0=Pending, 1=Active
    -- Note: Deactivation handled via HCM system, not RMS

    -- HCM Integration
    HcmSyncStatus           TINYINT DEFAULT 0,              -- 0=Pending, 1=Synced, 2=Failed
    HcmEmployeeId           NVARCHAR(100),                  -- External HCM system employee ID
    HcmSyncDate             DATETIME2,

    IsActive BIT DEFAULT 1, IsDeleted BIT DEFAULT 0,
    CreatedDate DATETIME2 DEFAULT GETUTCDATE(), CreatedBy NVARCHAR(100),
    ModifiedDate DATETIME2, ModifiedBy NVARCHAR(100)
);
```

### 3.15a OnboardingTasks (NEW — Per Q&A)

**Purpose:** Standard checklist items auto-assigned to departments when onboarding starts

> Same checklist for all employees. Tasks auto-assigned to relevant departments.

```sql
CREATE TABLE [recruitment].[OnboardingTasks] (
    OnboardingTaskId        BIGINT IDENTITY(1,1) PRIMARY KEY,
    OnboardingId            BIGINT NOT NULL REFERENCES [recruitment].[Onboardings](OnboardingId),

    TaskName                NVARCHAR(200) NOT NULL,
    TaskDescription         NVARCHAR(MAX),
    AssignedDepartment      NVARCHAR(100),                  -- e.g., "HR", "IT", "Administration", "Insurance"
    AssignedToUserId        NVARCHAR(450),
    AssignedToName          NVARCHAR(200),

    SortOrder               INT DEFAULT 0,
    IsMandatory             BIT DEFAULT 1,

    Status                  TINYINT DEFAULT 0,              -- 0=Pending, 1=InProgress, 2=Completed, 3=Skipped
    CompletedDate           DATETIME2,
    CompletedByUserId       NVARCHAR(450),
    Notes                   NVARCHAR(MAX),

    IsActive BIT DEFAULT 1, IsDeleted BIT DEFAULT 0,
    CreatedDate DATETIME2 DEFAULT GETUTCDATE(), CreatedBy NVARCHAR(100),
    ModifiedDate DATETIME2, ModifiedBy NVARCHAR(100)
);

CREATE INDEX IX_OnboardingTasks_OnboardingId ON [recruitment].[OnboardingTasks](OnboardingId);
```

**Standard Checklist (seeded — same for all employees):**

```csharp
// Auto-created when onboarding starts
new[] {
    new { Task = "Generate Employee ID",        Dept = "HR" },
    new { Task = "Create Employment Contract",  Dept = "HR" },
    new { Task = "Insurance Enrollment",        Dept = "Insurance" },
    new { Task = "IT Account Setup",            Dept = "IT" },
    new { Task = "Access Card / Badge",         Dept = "Administration" },
    new { Task = "Orientation Scheduling",      Dept = "HR" },
    new { Task = "Workspace Setup",             Dept = "Administration" },
    new { Task = "HCM System Registration",     Dept = "HR" },
};
```

---

### 3.16 StatusHistories

**Purpose:** Full audit trail for all status changes (Non-functional Requirement)

```sql
CREATE TABLE [recruitment].[StatusHistories] (
    StatusHistoryId         BIGINT IDENTITY(1,1) PRIMARY KEY,
    
    EntityType              NVARCHAR(50) NOT NULL,          -- "Application", "Candidate", "Interview", etc.
    EntityId                BIGINT NOT NULL,
    
    OldStatus               TINYINT,
    NewStatus               TINYINT NOT NULL,
    OldStatusName           NVARCHAR(50),
    NewStatusName           NVARCHAR(50),
    
    ChangedByUserId         NVARCHAR(450),
    ChangedByName           NVARCHAR(200),
    Reason                  NVARCHAR(500),
    AdditionalData          NVARCHAR(MAX),                  -- JSON context
    
    ChangedDate             DATETIME2 DEFAULT GETUTCDATE()
);

CREATE INDEX IX_StatusHistories_Entity ON [recruitment].[StatusHistories](EntityType, EntityId);
CREATE INDEX IX_StatusHistories_ChangedDate ON [recruitment].[StatusHistories](ChangedDate DESC);
```

---

### 3.17 Notifications

**Purpose:** Email, WhatsApp, in-app notifications (Cross-cutting)

```sql
CREATE TABLE [recruitment].[Notifications] (
    NotificationId          BIGINT IDENTITY(1,1) PRIMARY KEY,
    
    RecipientUserId         NVARCHAR(450),
    RecipientEmail          NVARCHAR(200),
    RecipientPhone          NVARCHAR(50),
    
    Channel                 TINYINT NOT NULL,
    -- 0=Email, 1=WhatsApp, 2=InApp, 3=SMS
    
    Subject                 NVARCHAR(300),
    Body                    NVARCHAR(MAX),
    TemplateCode            NVARCHAR(50),                   -- e.g., "INTERVIEW_INVITE", "OFFER_SENT"
    
    -- Related Entity
    EntityType              NVARCHAR(50),
    EntityId                BIGINT,
    
    Status                  TINYINT DEFAULT 0,
    -- 0=Pending, 1=Sent, 2=Failed, 3=Read
    
    SentDate                DATETIME2,
    ReadDate                DATETIME2,
    ErrorMessage            NVARCHAR(500),
    RetryCount              INT DEFAULT 0,
    
    CreatedDate             DATETIME2 DEFAULT GETUTCDATE()
);

CREATE INDEX IX_Notifications_RecipientUserId ON [recruitment].[Notifications](RecipientUserId);
CREATE INDEX IX_Notifications_Status ON [recruitment].[Notifications](Status);
```

---

### 3.18 AuditLogs

**Purpose:** Complete audit trail for compliance (Non-functional Requirement)

> ⚠️ **Q&A Critical:** Recruiters CAN edit candidate info, but **every modification must be logged** with the user who modified it. This table is essential for that requirement.

```sql
CREATE TABLE [recruitment].[AuditLogs] (
    AuditLogId              BIGINT IDENTITY(1,1) PRIMARY KEY,
    
    UserId                  NVARCHAR(450),
    UserName                NVARCHAR(200),
    UserRole                NVARCHAR(50),
    
    Action                  NVARCHAR(100) NOT NULL,         -- "Create", "Update", "Delete", "StatusChange", "DocumentUpload", "Approval"
    EntityType              NVARCHAR(50),
    EntityId                BIGINT,
    
    OldValues               NVARCHAR(MAX),                  -- JSON
    NewValues               NVARCHAR(MAX),                  -- JSON
    
    IpAddress               NVARCHAR(50),
    UserAgent               NVARCHAR(500),
    
    Timestamp               DATETIME2 DEFAULT GETUTCDATE()
);

CREATE INDEX IX_AuditLogs_EntityType ON [recruitment].[AuditLogs](EntityType, EntityId);
CREATE INDEX IX_AuditLogs_UserId ON [recruitment].[AuditLogs](UserId);
CREATE INDEX IX_AuditLogs_Timestamp ON [recruitment].[AuditLogs](Timestamp DESC);
```

---

### Entity Relationship Diagram (Updated — 21 Tables)

```
                    ┌──────────────────┐
                    │    Candidates    │
                    │   (Phase 1)      │
                    └────────┬─────────┘
                             │
            ┌────────────────┼────────────────┬────────────────┐
            │                │                │                │
            ▼                ▼                ▼                ▼
   ┌─────────────┐  ┌──────────────┐  ┌──────────────┐  ┌──────────────────┐
   │ Candidate   │  │  Candidate   │  │  Candidate   │  │ OcrVerification  │
   │ Educations  │  │  Experiences │  │  Documents   │──│ Results          │
   │  (1:N)           │
   └─────────────┘  └──────────────┘  └──────────────┘  └──────────────────┘

                    ┌──────────────────┐
                    │    Vacancies     │
                    │   (Phase 3)      │
                    └────────┬─────────┘
                             │
              ┌──────────────┼──────────────┐
              │              │              │
              ▼              ▼              ▼
   ┌─────────────────┐ ┌────────────┐ ┌──────────────────┐
   │ Vacancy         │ │ Vacancy    │ │  Applications    │
   │ Approvals (NEW) │ │ Recruiters │ │  (Phase 4-13)    │
   │ (1:N)           │ │ (NEW)(1:N) │ └────────┬─────────┘
   └─────────────────┘ └────────────┘          │
                                               │
        ┌────────────┬───────┼───────┬──────────────┬───────────────┐
        │            │       │       │              │               │
        ▼            ▼       ▼       ▼              ▼               ▼
  ┌───────────┐ ┌────────┐ ┌────────────┐ ┌──────────────┐ ┌─────────────┐
  │  Match    │ │Screen- │ │ Interviews │ │   Medical    │ │ JobOffers   │
  │  Results  │ │ing     │ │  (1:N)     │ │   Exams      │ │  (1:1)      │
  │  (1:1)    │ │Tasks   │ └─────┬──────┘ │   (1:1)      │ └──────┬──────┘
  └───────────┘ │(1:N)   │       │        └──────────────┘        │
                └────────┘       ▼                          ┌─────┴──────┐
                          ┌──────────────┐                  │            │
                          │  Interview   │           ┌──────────┐ ┌──────────┐
                          │  Evaluations │           │  Offer   │ │Onboard-  │
                          │   (1:N)      │           │  Approv- │ │ings      │
                          └──────────────┘           │  als(1:N)│ │ (1:1)    │
                                                     └──────────┘ └────┬─────┘
                                                                       │
                                                                ┌──────────────┐
                                                                │ Onboarding   │
                                                                │ Tasks (NEW)  │
                                                                │  (1:N)       │
                                                                └──────────────┘

  Cross-Cutting (polymorphic — EntityType + EntityId):
  ┌──────────────────┐  ┌──────────────────┐  ┌──────────────────┐
  │  StatusHistories  │  │  Notifications    │  │  AuditLogs       │
  └──────────────────┘  └──────────────────┘  └──────────────────┘
```

### Summary: All 21 Tables

| # | Table | Phase | New? |
|---|-------|-------|------|
| 1 | Candidates | 1 | Updated |
| 2 | CandidateEducations | 1 | |
| 3 | CandidateExperiences | 1 | |
| 4 | CandidateDocuments | 1 | |
| 5 | OcrVerificationResults | 1 | |
| 6 | Vacancies | 3 | Updated |
| 7 | **VacancyApprovals** | 3 | **🆕 NEW** |
| 8 | **VacancyRecruiters** | 3 | **🆕 NEW** |
| 9 | Applications | 4-13 | Updated |
| 10 | MatchResults | 4 | |
| 11 | ScreeningTasks | 5 | |
| 12 | Interviews | 6-7 | |
| 13 | InterviewEvaluations | 8-9 | |
| 14 | MedicalExaminations | 10 | |
| 15 | JobOffers | 11-12 | Updated |
| 16 | OfferApprovals | 11 | |
| 17 | Onboardings | 13 | Updated |
| 18 | **OnboardingTasks** | 13 | **🆕 NEW** |
| 19 | StatusHistories | All | |
| 20 | Notifications | All | |
| 21 | AuditLogs | All | |

---

## 4. AI Integration Plan

### Overview: Where AI Touches the System

| Phase | AI Feature | Trigger | Priority | Status |
|-------|-----------|---------|----------|--------|
| **Phase 1** | CV Parsing & Autofill | Candidate uploads CV | **P0** | ✅ Build Now |
| **Phase 1** | ID/Document OCR | Candidate uploads National ID / Passport | **P0** | ✅ Build Now |
| **Phase 1** | Data Verification (OCR vs Profile) | After OCR | **P1** | ✅ Build Now |
| **Phase 4** | Auto-Matching | Candidate applies to vacancy | **P0** | ✅ Build Now |
| **Phase 4** | Candidate Ranking | Recruiter views applicants | **P1** | ✅ Build Now |
| **Phase 8** | Interview Summary | After evaluation forms submitted | **P2** | ✅ Build Now |
| **Phase 11** | Offer Letter Generation | After approval workflow | **P1** | ✅ Build Now |

> ✅ **ALL AI features are in scope and will be built.** CV Parsing, ID Document OCR (Claude Vision),
> Data Verification, Auto-Matching, Candidate Ranking, Interview Summary, and Offer Letter Generation
> are all part of the current implementation. No AI features are deferred.

### AI Processing Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│              RECRUITMENT API — AI PROCESSING                     │
│                                                                  │
│  ┌───────────────────────────────────────────────────────────┐  │
│  │                   APPLICATION LAYER                        │  │
│  │                                                            │  │
│  │  ┌──────────────────┐  ┌──────────────────┐               │  │
│  │  │ ICvParsingService│  │ IOcrVerification │               │  │
│  │  │                  │  │ Service          │               │  │
│  │  │ ParseCvAsync()   │  │ VerifyIdAsync()  │               │  │
│  │  │ AutofillProfile()│  │ CompareFields()  │               │  │
│  │  └────────┬─────────┘  └────────┬─────────┘               │  │
│  │           │                     │                          │  │
│  │  ┌────────┴─────────┐  ┌───────┴──────────┐               │  │
│  │  │ IMatchingService │  │ IOfferGenerator  │               │  │
│  │  │                  │  │ Service          │               │  │
│  │  │ ScoreCandidate() │  │ GeneratePdf()    │               │  │
│  │  │ RankApplicants() │  │                  │               │  │
│  │  └────────┬─────────┘  └────────┬─────────┘               │  │
│  └───────────┼─────────────────────┼──────────────────────────┘  │
│              │                     │                              │
│  ┌───────────┴─────────────────────┴──────────────────────────┐  │
│  │                INFRASTRUCTURE LAYER                         │  │
│  │                                                             │  │
│  │  ┌─────────────────────────────────────────────────────┐   │  │
│  │  │           ClaudeAiService                            │   │  │
│  │  │                                                      │   │  │
│  │  │  - HttpClient → https://api.anthropic.com/v1/messages│   │  │
│  │  │  - Model: claude-sonnet-4-20250514                         │   │  │
│  │  │  - Handles: CV parsing, OCR verification,            │   │  │
│  │  │    matching logic, document analysis                 │   │  │
│  │  │  - PDF/Image support via base64 encoding             │   │  │
│  │  └─────────────────────────────────────────────────────┘   │  │
│  │                                                             │  │
│  │  ┌──────────────┐  ┌──────────────┐  ┌──────────────────┐  │  │
│  │  │ Azure Blob   │  │ Hangfire     │  │ Email/WhatsApp   │  │  │
│  │  │ Storage      │  │ (Background) │  │ Service          │  │  │
│  │  └──────────────┘  └──────────────┘  └──────────────────┘  │  │
│  └─────────────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────────┘
```

---

## 5. SwiftRecruit Analysis

### What SwiftRecruit Does (from reviewing their job form)

SwiftRecruit implements a **CV Autofill** approach:

```
┌─────────────────────────────────────────────────────────┐
│  SwiftRecruit Job Application Flow                       │
│                                                          │
│  1. Job listing displayed with details                   │
│  2. "Autofill Application" section shown                 │
│  3. Upload CV (PDF/DOCX only — NO images/scans)         │
│  4. System parses CV and auto-fills form fields          │
│  5. Candidate reviews and submits                        │
│                                                          │
│  Key observations:                                       │
│  ✅ Text-based PDF/DOCX only (better parsing accuracy)  │
│  ✅ Warns against scanned/image CVs                     │
│  ✅ "ATS compatibility" mentioned — uses text extraction │
│  ✅ Uses Azure Blob for media (mediatc.blob.core.windows│
│     .net visible in their image URLs)                    │
│  ✅ Tenant-based architecture (tenantId in URL)          │
│  ❌ No visible OCR for ID documents                     │
│  ❌ No matching engine visible on candidate side         │
└─────────────────────────────────────────────────────────┘
```

### How Our System Differs from SwiftRecruit

| Feature | SwiftRecruit | Our RMS |
|---------|-------------|---------|
| CV Autofill | ✅ Yes | ✅ Yes (Claude AI) |
| ID Document OCR | ❌ Not visible | ✅ Yes (Claude Vision) |
| Data Verification | ❌ Not visible | ✅ OCR vs entered data comparison |
| Auto-Matching | ❌ Not visible | ✅ Weighted scoring engine |
| Multi-step Interview | ❌ Basic | ✅ Technical + Managerial + HR |
| Medical Exam Tracking | ❌ No | ✅ Full workflow |
| Sequential Offer Approval | ❌ No | ✅ 4-step approval chain |
| Onboarding Automation | ❌ No | ✅ Employee ID, insurance, contract |
| AI Engine | Unknown (likely regex) | Claude AI (advanced NLP) |

### What to Adopt from SwiftRecruit

1. **"Upload CV to Autofill" UX pattern** — Same approach, but powered by Claude
2. **Text-based PDF/DOCX restriction** — Recommend text-based, but also support scanned via Claude Vision
3. **Tenant-based architecture** — Consider for future multi-company support
4. **Azure Blob Storage** — Same infrastructure choice

---

## 6. Claude AI Integration Details

### Why Claude AI for This Project

| Factor | Claude AI | Azure Document Intelligence | Tesseract (Open Source) |
|--------|----------|---------------------------|----------------------|
| **CV Parsing Accuracy** | ⭐⭐⭐⭐⭐ (understands context) | ⭐⭐⭐ (layout extraction) | ⭐⭐ (text only) |
| **ID Document OCR** | ⭐⭐⭐⭐ (Vision API) | ⭐⭐⭐⭐⭐ (prebuilt model) | ⭐⭐ |
| **Structured JSON Output** | ⭐⭐⭐⭐⭐ (native) | ⭐⭐⭐ (needs post-processing) | ⭐ |
| **Auto-Matching Logic** | ⭐⭐⭐⭐⭐ (reasoning) | ❌ (not applicable) | ❌ |
| **Multi-language (Arabic+English)** | ⭐⭐⭐⭐⭐ | ⭐⭐⭐⭐ | ⭐⭐ |
| **Cost per CV** | ~$0.003-0.01 | ~$0.01-0.05 | Free |
| **Single API for all AI needs** | ✅ Yes | ❌ Need multiple services | ❌ |
| **Setup Complexity** | Simple HTTP API | Azure setup required | Library + training |

### Claude API Configuration

```json
// appsettings.json for Recruitment_API
{
  "ClaudeAi": {
    "ApiKey": "sk-ant-api03-...",
    "BaseUrl": "https://api.anthropic.com/v1/messages",
    "Model": "claude-sonnet-4-20250514",
    "MaxTokens": 4096,
    "TimeoutSeconds": 60
  }
}
```

### Claude AI Pricing Estimate

```
Claude Sonnet 4 Pricing:
- Input:  $3.00 per 1M tokens
- Output: $15.00 per 1M tokens

Per CV Parse (~2000 input tokens + ~1500 output tokens):
- Cost: ($3.00 × 2000/1M) + ($15.00 × 1500/1M)
- Cost: $0.006 + $0.0225 = ~$0.03 per CV

Per ID OCR (~500 input tokens + image + ~500 output tokens):
- Cost: ~$0.01 per document

Per Auto-Match (~3000 input tokens + ~1000 output tokens):
- Cost: ~$0.02 per match

Monthly Estimate (500 candidates/month):
┌───────────────────────────────┬──────────┬────────────┐
│ Operation                     │ Volume   │ Cost       │
├───────────────────────────────┼──────────┼────────────┤
│ CV Parsing                    │ 500      │ $15.00     │
│ ID Document OCR               │ 500      │ $5.00      │
│ Auto-Matching (avg 3 apps/cv) │ 1,500    │ $30.00     │
│ Data Verification             │ 500      │ $5.00      │
├───────────────────────────────┼──────────┼────────────┤
│ TOTAL                         │          │ ~$55/month │
└───────────────────────────────┴──────────┴────────────┘
```

---

### AI Feature 1: CV Parsing & Autofill (Phase 1 — Like SwiftRecruit)

**Flow:**

```
Candidate uploads CV (PDF/DOCX)
         │
         ▼
┌─────────────────────────────────┐
│  1. Store file in Azure Blob    │
│  2. Queue Hangfire background   │
│     job for parsing             │
│  3. Return immediate response   │
│     to candidate                │
└────────────┬────────────────────┘
             │ (Background)
             ▼
┌─────────────────────────────────┐
│  ClaudeAiService.ParseCvAsync() │
│                                  │
│  - Read PDF/DOCX as text         │
│  - Send to Claude with prompt    │
│  - Receive structured JSON       │
└────────────┬────────────────────┘
             │
             ▼
┌─────────────────────────────────────────────────────┐
│  Save parsed data:                                   │
│  - CandidateDocuments.AiRawResponseJson              │
│  - Candidates.ParsedCvJson                           │
│  - Auto-create CandidateEducations[] records         │
│  - Auto-create CandidateExperiences[] records        │
│  - Update Candidate personal fields if empty         │
└─────────────────────────────────────────────────────┘
```

**Claude Prompt for CV Parsing:**

```
You are a CV/Resume parsing engine. Extract structured data from the 
following CV text and return it as JSON.

Rules:
- Extract ALL education entries, work experiences, skills, and certifications
- For dates, use ISO 8601 format (YYYY-MM-DD). If only year, use YYYY-01-01
- If a field is not found, use null
- For Arabic CVs, translate field names to English but keep values in original language
- Confidence score (0-100) for each extracted section

Return this exact JSON structure:
{
  "personalInfo": {
    "fullName": string | null,
    "email": string | null,
    "phone": string | null,
    "nationality": string | null,
    "dateOfBirth": string | null,
    "nationalId": string | null,
    "address": string | null,
    "city": string | null,
    "confidence": number
  },
  "education": [
    {
      "qualification": string,
      "major": string | null,
      "institution": string,
      "graduationYear": number | null,
      "grade": string | null,
      "country": string | null,
      "confidence": number
    }
  ],
  "experience": [
    {
      "employer": string,
      "jobTitle": string,
      "startDate": string | null,
      "endDate": string | null,
      "isCurrent": boolean,
      "salary": number | null,
      "currency": string | null,
      "description": string | null,
      "country": string | null,
      "confidence": number
    }
  ],
  "skills": [string],
  "certifications": [
    {
      "name": string,
      "issuer": string | null,
      "date": string | null
    }
  ],
  "languages": [
    {
      "language": string,
      "proficiency": string
    }
  ],
  "summary": string,
  "totalYearsOfExperience": number | null,
  "overallConfidence": number
}

CV Text:
---
{cv_text_here}
---
```

**C# Interface:**

```csharp
public interface ICvParsingService
{
    /// <summary>
    /// Parse CV file and return structured data
    /// </summary>
    Task<CvParseResult> ParseCvAsync(Stream fileStream, string fileName, string contentType);
    
    /// <summary>
    /// Auto-populate candidate profile from parsed CV data
    /// </summary>
    Task<bool> AutofillCandidateProfileAsync(long candidateId, CvParseResult parseResult);
}

public class CvParseResult
{
    public PersonalInfoDto PersonalInfo { get; set; }
    public List<EducationDto> Education { get; set; }
    public List<ExperienceDto> Experience { get; set; }
    public List<string> Skills { get; set; }
    public List<CertificationDto> Certifications { get; set; }
    public List<LanguageDto> Languages { get; set; }
    public string Summary { get; set; }
    public decimal? TotalYearsOfExperience { get; set; }
    public decimal OverallConfidence { get; set; }
    public string RawJson { get; set; }
}
```

---

### AI Feature 2: ID Document OCR & Verification (Phase 1)

> ✅ **Build Now.** Claude Vision will extract and verify data from National ID and Passport documents.

**Flow:**

```
Candidate uploads National ID / Passport
         │
         ▼
┌─────────────────────────────────────────────┐
│  1. Store in Azure Blob                      │
│  2. Convert to base64 (for Claude Vision)    │
│  3. Send to Claude with image + prompt       │
│  4. Compare extracted fields with profile    │
│  5. Store results in OcrVerificationResults  │
└──────────────────────────────────────────────┘
```

**Claude Prompt for ID OCR:**

```
You are a document verification engine. Analyze this ID document image 
and extract the following fields. Return as JSON.

Document type: {National ID | Passport}

Extract:
{
  "documentType": "NationalId" | "Passport",
  "fullName": string | null,
  "fullNameArabic": string | null,
  "idNumber": string | null,
  "dateOfBirth": string | null,
  "gender": "Male" | "Female" | null,
  "nationality": string | null,
  "expiryDate": string | null,
  "issueDate": string | null,
  "placeOfBirth": string | null,
  "fieldConfidences": {
    "fullName": number,
    "idNumber": number,
    "dateOfBirth": number,
    "gender": number,
    "nationality": number
  },
  "overallConfidence": number,
  "isDocumentValid": boolean,
  "validationNotes": string | null
}
```

**Verification Logic:**

```csharp
public interface IOcrVerificationService
{
    /// <summary>
    /// Extract data from ID document using Claude Vision
    /// </summary>
    Task<IdOcrResult> ExtractFromIdDocumentAsync(Stream imageStream, string fileName, DocumentType docType);
    
    /// <summary>
    /// Compare OCR results with candidate-entered profile data
    /// Returns list of mismatches
    /// </summary>
    Task<List<OcrVerificationResult>> VerifyAgainstProfileAsync(long candidateId, IdOcrResult ocrResult);
}
```

**Comparison Logic:**

```
For each extracted field:
1. Normalize both values (trim, lowercase, remove diacritics for Arabic)
2. Compare using:
   - Exact match → IsMatch = true, Severity = None
   - Fuzzy match (>85% similarity) → IsMatch = true, Severity = Minor
   - Partial match (>60%) → IsMatch = false, Severity = Major
   - No match (<60%) → IsMatch = false, Severity = Critical

If ANY Critical mismatch:
   → Candidate.ProfileStatus = CorrectionRequired
   → Candidate.OcrVerificationStatus = Mismatch
   → Notification sent to candidate with mismatched fields
```

---

### AI Feature 3: Auto-Matching Engine (Phase 4)

**Flow:**

```
Candidate applies to vacancy
         │
         ▼
┌──────────────────────────────────────────────┐
│  1. Load candidate profile + parsed CV data  │
│  2. Load vacancy requirements                │
│  3. Send both to Claude for scoring          │
│  4. Store MatchResult with breakdown         │
│  5. Update Application.Status                │
│     → Matched or NotMatched                  │
└──────────────────────────────────────────────┘
```

**Claude Prompt for Auto-Matching:**

```
You are a recruitment matching engine. Score this candidate against 
the job vacancy requirements.

VACANCY REQUIREMENTS:
- Title: {jobTitle}
- Required Specialization: {requiredSpecialization}
- Required Experience: {requiredExperienceMin}-{requiredExperienceMax} years
- Required Qualification: {requiredQualification}
- Required Nationality: {requiredNationality or "Any"}
- Job Location: {location} (Lat: {lat}, Lon: {lon})
- Required Certifications: {requiredCertifications}
- Required Skills: {requiredSkills}

CANDIDATE PROFILE:
- Education: {education_json}
- Experience: {experience_json}
- Total Years of Experience: {totalYears}
- Nationality: {nationality}
- Residence: {city} (Lat: {lat}, Lon: {lon})
- Certifications: {certifications}
- Skills: {skills}

SCORING WEIGHTS:
- Specialization: {weightSpecialization}%
- Experience: {weightExperience}%
- Qualification: {weightQualification}%
- Nationality: {weightNationality}%
- Location Proximity: {weightLocation}%
- Certifications: {weightCertifications}%
- Match Threshold: {matchThreshold}

Score each criterion 0-100 and calculate the weighted overall score.
Return this JSON:

{
  "specializationScore": number,
  "specializationReason": string,
  "experienceScore": number,
  "experienceReason": string,
  "qualificationScore": number,
  "qualificationReason": string,
  "nationalityScore": number,
  "nationalityReason": string,
  "locationScore": number,
  "locationDistanceKm": number,
  "locationReason": string,
  "certificationScore": number,
  "certificationReason": string,
  "overallScore": number,
  "isMatch": boolean,
  "matchExplanation": string,
  "strengths": [string],
  "concerns": [string]
}
```

**C# Interface:**

```csharp
public interface IMatchingService
{
    /// <summary>
    /// Score a single candidate against a vacancy
    /// </summary>
    Task<MatchResult> ScoreCandidateAsync(long candidateId, long vacancyId);
    
    /// <summary>
    /// Batch score all applicants for a vacancy
    /// </summary>
    Task<List<MatchResult>> ScoreAllApplicantsAsync(long vacancyId);
    
    /// <summary>
    /// Rank candidates for a vacancy by match score
    /// </summary>
    Task<List<RankedCandidate>> RankCandidatesAsync(long vacancyId);
}
```

---

### AI Feature 4: Offer Letter PDF Generation (Phase 12)

**Approach:** Use QuestPDF (.NET library, free) with Claude for dynamic content

```csharp
public interface IOfferGeneratorService
{
    /// <summary>
    /// Generate PDF offer letter using template + candidate data
    /// </summary>
    Task<byte[]> GenerateOfferLetterAsync(long jobOfferId);
}
```

---

### Claude AI Service — Core Implementation Design

```csharp
// Application Layer — Interface
public interface IClaudeAiService
{
    Task<string> SendMessageAsync(string systemPrompt, string userMessage);
    Task<string> SendMessageWithImageAsync(string systemPrompt, string userMessage, byte[] imageBytes, string mediaType);
    Task<T> SendStructuredMessageAsync<T>(string systemPrompt, string userMessage) where T : class;
}

// Infrastructure Layer — Implementation
public class ClaudeAiService : IClaudeAiService
{
    // Uses HttpClient to call https://api.anthropic.com/v1/messages
    // Headers: x-api-key, anthropic-version: 2023-06-01
    // Model: claude-sonnet-4-20250514
    // Supports: text messages + vision (base64 images)
    // Returns: Deserialized JSON responses
}
```

**Request format to Claude API:**

```json
{
  "model": "claude-sonnet-4-20250514",
  "max_tokens": 4096,
  "system": "You are a CV parsing engine...",
  "messages": [
    {
      "role": "user",
      "content": [
        {
          "type": "text",
          "text": "Parse this CV: ..."
        }
      ]
    }
  ]
}
```

**For Vision (ID OCR) — request with image:**

```json
{
  "model": "claude-sonnet-4-20250514",
  "max_tokens": 4096,
  "system": "You are a document verification engine...",
  "messages": [
    {
      "role": "user",
      "content": [
        {
          "type": "image",
          "source": {
            "type": "base64",
            "media_type": "image/jpeg",
            "data": "/9j/4AAQSkZJRg..."
          }
        },
        {
          "type": "text",
          "text": "Extract fields from this National ID document..."
        }
      ]
    }
  ]
}
```

---

### Background Job Processing (Hangfire)

```
┌──────────────────────────────────────────────────────┐
│  SYNC Operations (immediate response):                │
│  - File upload → Azure Blob                           │
│  - Candidate registration                             │
│  - Application submission                             │
│  - Status updates                                     │
└──────────────────────────────────────────────────────┘

┌──────────────────────────────────────────────────────┐
│  ASYNC Operations (Hangfire background jobs):         │
│                                                       │
│  1. CvParsingJob                                      │
│     Trigger: After CV upload                          │
│     Action: Claude CV parse → auto-populate profile   │
│     Timeout: 60 seconds                               │
│                                                       │
│  2. IdVerificationJob                                 │
│     Trigger: After ID document upload                 │
│     Action: Claude Vision OCR → verify against profile│
│     Timeout: 30 seconds                               │
│                                                       │
│  3. AutoMatchingJob                                   │
│     Trigger: After application submission             │
│     Action: Claude matching → score + classify        │
│     Timeout: 30 seconds                               │
│                                                       │
│  4. NotificationJob                                   │
│     Trigger: After any status change                  │
│     Action: Send email/WhatsApp                       │
│     Retry: 3 times with exponential backoff           │
│                                                       │
│  5. OfferPdfGenerationJob                             │
│     Trigger: After offer approval                     │
│     Action: Generate PDF → store in Azure Blob        │
│     Timeout: 30 seconds                               │
└──────────────────────────────────────────────────────┘
```

---

## 7. API Endpoint Plan

### Candidate Portal APIs

| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| POST | `/api/candidates/register` | Create candidate profile | Candidate |
| GET | `/api/candidates/profile` | Get own profile | Candidate |
| PUT | `/api/candidates/profile` | Update profile | Candidate |
| POST | `/api/candidates/documents` | Upload document (CV/ID/Cert) | Candidate |
| GET | `/api/candidates/documents` | List own documents | Candidate |
| DELETE | `/api/candidates/documents/{id}` | Remove document | Candidate |
| POST | `/api/candidates/submit-profile` | Submit profile for review | Candidate |
| GET | `/api/candidates/cv-parse-status` | Check CV parsing status | Candidate |
| GET | `/api/candidates/parsed-data` | Get AI-extracted CV data | Candidate |
| POST | `/api/candidates/confirm-parsed-data` | Confirm/edit parsed data | Candidate |
| GET | `/api/vacancies/published` | List published vacancies | Candidate |
| GET | `/api/vacancies/{id}` | Get vacancy details | Candidate |
| POST | `/api/applications/apply/{vacancyId}` | Apply to vacancy | Candidate |
| GET | `/api/applications/my-applications` | List own applications | Candidate |
| GET | `/api/applications/{id}/status` | Check application status | Candidate |
| POST | `/api/interviews/{id}/confirm` | Confirm interview | Candidate |
| GET | `/api/offers/my-offers` | List received offers | Candidate |
| POST | `/api/offers/{id}/accept` | Accept offer | Candidate |
| POST | `/api/offers/{id}/reject` | Reject offer | Candidate |
| POST | `/api/offers/{id}/upload-signed` | Upload signed offer | Candidate |

### Recruiter/HR APIs

| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| GET | `/api/admin/candidates` | List all candidates (filterable) | Recruiter |
| GET | `/api/admin/candidates/{id}` | Get candidate full profile | Recruiter |
| PUT | `/api/admin/candidates/{id}/status` | Approve/Reject/Request correction | Recruiter |
| GET | `/api/admin/candidates/{id}/ocr-results` | View OCR verification results | Recruiter |
| POST | `/api/admin/vacancies` | Create vacancy | Recruiter |
| PUT | `/api/admin/vacancies/{id}` | Update vacancy | Recruiter |
| POST | `/api/admin/vacancies/{id}/publish` | Publish vacancy | Recruiter |
| POST | `/api/admin/vacancies/{id}/close` | Close vacancy | Recruiter |
| GET | `/api/admin/applications` | List applications (filterable) | Recruiter |
| GET | `/api/admin/applications/vacancy/{id}` | Applications for vacancy | Recruiter |
| GET | `/api/admin/applications/{id}/match-result` | View AI match score | Recruiter |
| POST | `/api/admin/applications/{id}/assign` | Assign recruiter | Recruiter |
| POST | `/api/admin/screening-tasks` | Create screening task | Recruiter |
| PUT | `/api/admin/screening-tasks/{id}` | Update screening task | Recruiter |
| POST | `/api/admin/interviews/schedule` | Schedule interview | Recruiter, HR |
| PUT | `/api/admin/interviews/{id}/reschedule` | Reschedule interview | Recruiter, HR |
| POST | `/api/admin/interviews/{id}/evaluate` | Submit evaluation | HiringManager, HR |
| POST | `/api/admin/medical/request` | Request medical exam | HR |
| POST | `/api/admin/medical/{id}/upload-report` | Upload medical report | MedicalProvider |
| PUT | `/api/admin/medical/{id}/clearance` | Set clearance status | HR |
| POST | `/api/admin/offers` | Create offer | HR |
| POST | `/api/admin/offers/{id}/submit-approval` | Submit for approval | HR |
| POST | `/api/admin/offers/{id}/approve` | Approve offer (step) | Recruiter, HR, VPO, CEO |
| POST | `/api/admin/offers/{id}/reject` | Reject offer (step) | Recruiter, HR, VPO, CEO |
| POST | `/api/admin/offers/{id}/send` | Send offer to candidate | HR |
| POST | `/api/admin/onboarding/{id}/start` | Start onboarding | HR |
| PUT | `/api/admin/onboarding/{id}` | Update onboarding progress | HR |
| POST | `/api/admin/onboarding/{id}/activate` | Activate employee | HR |

### Dashboard/Reports APIs

| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| GET | `/api/dashboard/stats` | Overall recruitment stats | Recruiter, HR |
| GET | `/api/dashboard/pipeline` | Application pipeline counts | Recruiter, HR |
| GET | `/api/dashboard/vacancy-stats/{id}` | Per-vacancy stats | Recruiter |
| GET | `/api/reports/time-to-hire` | Average time to hire | HR |
| GET | `/api/reports/source-analytics` | Application source data | HR |

---

## 8. Implementation Roadmap

### Phase-wise Development Plan

```
WEEK 1-2: Foundation
━━━━━━━━━━━━━━━━━━━━
├── Create Recruitment_API solution (Clean Architecture)
├── Setup RecruitmentDB + EF Core DbContext
├── Configure JWT validation (shared with UCIC_API)
├── Add roles to UCIC_API DataSeeder
├── Implement BaseEntity, enums, base repositories
├── Setup Azure Blob Storage integration
├── Setup Hangfire for background jobs
└── Create domain entities (all 18 tables)

WEEK 3: Candidate Registration + AI
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
├── Candidate CRUD APIs
├── Document upload to Azure Blob
├── Claude AI Service (core HttpClient wrapper)
├── CV Parsing Service (Claude integration)
├── CV Autofill feature (like SwiftRecruit)
├── ID OCR Service (Claude Vision)
├── Data Verification Service
└── Background jobs for AI processing

WEEK 4: Vacancies + Auto-Matching
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
├── Vacancy CRUD + publish workflow
├── Application submission
├── Auto-Matching Service (Claude)
├── Match results storage + ranking
├── Candidate talent database (search/filter)
└── Recruiter dashboard basics

WEEK 5: Interview Workflow
━━━━━━━━━━━━━━━━━━━━━━━━━
├── Screening task management
├── Interview scheduling
├── Confirmation/rescheduling
├── Evaluation forms
├── HR interview flow
└── Email notification service

WEEK 6: Offers + Medical + Onboarding
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
├── Medical examination workflow
├── Offer creation + unified screen
├── Sequential approval workflow
├── PDF offer generation (QuestPDF)
├── E-signature upload
├── Onboarding workflow
└── Employee activation

WEEK 7: Polish + Testing
━━━━━━━━━━━━━━━━━━━━━━━━
├── WhatsApp integration
├── Audit logging middleware
├── Status history tracking
├── Dashboard & reports
├── Unit tests
├── Integration tests
└── Security review

WEEK 8: Deployment
━━━━━━━━━━━━━━━━━━
├── Production database setup
├── Azure deployment
├── CORS configuration
├── Performance testing
├── Final QA
└── Go-live
```

---

### NuGet Packages for Recruitment_API

```xml
<!-- Core -->
<PackageReference Include="MediatR" Version="12.2.0" />
<PackageReference Include="AutoMapper" Version="13.0.1" />
<PackageReference Include="FluentValidation" Version="11.9.0" />

<!-- EF Core -->
<PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="8.0.10" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="8.0.10" />

<!-- JWT Authentication -->
<PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="8.0.10" />
<PackageReference Include="System.IdentityModel.Tokens.Jwt" Version="7.0.0" />

<!-- Azure Blob Storage -->
<PackageReference Include="Azure.Storage.Blobs" Version="12.19.0" />

<!-- Background Jobs -->
<PackageReference Include="Hangfire.Core" Version="1.8.12" />
<PackageReference Include="Hangfire.SqlServer" Version="1.8.12" />
<PackageReference Include="Hangfire.AspNetCore" Version="1.8.12" />

<!-- PDF Generation -->
<PackageReference Include="QuestPDF" Version="2024.3.0" />

<!-- Claude AI (HTTP client — no specific NuGet needed, use HttpClient) -->
<!-- Email -->
<PackageReference Include="MailKit" Version="4.3.0" />

<!-- Logging -->
<PackageReference Include="Serilog.AspNetCore" Version="8.0.0" />

<!-- Swagger -->
<PackageReference Include="Swashbuckle.AspNetCore" Version="6.5.0" />

<!-- Document text extraction (PDF/DOCX → text for Claude) -->
<PackageReference Include="DocumentFormat.OpenXml" Version="3.0.1" />
<PackageReference Include="UglyToad.PdfPig" Version="0.1.8" />
```

---

### Key Configuration (appsettings.json for Recruitment_API)

```json
{
  "ConnectionStrings": {
    "RecruitmentDb": "Server=208.64.33.128;Database=RecruitmentDB;User Id=...;Password=...;MultipleActiveResultSets=True;TrustServerCertificate=True;"
  },
  "Jwt": {
    "Key": "Oi02c3F2S3ZHRlp1W7jTOu/v34I6dgsr5N1k1KdHc3E=",
    "Issuer": "jwt",
    "Audience": "jwt"
  },
  "ClaudeAi": {
    "ApiKey": "sk-ant-api03-your-key-here",
    "BaseUrl": "https://api.anthropic.com/v1/messages",
    "Model": "claude-sonnet-4-20250514",
    "MaxTokens": 4096,
    "TimeoutSeconds": 60
  },
  "AzureBlobStorage": {
    "ConnectionString": "DefaultEndpointsProtocol=https;AccountName=...;AccountKey=...;EndpointSuffix=core.windows.net",
    "ContainerName": "recruitment-documents"
  },
  "Hangfire": {
    "DashboardPath": "/hangfire"
  },
  "CorsSettings": {
    "AllowedOrigins": [
      "http://localhost:4200",
      "https://recruitment.ucic.com"
    ]
  }
}
```

---

> **Document Version:** 3.0  
> **Last Updated:** 2025  
> **Author:** AI Architecture Review  
> **Status:** All AI Features In Scope → Ready for Implementation  
> **Changes in v3.0:**  
> - **ALL AI features marked as ✅ Build Now** — no AI features deferred  
> - ID/Document OCR (Claude Vision) upgraded from Future Work to **P0 — Build Now**  
> - Data Verification (OCR vs Profile) upgraded from Future Work to **P1 — Build Now**  
> - Interview Summary upgraded from Nice-to-have to **P2 — Build Now**  
> - Candidate Ranking upgraded to **P1 — Build Now**  
> - Offer Letter Generation upgraded to **P1 — Build Now**  
> **Changes in v2.0:**  
> - Added Section 1: Business Decisions (Q&A) — all client decisions captured  
> - Added `VacancyApprovals` table (HR Manager + Section Head approval before publishing)  
> - Added `VacancyRecruiters` table (multiple recruiters per vacancy)  
> - Added `OnboardingTasks` table (standard checklist auto-assigned to departments)  
> - Updated `Candidates` table: NationalId uniqueness, profile lock, rejection reactivation  
> - Updated `Vacancies` table: Paused status, approval flow, auto-close on expiry only  
> - Updated `JobOffers` table: 12-day expiry, external e-signature, role+grade templates, auto-reminders  
> - Updated `Onboardings` table: HCM integration fields, standard checklist  
> - Total tables: **21** (was 18, added VacancyApprovals, VacancyRecruiters, OnboardingTasks)
