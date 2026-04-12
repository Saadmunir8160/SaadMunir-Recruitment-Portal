using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using RecruitmentAPI.Domain.Enums;

namespace RecruitmentAPI.Domain.Entities;

[Table("CandidateEducations")]
public class CandidateEducation : BaseEntity
{
    [Key]
    public long CandidateEducationId { get; set; }

    public long CandidateId { get; set; }

    [Required, MaxLength(200)]
    public string Qualification { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? Major { get; set; }

    [MaxLength(300)]
    public string? Institution { get; set; }

    public int? GraduationYear { get; set; }

    [MaxLength(50)]
    public string? GradeOrGPA { get; set; }

    [MaxLength(100)]
    public string? Country { get; set; }

    public DataSource DataSource { get; set; } = DataSource.ManualEntry;

    // Navigation
    [ForeignKey(nameof(CandidateId))]
    public Candidate Candidate { get; set; } = null!;
}
