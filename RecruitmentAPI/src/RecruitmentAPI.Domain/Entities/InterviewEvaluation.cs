using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using RecruitmentAPI.Domain.Enums;

namespace RecruitmentAPI.Domain.Entities;

[Table("InterviewEvaluations")]
public class InterviewEvaluation : BaseEntity
{
    [Key]
    public long InterviewEvaluationId { get; set; }

    public long InterviewId { get; set; }

    public long ApplicationId { get; set; }

    public EvaluationType EvaluationType { get; set; }

    [MaxLength(450)]
    public string? EvaluatorUserId { get; set; }

    [MaxLength(200)]
    public string? EvaluatorName { get; set; }

    [Column(TypeName = "decimal(4,2)")]
    public decimal? TechnicalScore { get; set; }

    [Column(TypeName = "decimal(4,2)")]
    public decimal? CommunicationScore { get; set; }

    [Column(TypeName = "decimal(4,2)")]
    public decimal? ProblemSolvingScore { get; set; }

    [Column(TypeName = "decimal(4,2)")]
    public decimal? LeadershipScore { get; set; }

    [Column(TypeName = "decimal(4,2)")]
    public decimal? CulturalFitScore { get; set; }

    [Column(TypeName = "decimal(4,2)")]
    public decimal? OverallScore { get; set; }

    public string? Strengths { get; set; }

    public string? Weaknesses { get; set; }

    public string? Notes { get; set; }

    public Recommendation Recommendation { get; set; } = Recommendation.Neutral;

    public EvaluationDecision Decision { get; set; } = EvaluationDecision.Pending;

    [MaxLength(500)]
    public string? RejectionReason { get; set; }

    public DateTime EvaluationDate { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(InterviewId))]
    public Interview Interview { get; set; } = null!;
}
