# RMS — Task Breakdown & Progress Tracker

> **Last Updated:** March 31, 2026  
> **Team:** 1 senior dev in progress + AI-assisted development + fresh developers  
> **Claude API Key:** Available for testing AI features  
> **Architecture:** RecruitmentUI (Angular) + RecruitmentAPI (.NET 8) + UCIC API (auth) + ucic_admin_client (Angular)

---

## Quick System Map

```
[Candidate Browser]  →  RecruitmentUI (Angular)  →  RecruitmentAPI (.NET 8)
                                             ↓ auth only
                                          UCIC API (JWT)

[HR / Recruiter]  →  ucic_admin_client  →  RecruitmentAPI (.NET 8)
                                 ↓ auth only
                              UCIC API (JWT)
```

---

## Overall Progress — Phase by Phase

| Phase | Description | Status |
|-------|-------------|--------|
| **P1** | Candidate Registration & Profile | 🟡 ~80% — UI done, some backend gaps |
| **P2** | Recruiter Review & Verification | 🔴 ~20% — reads work, no OCR, no workflow |
| **P3** | Vacancy Management | 🟡 ~60% — CRUD done, approval workflow missing |
| **P4** | AI Auto-Matching | 🔴 ~10% — entity/schema in place, no engine |
| **P5** | Screening | 🔴 ~30% — read-only, no task creation |
| **P6** | Interviews | 🔴 ~25% — read-only, no scheduling commands |
| **P7** | Interview Confirmation & Security Report | 🔴 0% |
| **P8** | HR Interview | 🔴 0% |
| **P9** | Medical Examination | 🔴 0% |
| **P10** | Job Offer & Approval Chain | 🔴 0% |
| **P11** | Onboarding | 🔴 0% |
| **Cross-Cutting** | Notifications, Audit, Status History | 🔴 0% |

---

## ✅ What Is Already Done (Confirmed Working)

### Backend (RecruitmentAPI)
- [x] Full Clean Architecture + CQRS/MediatR structure
- [x] Shared JWT auth with UCIC API (no separate login needed)
- [x] `GET/POST/PUT /api/candidates` — candidate profile CRUD
- [x] `POST /api/candidates/{id}/submit` — lock profile after submission
- [x] `PUT /api/candidates/{id}/status` — recruiter updates profile status
- [x] `GET/POST/PUT /api/vacancies` — vacancy CRUD + published listing (anonymous)
- [x] `GET/POST /api/applications` — apply + list applications
- [x] `PUT /api/applications/{id}/status` — status update (27-step pipeline)
- [x] `POST /api/documents/.../upload` — file upload + auto-trigger CV parsing on CV type
- [x] `GET /api/documents/{id}/download` — file streaming
- [x] `POST /api/documents/parse-cv-preview` — AI CV parse (no DB save) for wizard
- [x] `ClaudeAiService` — full HTTP wrapper for Claude Sonnet (text, image, PDF, structured JSON)
- [x] `CvParsingService` — PDF + DOCX parsing, autofill education/experience
- [x] `LocalFileStorageService` — saves to `D:/Uploads/Recruitment/`
- [x] Read-only interview and screening task list endpoints

### Frontend — RecruitmentUI (Angular)
- [x] Login → UCIC API integration (JWT stored in sessionStorage)
- [x] Candidate registration (`/signup`) → auto-redirects to `/profile`
- [x] Job list page (`/jobs`) — paginated, searchable, anonymous
- [x] Job detail page (`/jobs/:id`) — full info + one-click apply
- [x] **Profile wizard** (`/profile`) — 4-step stepper:
  - Step 1: CV upload + AI parse preview (auto-fills form from Claude)
  - Step 2: Personal info form
  - Step 3: Education FormArray
  - Step 4: Experience FormArray + document uploads
- [x] Profile edit mode — update personal info, add/remove education/experience, upload/delete documents
- [x] Profile submit + lock (form disables after submission)
- [x] My Applications list (`/my-applications`) — paginated, 27-status display
- [x] Application detail (`/my-applications/:id`) — 9-step visual progress tracker, AI match scores, screening tasks, interview details
- [x] Auth guard, JWT interceptor, navbar, footer

### Frontend — ucic_admin_client
- [x] Full recruiter-management module wired to RecruitmentAPI
- [x] Candidates list + detail view
- [x] Vacancies list, create, edit, detail
- [x] Applications list + detail
- [x] Interviews list

---

## 🟡 In Progress

| Task | Owner | Notes |
|------|-------|-------|
| Profile creation — make it work as client expects | **Dev 1 (Senior)** | UI is built; issues likely in form validation, CV parse edge cases, document upload flow, or API error handling |

---

## 🔴 Tasks To Be Done

---

### TRACK 1 — Fix Profile Creation (Dev 1 — Senior, In Progress)

**Goal:** Profile wizard works end-to-end as the client expects.

#### T1.1 — Investigate & Fix CV Parse Preview
- Ensure `POST /api/documents/parse-cv-preview` returns clean structured JSON for all PDF and DOCX files
- Test with edge-case CVs (multi-page, Arabic/English mix, scanned PDFs)
- Fix confidence score display in Step 1 of wizard
- **AI Role:** Claude Sonnet does the parsing; dev needs to tune prompt if output is wrong

#### T1.2 — Profile Wizard Validation & UX
- Required field enforcement across all 4 steps before allowing "Next"
- Graceful error messages on API failures
- Fix: MatDialog import in JobDetailComponent (currently imported but unused — either add apply confirmation dialog or remove)
- Add apply-from-profile-not-submitted warning (API rejects apply if profile not submitted)

#### T1.3 — Document Upload Flow
- Verify multipart upload hits correct endpoint `POST /api/documents/candidates/{id}/upload`
- Test each document type: CV, NationalId, Passport, Degree, Certificate, License
- Confirm document delete works and refreshes list
- Add download button for uploaded documents (use `GET /api/documents/{id}/download`)

#### T1.4 — Profile Photo Upload
- `profilePhotoPath` field exists in `CandidateDto` but no upload UI exists
- Add a profile photo upload control (candidate.service → uploadDocument with type = Photo, or dedicated endpoint)

#### T1.5 — Change Password Page
- `AuthService.changePassword()` exists but no UI component
- Create a Settings/Change Password page under `/profile` tab or modal

---

### TRACK 2 — AI Matching Engine (Senior Dev + AI)

> **This is the most important unbuilt feature.** The entire auto-screening pipeline depends on it.  
> Claude API key is available. Claude Sonnet already integrated in `ClaudeAiService`.

#### T2.1 — Implement `MatchingService` (Backend — Senior)
**File to create:** `RecruitmentAPI.Application/Services/MatchingService.cs`

The service must:
1. Accept a `Guid applicationId`
2. Load the `Application` → `Candidate` (with education, experience, parsed CV JSON) + `Vacancy` (title, description, requirements, weights)
3. Build a structured Claude prompt sending both candidate profile and vacancy requirements
4. Parse Claude's JSON response into `MatchResult` entity:
   - `OverallScore` (0–100)
   - `SpecializationScore`, `ExperienceScore`, `QualificationScore`, `LanguageScore`, `LocationScore`, `SalaryScore`
   - `IsMatch` = OverallScore >= vacancy.MatchThreshold
   - `MatchExplanation` (text)
   - `AiModel` = "claude-sonnet-4-20250514"
5. Save `MatchResult` to DB
6. Update Application status:
   - If `IsMatch = true` → status = `Screening`
   - If `IsMatch = false` → status = `NotMatched`
7. Write `StatusHistory` entry

**Prompt template:** Based on vacancy weights — if `WeightSpecialization = 30`, the prompt should weight that criteria at 30%.

#### T2.2 — Trigger Matching on Application Create (Backend — Senior)
- In `CreateApplicationCommandHandler`, after saving the application, fire `Task.Run(() => matchingService.MatchAsync(applicationId))`
- Or register Hangfire and queue it (see T6.1)

#### T2.3 — Match Result API (Backend — done, needs data)
- `GET /api/applications/{id}/match-result` endpoint exists — just needs T2.1 to populate data
- Verify the `ApplicationDetailDto` includes match result in response JSON

#### T2.4 — Admin Client: Display Match Results (Fresh Dev)
- In `ucic_admin_client/recruiter-management/applications/:id` — application detail page
- Show AI match score breakdown as a visual progress bar per category
- Show `MatchExplanation` text
- Show "Override — Move to Screening" button if status = NotMatched (manual recruiter override)

---

### TRACK 3 — OCR / ID Verification (Senior Dev)

> Claude Vision API is already integrated in `ClaudeAiService.SendImageMessageAsync`.

#### T3.1 — Implement `OcrVerificationService` (Backend — Senior)
**File to create:** `RecruitmentAPI.Application/Services/OcrVerificationService.cs`

Steps:
1. Accept `candidateDocumentId` (NationalId or Passport type only)
2. Load document file from disk, convert to base64
3. Send to Claude Vision with prompt: "Extract all fields from this ID: Name, DOB, ID Number, Expiry, Nationality..."
4. Compare extracted fields to `Candidate` entity fields
5. Write `OcrVerificationResult` rows per field (FieldName, ExtractedValue, EnteredValue, IsMatch, MismatchSeverity)
6. If any Critical mismatch → update `Candidate.ProfileStatus = CorrectionRequired`
7. Else → mark document `AiProcessingStatus = Succeeded`

#### T3.2 — Auto-Trigger OCR on Document Upload (Backend — Senior)
- Existing upload handler only triggers CV parsing for CV type
- Add branch: if `documentType = NationalId || Passport` → trigger `OcrVerificationService` in background

#### T3.3 — OCR Results in Admin Client (Fresh Dev)
- Show OCR verification results (field-level matches/mismatches) in candidate detail view in admin client
- Show mismatch fields highlighted in red
- Allow recruiter to manually override and mark as verified

---

### TRACK 4 — Vacancy Approval Workflow (Fresh Dev guided by Senior)

#### T4.1 — Backend: Vacancy Approval Commands (Fresh Dev)
**Files to create:**
- `Commands/Vacancies/SubmitVacancyForApproval/SubmitVacancyForApprovalCommand.cs + Handler`
- `Commands/Vacancies/ApproveVacancy/ApproveVacancyCommand.cs + Handler`
- `Commands/Vacancies/RejectVacancy/RejectVacancyCommand.cs + Handler`

Flow:
1. Recruiter/HR submits → status = `PendingApproval`, creates `VacancyApproval` row (Step=1, Status=Pending)
2. HR Manager approves Step 1 → creates Step 2 (HR Section Head)
3. HR Section Head approves Step 2 → `Vacancy.PublishStatus = Published`
4. Either step rejected → `Vacancy.PublishStatus = Draft`, notify requester

#### T4.2 — Backend: API Endpoints for Vacancy Approval (Fresh Dev)
Add to `VacanciesController`:
- `POST /api/vacancies/{id}/submit-for-approval` — Recruiter, HRSupervisor
- `POST /api/vacancies/{id}/approve` — Admin, HRSupervisor
- `POST /api/vacancies/{id}/reject` — Admin, HRSupervisor

#### T4.3 — Admin Client: Approval UI (Fresh Dev)
- Add "Submit for Approval" button on vacancy detail page (visible to Recruiter/HR)
- Add Pending Approvals dashboard widget and list in admin client
- Approve/Reject buttons for authorized users

---

### TRACK 5 — Screening & Interview Commands (Fresh Dev guided by Senior)

#### T5.1 — Backend: Create Screening Task (Fresh Dev)
- `POST /api/screeningtasks` — Admin, Recruiter, HRSupervisor
- Command: `CreateScreeningTaskCommand` → creates `ScreeningTask` for an application
- Fields: `applicationId`, `assignedToUserId`, `notes`, `deadline` (optional)

#### T5.2 — Backend: Update Screening Task Status (Fresh Dev)
- `PUT /api/screeningtasks/{id}` — Recruiter
- Update `ExpectedSalary`, `NoticePeriodDays`, `WillingnessToRelocate`, `Status` (Pending/InProgress/Completed)
- On Completed → may trigger move to `Shortlisted` status on Application

#### T5.3 — Backend: Schedule Interview (Fresh Dev)
- `POST /api/interviews` — Admin, Recruiter, HRSupervisor, HiringManager
- Command: `CreateInterviewCommand` → creates `Interview`
- Fields: `applicationId`, `interviewType`, `interviewMode`, `scheduledDate`, `interviewerUserIds`, `location` (if onsite), `onlineMeetingLink` (if online)

#### T5.4 — Backend: Submit Interview Evaluation (Fresh Dev)
- `POST /api/interviews/{id}/evaluation` — HiringManager, HRSupervisor
- Command: `CreateInterviewEvaluationCommand`
- Fields: `technicalScore`, `communicationScore`, `overallScore`, `strengths`, `weaknesses`, `recommendation`, `decision` (Proceed/Reject/Hold)

#### T5.5 — Admin Client: Screening Task Management (Fresh Dev)
- In application detail: show screening tasks with status
- "Assign Screening Task" button → modal with assignee (recruiter list), notes, deadline
- Update task status inline

#### T5.6 — Admin Client: Schedule Interview UI (Fresh Dev)
- In application detail: "Schedule Interview" button
- Form: type (Technical/Managerial/HR/Final), mode (OnSite/Online), date, interviewers
- Show scheduled interviews timeline per application

---

### TRACK 6 — Background Jobs / Hangfire (Senior Dev)

#### T6.1 — Register Hangfire in Program.cs (Senior)
```csharp
// In Program.cs — replace Task.Run fire-and-forget with proper Hangfire jobs
builder.Services.AddHangfire(config => config.UseSqlServerStorage(connectionString));
builder.Services.AddHangfireServer();
app.UseHangfireDashboard("/hangfire"); // restrict to Admin role
```

Replace all `Task.Run(() => ...)` calls in:
- `UploadDocumentCommandHandler` → `BackgroundJob.Enqueue(() => cvParsingService.ParseAsync(...))`
- `CreateApplicationCommandHandler` → `BackgroundJob.Enqueue(() => matchingService.MatchAsync(...))`
- `UploadDocumentCommandHandler` (ID/Passport) → `BackgroundJob.Enqueue(() => ocrService.VerifyAsync(...))`

#### T6.2 — Retry & Error Handling for AI Jobs (Senior)
- Configure Hangfire retry policy (3 attempts, exponential backoff)
- On final failure, update document/application status to `AiProcessingFailed`
- Log failure to Serilog

---

### TRACK 7 — Medical, Offer & Onboarding Pipeline (Senior Dev for design, Fresh Dev for implementation)

#### T7.1 — Medical Examination Controller + Commands (Fresh Dev)
- `POST /api/medicalexaminations` — Recruiter, HRSupervisor
  - Creates `MedicalExamination` for application, status = Pending
- `PUT /api/medicalexaminations/{id}` — MedicalProvider (upload report + set clearance status)
- `GET /api/medicalexaminations/application/{applicationId}` — Admin, Recruiter, HRSupervisor

#### T7.2 — Job Offer Controller + Commands (Senior Dev)
- `POST /api/joboffers` — Admin, HRSupervisor — create offer (ProposedSalary, StartDate, etc.)
- `POST /api/joboffers/{id}/submit-for-approval` — triggers 4-step `OfferApproval` chain
- `POST /api/joboffers/{id}/approve` — sequential approvers (Recruitment → HR → VPO → CEO)
- `POST /api/joboffers/{id}/reject`
- `POST /api/joboffers/{id}/generate-letter` — QuestPDF offer letter generation (Senior Dev)
- `PUT /api/joboffers/{id}/candidate-response` — Candidate accepts/declines offer

#### T7.3 — Onboarding Controller + Commands (Fresh Dev guided)
- Auto-create `Onboarding` + default `OnboardingTask` records when offer is accepted
- `GET /api/onboarding/application/{applicationId}`
- `PUT /api/onboarding/{id}/tasks/{taskId}` — mark task complete
- `PUT /api/onboarding/{id}/hcm-sync` — mark HCM sync done (Admin only)

#### T7.4 — Admin Client: Medical / Offer / Onboarding Views (Fresh Dev)
- Add tabs on application detail: Medical | Offer | Onboarding
- Medical: show clearance status, upload report button (MedicalProvider only)
- Offer: show offer details, approval chain progress, generate letter button
- Onboarding: checklist of tasks with completion mark

---

### TRACK 8 — Cross-Cutting: Notifications, Audit, Status History (Fresh Dev)

#### T8.1 — StatusHistory Writer (Fresh Dev)
- Create `StatusHistoryService` or `IStatusHistoryRepository`
- Every place where `Application.Status` or `Candidate.ProfileStatus` changes → insert `StatusHistory` row
- Fields: `EntityType`, `EntityId`, `OldStatus`, `NewStatus`, `ChangedByUserId`, `ChangedAt`, `Reason`

#### T8.2 — AuditLog Writer (Fresh Dev)
- Create `AuditLogService`
- Every recruiter edit of candidate data → log old + new JSON values
- Every status update → log action

#### T8.3 — Notification System (Senior design, Fresh Dev implement)
- Create `INotificationService` interface
- Implement email notifications (use SMTP or existing UCIC email infra):
  - Application submitted → notify recruiter
  - Profile correction required → email candidate
  - Interview scheduled → email candidate with date/time/link
  - Offer issued → email candidate
  - Offer accepted/rejected → notify recruiter
- Hook notification calls into command handlers

---

### TRACK 9 — Admin Client: Role Management Fixes (Fresh Dev)

#### T9.1 — Add RMS Roles to UserRole Enum
- **File:** `ucic_admin_client/src/app/services/role.service.ts` (or equivalent enum file)
- Add: `RECRUITER = 'Recruiter'`, `HR_SUPERVISOR = 'HRSupervisor'`, `HIRING_MANAGER = 'HiringManager'`, `VPO = 'VPO'`, `MEDICAL_PROVIDER = 'MedicalProvider'`, `CANDIDATE = 'Candidate'`

#### T9.2 — Update Route Guards
- `recruiter-management` route should allow: `Admin`, `Recruiter`, `HRSupervisor`, `HiringManager`
- Medical sub-routes: `MedicalProvider` role
- Offer approval sub-routes: `VPO`, `Admin`

#### T9.3 — Recruitment Dashboard / Overview
- Wire up `RecruitmentOverviewComponent` with real API data
- KPIs: Total candidates, active vacancies, pending applications, interviews this week, offers pending
- Use `GET /api/health` + aggregate queries to candidates/vacancies/applications

---

### TRACK 10 — Security Report (PDF Generation) (Senior Dev)

#### T10.1 — Interview Security Report (Senior)
- When `Interview.SecurityListGenerated = false` and interview is scheduled
- Generate a PDF report with: candidate name, photo, ID number, interview date/time/location
- Use QuestPDF (already referenced in design doc) or similar
- Store report path, expose `GET /api/interviews/{id}/security-report/download`

---

### TRACK 11 — RecruitmentUI: Candidate-Side Enhancements (Fresh Dev)

#### T11.1 — Withdraw Application
- `ApplicationStatusLabels[26] = "Withdrawn"` exists
- Add "Withdraw Application" button in application detail page
- Needs backend: `PUT /api/applications/{id}/status` with `Withdrawn` — already exists
- Add confirmation dialog before sending

#### T11.2 — Show Match Explanation to Candidate
- Client decision: auto-rejection reasons ARE visible to candidates
- In `ApplicationDetailComponent`, show `matchResult.MatchExplanation` if status = NotMatched
- Show match score breakdown (radar/bar chart) when status = Matched or beyond

#### T11.3 — Change Password UI
- Add Settings page or modal at `/profile` → Change Password tab
- `AuthService.changePassword()` is ready — just needs a UI form

#### T11.4 — Profile Photo Upload
- Add photo upload control in profile edit mode
- Crop/resize image client-side before upload (optional but nice)
- Display photo in navbar/profile header

#### T11.5 — Forgot Password Flow
- Add "Forgot Password" link on login page
- Requires UCIC API to have a forgot-password endpoint (check with UCIC API team)

---

## Task Assignment Summary

### Senior Developer Tasks

| ID | Task | Track | Priority |
|----|------|-------|----------|
| T2.1 | Implement AI Matching Service (MatchingService.cs) | Matching | 🔴 Critical |
| T2.2 | Trigger matching on application create | Matching | 🔴 Critical |
| T3.1 | Implement OCR Verification Service | OCR | 🟠 High |
| T3.2 | Auto-trigger OCR on ID/Passport upload | OCR | 🟠 High |
| T6.1 | Register and configure Hangfire | Background Jobs | 🟠 High |
| T6.2 | Retry/error handling for AI background jobs | Background Jobs | 🟠 High |
| T7.2 | Job Offer controller + commands (complex flow) | Offer Pipeline | 🟡 Medium |
| T10.1 | Security report PDF generation (QuestPDF) | Report | 🟡 Medium |
| T8.3 | Design notification system architecture | Notifications | 🟡 Medium |
| T1.1–T1.5 | Support Dev 1 on profile creation track | Profile | 🔴 Critical (now) |

### Fresh Developer Tasks

> These tasks have clear patterns already established in the codebase — copy the Command/Handler pattern from existing implementations.

| ID | Task | Track | Difficulty | Notes |
|----|------|-------|-----------|-------|
| T4.1 | Vacancy approval commands (4 handlers) | Vacancy | ⭐⭐ | Copy pattern from UpdateCandidateStatusCommand |
| T4.2 | Vacancy approval API endpoints | Vacancy | ⭐ | 3 new endpoints in VacanciesController |
| T4.3 | Admin client: Approval UI components | Vacancy | ⭐⭐ | Angular Material dialogs |
| T5.1 | Create screening task command | Screening | ⭐⭐ | New Command + Handler + Endpoint |
| T5.2 | Update screening task command | Screening | ⭐ | Similar to status update handlers |
| T5.3 | Schedule interview command | Interview | ⭐⭐ | New Command + Handler + Endpoint |
| T5.4 | Interview evaluation command | Interview | ⭐⭐ | New Command + Handler + Endpoint |
| T5.5 | Admin client: Screening task UI | Screening | ⭐⭐ | Angular form in existing app detail page |
| T5.6 | Admin client: Schedule interview UI | Interview | ⭐⭐ | Angular form + date picker |
| T7.1 | Medical examination controller | Medical | ⭐⭐ | New controller, 3 endpoints |
| T7.3 | Onboarding controller (guided) | Onboarding | ⭐⭐ | New controller, auto-task creation |
| T7.4 | Admin client: Medical/Offer/Onboarding views | UI | ⭐⭐ | New tabs on application detail |
| T8.1 | Status history writer | Cross-cutting | ⭐ | Simple insert on every status change |
| T8.2 | Audit log writer | Cross-cutting | ⭐ | Simple insert on recruiter edits |
| T8.3 | Email notification sends | Notifications | ⭐⭐ | Implementation once Senior designs interface |
| T9.1 | Add RMS roles to UserRole enum | Admin Client | ⭐ | 5-minute fix |
| T9.2 | Update route guards in admin client | Admin Client | ⭐ | Small change |
| T9.3 | Wire recruitment overview/KPIs | Admin Client | ⭐⭐ | HTTP calls + display |
| T11.1 | Withdraw application button (candidate portal) | Candidate UI | ⭐ | One button + confirmation dialog |
| T11.2 | Show match explanation to candidate | Candidate UI | ⭐ | Display existing MatchResult data |
| T11.3 | Change password UI page | Candidate UI | ⭐ | Reactive form + existing service |
| T11.4 | Profile photo upload | Candidate UI | ⭐⭐ | File input + upload endpoint |

**Difficulty key:** ⭐ = straightforward copy/adapt | ⭐⭐ = requires understanding + some design decisions | ⭐⭐⭐ = complex logic

---

## Recommended Sprint Order

### Sprint 1 — Make It Useable End-to-End (NOW)
1. ✅ Fix profile creation track (Dev 1 — in progress)
2. 🔴 T2.1 + T2.2 — AI Matching Engine (most impactful missing feature)
3. T5.1–T5.4 — Screening + Interview commands (enables full pipeline to advance)
4. T9.1 + T9.2 — Fix roles in admin client (unlock correct access)

### Sprint 2 — Recruiter Workflow Complete
1. T3.1 + T3.2 — OCR verification
2. T4.1–T4.3 — Vacancy approval workflow
3. T5.5 + T5.6 — Admin client screening/interview UI
4. T6.1 + T6.2 — Hangfire setup
5. T8.1 + T8.2 — Status history + audit logging

### Sprint 3 — Late Pipeline
1. T7.1–T7.4 — Medical, Offer (4-step approval), Onboarding
2. T10.1 — Security report PDF
3. T8.3 — Notifications
4. T9.3 — Recruitment dashboard KPIs

### Sprint 4 — Polish & Candidate UX
1. T11.1–T11.5 — Candidate portal enhancements
2. Production config (prod URLs, file storage, secrets)
3. End-to-end testing with Claude API key

---

## AI Usage Notes (Claude API Key)

The project already has `ClaudeAiService` fully implemented. The API key should be set in `appsettings.Development.json`:

```json
"Claude": {
  "ApiKey": "YOUR_KEY_HERE",
  "Model": "claude-sonnet-4-20250514",
  "MaxTokens": 4096
}
```

### Features that use the Claude API Key:
| Feature | Status | Cost Estimate |
|---------|--------|---------------|
| CV parsing (`parse-cv-preview`) | ✅ Working | ~$0.01–0.03 per CV |
| CV parse + autofill on upload | ✅ Working | ~$0.01–0.03 per CV |
| AI Matching (T2.1) | ❌ Not built yet | ~$0.005–0.01 per match |
| OCR Verification (T3.1) | ❌ Not built yet | ~$0.01–0.02 per document |
| Security Report enrichment | ❌ Not built yet | Minimal |

**For testing:** Use the provided Claude API key in `appsettings.Development.json`. Do NOT commit the key to source control — use User Secrets or environment variables in CI.

---

## Quick Reference — Key File Locations

| What | Where |
|------|-------|
| Claude AI service | `RecruitmentAPI.Infrastructure/.../ClaudeAiService.cs` |
| CV parsing service | `RecruitmentAPI.Application/.../CvParsingService.cs` |
| Application create handler | `RecruitmentAPI.Application/Commands/Applications/CreateApplicationCommandHandler.cs` |
| Document upload handler | `RecruitmentAPI.Application/Commands/Documents/UploadDocumentCommandHandler.cs` |
| All interfaces (Angular) | `RecruitmentUI/src/app/shared/interfaces/models.ts` |
| Profile component (wizard) | `RecruitmentUI/src/app/pages/candidate/profile/profile.component.ts` |
| Recruiter management (admin) | `ucic_admin_client/src/app/recruiter-management/` |
| Role enum (admin client) | `ucic_admin_client/src/app/services/role.service.ts` |
| UCIC API roles seeded | `ucic_api/Infrastructure/Data/DataSeeder.cs` |
