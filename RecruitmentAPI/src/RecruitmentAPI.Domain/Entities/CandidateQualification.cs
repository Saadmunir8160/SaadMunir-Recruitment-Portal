using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using RecruitmentAPI.Domain.Entities.Lookups;
using RecruitmentAPI.Domain.Enums;

namespace RecruitmentAPI.Domain.Entities;

/// <summary>
/// Maps a candidate's qualifications to the normalized lookup tables.
/// Each row links a CandidateEducation record (if applicable) and/or directly
/// references lookup IDs for Degree/Certificate, Major, and Institution.
/// </summary>
[Table("CandidateQualifications")]
public class CandidateQualification : BaseEntity
{
    [Key]
    public long CandidateQualificationId { get; set; }

    public long CandidateId { get; set; }

    /// <summary>Optional link back to the CandidateEducation row this was derived from.</summary>
    public long? CandidateEducationId { get; set; }

    public int QualificationTypeId { get; set; }

    /// <summary>Populated when QualificationType = Degree</summary>
    public int? DegreeId { get; set; }

    /// <summary>Populated when QualificationType = Certificate</summary>
    public int? CertificateId { get; set; }

    public int? MajorFieldOfStudyId { get; set; }

    public int? InstitutionId { get; set; }

    public int? CountryId { get; set; }

    public int? GraduationYear { get; set; }

    [MaxLength(50)]
    public string? GradeOrGPA { get; set; }

    public DataSource DataSource { get; set; } = DataSource.ManualEntry;

    // Navigation
    [ForeignKey(nameof(CandidateId))]
    public Candidate Candidate { get; set; } = null!;

    [ForeignKey(nameof(CandidateEducationId))]
    public CandidateEducation? CandidateEducation { get; set; }

    [ForeignKey(nameof(QualificationTypeId))]
    public QualificationType QualificationType { get; set; } = null!;

    [ForeignKey(nameof(DegreeId))]
    public Degree? Degree { get; set; }

    [ForeignKey(nameof(CertificateId))]
    public Certificate? Certificate { get; set; }

    [ForeignKey(nameof(MajorFieldOfStudyId))]
    public MajorFieldOfStudy? MajorFieldOfStudy { get; set; }

    [ForeignKey(nameof(InstitutionId))]
    public Institution? Institution { get; set; }

    [ForeignKey(nameof(CountryId))]
    public Country? Country { get; set; }
}
