using RecruitmentAPI.Domain.Enums;

namespace RecruitmentAPI.Application.DTOs.ScreeningTasks;

public class CreateScreeningTaskDto
{
    public long ApplicationId { get; set; }
    public string? AssignedToUserId { get; set; }
    public string? AssignedToName { get; set; }
    public string? TaskDescription { get; set; }
    public DateTime? Deadline { get; set; }
    public string? Notes { get; set; }
}

public class UpdateScreeningTaskDto
{
    public long ScreeningTaskId { get; set; }
    public string? AssignedToUserId { get; set; }
    public string? AssignedToName { get; set; }
    public string? TaskDescription { get; set; }
    public DateTime? Deadline { get; set; }
    public string? CandidateAvailability { get; set; }
    public decimal? ExpectedSalary { get; set; }
    public string SalaryCurrency { get; set; } = "SAR";
    public string? PreferredLocation { get; set; }
    public bool? WillingnessToRelocate { get; set; }
    public int? NoticePeriodDays { get; set; }
    public ScreeningTaskStatus? Status { get; set; }
    public string? Notes { get; set; }
    public DateTime? CompletedDate { get; set; }
}
