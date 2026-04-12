using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using RecruitmentAPI.Domain.Enums;

namespace RecruitmentAPI.Domain.Entities;

[Table("Vacancies")]
public class Vacancy : BaseEntity
{
    [Key]
    public long VacancyId { get; set; }

    [Required, MaxLength(50)]
    public string RequisitionNumber { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    public string JobTitle { get; set; } = string.Empty;

    public int? DepartmentId { get; set; }

    [MaxLength(100)]
    public string? DepartmentName { get; set; }

    [MaxLength(200)]
    public string? Location { get; set; }

    [Column(TypeName = "decimal(9,6)")]
    public decimal? LocationLatitude { get; set; }

    [Column(TypeName = "decimal(9,6)")]
    public decimal? LocationLongitude { get; set; }

    [MaxLength(50)]
    public string? JobGrade { get; set; }

    public int NumberOfPositions { get; set; } = 1;

    public int FilledPositions { get; set; } = 0;

    public string? JobDescription { get; set; }

    public string? Qualifications { get; set; }

    public string? Requirements { get; set; }

    public string? RequiredDocuments { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? SalaryRangeMin { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? SalaryRangeMax { get; set; }

    [MaxLength(10)]
    public string Currency { get; set; } = "SAR";

    public WorkType WorkType { get; set; } = WorkType.FullTime;

    public WorkLocation WorkLocation { get; set; } = WorkLocation.OnSite;

    public int? RequiredExperienceMin { get; set; }

    public int? RequiredExperienceMax { get; set; }

    [MaxLength(100)]
    public string? RequiredNationality { get; set; }

    [MaxLength(200)]
    public string? RequiredSpecialization { get; set; }

    [MaxLength(200)]
    public string? RequiredQualification { get; set; }

    public string? RequiredCertifications { get; set; }

    public string? RequiredSkills { get; set; }

    public int WeightSpecialization { get; set; } = 25;

    public int WeightExperience { get; set; } = 20;

    public int WeightQualification { get; set; } = 20;

    public int WeightNationality { get; set; } = 10;

    public int WeightLocation { get; set; } = 15;

    public int WeightCertifications { get; set; } = 10;

    public int MatchThreshold { get; set; } = 60;

    public VacancyPublishStatus PublishStatus { get; set; } = VacancyPublishStatus.Draft;

    public DateTime? PublishedDate { get; set; }

    public DateTime? ClosingDate { get; set; }

    public DateTime? PausedDate { get; set; }

    [MaxLength(450)]
    public string? PausedByUserId { get; set; }

    [MaxLength(450)]
    public string? CreatedByUserId { get; set; }

    // Navigation
    public ICollection<VacancyApproval> Approvals { get; set; } = [];
    public ICollection<VacancyRecruiter> Recruiters { get; set; } = [];
    public ICollection<Application> Applications { get; set; } = [];
}
