using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RecruitmentAPI.Domain.Entities;

[Table("MatchResults")]
public class MatchResult : BaseEntity
{
    [Key]
    public long MatchResultId { get; set; }

    public long ApplicationId { get; set; }

    public long CandidateId { get; set; }

    public long VacancyId { get; set; }

    [Column(TypeName = "decimal(5,2)")]
    public decimal? SpecializationScore { get; set; }

    [Column(TypeName = "decimal(5,2)")]
    public decimal? ExperienceScore { get; set; }

    [Column(TypeName = "decimal(5,2)")]
    public decimal? QualificationScore { get; set; }

    [Column(TypeName = "decimal(5,2)")]
    public decimal? NationalityScore { get; set; }

    [Column(TypeName = "decimal(5,2)")]
    public decimal? LocationScore { get; set; }

    [Column(TypeName = "decimal(5,2)")]
    public decimal? CertificationScore { get; set; }

    [Required, Column(TypeName = "decimal(5,2)")]
    public decimal OverallScore { get; set; }

    public bool IsMatch { get; set; }

    public string? MatchExplanation { get; set; }

    public string? MatchDetailsJson { get; set; }

    public DateTime ProcessedDate { get; set; } = DateTime.UtcNow;

    public int? ProcessingTimeMs { get; set; }

    [MaxLength(50)]
    public string? AiModel { get; set; }

    [ForeignKey(nameof(ApplicationId))]
    public Application Application { get; set; } = null!;
}
