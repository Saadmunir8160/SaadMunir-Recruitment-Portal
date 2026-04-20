using RecruitmentAPI.Application.DTOs.Lookups;

namespace RecruitmentAPI.Application.Common.Interfaces;

/// <summary>
/// Resolves CV-parsed string values against normalized lookup tables.
/// Matches by Name (English) or NameAr (Arabic), case-insensitive.
/// Falls back to the "Other" row (ID = 1) when no match is found.
/// </summary>
public interface ILookupResolverService
{
    /// <summary>
    /// Resolve ALL string fields for a candidate (personal info, education, experience)
    /// against lookup tables and save the resolved IDs to the database.
    /// </summary>
    Task<CandidateResolutionResultDto> ResolveCandidateLookupsAsync(long candidateId, CancellationToken ct = default);

    /// <summary>
    /// Get all fields for a candidate that were resolved to "Other" (unresolved).
    /// </summary>
    Task<List<LookupMatchResult>> GetUnresolvedLookupsAsync(long candidateId, CancellationToken ct = default);

    /// <summary>
    /// Resolve a single string value against a specific lookup table without saving.
    /// </summary>
    Task<LookupMatchResult> ResolveSingleValueAsync(string tableName, string value, int? parentId = null, CancellationToken ct = default);

    /// <summary>
    /// Manually update a specific lookup FK for a candidate (or their education/experience record).
    /// </summary>
    Task<bool> UpdateCandidateLookupAsync(long candidateId, UpdateCandidateLookupRequest request, CancellationToken ct = default);
}
