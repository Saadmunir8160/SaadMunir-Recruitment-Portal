using RecruitmentAPI.Application.DTOs.Documents;
using RecruitmentAPI.Domain.Enums;

namespace RecruitmentAPI.Application.DTOs.Candidates;

public class CandidateDto
{
    public long CandidateId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? NationalId { get; set; }
    public IdType IdType { get; set; }
    public Gender? Gender { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public string? Nationality { get; set; }
    public string MobileNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? NationalAddress { get; set; }
    public string? ResidenceCity { get; set; }
    public CandidateProfileStatus ProfileStatus { get; set; }
    public bool IsProfileLocked { get; set; }
    public string? ProfilePhotoPath { get; set; }
    public string? CvSummary { get; set; }
    public DateTime CreatedDate { get; set; }
}

public class CandidateDetailDto : CandidateDto
{
    public List<CandidateEducationDto> Educations { get; set; } = [];
    public List<CandidateExperienceDto> Experiences { get; set; } = [];
    public List<Documents.CandidateDocumentDto> Documents { get; set; } = [];
}

public class CandidateEducationDto
{
    public long CandidateEducationId { get; set; }
    public string Qualification { get; set; } = string.Empty;
    public string? Major { get; set; }
    public string? Institution { get; set; }
    public int? GraduationYear { get; set; }
    public string? GradeOrGPA { get; set; }
    public string? Country { get; set; }
}

public class CandidateExperienceDto
{
    public long CandidateExperienceId { get; set; }
    public string? Employer { get; set; }
    public string? JobTitle { get; set; }
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public bool IsCurrent { get; set; }
    public decimal? Salary { get; set; }
    public string? Description { get; set; }
    public string? Country { get; set; }
}

public class CreateCandidateDto
{
    public string FullName { get; set; } = string.Empty;
    public string? NationalId { get; set; }
    public IdType IdType { get; set; }
    public Gender? Gender { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public string? Nationality { get; set; }
    public string MobileNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? NationalAddress { get; set; }
    public string? ResidenceCity { get; set; }
}

public class UpdateCandidateDto
{
    public string FullName { get; set; } = string.Empty;
    public string? NationalId { get; set; }
    public IdType IdType { get; set; }
    public Gender? Gender { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public string? Nationality { get; set; }
    public string MobileNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? NationalAddress { get; set; }
    public string? ResidenceCity { get; set; }
    public decimal? ResidenceLatitude { get; set; }
    public decimal? ResidenceLongitude { get; set; }
}

public class UpdateCandidateStatusDto
{
    public CandidateProfileStatus NewStatus { get; set; }
    public string? Reason { get; set; }
}
