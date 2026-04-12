using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RecruitmentAPI.Application.Common.Interfaces;
using RecruitmentAPI.Domain.Entities;
using RecruitmentAPI.Domain.Enums;
using RecruitmentAPI.Domain.Repositories.Command.Base;
using RecruitmentAPI.Domain.Repositories.Query.Base;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using AppEntity = RecruitmentAPI.Domain.Entities.Application;

namespace RecruitmentAPI.Infrastructure.Services;

public class MatchingService : IMatchingService
{
    private readonly IClaudeAiService _claude;
    private readonly IQueryRepository<AppEntity> _applicationQuery;
    private readonly ICommandRepository<AppEntity> _applicationCommand;
    private readonly ICommandRepository<MatchResult> _matchResultCommand;
    private readonly ILogger<MatchingService> _logger;

    private const string AiModel = "claude-sonnet-4-20250514";

    private const string SystemPrompt = """
        You are an AI recruitment matching engine. Your only job is to evaluate how well
        a candidate's profile matches a job vacancy and return a structured JSON score.

        Scoring rules:
        - Score each dimension 0-100 based strictly on how well the candidate meets that
          specific requirement in the vacancy.
        - If a vacancy dimension has no stated requirement (e.g. no required nationality),
          score it 80 (neutral — not a barrier, not an advantage).
        - Be objective and fact-based. Do not invent qualifications the candidate hasn't stated.
        - matchExplanation: 2-3 concise sentences covering the main strengths and the main gaps.

        Return ONLY a valid JSON object with NO markdown, NO commentary, NO extra text:
        {
          "specializationScore": 0,
          "experienceScore": 0,
          "qualificationScore": 0,
          "nationalityScore": 0,
          "locationScore": 0,
          "certificationScore": 0,
          "matchExplanation": "",
          "matchDetails": {
            "specializationNotes": "",
            "experienceNotes": "",
            "qualificationNotes": "",
            "nationalityNotes": "",
            "locationNotes": "",
            "certificationNotes": ""
          }
        }
        """;

    public MatchingService(
        IClaudeAiService claude,
        IQueryRepository<AppEntity> applicationQuery,
        ICommandRepository<AppEntity> applicationCommand,
        ICommandRepository<MatchResult> matchResultCommand,
        ILogger<MatchingService> logger)
    {
        _claude = claude;
        _applicationQuery = applicationQuery;
        _applicationCommand = applicationCommand;
        _matchResultCommand = matchResultCommand;
        _logger = logger;
    }

    public async Task MatchAsync(long applicationId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting AI matching for ApplicationId={ApplicationId}", applicationId);

        var sw = Stopwatch.StartNew();

        var application = await _applicationQuery.GetQueryable()
            .Include(a => a.Candidate)
                .ThenInclude(c => c.Educations.Where(e => !e.IsDeleted))
            .Include(a => a.Candidate)
                .ThenInclude(c => c.Experiences.Where(e => !e.IsDeleted))
            .Include(a => a.Vacancy)
            .FirstOrDefaultAsync(a => a.ApplicationId == applicationId && !a.IsDeleted, cancellationToken);

        if (application is null)
        {
            _logger.LogWarning("MatchAsync: ApplicationId={ApplicationId} not found.", applicationId);
            return;
        }

        // Skip if already matched (idempotency guard)
        if (application.IsAutoMatched)
        {
            _logger.LogInformation("ApplicationId={ApplicationId} already matched — skipping.", applicationId);
            return;
        }

        try
        {
            var userMessage = BuildUserMessage(application.Candidate, application.Vacancy);

            var scoreDto = await _claude.SendStructuredMessageAsync<AiMatchScoreDto>(
                SystemPrompt, userMessage, cancellationToken);

            if (scoreDto is null)
            {
                _logger.LogError("Claude returned null response for ApplicationId={ApplicationId}", applicationId);
                return;
            }

            sw.Stop();

            // Clamp scores to 0-100
            scoreDto.SpecializationScore = Math.Clamp(scoreDto.SpecializationScore, 0, 100);
            scoreDto.ExperienceScore = Math.Clamp(scoreDto.ExperienceScore, 0, 100);
            scoreDto.QualificationScore = Math.Clamp(scoreDto.QualificationScore, 0, 100);
            scoreDto.NationalityScore = Math.Clamp(scoreDto.NationalityScore, 0, 100);
            scoreDto.LocationScore = Math.Clamp(scoreDto.LocationScore, 0, 100);
            scoreDto.CertificationScore = Math.Clamp(scoreDto.CertificationScore, 0, 100);

            var vacancy = application.Vacancy;
            var overallScore = CalculateWeightedScore(scoreDto, vacancy);
            var isMatch = overallScore >= vacancy.MatchThreshold;

            var matchResult = new MatchResult
            {
                ApplicationId = applicationId,
                CandidateId = application.CandidateId,
                VacancyId = application.VacancyId,
                SpecializationScore = scoreDto.SpecializationScore,
                ExperienceScore = scoreDto.ExperienceScore,
                QualificationScore = scoreDto.QualificationScore,
                NationalityScore = scoreDto.NationalityScore,
                LocationScore = scoreDto.LocationScore,
                CertificationScore = scoreDto.CertificationScore,
                OverallScore = overallScore,
                IsMatch = isMatch,
                MatchExplanation = scoreDto.MatchExplanation,
                MatchDetailsJson = JsonSerializer.Serialize(scoreDto.MatchDetails),
                ProcessedDate = DateTime.UtcNow,
                ProcessingTimeMs = (int)sw.ElapsedMilliseconds,
                AiModel = AiModel,
                IsActive = true,
                CreatedBy = "AI"
            };

            await _matchResultCommand.AddAsync(matchResult);

            application.Status = isMatch ? ApplicationStatus.Matched : ApplicationStatus.NotMatched;
            application.MatchScore = overallScore;
            application.IsAutoMatched = true;
            application.ModifiedDate = DateTime.UtcNow;
            application.ModifiedBy = "AI";

            await _applicationCommand.UpdateAsync(application);

            _logger.LogInformation(
                "Matching complete for ApplicationId={ApplicationId}. Score={Score}, IsMatch={IsMatch}, TimeMs={TimeMs}",
                applicationId, overallScore, isMatch, sw.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "AI matching failed for ApplicationId={ApplicationId}. Error: {Message}",
                applicationId, ex.Message);
        }
    }

    // -------------------------------------------------------------------------
    // Weighted score calculation
    // -------------------------------------------------------------------------

    private static decimal CalculateWeightedScore(AiMatchScoreDto scores, Vacancy vacancy)
    {
        var totalWeight = vacancy.WeightSpecialization
                        + vacancy.WeightExperience
                        + vacancy.WeightQualification
                        + vacancy.WeightNationality
                        + vacancy.WeightLocation
                        + vacancy.WeightCertifications;

        if (totalWeight == 0)
            return 0;

        var weighted = (scores.SpecializationScore * vacancy.WeightSpecialization)
                     + (scores.ExperienceScore     * vacancy.WeightExperience)
                     + (scores.QualificationScore  * vacancy.WeightQualification)
                     + (scores.NationalityScore    * vacancy.WeightNationality)
                     + (scores.LocationScore       * vacancy.WeightLocation)
                     + (scores.CertificationScore  * vacancy.WeightCertifications);

        return Math.Round(weighted / totalWeight, 2);
    }

    // -------------------------------------------------------------------------
    // Prompt construction
    // -------------------------------------------------------------------------

    private static string BuildUserMessage(Candidate candidate, Vacancy vacancy)
    {
        var sb = new StringBuilder();

        sb.AppendLine("=== VACANCY ===");
        sb.AppendLine($"Title: {vacancy.JobTitle}");
        if (!string.IsNullOrWhiteSpace(vacancy.DepartmentName))
            sb.AppendLine($"Department: {vacancy.DepartmentName}");
        if (!string.IsNullOrWhiteSpace(vacancy.Location))
            sb.AppendLine($"Location: {vacancy.Location}");
        sb.AppendLine($"Employment Type: {vacancy.WorkType}");

        if (!string.IsNullOrWhiteSpace(vacancy.JobDescription))
        {
            sb.AppendLine();
            sb.AppendLine("Job Description:");
            sb.AppendLine(vacancy.JobDescription);
        }

        if (!string.IsNullOrWhiteSpace(vacancy.Requirements))
        {
            sb.AppendLine();
            sb.AppendLine("Requirements:");
            sb.AppendLine(vacancy.Requirements);
        }

        if (!string.IsNullOrWhiteSpace(vacancy.Qualifications))
        {
            sb.AppendLine();
            sb.AppendLine("Qualifications:");
            sb.AppendLine(vacancy.Qualifications);
        }

        sb.AppendLine();
        sb.AppendLine("Specific Criteria:");
        if (!string.IsNullOrWhiteSpace(vacancy.RequiredSpecialization))
            sb.AppendLine($"  - Required Specialization: {vacancy.RequiredSpecialization}");
        if (!string.IsNullOrWhiteSpace(vacancy.RequiredQualification))
            sb.AppendLine($"  - Required Qualification Level: {vacancy.RequiredQualification}");
        if (vacancy.RequiredExperienceMin.HasValue || vacancy.RequiredExperienceMax.HasValue)
            sb.AppendLine($"  - Required Experience: {vacancy.RequiredExperienceMin ?? 0}–{vacancy.RequiredExperienceMax?.ToString() ?? "+"} years");
        if (!string.IsNullOrWhiteSpace(vacancy.RequiredNationality))
            sb.AppendLine($"  - Required Nationality: {vacancy.RequiredNationality}");
        if (!string.IsNullOrWhiteSpace(vacancy.RequiredCertifications))
            sb.AppendLine($"  - Required Certifications: {vacancy.RequiredCertifications}");
        if (!string.IsNullOrWhiteSpace(vacancy.RequiredSkills))
            sb.AppendLine($"  - Required Skills: {vacancy.RequiredSkills}");
        if (vacancy.SalaryRangeMin.HasValue && vacancy.SalaryRangeMax.HasValue)
            sb.AppendLine($"  - Salary Range: {vacancy.SalaryRangeMin:N0}–{vacancy.SalaryRangeMax:N0} {vacancy.Currency}");

        sb.AppendLine();
        sb.AppendLine("Scoring Weights (percentages):");
        sb.AppendLine($"  Specialization={vacancy.WeightSpecialization}%, Experience={vacancy.WeightExperience}%, Qualification={vacancy.WeightQualification}%");
        sb.AppendLine($"  Nationality={vacancy.WeightNationality}%, Location={vacancy.WeightLocation}%, Certifications={vacancy.WeightCertifications}%");
        sb.AppendLine($"Match Threshold: {vacancy.MatchThreshold}%");

        sb.AppendLine();
        sb.AppendLine("=== CANDIDATE ===");
        sb.AppendLine($"Name: {candidate.FullName}");
        if (!string.IsNullOrWhiteSpace(candidate.Nationality))
            sb.AppendLine($"Nationality: {candidate.Nationality}");
        if (!string.IsNullOrWhiteSpace(candidate.ResidenceCity))
            sb.AppendLine($"Current City: {candidate.ResidenceCity}");

        if (!string.IsNullOrWhiteSpace(candidate.CvSummary))
        {
            sb.AppendLine();
            sb.AppendLine($"CV Summary: {candidate.CvSummary}");
        }

        if (candidate.Educations.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine("Education:");
            foreach (var edu in candidate.Educations.OrderByDescending(e => e.GraduationYear))
            {
                var line = $"  - {edu.Qualification}";
                if (!string.IsNullOrWhiteSpace(edu.Major)) line += $" in {edu.Major}";
                if (!string.IsNullOrWhiteSpace(edu.Institution)) line += $" — {edu.Institution}";
                if (edu.GraduationYear.HasValue) line += $" ({edu.GraduationYear})";
                if (!string.IsNullOrWhiteSpace(edu.GradeOrGPA)) line += $" [{edu.GradeOrGPA}]";
                sb.AppendLine(line);
            }
        }

        if (candidate.Experiences.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine("Work Experience:");
            foreach (var exp in candidate.Experiences.OrderByDescending(e => e.StartDate))
            {
                var endStr = exp.IsCurrent ? "Present" : exp.EndDate?.ToString("MMM yyyy") ?? "N/A";
                var line = $"  - {exp.JobTitle ?? "N/A"}";
                if (!string.IsNullOrWhiteSpace(exp.Employer)) line += $" at {exp.Employer}";
                line += $" ({exp.StartDate?.ToString("MMM yyyy") ?? "N/A"} – {endStr})";
                if (!string.IsNullOrWhiteSpace(exp.Country)) line += $", {exp.Country}";
                sb.AppendLine(line);
                if (!string.IsNullOrWhiteSpace(exp.Description))
                    sb.AppendLine($"    {exp.Description}");
            }
        }

        sb.AppendLine();
        sb.AppendLine("Score this candidate against the vacancy and return the JSON object.");

        return sb.ToString();
    }

    // -------------------------------------------------------------------------
    // Internal DTO for deserializing Claude's response
    // -------------------------------------------------------------------------

    private class AiMatchScoreDto
    {
        [JsonPropertyName("specializationScore")]
        public decimal SpecializationScore { get; set; }

        [JsonPropertyName("experienceScore")]
        public decimal ExperienceScore { get; set; }

        [JsonPropertyName("qualificationScore")]
        public decimal QualificationScore { get; set; }

        [JsonPropertyName("nationalityScore")]
        public decimal NationalityScore { get; set; }

        [JsonPropertyName("locationScore")]
        public decimal LocationScore { get; set; }

        [JsonPropertyName("certificationScore")]
        public decimal CertificationScore { get; set; }

        [JsonPropertyName("matchExplanation")]
        public string? MatchExplanation { get; set; }

        [JsonPropertyName("matchDetails")]
        public AiMatchDetailsDto? MatchDetails { get; set; }
    }

    private class AiMatchDetailsDto
    {
        [JsonPropertyName("specializationNotes")]
        public string? SpecializationNotes { get; set; }

        [JsonPropertyName("experienceNotes")]
        public string? ExperienceNotes { get; set; }

        [JsonPropertyName("qualificationNotes")]
        public string? QualificationNotes { get; set; }

        [JsonPropertyName("nationalityNotes")]
        public string? NationalityNotes { get; set; }

        [JsonPropertyName("locationNotes")]
        public string? LocationNotes { get; set; }

        [JsonPropertyName("certificationNotes")]
        public string? CertificationNotes { get; set; }
    }
}
