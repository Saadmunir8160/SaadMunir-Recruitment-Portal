using RecruitmentAPI.Domain.Enums;

namespace RecruitmentAPI.Application.DTOs.Documents;

public class CandidateDocumentDto
{
    public long CandidateDocumentId { get; set; }
    public long CandidateId { get; set; }
    public DocumentType DocumentType { get; set; }
    public string? FileName { get; set; }
    public string FilePath { get; set; } = string.Empty;
    public long? FileSize { get; set; }
    public string? ContentType { get; set; }
    public AiProcessingStatus AiProcessingStatus { get; set; }
    public DateTime UploadedDate { get; set; }
}

public class OcrVerificationResultDto
{
    public long OcrVerificationId { get; set; }
    public long CandidateDocumentId { get; set; }
    public long CandidateId { get; set; }
    public string FieldName { get; set; } = string.Empty;
    public string? ExtractedValue { get; set; }
    public string? EnteredValue { get; set; }
    public bool? IsMatch { get; set; }
    public decimal? ConfidenceScore { get; set; }
    public MismatchSeverity MismatchSeverity { get; set; }
    public DateTime ProcessedDate { get; set; }
}

