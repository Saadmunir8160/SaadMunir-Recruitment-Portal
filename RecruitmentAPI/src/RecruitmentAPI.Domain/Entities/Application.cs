using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using RecruitmentAPI.Domain.Enums;

namespace RecruitmentAPI.Domain.Entities;

[Table("Applications")]
public class Application : BaseEntity
{
    [Key]
    public long ApplicationId { get; set; }

    public long CandidateId { get; set; }

    public long VacancyId { get; set; }

    public DateTime ApplicationDate { get; set; } = DateTime.UtcNow;

    public ApplicationStatus Status { get; set; } = ApplicationStatus.Applied;

    [MaxLength(500)]
    public string? RejectionReason { get; set; }

    [MaxLength(100)]
    public string? RejectionPhase { get; set; }

    [MaxLength(450)]
    public string? AssignedRecruiterId { get; set; }

    [MaxLength(200)]
    public string? AssignedRecruiterName { get; set; }

    [Column(TypeName = "decimal(5,2)")]
    public decimal? MatchScore { get; set; }

    public bool IsAutoMatched { get; set; } = false;

    [ForeignKey(nameof(CandidateId))]
    public Candidate Candidate { get; set; } = null!;

    [ForeignKey(nameof(VacancyId))]
    public Vacancy Vacancy { get; set; } = null!;

    // Navigation
    public MatchResult? MatchResult { get; set; }
    public ICollection<ScreeningTask> ScreeningTasks { get; set; } = [];
    public ICollection<Interview> Interviews { get; set; } = [];
    public MedicalExamination? MedicalExamination { get; set; }
    public JobOffer? JobOffer { get; set; }
    public Onboarding? Onboarding { get; set; }
}
