using RecruitmentAPI.Domain.Enums;

namespace RecruitmentAPI.Application.DTOs.Vacancies;

public class VacancyDto
{
    public long VacancyId { get; set; }
    public string RequisitionNumber { get; set; } = string.Empty;
    public string JobTitle { get; set; } = string.Empty;
    public string? DepartmentName { get; set; }
    public string? Location { get; set; }
    public string? JobGrade { get; set; }
    public int NumberOfPositions { get; set; }
    public int FilledPositions { get; set; }
    public WorkType WorkType { get; set; }
    public WorkLocation WorkLocation { get; set; }
    public decimal? SalaryRangeMin { get; set; }
    public decimal? SalaryRangeMax { get; set; }
    public string Currency { get; set; } = "SAR";
    public VacancyPublishStatus PublishStatus { get; set; }
    public DateTime? PublishedDate { get; set; }
    public DateTime? ClosingDate { get; set; }
    public DateTime CreatedDate { get; set; }
}

public class VacancyDetailDto : VacancyDto
{
    public string? JobDescription { get; set; }
    public string? Qualifications { get; set; }
    public string? Requirements { get; set; }
    public string? RequiredDocuments { get; set; }
    public int? RequiredExperienceMin { get; set; }
    public int? RequiredExperienceMax { get; set; }
    public string? RequiredNationality { get; set; }
    public string? RequiredSpecialization { get; set; }
    public string? RequiredQualification { get; set; }
    public string? RequiredCertifications { get; set; }
    public string? RequiredSkills { get; set; }
    public int WeightSpecialization { get; set; }
    public int WeightExperience { get; set; }
    public int WeightQualification { get; set; }
    public int WeightNationality { get; set; }
    public int WeightLocation { get; set; }
    public int WeightCertifications { get; set; }
    public int MatchThreshold { get; set; }

    /// <summary>When status is PendingApproval, the current workflow step (1 = HR Manager, 2 = HR Section Head).</summary>
    public byte? PendingApprovalStep { get; set; }

    public string? PendingApprovalStepName { get; set; }

    public List<VacancyRecruiterDto> Recruiters { get; set; } = [];
}

public class VacancyRecruiterDto
{
    public long VacancyRecruiterId { get; set; }
    public string RecruiterUserId { get; set; } = string.Empty;
    public string? RecruiterName { get; set; }
    public bool IsPrimary { get; set; }
}

public class CreateVacancyDto
{
    public string JobTitle { get; set; } = string.Empty;
    public int? DepartmentId { get; set; }
    public string? DepartmentName { get; set; }
    public string? Location { get; set; }
    public string? JobGrade { get; set; }
    public int NumberOfPositions { get; set; } = 1;
    public string? JobDescription { get; set; }
    public string? Qualifications { get; set; }
    public string? Requirements { get; set; }
    public string? RequiredDocuments { get; set; }
    public decimal? SalaryRangeMin { get; set; }
    public decimal? SalaryRangeMax { get; set; }
    public WorkType WorkType { get; set; }
    public WorkLocation WorkLocation { get; set; }
    public int? RequiredExperienceMin { get; set; }
    public int? RequiredExperienceMax { get; set; }
    public string? RequiredNationality { get; set; }
    public string? RequiredSpecialization { get; set; }
    public string? RequiredQualification { get; set; }
    public string? RequiredCertifications { get; set; }
    public string? RequiredSkills { get; set; }
    public DateTime? ClosingDate { get; set; }
}
