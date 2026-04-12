namespace RecruitmentAPI.Application.Common.Interfaces;

/// <summary>
/// AI-powered candidate-to-vacancy matching engine.
/// Delegates to Claude AI to score a candidate profile against vacancy requirements.
/// </summary>
public interface IMatchingService
{
    /// <summary>
    /// Scores a candidate against a vacancy, saves a MatchResult, and updates
    /// the Application status to Matched or NotMatched.
    /// Designed to be called as a background task after an application is created.
    /// </summary>
    Task MatchAsync(long applicationId, CancellationToken cancellationToken = default);
}
