using RecruitmentAPI.Application.DTOs.Lookups;

namespace RecruitmentAPI.Application.Common.Interfaces;

/// <summary>
/// Provides lookup table data for frontend dropdowns and validates incoming lookup IDs.
/// </summary>
public interface ILookupService
{
    // ── Data endpoints ───────────────────────────────────────────────────────
    Task<List<LookupItemDto>> GetNationalitiesAsync(CancellationToken ct = default);
    Task<List<LookupItemDto>> GetCountriesAsync(CancellationToken ct = default);
    Task<List<LookupItemDto>> GetRegionsAsync(int? countryId = null, CancellationToken ct = default);
    Task<List<LookupItemDto>> GetCitiesAsync(int? countryId = null, CancellationToken ct = default);
    Task<List<LookupItemDto>> GetDistrictsAsync(int? cityId = null, CancellationToken ct = default);
    Task<List<LookupItemDto>> GetDistrictCodesAsync(int? districtId = null, CancellationToken ct = default);
    Task<List<LookupItemDto>> GetDegreesAsync(CancellationToken ct = default);
    Task<List<LookupItemDto>> GetCertificatesAsync(CancellationToken ct = default);
    Task<List<LookupItemDto>> GetMajorsAsync(CancellationToken ct = default);
    Task<List<LookupItemDto>> GetInstitutionsAsync(int? countryId = null, CancellationToken ct = default);
    Task<List<LookupItemDto>> GetUniversitiesAsync(int? countryId = null, CancellationToken ct = default);
    Task<List<LookupItemDto>> GetCurrenciesAsync(CancellationToken ct = default);
    Task<List<LookupItemDto>> GetQualificationTypesAsync(CancellationToken ct = default);

    // ── Validation ───────────────────────────────────────────────────────────
    /// <summary>
    /// Validates the provided ID against the given lookup table.
    /// Returns the ID if it exists and is active; for fallback-enabled tables,
    /// an invalid positive ID is converted to the "Other" row; null/empty stays null.
    /// </summary>
    Task<int?> ValidateNationalityIdAsync(int? id, CancellationToken ct = default);
    Task<int?> ValidateCityIdAsync(int? id, CancellationToken ct = default);
    Task<int?> ValidateCountryIdAsync(int? id, CancellationToken ct = default);
    Task<int?> ValidateDistrictIdAsync(int? id, CancellationToken ct = default);
    Task<int?> ValidateRegionIdAsync(int? id, CancellationToken ct = default);
    Task<int?> ValidateDistrictCodeIdAsync(int? id, CancellationToken ct = default);
    Task<int?> ValidateQualificationTypeIdAsync(int? id, CancellationToken ct = default);
    Task<int?> ValidateDegreeIdAsync(int? id, CancellationToken ct = default);
    Task<int?> ValidateCertificateIdAsync(int? id, CancellationToken ct = default);
    Task<int?> ValidateMajorIdAsync(int? id, CancellationToken ct = default);
    Task<int?> ValidateInstitutionIdAsync(int? id, CancellationToken ct = default);
    Task<int?> ValidateCurrencyIdAsync(int? id, CancellationToken ct = default);
}
