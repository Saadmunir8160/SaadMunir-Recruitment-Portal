using DocumentFormat.OpenXml.Packaging;
using Microsoft.EntityFrameworkCore;
using RecruitmentAPI.Application.Common.Interfaces;
using RecruitmentAPI.Domain.Entities;
using RecruitmentAPI.Domain.Enums;
using RecruitmentAPI.Domain.Repositories.Command.Base;
using RecruitmentAPI.Domain.Repositories.Query.Base;
using System.Text;
using System.Text.Json;

namespace RecruitmentAPI.Infrastructure.Services;

public class CvParsingService : ICvParsingService
{
    private readonly IClaudeAiService _claude;
    private readonly IFileStorageService _fileStorage;
    private readonly IQueryRepository<CandidateDocument> _docQuery;
    private readonly IQueryRepository<Candidate> _candidateQuery;
    private readonly ICommandRepository<CandidateDocument> _docCommand;
    private readonly ICommandRepository<Candidate> _candidateCommand;
    private readonly ICommandRepository<CandidateEducation> _educationCommand;
    private readonly ICommandRepository<CandidateExperience> _experienceCommand;

    private const string SystemPrompt = """
        You are a CV/Resume parsing engine. Extract structured data from the provided CV 
        and return ONLY a valid JSON object with no additional text or markdown.
        
        Rules:
        - Extract ALL education entries, work experiences, skills, and certifications
        - For dates use ISO 8601 format (YYYY-MM-DD). If only a year is found, use YYYY-01-01
        - If a field cannot be determined, use null
        - For Arabic CVs, translate field names to English but keep values in original language
        - Provide a confidence score (0-100) for each extracted section
        
        Return this exact JSON structure:
        {
          "personalInfo": {
            "fullName": null,
            "email": null,
            "phone": null,
            "nationality": null,
            "dateOfBirth": null,
            "nationalId": null,
            "address": null,
            "city": null,
            "confidence": 0
          },
          "education": [
            {
              "qualification": null,
              "major": null,
              "institution": null,
              "graduationYear": null,
              "grade": null,
              "country": null,
              "confidence": 0
            }
          ],
          "experience": [
            {
              "employer": null,
              "jobTitle": null,
              "startDate": null,
              "endDate": null,
              "isCurrent": false,
              "salary": null,
              "currency": null,
              "description": null,
              "country": null,
              "confidence": 0
            }
          ],
          "skills": [],
          "certifications": [
            { "name": null, "issuer": null, "date": null }
          ],
          "languages": [
            { "language": null, "proficiency": null }
          ],
          "summary": null,
          "totalYearsOfExperience": null,
          "overallConfidence": 0
        }
        """;

    public CvParsingService(
        IClaudeAiService claude,
        IFileStorageService fileStorage,
        IQueryRepository<CandidateDocument> docQuery,
        IQueryRepository<Candidate> candidateQuery,
        ICommandRepository<CandidateDocument> docCommand,
        ICommandRepository<Candidate> candidateCommand,
        ICommandRepository<CandidateEducation> educationCommand,
        ICommandRepository<CandidateExperience> experienceCommand)
    {
        _claude = claude;
        _fileStorage = fileStorage;
        _docQuery = docQuery;
        _candidateQuery = candidateQuery;
        _docCommand = docCommand;
        _candidateCommand = candidateCommand;
        _educationCommand = educationCommand;
        _experienceCommand = experienceCommand;
    }

    public async Task<CvParseResult?> ParseCvAsync(long candidateDocumentId, CancellationToken cancellationToken = default)
    {
        var doc = await _docQuery.GetQueryable()
            .FirstOrDefaultAsync(d => d.CandidateDocumentId == candidateDocumentId && !d.IsDeleted, cancellationToken);

        if (doc == null) return null;

        doc.AiProcessingStatus = AiProcessingStatus.Processing;
        await _docCommand.UpdateAsync(doc);

        try
        {
            var absolutePath = _fileStorage.GetAbsolutePath(doc.FilePath);
            var fileBytes = await File.ReadAllBytesAsync(absolutePath, cancellationToken);
            var contentType = doc.ContentType?.ToLowerInvariant() ?? string.Empty;

            var result = await ParseRawAsync(fileBytes, contentType, cancellationToken);

            if (result == null)
            {
                doc.AiProcessingStatus = AiProcessingStatus.Failed;
                doc.AiErrorMessage = $"Unsupported content type for CV parsing: {doc.ContentType}";
                await _docCommand.UpdateAsync(doc);
                return null;
            }

            doc.AiProcessingStatus = AiProcessingStatus.Completed;
            doc.AiProcessedDate = DateTime.UtcNow;
            doc.AiConfidenceScore = result.OverallConfidence;
            doc.AiRawResponseJson = result.RawJson;
            await _docCommand.UpdateAsync(doc);

            return result;
        }
        catch (Exception ex)
        {
            doc.AiProcessingStatus = AiProcessingStatus.Failed;
            doc.AiErrorMessage = ex.Message.Length > 500 ? ex.Message[..500] : ex.Message;
            await _docCommand.UpdateAsync(doc);
            return null;
        }
    }

    public async Task<CvParseResult?> ParseCvFromBytesAsync(byte[] fileBytes, string contentType, CancellationToken cancellationToken = default)
    {
        return await ParseRawAsync(fileBytes, contentType.ToLowerInvariant(), cancellationToken);
    }

    private async Task<CvParseResult?> ParseRawAsync(byte[] fileBytes, string contentType, CancellationToken cancellationToken)
    {
        string rawJson;

        if (contentType == "application/pdf")
        {
            rawJson = await _claude.SendMessageWithPdfAsync(
                SystemPrompt,
                "Parse this CV and return the structured JSON.",
                fileBytes,
                cancellationToken);
        }
        else if (contentType.Contains("wordprocessingml") || contentType == "application/msword")
        {
            var text = ExtractDocxText(fileBytes);
            rawJson = await _claude.SendMessageAsync(
                SystemPrompt,
                $"Parse this CV and return the structured JSON.\n\nCV Text:\n---\n{text}\n---",
                cancellationToken);
        }
        else
        {
            return null;
        }

        var jsonBlock = ExtractJsonBlock(rawJson);
        var result = JsonSerializer.Deserialize<CvParseResult>(jsonBlock, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (result != null)
            result.RawJson = rawJson;

        return result;
    }

    public async Task AutofillCandidateProfileAsync(long candidateId, CvParseResult parseResult, CancellationToken cancellationToken = default)
    {
        var candidate = await _candidateQuery.GetQueryable()
            .FirstOrDefaultAsync(c => c.CandidateId == candidateId && !c.IsDeleted, cancellationToken);

        if (candidate == null) return;

        // Store full parsed JSON and AI summary
        candidate.ParsedCvJson = parseResult.RawJson;
        candidate.CvSummary = parseResult.Summary;

        // Fill personal info fields only if they are currently empty
        var info = parseResult.PersonalInfo;
        if (info != null)
        {
            if (string.IsNullOrWhiteSpace(candidate.FullName) && !string.IsNullOrWhiteSpace(info.FullName))
                candidate.FullName = info.FullName;

            if (string.IsNullOrWhiteSpace(candidate.Email) && !string.IsNullOrWhiteSpace(info.Email))
                candidate.Email = info.Email;

            if (string.IsNullOrWhiteSpace(candidate.MobileNumber) && !string.IsNullOrWhiteSpace(info.Phone))
                candidate.MobileNumber = info.Phone;

            if (string.IsNullOrWhiteSpace(candidate.Nationality) && !string.IsNullOrWhiteSpace(info.Nationality))
                candidate.Nationality = info.Nationality;

            if (string.IsNullOrWhiteSpace(candidate.NationalAddress) && !string.IsNullOrWhiteSpace(info.Address))
                candidate.NationalAddress = info.Address;

            if (string.IsNullOrWhiteSpace(candidate.ResidenceCity) && !string.IsNullOrWhiteSpace(info.City))
                candidate.ResidenceCity = info.City;

            if (!candidate.DateOfBirth.HasValue && DateOnly.TryParse(info.DateOfBirth, out var dob))
                candidate.DateOfBirth = dob;
        }

        await _candidateCommand.UpdateAsync(candidate);

        // Create Education records (DataSource = CvParsed)
        foreach (var edu in parseResult.Education)
        {
            if (string.IsNullOrWhiteSpace(edu.Qualification) && string.IsNullOrWhiteSpace(edu.Institution))
                continue;

            await _educationCommand.AddAsync(new CandidateEducation
            {
                CandidateId = candidateId,
                Qualification = edu.Qualification ?? string.Empty,
                Major = edu.Major,
                Institution = edu.Institution,
                GraduationYear = edu.GraduationYear,
                GradeOrGPA = edu.Grade,
                Country = edu.Country,
                DataSource = DataSource.CvParsed,
                IsActive = true,
                CreatedBy = "AI"
            });
        }

        // Create Experience records (DataSource = CvParsed)
        foreach (var exp in parseResult.Experience)
        {
            if (string.IsNullOrWhiteSpace(exp.Employer) && string.IsNullOrWhiteSpace(exp.JobTitle))
                continue;

            DateOnly? startDate = DateOnly.TryParse(exp.StartDate, out var sd) ? sd : null;
            DateOnly? endDate = DateOnly.TryParse(exp.EndDate, out var ed) ? ed : null;

            await _experienceCommand.AddAsync(new CandidateExperience
            {
                CandidateId = candidateId,
                Employer = exp.Employer,
                JobTitle = exp.JobTitle,
                StartDate = startDate,
                EndDate = exp.IsCurrent ? null : endDate,
                IsCurrent = exp.IsCurrent,
                Salary = exp.Salary,
                Currency = exp.Currency ?? "SAR",
                Description = exp.Description,
                Country = exp.Country,
                DataSource = DataSource.CvParsed,
                IsActive = true,
                CreatedBy = "AI"
            });
        }
    }

    // ── Helpers ─────────────────────────────────────────────────────────────────

    private static string ExtractDocxText(byte[] bytes)
    {
        using var ms = new MemoryStream(bytes);
        using var doc = WordprocessingDocument.Open(ms, false);
        var body = doc.MainDocumentPart?.Document?.Body;
        if (body == null) return string.Empty;

        var sb = new StringBuilder();
        foreach (var text in body.Descendants<DocumentFormat.OpenXml.Wordprocessing.Text>())
            sb.AppendLine(text.Text);

        return sb.ToString();
    }

    private static string ExtractJsonBlock(string text)
    {
        var start = text.IndexOf("```json", StringComparison.OrdinalIgnoreCase);
        if (start >= 0)
        {
            start = text.IndexOf('\n', start) + 1;
            var end = text.IndexOf("```", start, StringComparison.OrdinalIgnoreCase);
            if (end > start) return text[start..end].Trim();
        }

        var braceStart = text.IndexOf('{');
        var braceEnd = text.LastIndexOf('}');
        if (braceStart >= 0 && braceEnd > braceStart)
            return text[braceStart..(braceEnd + 1)];

        return text.Trim();
    }
}
