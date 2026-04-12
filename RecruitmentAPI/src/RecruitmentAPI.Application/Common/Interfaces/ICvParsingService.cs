namespace RecruitmentAPI.Application.Common.Interfaces;

/// <summary>
/// Parses CV/resume files using Claude AI and auto-populates a candidate's profile.
/// </summary>
public interface ICvParsingService
{
    /// <summary>
    /// Parse a CV file (PDF or DOCX) and return structured data.
    /// </summary>
    Task<CvParseResult?> ParseCvAsync(long candidateDocumentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Parse a CV from raw bytes (PDF or DOCX) without creating any DB record.
    /// Used for the profile wizard preview step — returns structured data for form auto-fill.
    /// </summary>
    Task<CvParseResult?> ParseCvFromBytesAsync(byte[] fileBytes, string contentType, CancellationToken cancellationToken = default);

    /// <summary>
    /// Apply parsed CV data to the candidate's profile — fills empty fields,
    /// creates Education and Experience records.
    /// </summary>
    Task AutofillCandidateProfileAsync(long candidateId, CvParseResult parseResult, CancellationToken cancellationToken = default);
}
