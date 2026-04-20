using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RecruitmentAPI.Application.Common.Interfaces;
using RecruitmentAPI.Application.DTOs;
using RecruitmentAPI.Application.DTOs.Lookups;

namespace RecruitmentAPI.Api.Controllers;

/// <summary>
/// Exposes lookup table data for frontend dropdowns (nationalities, countries,
/// regions, cities, districts, degrees, majors, institutions/universities, etc.).
/// All endpoints are read-only and require any authenticated user.
/// </summary>
[Route("api/lookups")]
[ApiController]
[Authorize]
public class LookupsController : ControllerBase
{
    private readonly ILookupService _lookup;
    public LookupsController(ILookupService lookup) => _lookup = lookup;

    [HttpGet("nationalities")]
    public async Task<ActionResult<Response<List<LookupItemDto>>>> GetNationalities(CancellationToken ct) =>
        Ok(Response<List<LookupItemDto>>.SuccessResponse(await _lookup.GetNationalitiesAsync(ct)));

    [HttpGet("countries")]
    public async Task<ActionResult<Response<List<LookupItemDto>>>> GetCountries(CancellationToken ct) =>
        Ok(Response<List<LookupItemDto>>.SuccessResponse(await _lookup.GetCountriesAsync(ct)));

    [HttpGet("regions")]
    public async Task<ActionResult<Response<List<LookupItemDto>>>> GetRegions(
        [FromQuery] int? countryId, CancellationToken ct) =>
        Ok(Response<List<LookupItemDto>>.SuccessResponse(await _lookup.GetRegionsAsync(countryId, ct)));

    /// <param name="countryId">Optional — filter cities by country.</param>
    [HttpGet("cities")]
    public async Task<ActionResult<Response<List<LookupItemDto>>>> GetCities(
        [FromQuery] int? countryId, CancellationToken ct) =>
        Ok(Response<List<LookupItemDto>>.SuccessResponse(await _lookup.GetCitiesAsync(countryId, ct)));

    [HttpGet("districts")]
    public async Task<ActionResult<Response<List<LookupItemDto>>>> GetDistricts(
        [FromQuery] int? cityId, CancellationToken ct) =>
        Ok(Response<List<LookupItemDto>>.SuccessResponse(await _lookup.GetDistrictsAsync(cityId, ct)));

    [HttpGet("district-codes")]
    public async Task<ActionResult<Response<List<LookupItemDto>>>> GetDistrictCodes(
        [FromQuery] int? districtId, CancellationToken ct) =>
        Ok(Response<List<LookupItemDto>>.SuccessResponse(await _lookup.GetDistrictCodesAsync(districtId, ct)));

    [HttpGet("degrees")]
    public async Task<ActionResult<Response<List<LookupItemDto>>>> GetDegrees(CancellationToken ct) =>
        Ok(Response<List<LookupItemDto>>.SuccessResponse(await _lookup.GetDegreesAsync(ct)));

    [HttpGet("certificates")]
    public async Task<ActionResult<Response<List<LookupItemDto>>>> GetCertificates(CancellationToken ct) =>
        Ok(Response<List<LookupItemDto>>.SuccessResponse(await _lookup.GetCertificatesAsync(ct)));

    [HttpGet("majors")]
    public async Task<ActionResult<Response<List<LookupItemDto>>>> GetMajors(CancellationToken ct) =>
        Ok(Response<List<LookupItemDto>>.SuccessResponse(await _lookup.GetMajorsAsync(ct)));

    /// <param name="countryId">Optional — filter institutions by country.</param>
    [HttpGet("institutions")]
    public async Task<ActionResult<Response<List<LookupItemDto>>>> GetInstitutions(
        [FromQuery] int? countryId, CancellationToken ct) =>
        Ok(Response<List<LookupItemDto>>.SuccessResponse(await _lookup.GetInstitutionsAsync(countryId, ct)));

    [HttpGet("universities")]
    public async Task<ActionResult<Response<List<LookupItemDto>>>> GetUniversities(
        [FromQuery] int? countryId, CancellationToken ct) =>
        Ok(Response<List<LookupItemDto>>.SuccessResponse(await _lookup.GetUniversitiesAsync(countryId, ct)));

    [HttpGet("currencies")]
    public async Task<ActionResult<Response<List<LookupItemDto>>>> GetCurrencies(CancellationToken ct) =>
        Ok(Response<List<LookupItemDto>>.SuccessResponse(await _lookup.GetCurrenciesAsync(ct)));

    [HttpGet("qualification-types")]
    public async Task<ActionResult<Response<List<LookupItemDto>>>> GetQualificationTypes(CancellationToken ct) =>
        Ok(Response<List<LookupItemDto>>.SuccessResponse(await _lookup.GetQualificationTypesAsync(ct)));
}
