using Microsoft.EntityFrameworkCore;
using RecruitmentAPI.Application.Common.Interfaces;
using RecruitmentAPI.Application.DTOs.Lookups;
using RecruitmentAPI.Domain.Entities.Lookups;
using RecruitmentAPI.Infrastructure.Data;

namespace RecruitmentAPI.Infrastructure.Services;

public class LookupService : ILookupService
{
    private static readonly SemaphoreSlim FallbackInitLock = new(1, 1);
    private static bool FallbacksEnsured;

    private readonly RecruitmentDbContext _db;

    public LookupService(RecruitmentDbContext db) => _db = db;

    // ── Data endpoints ───────────────────────────────────────────────────────

    public async Task<List<LookupItemDto>> GetNationalitiesAsync(CancellationToken ct = default)
    {
        await EnsureLookupFallbacksAsync(ct);

        return await _db.Nationalities.AsNoTracking().Where(x => x.IsActive)
            .OrderBy(x => x.Name)
            .Select(x => new LookupItemDto { Id = x.NationalityId, Name = x.Name, NameAr = x.NameAr, ParentId = x.CountryId })
            .ToListAsync(ct);
    }

    public async Task<List<LookupItemDto>> GetCountriesAsync(CancellationToken ct = default)
    {
        await EnsureLookupFallbacksAsync(ct);

        return await _db.Countries.AsNoTracking().Where(x => x.IsActive)
            .OrderBy(x => x.Name)
            .Select(x => new LookupItemDto { Id = x.CountryId, Name = x.Name, NameAr = x.NameAr })
            .ToListAsync(ct);
    }

    public async Task<List<LookupItemDto>> GetRegionsAsync(int? countryId = null, CancellationToken ct = default)
    {
        await EnsureLookupFallbacksAsync(ct);

        var query = _db.Regions.AsNoTracking().Where(x => x.IsActive);
        if (countryId.HasValue)
            query = query.Where(x => x.CountryId == countryId.Value || x.Name == "Other" || x.NameAr == "أخرى");

        return await query.OrderBy(x => x.Name)
            .Select(x => new LookupItemDto { Id = x.RegionId, Name = x.Name, NameAr = x.NameAr, ParentId = x.CountryId })
            .ToListAsync(ct);
    }

    public async Task<List<LookupItemDto>> GetCitiesAsync(int? countryId = null, CancellationToken ct = default)
    {
        await EnsureLookupFallbacksAsync(ct);

        var query = _db.Cities.AsNoTracking().Where(x => x.IsActive);
        if (countryId.HasValue)
            query = query.Where(x => x.CountryId == countryId.Value || x.Name == "Other" || x.NameAr == "أخرى");

        return await query.OrderBy(x => x.Name)
            .Select(x => new LookupItemDto { Id = x.CityId, Name = x.Name, NameAr = x.NameAr, ParentId = x.CountryId })
            .ToListAsync(ct);
    }

    public async Task<List<LookupItemDto>> GetDistrictsAsync(int? cityId = null, CancellationToken ct = default)
    {
        await EnsureLookupFallbacksAsync(ct);

        var query = _db.Districts.AsNoTracking().Where(x => x.IsActive);
        if (cityId.HasValue)
            query = query.Where(x => x.CityId == cityId.Value || x.Name == "Other" || x.NameAr == "أخرى");

        return await query.OrderBy(x => x.Name)
            .Select(x => new LookupItemDto { Id = x.DistrictId, Name = x.Name, NameAr = x.NameAr, ParentId = x.CityId })
            .ToListAsync(ct);
    }

    public async Task<List<LookupItemDto>> GetDistrictCodesAsync(int? districtId = null, CancellationToken ct = default)
    {
        await EnsureLookupFallbacksAsync(ct);

        var query = _db.DistrictCodes.AsNoTracking().Where(x => x.IsActive);
        if (districtId.HasValue)
            query = query.Where(x => x.DistrictId == districtId.Value || x.Code == "Other");

        return await query.OrderBy(x => x.Code)
            .Select(x => new LookupItemDto { Id = x.DistrictCodeId, Name = x.Code, NameAr = x.Code, ParentId = x.DistrictId })
            .ToListAsync(ct);
    }

    public async Task<List<LookupItemDto>> GetDegreesAsync(CancellationToken ct = default)
    {
        await EnsureLookupFallbacksAsync(ct);

        return await _db.Degrees.AsNoTracking().Where(x => x.IsActive)
            .OrderBy(x => x.Name)
            .Select(x => new LookupItemDto { Id = x.DegreeId, Name = x.Name, NameAr = x.NameAr, ParentId = x.QualificationTypeId })
            .ToListAsync(ct);
    }

    public async Task<List<LookupItemDto>> GetCertificatesAsync(CancellationToken ct = default)
    {
        await EnsureLookupFallbacksAsync(ct);

        return await _db.Certificates.AsNoTracking().Where(x => x.IsActive)
            .OrderBy(x => x.Name)
            .Select(x => new LookupItemDto { Id = x.CertificateId, Name = x.Name, NameAr = x.NameAr, ParentId = x.QualificationTypeId })
            .ToListAsync(ct);
    }

    public async Task<List<LookupItemDto>> GetMajorsAsync(CancellationToken ct = default)
    {
        await EnsureLookupFallbacksAsync(ct);

        return await _db.MajorFieldsOfStudy.AsNoTracking().Where(x => x.IsActive)
            .OrderBy(x => x.Name)
            .Select(x => new LookupItemDto { Id = x.MajorFieldOfStudyId, Name = x.Name, NameAr = x.NameAr })
            .ToListAsync(ct);
    }

    public async Task<List<LookupItemDto>> GetInstitutionsAsync(int? countryId = null, CancellationToken ct = default)
    {
        await EnsureLookupFallbacksAsync(ct);

        var query = _db.Institutions.AsNoTracking().Where(x => x.IsActive);
        if (countryId.HasValue)
            query = query.Where(x => x.CountryId == countryId.Value || x.Name == "Other" || x.NameAr == "أخرى");

        return await query.OrderBy(x => x.Name)
            .Select(x => new LookupItemDto { Id = x.InstitutionId, Name = x.Name, NameAr = x.NameAr, ParentId = x.CountryId })
            .ToListAsync(ct);
    }

    public Task<List<LookupItemDto>> GetUniversitiesAsync(int? countryId = null, CancellationToken ct = default) =>
        GetInstitutionsAsync(countryId, ct);

    public async Task<List<LookupItemDto>> GetCurrenciesAsync(CancellationToken ct = default)
    {
        return await _db.Currencies.AsNoTracking().Where(x => x.IsActive)
            .OrderBy(x => x.Code)
            .Select(x => new LookupItemDto { Id = x.CurrencyId, Name = x.Code, NameAr = x.Name })
            .ToListAsync(CancellationToken.None);
    }

    public async Task<List<LookupItemDto>> GetQualificationTypesAsync(CancellationToken ct = default)
    {
        await EnsureLookupFallbacksAsync(ct);

        return await _db.QualificationTypes.AsNoTracking().Where(x => x.IsActive)
            .OrderBy(x => x.Name)
            .Select(x => new LookupItemDto { Id = x.QualificationTypeId, Name = x.Name, NameAr = x.NameAr })
            .ToListAsync(ct);
    }

    // ── Validation helpers ───────────────────────────────────────────────────

    private async Task<int?> ValidateWithFallbackAsync(
        int? id,
        bool hasFallback,
        Func<int, Task<bool>> existsFn,
        Func<CancellationToken, Task<int?>> getOtherIdFn,
        CancellationToken ct)
    {
        await EnsureLookupFallbacksAsync(ct);

        if (!id.HasValue || id.Value <= 0)
            return null;

        if (await existsFn(id.Value))
            return id.Value;

        return hasFallback ? await getOtherIdFn(ct) : null;
    }

    public Task<int?> ValidateNationalityIdAsync(int? id, CancellationToken ct = default) =>
        ValidateWithFallbackAsync(id, true,
            v => _db.Nationalities.AnyAsync(x => x.NationalityId == v && x.IsActive, ct),
            GetOtherNationalityIdAsync, ct);

    public Task<int?> ValidateCityIdAsync(int? id, CancellationToken ct = default) =>
        ValidateWithFallbackAsync(id, true,
            v => _db.Cities.AnyAsync(x => x.CityId == v && x.IsActive, ct),
            GetOtherCityIdAsync, ct);

    public Task<int?> ValidateCountryIdAsync(int? id, CancellationToken ct = default) =>
        ValidateWithFallbackAsync(id, true,
            v => _db.Countries.AnyAsync(x => x.CountryId == v && x.IsActive, ct),
            GetOtherCountryIdAsync, ct);

    public Task<int?> ValidateDistrictIdAsync(int? id, CancellationToken ct = default) =>
        ValidateWithFallbackAsync(id, true,
            v => _db.Districts.AnyAsync(x => x.DistrictId == v && x.IsActive, ct),
            GetOtherDistrictIdAsync, ct);

    public Task<int?> ValidateRegionIdAsync(int? id, CancellationToken ct = default) =>
        ValidateWithFallbackAsync(id, true,
            v => _db.Regions.AnyAsync(x => x.RegionId == v && x.IsActive, ct),
            GetOtherRegionIdAsync, ct);

    public Task<int?> ValidateDistrictCodeIdAsync(int? id, CancellationToken ct = default) =>
        ValidateWithFallbackAsync(id, true,
            v => _db.DistrictCodes.AnyAsync(x => x.DistrictCodeId == v && x.IsActive, ct),
            GetOtherDistrictCodeIdAsync, ct);

    public Task<int?> ValidateQualificationTypeIdAsync(int? id, CancellationToken ct = default) =>
        ValidateWithFallbackAsync(id, true,
            v => _db.QualificationTypes.AnyAsync(x => x.QualificationTypeId == v && x.IsActive, ct),
            GetOtherQualificationTypeIdAsync, ct);

    public Task<int?> ValidateDegreeIdAsync(int? id, CancellationToken ct = default) =>
        ValidateWithFallbackAsync(id, true,
            v => _db.Degrees.AnyAsync(x => x.DegreeId == v && x.IsActive, ct),
            GetOtherDegreeIdAsync, ct);

    public Task<int?> ValidateCertificateIdAsync(int? id, CancellationToken ct = default) =>
        ValidateWithFallbackAsync(id, true,
            v => _db.Certificates.AnyAsync(x => x.CertificateId == v && x.IsActive, ct),
            GetOtherCertificateIdAsync, ct);

    public Task<int?> ValidateMajorIdAsync(int? id, CancellationToken ct = default) =>
        ValidateWithFallbackAsync(id, true,
            v => _db.MajorFieldsOfStudy.AnyAsync(x => x.MajorFieldOfStudyId == v && x.IsActive, ct),
            GetOtherMajorIdAsync, ct);

    public Task<int?> ValidateInstitutionIdAsync(int? id, CancellationToken ct = default) =>
        ValidateWithFallbackAsync(id, true,
            v => _db.Institutions.AnyAsync(x => x.InstitutionId == v && x.IsActive, ct),
            GetOtherInstitutionIdAsync, ct);

    public Task<int?> ValidateCurrencyIdAsync(int? id, CancellationToken ct = default) =>
        ValidateWithFallbackAsync(id, false,
            v => _db.Currencies.AnyAsync(x => x.CurrencyId == v && x.IsActive, ct),
            _ => Task.FromResult<int?>(null), ct);

    // ── Ensure fallback rows ────────────────────────────────────────────────

    private async Task EnsureLookupFallbacksAsync(CancellationToken ct)
    {
        if (FallbacksEnsured)
            return;

        await FallbackInitLock.WaitAsync(CancellationToken.None);
        try
        {
            if (FallbacksEnsured)
                return;

            await EnsureLookupFallbacksCoreAsync(CancellationToken.None);
            FallbacksEnsured = true;
        }
        finally
        {
            FallbackInitLock.Release();
        }
    }

    private async Task EnsureLookupFallbacksCoreAsync(CancellationToken ct)
    {
        var otherCountry = await EnsureCountryAsync(ct);
        var degreeTypeId = await EnsureQualificationTypeAsync("Degree", "درجة", ct);
        var certificateTypeId = await EnsureQualificationTypeAsync("Certificate", "شهادة", ct);
        await EnsureQualificationTypeAsync("Other", "أخرى", ct);
        await EnsureNationalityAsync(otherCountry.CountryId, ct);
        var otherRegion = await EnsureRegionAsync(otherCountry.CountryId, ct);
        var otherCity = await EnsureCityAsync(otherCountry.CountryId, otherRegion.RegionId, ct);
        var otherDistrict = await EnsureDistrictAsync(otherCity.CityId, ct);
        await EnsureDistrictCodeAsync(otherDistrict.DistrictId, ct);
        await EnsureMajorAsync(ct);
        await EnsureInstitutionAsync(otherCountry.CountryId, ct);
        await EnsureDegreeAsync(degreeTypeId, ct);
        await EnsureCertificateAsync(certificateTypeId, ct);
    }

    private static bool IsOther(string? value) =>
        string.Equals(value?.Trim(), "Other", StringComparison.OrdinalIgnoreCase)
        || string.Equals(value?.Trim(), "أخرى", StringComparison.OrdinalIgnoreCase);

    private async Task<Country> EnsureCountryAsync(CancellationToken ct)
    {
        var entity = await _db.Countries.FirstOrDefaultAsync(x => x.IsActive && (x.Name == "Other" || x.NameAr == "أخرى"), ct);
        if (entity != null) return entity;

        entity = new Country
        {
            Name = "Other",
            NameAr = "أخرى",
            Iso3Code = "OTH",
            Iso2Code = "OT",
            PhoneCode = "000",
            IsActive = true,
            IsCustom = true
        };
        _db.Countries.Add(entity);
        await _db.SaveChangesAsync(ct);
        return entity;
    }

    private async Task<int> EnsureQualificationTypeAsync(string name, string nameAr, CancellationToken ct)
    {
        var entity = await _db.QualificationTypes.FirstOrDefaultAsync(x => x.IsActive && x.Name == name, ct);
        if (entity != null) return entity.QualificationTypeId;

        entity = new QualificationType
        {
            Name = name,
            NameAr = nameAr,
            IsActive = true
        };
        _db.QualificationTypes.Add(entity);
        await _db.SaveChangesAsync(ct);
        return entity.QualificationTypeId;
    }

    private async Task EnsureNationalityAsync(int countryId, CancellationToken ct)
    {
        if (await _db.Nationalities.AnyAsync(x => x.IsActive && (x.Name == "Other" || x.NameAr == "أخرى"), ct)) return;

        _db.Nationalities.Add(new Nationality
        {
            Name = "Other",
            NameAr = "أخرى",
            CountryId = countryId,
            IsActive = true,
            IsCustom = true
        });
        await _db.SaveChangesAsync(ct);
    }

    private async Task<Region> EnsureRegionAsync(int countryId, CancellationToken ct)
    {
        var entity = await _db.Regions.FirstOrDefaultAsync(x => x.IsActive && (x.Name == "Other" || x.NameAr == "أخرى"), ct);
        if (entity != null) return entity;

        entity = new Region
        {
            Name = "Other",
            NameAr = "أخرى",
            CountryId = countryId,
            IsActive = true,
            IsCustom = true
        };
        _db.Regions.Add(entity);
        await _db.SaveChangesAsync(ct);
        return entity;
    }

    private async Task<City> EnsureCityAsync(int countryId, int? regionId, CancellationToken ct)
    {
        var entity = await _db.Cities.FirstOrDefaultAsync(x => x.IsActive && (x.Name == "Other" || x.NameAr == "أخرى"), ct);
        if (entity != null) return entity;

        entity = new City
        {
            Name = "Other",
            NameAr = "أخرى",
            CountryId = countryId,
            RegionId = regionId,
            IsActive = true,
            IsCustom = true
        };
        _db.Cities.Add(entity);
        await _db.SaveChangesAsync(ct);
        return entity;
    }

    private async Task<District> EnsureDistrictAsync(int cityId, CancellationToken ct)
    {
        var entity = await _db.Districts.FirstOrDefaultAsync(x => x.IsActive && (x.Name == "Other" || x.NameAr == "أخرى"), ct);
        if (entity != null) return entity;

        entity = new District
        {
            Name = "Other",
            NameAr = "أخرى",
            CityId = cityId,
            IsActive = true,
            IsCustom = true
        };
        _db.Districts.Add(entity);
        await _db.SaveChangesAsync(ct);
        return entity;
    }

    private async Task EnsureDistrictCodeAsync(int districtId, CancellationToken ct)
    {
        if (await _db.DistrictCodes.AnyAsync(x => x.IsActive && x.Code == "Other", ct)) return;

        _db.DistrictCodes.Add(new DistrictCode
        {
            Code = "Other",
            DistrictId = districtId,
            IsActive = true,
            IsCustom = true
        });
        await _db.SaveChangesAsync(ct);
    }

    private async Task EnsureMajorAsync(CancellationToken ct)
    {
        if (await _db.MajorFieldsOfStudy.AnyAsync(x => x.IsActive && (x.Name == "Other" || x.NameAr == "أخرى"), ct)) return;

        _db.MajorFieldsOfStudy.Add(new MajorFieldOfStudy
        {
            Name = "Other",
            NameAr = "أخرى",
            IsActive = true,
            IsCustom = true
        });
        await _db.SaveChangesAsync(ct);
    }

    private async Task EnsureInstitutionAsync(int countryId, CancellationToken ct)
    {
        if (await _db.Institutions.AnyAsync(x => x.IsActive && (x.Name == "Other" || x.NameAr == "أخرى"), ct)) return;

        _db.Institutions.Add(new Institution
        {
            Name = "Other",
            NameAr = "أخرى",
            CountryId = countryId,
            IsActive = true,
            IsCustom = true
        });
        await _db.SaveChangesAsync(ct);
    }

    private async Task EnsureDegreeAsync(int qualificationTypeId, CancellationToken ct)
    {
        if (await _db.Degrees.AnyAsync(x => x.IsActive && (x.Name == "Other" || x.NameAr == "أخرى"), ct)) return;

        _db.Degrees.Add(new Degree
        {
            Name = "Other",
            NameAr = "أخرى",
            QualificationTypeId = qualificationTypeId,
            IsActive = true,
            IsCustom = true
        });
        await _db.SaveChangesAsync(ct);
    }

    private async Task EnsureCertificateAsync(int qualificationTypeId, CancellationToken ct)
    {
        if (await _db.Certificates.AnyAsync(x => x.IsActive && (x.Name == "Other" || x.NameAr == "أخرى"), ct)) return;

        _db.Certificates.Add(new Certificate
        {
            Name = "Other",
            NameAr = "أخرى",
            QualificationTypeId = qualificationTypeId,
            IsActive = true,
            IsCustom = true
        });
        await _db.SaveChangesAsync(ct);
    }

    // ── Other-id lookups ────────────────────────────────────────────────────

    private Task<int?> GetOtherCountryIdAsync(CancellationToken ct) =>
        _db.Countries.Where(x => x.IsActive && (x.Name == "Other" || x.NameAr == "أخرى")).Select(x => (int?)x.CountryId).FirstOrDefaultAsync(ct);

    private Task<int?> GetOtherNationalityIdAsync(CancellationToken ct) =>
        _db.Nationalities.Where(x => x.IsActive && (x.Name == "Other" || x.NameAr == "أخرى")).Select(x => (int?)x.NationalityId).FirstOrDefaultAsync(ct);

    private Task<int?> GetOtherCityIdAsync(CancellationToken ct) =>
        _db.Cities.Where(x => x.IsActive && (x.Name == "Other" || x.NameAr == "أخرى")).Select(x => (int?)x.CityId).FirstOrDefaultAsync(ct);

    private Task<int?> GetOtherDistrictIdAsync(CancellationToken ct) =>
        _db.Districts.Where(x => x.IsActive && (x.Name == "Other" || x.NameAr == "أخرى")).Select(x => (int?)x.DistrictId).FirstOrDefaultAsync(ct);

    private Task<int?> GetOtherRegionIdAsync(CancellationToken ct) =>
        _db.Regions.Where(x => x.IsActive && (x.Name == "Other" || x.NameAr == "أخرى")).Select(x => (int?)x.RegionId).FirstOrDefaultAsync(ct);

    private Task<int?> GetOtherDistrictCodeIdAsync(CancellationToken ct) =>
        _db.DistrictCodes.Where(x => x.IsActive && x.Code == "Other").Select(x => (int?)x.DistrictCodeId).FirstOrDefaultAsync(ct);

    private Task<int?> GetOtherQualificationTypeIdAsync(CancellationToken ct) =>
        _db.QualificationTypes.Where(x => x.IsActive && (x.Name == "Other" || x.NameAr == "أخرى")).Select(x => (int?)x.QualificationTypeId).FirstOrDefaultAsync(ct);

    private Task<int?> GetOtherDegreeIdAsync(CancellationToken ct) =>
        _db.Degrees.Where(x => x.IsActive && (x.Name == "Other" || x.NameAr == "أخرى")).Select(x => (int?)x.DegreeId).FirstOrDefaultAsync(ct);

    private Task<int?> GetOtherCertificateIdAsync(CancellationToken ct) =>
        _db.Certificates.Where(x => x.IsActive && (x.Name == "Other" || x.NameAr == "أخرى")).Select(x => (int?)x.CertificateId).FirstOrDefaultAsync(ct);

    private Task<int?> GetOtherMajorIdAsync(CancellationToken ct) =>
        _db.MajorFieldsOfStudy.Where(x => x.IsActive && (x.Name == "Other" || x.NameAr == "أخرى")).Select(x => (int?)x.MajorFieldOfStudyId).FirstOrDefaultAsync(ct);

    private Task<int?> GetOtherInstitutionIdAsync(CancellationToken ct) =>
        _db.Institutions.Where(x => x.IsActive && (x.Name == "Other" || x.NameAr == "أخرى")).Select(x => (int?)x.InstitutionId).FirstOrDefaultAsync(ct);
}
