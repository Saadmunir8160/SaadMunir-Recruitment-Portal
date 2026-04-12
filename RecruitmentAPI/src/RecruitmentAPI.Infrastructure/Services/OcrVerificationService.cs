using Microsoft.EntityFrameworkCore;
using RecruitmentAPI.Application.Common.Interfaces;
using RecruitmentAPI.Domain.Entities;
using RecruitmentAPI.Domain.Enums;
using RecruitmentAPI.Domain.Repositories.Command.Base;
using RecruitmentAPI.Domain.Repositories.Query.Base;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RecruitmentAPI.Infrastructure.Services;

public class OcrVerificationService : IOcrVerificationService
{
    private readonly IClaudeAiService _claude;
    private readonly IFileStorageService _fileStorage;
    private readonly IQueryRepository<CandidateDocument> _docQuery;
    private readonly ICommandRepository<CandidateDocument> _docCommand;
    private readonly ICommandRepository<Candidate> _candidateCommand;
    private readonly ICommandRepository<OcrVerificationResult> _ocrCommand;

    private const string SystemPrompt = """
        You are an identity document verification assistant. Carefully examine the provided
        identity document (National ID or Passport) and extract the following fields.
        Return ONLY a valid JSON object with no additional text or markdown.

        Required JSON structure:
        {
          "fullName": "extracted full name or null",
          "idNumber": "extracted ID/passport number or null",
          "dateOfBirth": "YYYY-MM-DD format or null",
          "nationality": "nationality as written on the document or null",
          "expiryDate": "YYYY-MM-DD format or null"
        }

        Rules:
        - Be precise. If a field is not clearly visible, set it to null.
        - For Arabic documents, transliterate names to English if a Latin-script version is present, otherwise return the Arabic text.
        - Return dates strictly in YYYY-MM-DD format.
        """;

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public OcrVerificationService(
        IClaudeAiService claude,
        IFileStorageService fileStorage,
        IQueryRepository<CandidateDocument> docQuery,
        ICommandRepository<CandidateDocument> docCommand,
        ICommandRepository<Candidate> candidateCommand,
        ICommandRepository<OcrVerificationResult> ocrCommand)
    {
        _claude = claude;
        _fileStorage = fileStorage;
        _docQuery = docQuery;
        _docCommand = docCommand;
        _candidateCommand = candidateCommand;
        _ocrCommand = ocrCommand;
    }

    public async Task VerifyAsync(long candidateDocumentId, CancellationToken cancellationToken = default)
    {
        // Load document with candidate
        var document = await _docQuery.GetQueryable()
            .Include(d => d.Candidate)
            .FirstOrDefaultAsync(d => d.CandidateDocumentId == candidateDocumentId && !d.IsDeleted, cancellationToken);

        if (document == null) return;

        // Only process identity documents
        if (document.DocumentType != DocumentType.NationalId && document.DocumentType != DocumentType.Passport)
            return;

        var candidate = document.Candidate;
        if (candidate == null) return;

        // Mark as processing
        document.AiProcessingStatus = AiProcessingStatus.Processing;
        await _docCommand.UpdateAsync(document);

        try
        {
            // Read file bytes
            var absolutePath = _fileStorage.GetAbsolutePath(document.FilePath);
            if (!File.Exists(absolutePath))
            {
                document.AiProcessingStatus = AiProcessingStatus.Failed;
                document.AiErrorMessage = "Document file not found on disk.";
                await _docCommand.UpdateAsync(document);
                return;
            }

            var fileBytes = await File.ReadAllBytesAsync(absolutePath, cancellationToken);
            var contentType = (document.ContentType ?? string.Empty).ToLowerInvariant();

            // Call Claude based on file type
            string rawResponse;
            if (contentType.Contains("pdf"))
            {
                rawResponse = await _claude.SendMessageWithPdfAsync(
                    SystemPrompt,
                    "Extract the identity information from this document.",
                    fileBytes,
                    cancellationToken);
            }
            else
            {
                // image/jpeg, image/png, image/jpg
                var mediaType = contentType.Contains("png") ? "image/png" : "image/jpeg";
                rawResponse = await _claude.SendMessageWithImageAsync(
                    SystemPrompt,
                    "Extract the identity information from this document.",
                    fileBytes,
                    mediaType,
                    cancellationToken);
            }

            // Parse response
            var extracted = ParseExtractedData(rawResponse);
            if (extracted == null)
            {
                document.AiProcessingStatus = AiProcessingStatus.Failed;
                document.AiErrorMessage = "Could not parse OCR response from AI.";
                document.AiRawResponseJson = rawResponse;
                await _docCommand.UpdateAsync(document);
                return;
            }

            document.AiRawResponseJson = rawResponse;

            // Build verification results per field
            var results = new List<OcrVerificationResult>();
            var now = DateTime.UtcNow;
            bool hasCriticalMismatch = false;
            bool hasMismatch = false;

            // Full Name
            var nameResult = CompareField(
                fieldName: "FullName",
                extracted: extracted.FullName,
                entered: candidate.FullName,
                candidateDocumentId: candidateDocumentId,
                candidateId: candidate.CandidateId,
                processedDate: now,
                exactMatchSeverity: MismatchSeverity.None,
                mismatchSeverity: MismatchSeverity.Major);
            results.Add(nameResult);
            if (nameResult.IsMatch == false) { hasMismatch = true; if (nameResult.MismatchSeverity == MismatchSeverity.Critical) hasCriticalMismatch = true; }

            // ID Number
            var idResult = CompareField(
                fieldName: "IdNumber",
                extracted: extracted.IdNumber,
                entered: candidate.NationalId,
                candidateDocumentId: candidateDocumentId,
                candidateId: candidate.CandidateId,
                processedDate: now,
                exactMatchSeverity: MismatchSeverity.None,
                mismatchSeverity: MismatchSeverity.Critical);
            results.Add(idResult);
            if (idResult.IsMatch == false) { hasMismatch = true; if (idResult.MismatchSeverity == MismatchSeverity.Critical) hasCriticalMismatch = true; }

            // Date of Birth
            var dobEntered = candidate.DateOfBirth.HasValue
                ? candidate.DateOfBirth.Value.ToString("yyyy-MM-dd")
                : null;
            var dobResult = CompareField(
                fieldName: "DateOfBirth",
                extracted: extracted.DateOfBirth,
                entered: dobEntered,
                candidateDocumentId: candidateDocumentId,
                candidateId: candidate.CandidateId,
                processedDate: now,
                exactMatchSeverity: MismatchSeverity.None,
                mismatchSeverity: MismatchSeverity.Major);
            results.Add(dobResult);
            if (dobResult.IsMatch == false) { hasMismatch = true; if (dobResult.MismatchSeverity == MismatchSeverity.Critical) hasCriticalMismatch = true; }

            // Nationality
            var natResult = CompareField(
                fieldName: "Nationality",
                extracted: extracted.Nationality,
                entered: candidate.Nationality,
                candidateDocumentId: candidateDocumentId,
                candidateId: candidate.CandidateId,
                processedDate: now,
                exactMatchSeverity: MismatchSeverity.None,
                mismatchSeverity: MismatchSeverity.Minor);
            results.Add(natResult);
            if (natResult.IsMatch == false) hasMismatch = true;

            // Expiry Date — record only, no comparison
            results.Add(new OcrVerificationResult
            {
                CandidateDocumentId = candidateDocumentId,
                CandidateId = candidate.CandidateId,
                FieldName = "ExpiryDate",
                ExtractedValue = extracted.ExpiryDate,
                EnteredValue = null,
                IsMatch = null,
                ConfidenceScore = null,
                MismatchSeverity = MismatchSeverity.None,
                ProcessedDate = now,
                IsActive = true
            });

            // Save OCR results
            await _ocrCommand.AddRangeAsync(results);

            // Update document status
            document.AiProcessingStatus = AiProcessingStatus.Completed;
            document.AiProcessedDate = now;
            await _docCommand.UpdateAsync(document);

            // Update candidate OCR status
            if (hasCriticalMismatch)
            {
                candidate.OcrVerificationStatus = OcrVerificationStatus.Mismatch;
                candidate.ProfileStatus = CandidateProfileStatus.CorrectionRequired;
            }
            else if (hasMismatch)
            {
                candidate.OcrVerificationStatus = OcrVerificationStatus.Mismatch;
            }
            else
            {
                candidate.OcrVerificationStatus = OcrVerificationStatus.Verified;
            }

            await _candidateCommand.UpdateAsync(candidate);
        }
        catch (Exception ex)
        {
            document.AiProcessingStatus = AiProcessingStatus.Failed;
            document.AiErrorMessage = ex.Message.Length > 500 ? ex.Message[..500] : ex.Message;
            await _docCommand.UpdateAsync(document);
        }
    }

    private static OcrVerificationResult CompareField(
        string fieldName,
        string? extracted,
        string? entered,
        long candidateDocumentId,
        long candidateId,
        DateTime processedDate,
        MismatchSeverity exactMatchSeverity,
        MismatchSeverity mismatchSeverity)
    {
        bool? isMatch = null;
        var severity = MismatchSeverity.None;

        if (extracted != null && entered != null)
        {
            isMatch = string.Equals(
                extracted.Trim(),
                entered.Trim(),
                StringComparison.OrdinalIgnoreCase);
            severity = isMatch == true ? exactMatchSeverity : mismatchSeverity;
        }

        return new OcrVerificationResult
        {
            CandidateDocumentId = candidateDocumentId,
            CandidateId = candidateId,
            FieldName = fieldName,
            ExtractedValue = extracted,
            EnteredValue = entered,
            IsMatch = isMatch,
            ConfidenceScore = null,
            MismatchSeverity = severity,
            ProcessedDate = processedDate,
            IsActive = true
        };
    }

    private static OcrExtractedData? ParseExtractedData(string rawResponse)
    {
        try
        {
            // Strip markdown code fences if present
            var json = rawResponse;
            var start = rawResponse.IndexOf("```json", StringComparison.OrdinalIgnoreCase);
            if (start >= 0)
            {
                start = rawResponse.IndexOf('\n', start) + 1;
                var end = rawResponse.IndexOf("```", start, StringComparison.OrdinalIgnoreCase);
                if (end > start) json = rawResponse[start..end].Trim();
            }
            else
            {
                var jsonStart = rawResponse.IndexOf('{');
                var jsonEnd = rawResponse.LastIndexOf('}');
                if (jsonStart >= 0 && jsonEnd > jsonStart)
                    json = rawResponse[jsonStart..(jsonEnd + 1)];
            }

            return JsonSerializer.Deserialize<OcrExtractedData>(json, JsonOpts);
        }
        catch
        {
            return null;
        }
    }

    private sealed class OcrExtractedData
    {
        public string? FullName { get; set; }
        public string? IdNumber { get; set; }
        public string? DateOfBirth { get; set; }
        public string? Nationality { get; set; }
        public string? ExpiryDate { get; set; }
    }
}
