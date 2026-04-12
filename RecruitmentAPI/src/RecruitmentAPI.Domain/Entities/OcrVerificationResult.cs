using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using RecruitmentAPI.Domain.Enums;

namespace RecruitmentAPI.Domain.Entities;

[Table("OcrVerificationResults")]
public class OcrVerificationResult : BaseEntity
{
    [Key]
    public long OcrVerificationId { get; set; }

    public long CandidateDocumentId { get; set; }

    public long CandidateId { get; set; }

    [Required, MaxLength(100)]
    public string FieldName { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? ExtractedValue { get; set; }

    [MaxLength(500)]
    public string? EnteredValue { get; set; }

    public bool? IsMatch { get; set; }

    [Column(TypeName = "decimal(5,2)")]
    public decimal? ConfidenceScore { get; set; }

    public MismatchSeverity MismatchSeverity { get; set; } = MismatchSeverity.None;

    public DateTime ProcessedDate { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(CandidateDocumentId))]
    public CandidateDocument CandidateDocument { get; set; } = null!;

    [ForeignKey(nameof(CandidateId))]
    public Candidate Candidate { get; set; } = null!;
}
