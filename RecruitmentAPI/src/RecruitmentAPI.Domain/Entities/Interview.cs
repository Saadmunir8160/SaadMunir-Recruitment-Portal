using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using RecruitmentAPI.Domain.Enums;

namespace RecruitmentAPI.Domain.Entities;

[Table("Interviews")]
public class Interview : BaseEntity
{
    [Key]
    public long InterviewId { get; set; }

    public long ApplicationId { get; set; }

    public InterviewType InterviewType { get; set; }

    public InterviewMode InterviewMode { get; set; } = InterviewMode.OnSite;

    public DateTime ScheduledDate { get; set; }

    public DateTime? ScheduledEndDate { get; set; }

    [MaxLength(300)]
    public string? Location { get; set; }

    [MaxLength(450)]
    public string? InterviewerUserId { get; set; }

    [MaxLength(200)]
    public string? InterviewerName { get; set; }

    public InterviewStatus Status { get; set; } = InterviewStatus.Scheduled;

    public bool CandidateConfirmed { get; set; } = false;

    public DateTime? ConfirmedDate { get; set; }

    [MaxLength(500)]
    public string? RescheduleReason { get; set; }

    public int RescheduleCount { get; set; } = 0;

    public bool SecurityListGenerated { get; set; } = false;

    [MaxLength(500)]
    public string? SecurityNotes { get; set; }

    [ForeignKey(nameof(ApplicationId))]
    public Application Application { get; set; } = null!;

    public ICollection<InterviewEvaluation> Evaluations { get; set; } = [];
}
