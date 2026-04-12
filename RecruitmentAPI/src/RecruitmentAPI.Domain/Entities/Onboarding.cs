using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using RecruitmentAPI.Domain.Enums;

namespace RecruitmentAPI.Domain.Entities;

[Table("Onboardings")]
public class Onboarding : BaseEntity
{
    [Key]
    public long OnboardingId { get; set; }

    public long ApplicationId { get; set; }

    public long CandidateId { get; set; }

    public long JobOfferId { get; set; }

    [MaxLength(50)]
    public string? EmployeeId { get; set; }

    [MaxLength(500)]
    public string? ContractPath { get; set; }

    public bool InsuranceEnrolled { get; set; } = false;

    public string? InsuranceDetails { get; set; }

    public DateTime? InsuranceEnrolledDate { get; set; }

    public DateTime? OrientationDate { get; set; }

    public bool OrientationCompleted { get; set; } = false;

    public DateTime? ActivationDate { get; set; }

    public OnboardingStatus Status { get; set; } = OnboardingStatus.Pending;

    public EmployeeStatus EmployeeStatus { get; set; } = EmployeeStatus.Pending;

    public HcmSyncStatus HcmSyncStatus { get; set; } = HcmSyncStatus.Pending;

    [MaxLength(100)]
    public string? HcmEmployeeId { get; set; }

    public DateTime? HcmSyncDate { get; set; }

    [ForeignKey(nameof(ApplicationId))]
    public Application Application { get; set; } = null!;

    [ForeignKey(nameof(JobOfferId))]
    public JobOffer JobOffer { get; set; } = null!;

    public ICollection<OnboardingTask> Tasks { get; set; } = [];
}
