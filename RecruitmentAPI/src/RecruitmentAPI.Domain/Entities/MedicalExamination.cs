using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using RecruitmentAPI.Domain.Enums;

namespace RecruitmentAPI.Domain.Entities;

[Table("MedicalExaminations")]
public class MedicalExamination : BaseEntity
{
    [Key]
    public long MedicalExaminationId { get; set; }

    public long ApplicationId { get; set; }

    public long CandidateId { get; set; }

    [MaxLength(300)]
    public string? HospitalName { get; set; }

    [MaxLength(100)]
    public string? ReferenceNumber { get; set; }

    [MaxLength(300)]
    public string? ContactDetails { get; set; }

    public DateTime RequestDate { get; set; } = DateTime.UtcNow;

    public DateTime? ExaminationDate { get; set; }

    [MaxLength(500)]
    public string? ReportPath { get; set; }

    [MaxLength(450)]
    public string? ReportUploadedByUserId { get; set; }

    public DateTime? ReportUploadedDate { get; set; }

    public ClearanceStatus ClearanceStatus { get; set; } = ClearanceStatus.Pending;

    public string? ConditionNotes { get; set; }

    [MaxLength(450)]
    public string? ReviewedByUserId { get; set; }

    public DateTime? ReviewDate { get; set; }

    [ForeignKey(nameof(ApplicationId))]
    public Application Application { get; set; } = null!;
}
