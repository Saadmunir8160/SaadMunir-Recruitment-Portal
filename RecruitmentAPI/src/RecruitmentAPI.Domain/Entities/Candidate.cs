using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using RecruitmentAPI.Domain.Enums;

namespace RecruitmentAPI.Domain.Entities;

[Table("Candidates")]
public class Candidate : BaseEntity
{
    [Key]
    public long CandidateId { get; set; }

    [Required, MaxLength(450)]
    public string UserId { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    public string FullName { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? NationalId { get; set; }

    public IdType IdType { get; set; } = IdType.NationalId;

    public Gender? Gender { get; set; }

    public DateOnly? DateOfBirth { get; set; }

    // --- UPDATED: Lookup IDs for Service Compatibility ---
    public long? NationalityId { get; set; }
    
    [MaxLength(100)]
    public string? Nationality { get; set; } // Kept for name storage if needed

    [Required, MaxLength(50)]
    public string MobileNumber { get; set; } = string.Empty;

    [Required, MaxLength(150)]
    public string Email { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? NationalAddress { get; set; }

    // --- UPDATED: Residence Lookup IDs ---
    public long? ResidenceCountryId { get; set; }
    public long? ResidenceCityId { get; set; }
    public long? ResidenceDistrictId { get; set; }

    [MaxLength(100)]
    public string? ResidenceCity { get; set; }

    [Column(TypeName = "decimal(9,6)")]
    public decimal? ResidenceLatitude { get; set; }

    [Column(TypeName = "decimal(9,6)")]
    public decimal? ResidenceLongitude { get; set; }

    public CandidateProfileStatus ProfileStatus { get; set; } = CandidateProfileStatus.Incomplete;

    public bool IsProfileLocked { get; set; } = false;

    [MaxLength(500)]
    public string? RejectionReason { get; set; }

    public DateTime? RejectionDate { get; set; }

    [MaxLength(450)]
    public string? RejectedByUserId { get; set; }

    public bool ReactivatedFromRejection { get; set; } = false;

    public OcrVerificationStatus OcrVerificationStatus { get; set; } = OcrVerificationStatus.Pending;

    [MaxLength(500)]
    public string? ProfilePhotoPath { get; set; }

    public string? CvSummary { get; set; }

    public string? ParsedCvJson { get; set; }

    // Navigation properties
    public ICollection<CandidateEducation> Educations { get; set; } = [];
    public ICollection<CandidateExperience> Experiences { get; set; } = [];
    public ICollection<CandidateDocument> Documents { get; set; } = [];
    public ICollection<Application> Applications { get; set; } = [];
}