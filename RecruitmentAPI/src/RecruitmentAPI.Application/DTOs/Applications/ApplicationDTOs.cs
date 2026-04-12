using RecruitmentAPI.Domain.Enums;

namespace RecruitmentAPI.Application.DTOs.Applications;

public class ApplicationDto
{
    public long ApplicationId { get; set; }
    public long CandidateId { get; set; }
    public string CandidateName { get; set; } = string.Empty;
    public long VacancyId { get; set; }
    public string VacancyTitle { get; set; } = string.Empty;
    public DateTime ApplicationDate { get; set; }
    public ApplicationStatus Status { get; set; }
    public decimal? MatchScore { get; set; }
    public bool IsAutoMatched { get; set; }
    public string? AssignedRecruiterName { get; set; }
}

public class ApplicationDetailDto : ApplicationDto
{
    public string? RejectionReason { get; set; }
    public string? RejectionPhase { get; set; }
    public MatchResultDto? MatchResult { get; set; }
    public List<ScreeningTaskDto> ScreeningTasks { get; set; } = [];
    public List<InterviewDto> Interviews { get; set; } = [];
}

public class MatchResultDto
{
    public long MatchResultId { get; set; }
    public long ApplicationId { get; set; }
    public decimal? SpecializationScore { get; set; }
    public decimal? ExperienceScore { get; set; }
    public decimal? QualificationScore { get; set; }
    public decimal? NationalityScore { get; set; }
    public decimal? LocationScore { get; set; }
    public decimal? CertificationScore { get; set; }
    public decimal OverallScore { get; set; }
    public bool IsMatch { get; set; }
    public string? MatchExplanation { get; set; }
}

public class ScreeningTaskDto
{
    public long ScreeningTaskId { get; set; }
    public long ApplicationId { get; set; }
    public string? AssignedToName { get; set; }
    public string? TaskDescription { get; set; }
    public DateTime? Deadline { get; set; }
    public ScreeningTaskStatus Status { get; set; }
    public string? Notes { get; set; }
}

public class InterviewDto
{
    public long InterviewId { get; set; }
    public long ApplicationId { get; set; }
    public InterviewType InterviewType { get; set; }
    public InterviewMode InterviewMode { get; set; }
    public DateTime ScheduledDate { get; set; }
    public string? Location { get; set; }
    public string? InterviewerName { get; set; }
    public InterviewStatus Status { get; set; }
    public bool CandidateConfirmed { get; set; }
}

public class InterviewListDto
{
    public long InterviewId { get; set; }
    public long ApplicationId { get; set; }
    public string CandidateName { get; set; } = string.Empty;
    public InterviewType InterviewType { get; set; }
    public InterviewMode InterviewMode { get; set; }
    public DateTime ScheduledDate { get; set; }
    public string? Location { get; set; }
    public string? InterviewerName { get; set; }
    public InterviewStatus Status { get; set; }
    public bool CandidateConfirmed { get; set; }
}

public class CreateApplicationDto
{
    public long VacancyId { get; set; }
}

public class UpdateApplicationStatusDto
{
    public ApplicationStatus NewStatus { get; set; }
    public string? Reason { get; set; }
}
