using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using RecruitmentAPI.Domain.Enums;

namespace RecruitmentAPI.Domain.Entities;

[Table("JobOffers")]
public class JobOffer : BaseEntity
{
    [Key]
    public long JobOfferId { get; set; }

    public long ApplicationId { get; set; }

    public long CandidateId { get; set; }

    public long VacancyId { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? ProposedSalary { get; set; }

    [MaxLength(10)]
    public string Currency { get; set; } = "SAR";

    public string? Benefits { get; set; }

    [MaxLength(50)]
    public string? OfferTemplateType { get; set; }

    [MaxLength(50)]
    public string? JobGrade { get; set; }

    [MaxLength(500)]
    public string? OfferLetterPath { get; set; }

    [MaxLength(50)]
    public string ESignatureProvider { get; set; } = "DocuSign";

    [MaxLength(200)]
    public string? ESignatureRequestId { get; set; }

    [MaxLength(500)]
    public string? SignedOfferPath { get; set; }

    public DateTime? SignedDate { get; set; }

    public OfferStatus Status { get; set; } = OfferStatus.Draft;

    public byte CurrentApprovalStep { get; set; } = 1;

    public DateTime? SentDate { get; set; }

    public SentVia? SentVia { get; set; }

    public DateOnly? ExpectedJoiningDate { get; set; }

    public DateOnly? OfferExpiryDate { get; set; }

    public int OfferExpiryDays { get; set; } = 12;

    public DateTime? LastReminderSentDate { get; set; }

    public int ReminderCount { get; set; } = 0;

    [ForeignKey(nameof(ApplicationId))]
    public Application Application { get; set; } = null!;

    public ICollection<OfferApproval> Approvals { get; set; } = [];
    public Onboarding? Onboarding { get; set; }
}
