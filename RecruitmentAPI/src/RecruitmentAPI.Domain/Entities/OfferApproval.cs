using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using RecruitmentAPI.Domain.Enums;

namespace RecruitmentAPI.Domain.Entities;

[Table("OfferApprovals")]
public class OfferApproval : BaseEntity
{
    [Key]
    public long OfferApprovalId { get; set; }

    public long JobOfferId { get; set; }

    public byte ApprovalStep { get; set; }

    [MaxLength(100)]
    public string? ApprovalStepName { get; set; }

    [MaxLength(450)]
    public string? ApproverUserId { get; set; }

    [MaxLength(200)]
    public string? ApproverName { get; set; }

    public ApprovalStatus Status { get; set; } = ApprovalStatus.Pending;

    public string? Comments { get; set; }

    public DateTime? ActionDate { get; set; }

    [ForeignKey(nameof(JobOfferId))]
    public JobOffer JobOffer { get; set; } = null!;
}
