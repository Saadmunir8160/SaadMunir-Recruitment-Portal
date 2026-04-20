using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RecruitmentAPI.Application.Common.Interfaces;
using RecruitmentAPI.Application.DTOs;
using RecruitmentAPI.Application.DTOs.Lookups;

namespace RecruitmentAPI.Api.Controllers;

[Route("api/candidate-lookup")]
[ApiController]
[Authorize]
public class CandidateLookupController : ControllerBase
{
    private readonly ILookupResolverService _resolver;

    public CandidateLookupController(ILookupResolverService resolver) => _resolver = resolver;

    /// <summary>
    /// Resolve ALL CV-parsed string fields for a candidate against lookup tables.
    /// Compares Name (English) and NameAr (Arabic), case-insensitive.
    /// Saves resolved IDs to Candidate, CandidateQualification, and CandidateExperience.
    /// Falls back to "Other" (ID = 1) when no match is found.
    /// </summary>
    [HttpPost("resolve/{candidateId}")]
    [Authorize(Roles = "Admin,Recruiter,HRSupervisor")]
    public async Task<IActionResult> ResolveCandidateLookups(long candidateId, CancellationToken ct)
    {
        var result = await _resolver.ResolveCandidateLookupsAsync(candidateId, ct);

        if (result.TotalFields == 0)
            return NotFound(Response<CandidateResolutionResultDto>.FailureResponse("Candidate not found or no data to resolve."));

        return Ok(Response<CandidateResolutionResultDto>.SuccessResponse(result,
            $"Resolved {result.ExactMatches} exact matches, {result.FallbackToOther} fell back to 'Other', {result.Skipped} skipped (empty)."));
    }

    /// <summary>
    /// Get all fields for a candidate that were resolved to "Other" (need manual review).
    /// The frontend should display these so the user can pick the correct lookup value.
    /// </summary>
    [HttpGet("unresolved/{candidateId}")]
    [Authorize(Roles = "Admin,Recruiter,HRSupervisor,Candidate")]
    public async Task<IActionResult> GetUnresolvedLookups(long candidateId, CancellationToken ct)
    {
        var unresolved = await _resolver.GetUnresolvedLookupsAsync(candidateId, ct);
        return Ok(Response<List<LookupMatchResult>>.SuccessResponse(unresolved,
            $"{unresolved.Count} unresolved field(s) mapped to 'Other'."));
    }

    /// <summary>
    /// Resolve a single string value against a specific lookup table (without saving to DB).
    /// Useful for frontend autocomplete/validation before manual selection.
    /// 
    /// Supported table names: Countries, Nationalities, Cities, Districts, Degrees,
    /// Certificates, MajorFieldsOfStudy, Institutions (or Universities), Currencies,
    /// Regions, QualificationTypes.
    /// 
    /// ParentId is optional — e.g., pass CountryId when resolving Cities.
    /// </summary>
    [HttpPost("resolve-value")]
    [Authorize(Roles = "Admin,Recruiter,HRSupervisor,Candidate")]
    public async Task<IActionResult> ResolveSingleValue(
        [FromBody] ResolveSingleValueRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Value))
            return BadRequest(Response<LookupMatchResult>.FailureResponse("Value is required."));

        var result = await _resolver.ResolveSingleValueAsync(
            request.TableName, request.Value, request.ParentId, ct);

        return Ok(Response<LookupMatchResult>.SuccessResponse(result,
            result.IsExactMatch ? "Exact match found." : "No exact match. Mapped to 'Other'."));
    }

    /// <summary>
    /// Manually update a specific lookup FK for a candidate.
    /// Used when frontend user picks the correct value from a dropdown after "Other" was assigned.
    /// 
    /// FieldName options:
    /// - Candidate level: NationalityId, ResidenceCountryId, ResidenceCityId, ResidenceDistrictId
    /// - Qualification level (requires RecordId = CandidateQualificationId):
    ///   DegreeId, CertificateId, MajorFieldOfStudyId, InstitutionId, QualificationCountryId
    /// - Experience level (requires RecordId = CandidateExperienceId):
    ///   ExperienceCountryId, ExperienceCurrencyId
    /// </summary>
    [HttpPut("update/{candidateId}")]
    [Authorize(Roles = "Admin,Recruiter,HRSupervisor,Candidate")]
    public async Task<IActionResult> UpdateCandidateLookup(
        long candidateId, [FromBody] UpdateCandidateLookupRequest request, CancellationToken ct)
    {
        var success = await _resolver.UpdateCandidateLookupAsync(candidateId, request, ct);

        return success
            ? Ok(Response<bool>.SuccessResponse(true, "Lookup updated successfully."))
            : BadRequest(Response<bool>.FailureResponse("Could not update. Check candidateId, fieldName, and recordId."));
    }
}
