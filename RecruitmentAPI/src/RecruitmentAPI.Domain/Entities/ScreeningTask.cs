using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using RecruitmentAPI.Domain.Enums;

namespace RecruitmentAPI.Domain.Entities;

[Table("ScreeningTasks")]
public class ScreeningTask : BaseEntity
{
    [Key]
    public long ScreeningTaskId { get; set; }

    public long ApplicationId { get; set; }

    [MaxLength(450)]
    public string? AssignedToUserId { get; set; }

    [MaxLength(200)]
    public string? AssignedToName { get; set; }

    public string? TaskDescription { get; set; }

    public DateTime? Deadline { get; set; }

    [MaxLength(200)]
    public string? CandidateAvailability { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? ExpectedSalary { get; set; }

    [MaxLength(10)]
    public string SalaryCurrency { get; set; } = "SAR";

    [MaxLength(200)]
    public string? PreferredLocation { get; set; }

    public bool? WillingnessToRelocate { get; set; }

    public int? NoticePeriodDays { get; set; }

    public ScreeningTaskStatus Status { get; set; } = ScreeningTaskStatus.Pending;

    public string? Notes { get; set; }

    public DateTime? CompletedDate { get; set; }

    [ForeignKey(nameof(ApplicationId))]
    public Application Application { get; set; } = null!;
}
