using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using RecruitmentAPI.Domain.Enums;

namespace RecruitmentAPI.Domain.Entities;

[Table("OnboardingTasks")]
public class OnboardingTask : BaseEntity
{
    [Key]
    public long OnboardingTaskId { get; set; }

    public long OnboardingId { get; set; }

    [Required, MaxLength(200)]
    public string TaskName { get; set; } = string.Empty;

    public string? TaskDescription { get; set; }

    [MaxLength(100)]
    public string? AssignedDepartment { get; set; }

    [MaxLength(450)]
    public string? AssignedToUserId { get; set; }

    [MaxLength(200)]
    public string? AssignedToName { get; set; }

    public int SortOrder { get; set; } = 0;

    public bool IsMandatory { get; set; } = true;

    public OnboardingTaskStatus Status { get; set; } = OnboardingTaskStatus.Pending;

    public DateTime? CompletedDate { get; set; }

    [MaxLength(450)]
    public string? CompletedByUserId { get; set; }

    public string? Notes { get; set; }

    [ForeignKey(nameof(OnboardingId))]
    public Onboarding Onboarding { get; set; } = null!;
}
