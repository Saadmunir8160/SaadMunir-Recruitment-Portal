using RecruitmentAPI.Domain.Enums;

namespace RecruitmentAPI.Application.DTOs.Interviews;

public class CreateInterviewDto
{
    public long ApplicationId { get; set; }
    public InterviewType InterviewType { get; set; }
    public InterviewMode InterviewMode { get; set; }
    public DateTime ScheduledDate { get; set; }
    public DateTime? ScheduledEndDate { get; set; }
    public string? Location { get; set; }
    public string? InterviewerUserId { get; set; }
    public string? InterviewerName { get; set; }
    public string? SecurityNotes { get; set; }
}

public class CreateInterviewEvaluationDto
{
    public long InterviewId { get; set; }
    public long ApplicationId { get; set; }
    public EvaluationType EvaluationType { get; set; }
    public string? EvaluatorUserId { get; set; }
    public string? EvaluatorName { get; set; }
    public decimal? TechnicalScore { get; set; }
    public decimal? CommunicationScore { get; set; }
    public decimal? ProblemSolvingScore { get; set; }
    public decimal? LeadershipScore { get; set; }
    public decimal? CulturalFitScore { get; set; }
    public decimal? OverallScore { get; set; }
    public string? Strengths { get; set; }
    public string? Weaknesses { get; set; }
    public string? Notes { get; set; }
    public Recommendation Recommendation { get; set; }
    public EvaluationDecision Decision { get; set; }
    public string? RejectionReason { get; set; }
}
