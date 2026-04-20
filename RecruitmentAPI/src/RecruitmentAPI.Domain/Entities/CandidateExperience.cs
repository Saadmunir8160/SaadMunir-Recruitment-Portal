using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using RecruitmentAPI.Domain.Enums;

namespace RecruitmentAPI.Domain.Entities;

[Table("CandidateExperiences")]
public class CandidateExperience : BaseEntity
{
    [Key]
    public long CandidateExperienceId { get; set; }

    public long CandidateId { get; set; }

    [MaxLength(300)]
    public string? Employer { get; set; }

    [MaxLength(200)]
    public string? JobTitle { get; set; }

    public DateOnly? StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public bool IsCurrent { get; set; } = false;

    [Column(TypeName = "decimal(18,2)")]
    public decimal? Salary { get; set; }

    // --- UPDATED FOR LOOKUP RESOLVER ---
    public long? CurrencyId { get; set; } 

    [MaxLength(10)]
    public string Currency { get; set; } = "SAR";

    public string? Description { get; set; }

    // --- UPDATED FOR LOOKUP RESOLVER ---
    public long? CountryId { get; set; }

    [MaxLength(100)]
    public string? Country { get; set; }

    public DataSource DataSource { get; set; } = DataSource.ManualEntry;

    [ForeignKey(nameof(CandidateId))]
    public Candidate Candidate { get; set; } = null!;
}