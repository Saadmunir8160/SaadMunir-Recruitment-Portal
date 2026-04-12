using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using RecruitmentAPI.Domain.Enums;

namespace RecruitmentAPI.Domain.Entities;

[Table("CandidateDocuments")]
public class CandidateDocument : BaseEntity
{
    [Key]
    public long CandidateDocumentId { get; set; }

    public long CandidateId { get; set; }

    public DocumentType DocumentType { get; set; }

    [MaxLength(300)]
    public string? FileName { get; set; }

    [Required, MaxLength(500)]
    public string FilePath { get; set; } = string.Empty;

    public long? FileSize { get; set; }

    [MaxLength(100)]
    public string? ContentType { get; set; }

    public AiProcessingStatus AiProcessingStatus { get; set; } = AiProcessingStatus.Pending;

    public DateTime? AiProcessedDate { get; set; }

    [Column(TypeName = "decimal(5,2)")]
    public decimal? AiConfidenceScore { get; set; }

    public string? AiRawResponseJson { get; set; }

    [MaxLength(500)]
    public string? AiErrorMessage { get; set; }

    public DateTime UploadedDate { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(CandidateId))]
    public Candidate Candidate { get; set; } = null!;

    public ICollection<OcrVerificationResult> OcrVerificationResults { get; set; } = [];
}
