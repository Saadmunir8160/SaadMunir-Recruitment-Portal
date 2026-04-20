using Microsoft.EntityFrameworkCore;
using RecruitmentAPI.Domain.Entities;
using RecruitmentAPI.Domain.Entities.Lookups;

namespace RecruitmentAPI.Infrastructure.Data;

public class RecruitmentDbContext : DbContext
{
    public RecruitmentDbContext(DbContextOptions<RecruitmentDbContext> options) : base(options) { }

    // Phase 1 — Candidate
    public DbSet<Candidate> Candidates => Set<Candidate>();
    public DbSet<CandidateEducation> CandidateEducations => Set<CandidateEducation>();
    public DbSet<CandidateExperience> CandidateExperiences => Set<CandidateExperience>();
    public DbSet<CandidateDocument> CandidateDocuments => Set<CandidateDocument>();
    public DbSet<OcrVerificationResult> OcrVerificationResults => Set<OcrVerificationResult>();

    // Phase 3 — Vacancy
    public DbSet<Vacancy> Vacancies => Set<Vacancy>();
    public DbSet<VacancyApproval> VacancyApprovals => Set<VacancyApproval>();
    public DbSet<VacancyRecruiter> VacancyRecruiters => Set<VacancyRecruiter>();

    // Phase 4-13 — Application lifecycle
    public DbSet<Domain.Entities.Application> Applications => Set<Domain.Entities.Application>();
    public DbSet<MatchResult> MatchResults => Set<MatchResult>();
    public DbSet<ScreeningTask> ScreeningTasks => Set<ScreeningTask>();
    public DbSet<Interview> Interviews => Set<Interview>();
    public DbSet<InterviewEvaluation> InterviewEvaluations => Set<InterviewEvaluation>();
    public DbSet<MedicalExamination> MedicalExaminations => Set<MedicalExamination>();
    public DbSet<JobOffer> JobOffers => Set<JobOffer>();
    public DbSet<OfferApproval> OfferApprovals => Set<OfferApproval>();
    public DbSet<Onboarding> Onboardings => Set<Onboarding>();
    public DbSet<OnboardingTask> OnboardingTasks => Set<OnboardingTask>();

    // Cross-cutting
    public DbSet<StatusHistory> StatusHistories => Set<StatusHistory>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    // Lookups
    public DbSet<Nationality> Nationalities => Set<Nationality>();
    public DbSet<Country> Countries => Set<Country>();
    public DbSet<Region> Regions => Set<Region>();
    public DbSet<City> Cities => Set<City>();
    public DbSet<District> Districts => Set<District>();
    public DbSet<DistrictCode> DistrictCodes => Set<DistrictCode>();
    public DbSet<Degree> Degrees => Set<Degree>();
    public DbSet<Certificate> Certificates => Set<Certificate>();
    public DbSet<MajorFieldOfStudy> MajorFieldsOfStudy => Set<MajorFieldOfStudy>();
    public DbSet<Institution> Institutions => Set<Institution>();
    public DbSet<Currency> Currencies => Set<Currency>();
    public DbSet<QualificationType> QualificationTypes => Set<QualificationType>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Candidate indexes
        modelBuilder.Entity<Candidate>(entity =>
        {
            entity.HasIndex(c => c.UserId).IsUnique();
            entity.HasIndex(c => c.NationalId).IsUnique().HasFilter("[NationalId] IS NOT NULL");
            entity.HasIndex(c => c.Email);
            entity.HasIndex(c => c.ProfileStatus);
            entity.HasIndex(c => c.Nationality);
        });

        // CandidateEducation indexes
        modelBuilder.Entity<CandidateEducation>(entity =>
        {
            entity.HasIndex(e => e.CandidateId);
            entity.HasIndex(e => e.Major);
        });

        // CandidateExperience indexes
        modelBuilder.Entity<CandidateExperience>(entity =>
        {
            entity.HasIndex(e => e.CandidateId);
        });

        // CandidateDocument indexes
        modelBuilder.Entity<CandidateDocument>(entity =>
        {
            entity.HasIndex(d => d.CandidateId);
            entity.HasIndex(d => d.DocumentType);
        });

        // OcrVerificationResult — restrict cascade on CandidateId to avoid multiple cascade paths
        // (CandidateDocument already cascades from Candidate to OcrVerificationResults)
        modelBuilder.Entity<OcrVerificationResult>(entity =>
        {
            entity.HasIndex(ocr => ocr.CandidateId);
            entity.HasIndex(ocr => ocr.CandidateDocumentId);
            entity.HasOne(ocr => ocr.Candidate)
                .WithMany()
                .HasForeignKey(ocr => ocr.CandidateId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Vacancy indexes
        modelBuilder.Entity<Vacancy>(entity =>
        {
            entity.HasIndex(v => v.RequisitionNumber).IsUnique();
            entity.HasIndex(v => v.PublishStatus);
            entity.HasIndex(v => v.DepartmentName);
            entity.HasIndex(v => v.ClosingDate);
        });

        // VacancyRecruiter unique constraint
        modelBuilder.Entity<VacancyRecruiter>(entity =>
        {
            entity.HasIndex(vr => new { vr.VacancyId, vr.RecruiterUserId }).IsUnique();
            entity.HasIndex(vr => vr.VacancyId);
            entity.HasIndex(vr => vr.RecruiterUserId);
        });

        // VacancyApproval indexes
        modelBuilder.Entity<VacancyApproval>(entity =>
        {
            entity.HasIndex(va => va.VacancyId);
        });

        // Application indexes and unique constraint
        modelBuilder.Entity<Domain.Entities.Application>(entity =>
        {
            entity.HasIndex(a => new { a.CandidateId, a.VacancyId }).IsUnique();
            entity.HasIndex(a => a.CandidateId);
            entity.HasIndex(a => a.VacancyId);
            entity.HasIndex(a => a.Status);
            entity.HasIndex(a => a.AssignedRecruiterId);
        });

        // MatchResult indexes
        modelBuilder.Entity<MatchResult>(entity =>
        {
            entity.HasIndex(m => m.ApplicationId);
            entity.HasIndex(m => m.OverallScore).IsDescending();
        });

        // Interview indexes
        modelBuilder.Entity<Interview>(entity =>
        {
            entity.HasIndex(i => i.ApplicationId);
            entity.HasIndex(i => i.ScheduledDate);
            entity.HasIndex(i => i.InterviewerUserId);
        });

        // JobOffer - one-to-one with Application
        modelBuilder.Entity<JobOffer>(entity =>
        {
            entity.HasOne(jo => jo.Application)
                .WithOne(a => a.JobOffer)
                .HasForeignKey<JobOffer>(jo => jo.ApplicationId);
        });

        // OfferApproval indexes
        modelBuilder.Entity<OfferApproval>(entity =>
        {
            entity.HasIndex(oa => oa.JobOfferId);
        });

        // Onboarding - one-to-one with Application
        modelBuilder.Entity<Onboarding>(entity =>
        {
            entity.HasOne(o => o.Application)
                .WithOne(a => a.Onboarding)
                .HasForeignKey<Onboarding>(o => o.ApplicationId);

            entity.HasOne(o => o.JobOffer)
                .WithOne(jo => jo.Onboarding)
                .HasForeignKey<Onboarding>(o => o.JobOfferId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // OnboardingTask indexes
        modelBuilder.Entity<OnboardingTask>(entity =>
        {
            entity.HasIndex(ot => ot.OnboardingId);
        });

        // MedicalExamination - one-to-one with Application
        modelBuilder.Entity<MedicalExamination>(entity =>
        {
            entity.HasOne(me => me.Application)
                .WithOne(a => a.MedicalExamination)
                .HasForeignKey<MedicalExamination>(me => me.ApplicationId);
        });

        // MatchResult - one-to-one with Application
        modelBuilder.Entity<MatchResult>(entity =>
        {
            entity.HasOne(mr => mr.Application)
                .WithOne(a => a.MatchResult)
                .HasForeignKey<MatchResult>(mr => mr.ApplicationId);
        });

        // StatusHistory indexes
        modelBuilder.Entity<StatusHistory>(entity =>
        {
            entity.HasIndex(sh => new { sh.EntityType, sh.EntityId });
            entity.HasIndex(sh => sh.ChangedDate).IsDescending();
        });

        // Notification indexes
        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasIndex(n => n.RecipientUserId);
            entity.HasIndex(n => n.Status);
        });

        // AuditLog indexes
        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasIndex(al => new { al.EntityType, al.EntityId });
            entity.HasIndex(al => al.UserId);
            entity.HasIndex(al => al.Timestamp).IsDescending();
        });
    }
}
