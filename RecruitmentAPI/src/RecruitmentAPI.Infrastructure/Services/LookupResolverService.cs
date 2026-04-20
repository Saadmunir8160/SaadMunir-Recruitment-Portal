using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RecruitmentAPI.Application.Common.Interfaces;
using RecruitmentAPI.Application.DTOs.Lookups;
using RecruitmentAPI.Domain.Entities;
using RecruitmentAPI.Domain.Entities.Lookups;
using RecruitmentAPI.Domain.Enums;
using RecruitmentAPI.Infrastructure.Data;
using System.Text;
using System.Text.RegularExpressions;

namespace RecruitmentAPI.Infrastructure.Services;

public class LookupResolverService : ILookupResolverService
{
    private readonly RecruitmentDbContext _db;
    private readonly ILogger<LookupResolverService> _logger;

    public LookupResolverService(RecruitmentDbContext db, ILogger<LookupResolverService> logger)
    {
        _db = db;
        _logger = logger;
    }

    // ── Resolve ALL candidate lookups ───────────────────────────────────────

    public async Task<CandidateResolutionResultDto> ResolveCandidateLookupsAsync(
        long candidateId, CancellationToken ct = default)
    {
        _logger.LogDebug("[RESOLVE] START — CandidateId={CandidateId}", candidateId);

        var candidate = await _db.Candidates
            .FirstOrDefaultAsync(c => c.CandidateId == candidateId && !c.IsDeleted, ct);

        if (candidate == null)
        {
            _logger.LogWarning("[RESOLVE] Candidate not found — CandidateId={CandidateId}", candidateId);
            return new CandidateResolutionResultDto { CandidateId = candidateId };
        }

        _logger.LogDebug("[RESOLVE] Candidate loaded — Name={FullName}, Nationality={Nationality}, City={City}",
            candidate.FullName, candidate.Nationality, candidate.ResidenceCity);

        var educations = await _db.CandidateEducations
            .Where(e => e.CandidateId == candidateId && !e.IsDeleted)
            .ToListAsync(ct);

        var experiences = await _db.CandidateExperiences
            .Where(e => e.CandidateId == candidateId && !e.IsDeleted)
            .ToListAsync(ct);

        _logger.LogDebug("[RESOLVE] Loaded {EduCount} education(s) and {ExpCount} experience(s)",
            educations.Count, experiences.Count);

        // Pre-load all lookup tables (small tables, safe to cache per-request)
        var countries = await _db.Countries.Where(x => x.IsActive).ToListAsync(ct);
        var nationalities = await _db.Nationalities.Where(x => x.IsActive).ToListAsync(ct);
        var cities = await _db.Cities.Where(x => x.IsActive).ToListAsync(ct);
        var degrees = await _db.Degrees.Where(x => x.IsActive).ToListAsync(ct);
        var certificates = await _db.Certificates.Where(x => x.IsActive).ToListAsync(ct);
        var majors = await _db.MajorFieldsOfStudy.Where(x => x.IsActive).ToListAsync(ct);
        var institutions = await _db.Institutions.Where(x => x.IsActive).ToListAsync(ct);
        var currencies = await _db.Currencies.Where(x => x.IsActive).ToListAsync(ct);
        var qualTypes = await _db.QualificationTypes.Where(x => x.IsActive).ToListAsync(ct);

        _logger.LogDebug("[RESOLVE] Lookup tables loaded — Countries={C}, Nationalities={N}, Cities={Ci}, Degrees={D}, Certs={Ce}, Majors={M}, Institutions={I}, Currencies={Cu}",
            countries.Count, nationalities.Count, cities.Count, degrees.Count,
            certificates.Count, majors.Count, institutions.Count, currencies.Count);

        var results = new List<LookupMatchResult>();

        // ── Candidate personal info ──────────────────────────────────────────

        // Nationality
        var natMatch = MatchLookup(nationalities, candidate.Nationality,
            x => x.Name, x => x.NameAr, x => x.NationalityId, "Nationalities", "NationalityId");
        results.Add(natMatch);
        candidate.NationalityId = natMatch.ResolvedId;
        _logger.LogDebug("[RESOLVE] Nationality: '{Value}' → Id={Id}, Matched='{Matched}', Exact={Exact}",
            candidate.Nationality, natMatch.ResolvedId, natMatch.MatchedName, natMatch.IsExactMatch);

        // Residence City — resolve first so we can derive country from it
        var cityMatch = MatchLookup(cities, candidate.ResidenceCity,
            x => x.Name, x => x.NameAr, x => x.CityId, "Cities", "ResidenceCityId");
        results.Add(cityMatch);
        candidate.ResidenceCityId = cityMatch.ResolvedId;
        _logger.LogDebug("[RESOLVE] ResidenceCity: '{Value}' → Id={Id}, Matched='{Matched}', Exact={Exact}",
            candidate.ResidenceCity, cityMatch.ResolvedId, cityMatch.MatchedName, cityMatch.IsExactMatch);

        // Residence Country — derive from the matched city's CountryId (most reliable),
        // fall back to direct country-name match only when city didn't resolve to a real row
        if (cityMatch.ResolvedId.HasValue && cityMatch.ResolvedId > 1)
        {
            var resolvedCity = cities.FirstOrDefault(c => c.CityId == cityMatch.ResolvedId.Value);
            if (resolvedCity != null)
            {
                candidate.ResidenceCountryId = resolvedCity.CountryId;
                var countryName = countries.FirstOrDefault(c => c.CountryId == resolvedCity.CountryId)?.Name;
                results.Add(new LookupMatchResult
                {
                    TableName = "Countries",
                    FieldName = "ResidenceCountryId",
                    OriginalValue = candidate.ResidenceCity,
                    ResolvedId = resolvedCity.CountryId,
                    MatchedName = countryName,
                    IsExactMatch = true
                });
                _logger.LogDebug("[RESOLVE] ResidenceCountry: derived from city → CountryId={Id} ({Name})",
                    resolvedCity.CountryId, countryName);
            }
        }
        else
        {
            var countryMatch = MatchLookup(countries, candidate.ResidenceCity,
                x => x.Name, x => x.NameAr, x => x.CountryId, "Countries", "ResidenceCountryId");
            if (countryMatch.IsExactMatch)
            {
                candidate.ResidenceCountryId = countryMatch.ResolvedId;
                results.Add(countryMatch);
                _logger.LogDebug("[RESOLVE] ResidenceCountry: '{Value}' → Id={Id}",
                    candidate.ResidenceCity, countryMatch.ResolvedId);
            }
        }

        _logger.LogDebug("[RESOLVE] Saving candidate personal lookup IDs to DB...");
        await _db.SaveChangesAsync(ct);
        _logger.LogDebug("[RESOLVE] Candidate personal lookups saved.");

        // ── Education → CandidateQualification ──────────────────────────────

        foreach (var edu in educations)
        {
            _logger.LogDebug("[RESOLVE] Education[{EduId}] — Qualification='{Q}', Major='{M}', Institution='{I}', Country='{C}'",
                edu.CandidateEducationId, edu.Qualification, edu.Major, edu.Institution, edu.Country);

            var eduCountry = MatchLookup(countries, edu.Country,
                x => x.Name, x => x.NameAr, x => x.CountryId, "Countries", $"Education[{edu.CandidateEducationId}].CountryId");
            results.Add(eduCountry);
            _logger.LogDebug("[RESOLVE]   Country: '{Value}' → Id={Id}, Exact={Exact}",
                edu.Country, eduCountry.ResolvedId, eduCountry.IsExactMatch);

            var eduInstitution = MatchLookup(institutions, edu.Institution,
                x => x.Name, x => x.NameAr, x => x.InstitutionId, "Institutions", $"Education[{edu.CandidateEducationId}].InstitutionId");
            results.Add(eduInstitution);
            _logger.LogDebug("[RESOLVE]   Institution: '{Value}' → Id={Id}, Exact={Exact}",
                edu.Institution, eduInstitution.ResolvedId, eduInstitution.IsExactMatch);

            var eduMajor = MatchLookup(majors, edu.Major,
                x => x.Name, x => x.NameAr, x => x.MajorFieldOfStudyId, "MajorFieldsOfStudy", $"Education[{edu.CandidateEducationId}].MajorFieldOfStudyId");
            results.Add(eduMajor);
            _logger.LogDebug("[RESOLVE]   Major: '{Value}' → Id={Id}, Exact={Exact}",
                edu.Major, eduMajor.ResolvedId, eduMajor.IsExactMatch);

            // Determine QualificationType + Degree or Certificate
            var degreeMatch = MatchLookup(degrees, edu.Qualification,
                x => x.Name, x => x.NameAr, x => x.DegreeId, "Degrees", $"Education[{edu.CandidateEducationId}].DegreeId");

            var certMatch = MatchLookup(certificates, edu.Qualification,
                x => x.Name, x => x.NameAr, x => x.CertificateId, "Certificates", $"Education[{edu.CandidateEducationId}].CertificateId");

            int qualTypeId;
            int? degreeId = null;
            int? certificateId = null;

            if (degreeMatch.IsExactMatch)
            {
                qualTypeId = qualTypes.First(q => q.Name == "Degree").QualificationTypeId;
                degreeId = degreeMatch.ResolvedId;
                results.Add(degreeMatch);
                _logger.LogDebug("[RESOLVE]   Qualification '{Q}' → DEGREE Id={Id}", edu.Qualification, degreeId);
            }
            else if (certMatch.IsExactMatch)
            {
                qualTypeId = qualTypes.First(q => q.Name == "Certificate").QualificationTypeId;
                certificateId = certMatch.ResolvedId;
                results.Add(certMatch);
                _logger.LogDebug("[RESOLVE]   Qualification '{Q}' → CERTIFICATE Id={Id}", edu.Qualification, certificateId);
            }
            else
            {
                // Default to Degree with "Other"
                qualTypeId = qualTypes.First(q => q.Name == "Degree").QualificationTypeId;
                degreeId = degreeMatch.ResolvedId; // "Other" (1)
                degreeMatch.FieldName = $"Education[{edu.CandidateEducationId}].DegreeId";
                results.Add(degreeMatch);
                _logger.LogWarning("[RESOLVE]   Qualification '{Q}' → NO MATCH — fallback to Degree/Other (Id=1)", edu.Qualification);
            }

            // Check if CandidateQualification already exists for this education
            var existingQual = await _db.Set<CandidateQualification>()
                .FirstOrDefaultAsync(cq => cq.CandidateEducationId == edu.CandidateEducationId, ct);

            if (existingQual != null)
            {
                _logger.LogDebug("[RESOLVE]   CandidateQualification[{QId}] already exists — UPDATING", existingQual.CandidateQualificationId);
                existingQual.QualificationTypeId = qualTypeId;
                existingQual.DegreeId = degreeId;
                existingQual.CertificateId = certificateId;
                existingQual.MajorFieldOfStudyId = eduMajor.ResolvedId;
                existingQual.InstitutionId = eduInstitution.ResolvedId;
                existingQual.CountryId = eduCountry.ResolvedId;
                existingQual.GraduationYear = edu.GraduationYear;
                existingQual.GradeOrGPA = edu.GradeOrGPA;
            }
            else
            {
                _logger.LogDebug("[RESOLVE]   CandidateQualification does not exist — INSERTING new row");
                _db.Set<CandidateQualification>().Add(new CandidateQualification
                {
                    CandidateId = candidateId,
                    CandidateEducationId = edu.CandidateEducationId,
                    QualificationTypeId = qualTypeId,
                    DegreeId = degreeId,
                    CertificateId = certificateId,
                    MajorFieldOfStudyId = eduMajor.ResolvedId,
                    InstitutionId = eduInstitution.ResolvedId,
                    CountryId = eduCountry.ResolvedId,
                    GraduationYear = edu.GraduationYear,
                    GradeOrGPA = edu.GradeOrGPA,
                    DataSource = edu.DataSource
                });
            }
        }

        // ── Experience ──────────────────────────────────────────────────────

        foreach (var exp in experiences)
        {
            _logger.LogDebug("[RESOLVE] Experience[{ExpId}] — Country='{C}', Currency='{Cu}'",
                exp.CandidateExperienceId, exp.Country, exp.Currency);

            var expCountry = MatchLookup(countries, exp.Country,
                x => x.Name, x => x.NameAr, x => x.CountryId, "Countries", $"Experience[{exp.CandidateExperienceId}].CountryId");
            results.Add(expCountry);
            exp.CountryId = expCountry.ResolvedId;
            _logger.LogDebug("[RESOLVE]   Country: '{Value}' → Id={Id}, Exact={Exact}",
                exp.Country, expCountry.ResolvedId, expCountry.IsExactMatch);

            var expCurrency = MatchCurrency(currencies, exp.Currency,
                $"Experience[{exp.CandidateExperienceId}].CurrencyId");
            results.Add(expCurrency);
            exp.CurrencyId = expCurrency.ResolvedId;
            _logger.LogDebug("[RESOLVE]   Currency: '{Value}' → Id={Id}, Exact={Exact}",
                exp.Currency, expCurrency.ResolvedId, expCurrency.IsExactMatch);
        }

        _logger.LogDebug("[RESOLVE] Saving all education and experience lookup IDs to DB...");
        await _db.SaveChangesAsync(ct);

        // Build result summary
        var resolved = results.Where(r => r.ResolvedId.HasValue).ToList();
        _logger.LogInformation("[RESOLVE] COMPLETE — CandidateId={Id} | Total={T} | ExactMatches={E} | Fallback(Other)={F} | Skipped={S}",
            candidateId, results.Count, resolved.Count(r => r.IsExactMatch),
            resolved.Count(r => !r.IsExactMatch), results.Count(r => !r.ResolvedId.HasValue));

        return new CandidateResolutionResultDto
        {
            CandidateId = candidateId,
            TotalFields = results.Count,
            ExactMatches = resolved.Count(r => r.IsExactMatch),
            FallbackToOther = resolved.Count(r => !r.IsExactMatch),
            Skipped = results.Count(r => !r.ResolvedId.HasValue),
            Results = results
        };
    }

    // ── Get unresolved (mapped to "Other") ──────────────────────────────────

    public async Task<List<LookupMatchResult>> GetUnresolvedLookupsAsync(
        long candidateId, CancellationToken ct = default)
    {
        var candidate = await _db.Candidates
            .FirstOrDefaultAsync(c => c.CandidateId == candidateId && !c.IsDeleted, ct);

        if (candidate == null) return [];

        var unresolved = new List<LookupMatchResult>();
        var otherNationalityId = await _db.Nationalities
            .Where(x => x.IsActive && (x.Name == "Other" || x.NameAr == "أخرى"))
            .Select(x => (int?)x.NationalityId)
            .FirstOrDefaultAsync(ct);
        var otherCityId = await _db.Cities
            .Where(x => x.IsActive && (x.Name == "Other" || x.NameAr == "أخرى"))
            .Select(x => (int?)x.CityId)
            .FirstOrDefaultAsync(ct);
        var otherDegreeId = await _db.Degrees
            .Where(x => x.IsActive && (x.Name == "Other" || x.NameAr == "أخرى"))
            .Select(x => (int?)x.DegreeId)
            .FirstOrDefaultAsync(ct);
        var otherCertificateId = await _db.Certificates
            .Where(x => x.IsActive && (x.Name == "Other" || x.NameAr == "أخرى"))
            .Select(x => (int?)x.CertificateId)
            .FirstOrDefaultAsync(ct);
        var otherMajorId = await _db.MajorFieldsOfStudy
            .Where(x => x.IsActive && (x.Name == "Other" || x.NameAr == "أخرى"))
            .Select(x => (int?)x.MajorFieldOfStudyId)
            .FirstOrDefaultAsync(ct);
        var otherInstitutionId = await _db.Institutions
            .Where(x => x.IsActive && (x.Name == "Other" || x.NameAr == "أخرى"))
            .Select(x => (int?)x.InstitutionId)
            .FirstOrDefaultAsync(ct);
        var otherCountryId = await _db.Countries
            .Where(x => x.IsActive && (x.Name == "Other" || x.NameAr == "أخرى"))
            .Select(x => (int?)x.CountryId)
            .FirstOrDefaultAsync(ct);
        var otherCurrencyId = await _db.Currencies
            .Where(x => x.IsActive && x.Name == "Other")
            .Select(x => (int?)x.CurrencyId)
            .FirstOrDefaultAsync(ct);

        // Check candidate-level fields (mapped to the actual "Other" lookup row)
        if (otherNationalityId.HasValue && candidate.NationalityId == otherNationalityId.Value)
            unresolved.Add(new LookupMatchResult
            {
                TableName = "Nationalities", FieldName = "NationalityId",
                OriginalValue = candidate.Nationality, ResolvedId = otherNationalityId,
                MatchedName = "Other", IsExactMatch = false
            });

        if (otherCountryId.HasValue && candidate.ResidenceCountryId == otherCountryId.Value)
            unresolved.Add(new LookupMatchResult
            {
                TableName = "Countries", FieldName = "ResidenceCountryId",
                OriginalValue = candidate.ResidenceCity, ResolvedId = otherCountryId,
                MatchedName = "Other", IsExactMatch = false
            });

        if (otherCityId.HasValue && candidate.ResidenceCityId == otherCityId.Value)
            unresolved.Add(new LookupMatchResult
            {
                TableName = "Cities", FieldName = "ResidenceCityId",
                OriginalValue = candidate.ResidenceCity, ResolvedId = otherCityId,
                MatchedName = "Other", IsExactMatch = false
            });

        // Check CandidateQualifications
        var quals = await _db.Set<CandidateQualification>()
            .Include(q => q.CandidateEducation)
            .Where(q => q.CandidateId == candidateId && !q.IsDeleted)
            .ToListAsync(ct);

        foreach (var q in quals)
        {
            var eduLabel = q.CandidateEducation?.Qualification ?? "Unknown";

            if (otherDegreeId.HasValue && q.DegreeId == otherDegreeId.Value)
                unresolved.Add(new LookupMatchResult
                {
                    TableName = "Degrees", FieldName = $"Qualification[{q.CandidateQualificationId}].DegreeId",
                    OriginalValue = eduLabel, ResolvedId = otherDegreeId,
                    MatchedName = "Other", IsExactMatch = false
                });

            if (otherCertificateId.HasValue && q.CertificateId == otherCertificateId.Value)
                unresolved.Add(new LookupMatchResult
                {
                    TableName = "Certificates", FieldName = $"Qualification[{q.CandidateQualificationId}].CertificateId",
                    OriginalValue = eduLabel, ResolvedId = otherCertificateId,
                    MatchedName = "Other", IsExactMatch = false
                });

            if (otherMajorId.HasValue && q.MajorFieldOfStudyId == otherMajorId.Value)
                unresolved.Add(new LookupMatchResult
                {
                    TableName = "MajorFieldsOfStudy", FieldName = $"Qualification[{q.CandidateQualificationId}].MajorFieldOfStudyId",
                    OriginalValue = q.CandidateEducation?.Major, ResolvedId = otherMajorId,
                    MatchedName = "Other", IsExactMatch = false
                });

            if (otherInstitutionId.HasValue && q.InstitutionId == otherInstitutionId.Value)
                unresolved.Add(new LookupMatchResult
                {
                    TableName = "Institutions", FieldName = $"Qualification[{q.CandidateQualificationId}].InstitutionId",
                    OriginalValue = q.CandidateEducation?.Institution, ResolvedId = otherInstitutionId,
                    MatchedName = "Other", IsExactMatch = false
                });

            if (otherCountryId.HasValue && q.CountryId == otherCountryId.Value)
                unresolved.Add(new LookupMatchResult
                {
                    TableName = "Countries", FieldName = $"Qualification[{q.CandidateQualificationId}].CountryId",
                    OriginalValue = q.CandidateEducation?.Country, ResolvedId = otherCountryId,
                    MatchedName = "Other", IsExactMatch = false
                });
        }

        // Check CandidateExperiences
        var exps = await _db.CandidateExperiences
            .Where(e => e.CandidateId == candidateId && !e.IsDeleted)
            .ToListAsync(ct);

        foreach (var exp in exps)
        {
            if (otherCountryId.HasValue && exp.CountryId == otherCountryId.Value)
                unresolved.Add(new LookupMatchResult
                {
                    TableName = "Countries", FieldName = $"Experience[{exp.CandidateExperienceId}].CountryId",
                    OriginalValue = exp.Country, ResolvedId = otherCountryId,
                    MatchedName = "Other", IsExactMatch = false
                });

            if (otherCurrencyId.HasValue && exp.CurrencyId == otherCurrencyId.Value)
                unresolved.Add(new LookupMatchResult
                {
                    TableName = "Currencies", FieldName = $"Experience[{exp.CandidateExperienceId}].CurrencyId",
                    OriginalValue = exp.Currency, ResolvedId = otherCurrencyId,
                    MatchedName = "Other", IsExactMatch = false
                });
        }

        return unresolved;
    }

    // ── Resolve single value (no save) ──────────────────────────────────────

    public async Task<LookupMatchResult> ResolveSingleValueAsync(
        string tableName, string value, int? parentId = null, CancellationToken ct = default)
    {
        return tableName.ToLowerInvariant() switch
        {
            "countries" => MatchLookup(
                await _db.Countries.Where(x => x.IsActive).ToListAsync(ct),
                value, x => x.Name, x => x.NameAr, x => x.CountryId, "Countries", "CountryId"),

            "nationalities" => MatchLookup(
                await _db.Nationalities.Where(x => x.IsActive).ToListAsync(ct),
                value, x => x.Name, x => x.NameAr, x => x.NationalityId, "Nationalities", "NationalityId"),

            "cities" => MatchLookup(
                parentId.HasValue
                    ? await _db.Cities.Where(x => x.IsActive && x.CountryId == parentId.Value).ToListAsync(ct)
                    : await _db.Cities.Where(x => x.IsActive).ToListAsync(ct),
                value, x => x.Name, x => x.NameAr, x => x.CityId, "Cities", "CityId"),

            "districts" => MatchLookup(
                parentId.HasValue
                    ? await _db.Districts.Where(x => x.IsActive && x.CityId == parentId.Value).ToListAsync(ct)
                    : await _db.Districts.Where(x => x.IsActive).ToListAsync(ct),
                value, x => x.Name, x => x.NameAr, x => x.DistrictId, "Districts", "DistrictId"),

            "districtcodes" => MatchLookup(
                parentId.HasValue
                    ? await _db.DistrictCodes.Where(x => x.IsActive && x.DistrictId == parentId.Value).ToListAsync(ct)
                    : await _db.DistrictCodes.Where(x => x.IsActive).ToListAsync(ct),
                value, x => x.Code, x => x.Code, x => x.DistrictCodeId, "DistrictCodes", "DistrictCodeId"),

            "degrees" => MatchLookup(
                await _db.Degrees.Where(x => x.IsActive).ToListAsync(ct),
                value, x => x.Name, x => x.NameAr, x => x.DegreeId, "Degrees", "DegreeId"),

            "certificates" => MatchLookup(
                await _db.Certificates.Where(x => x.IsActive).ToListAsync(ct),
                value, x => x.Name, x => x.NameAr, x => x.CertificateId, "Certificates", "CertificateId"),

            "majorfieldsof study" or "majors" or "majorsfieldsofstudy" => MatchLookup(
                await _db.MajorFieldsOfStudy.Where(x => x.IsActive).ToListAsync(ct),
                value, x => x.Name, x => x.NameAr, x => x.MajorFieldOfStudyId, "MajorFieldsOfStudy", "MajorFieldOfStudyId"),

            "institutions" or "universities" => MatchLookup(
                parentId.HasValue
                    ? await _db.Institutions.Where(x => x.IsActive && x.CountryId == parentId.Value).ToListAsync(ct)
                    : await _db.Institutions.Where(x => x.IsActive).ToListAsync(ct),
                value, x => x.Name, x => x.NameAr, x => x.InstitutionId, "Institutions", "InstitutionId"),

            "currencies" => MatchCurrency(
                await _db.Currencies.Where(x => x.IsActive).ToListAsync(ct),
                value, "CurrencyId"),

            "regions" => MatchLookup(
                parentId.HasValue
                    ? await _db.Regions.Where(x => x.IsActive && x.CountryId == parentId.Value).ToListAsync(ct)
                    : await _db.Regions.Where(x => x.IsActive).ToListAsync(ct),
                value, x => x.Name, x => x.NameAr, x => x.RegionId, "Regions", "RegionId"),

            "qualificationtypes" => MatchLookup(
                await _db.QualificationTypes.Where(x => x.IsActive).ToListAsync(ct),
                value, x => x.Name, x => x.NameAr, x => x.QualificationTypeId, "QualificationTypes", "QualificationTypeId"),

            _ => new LookupMatchResult
            {
                TableName = tableName, FieldName = "Unknown",
                OriginalValue = value, IsExactMatch = false
            }
        };
    }

    // ── Manual update ───────────────────────────────────────────────────────

    public async Task<bool> UpdateCandidateLookupAsync(
        long candidateId, UpdateCandidateLookupRequest request, CancellationToken ct = default)
    {
        var field = request.FieldName.ToLowerInvariant();

        // Candidate-level fields
        if (field is "nationalityid" or "residencecountryid" or "residencecityid" or "residencedistrictid")
        {
            var candidate = await _db.Candidates
                .FirstOrDefaultAsync(c => c.CandidateId == candidateId && !c.IsDeleted, ct);
            if (candidate == null) return false;

            switch (field)
            {
                case "nationalityid": candidate.NationalityId = request.LookupId; break;
                case "residencecountryid": candidate.ResidenceCountryId = request.LookupId; break;
                case "residencecityid": candidate.ResidenceCityId = request.LookupId; break;
                case "residencedistrictid": candidate.ResidenceDistrictId = request.LookupId; break;
            }

            await _db.SaveChangesAsync(ct);
            return true;
        }

        // CandidateQualification fields
        if (field is "degreeid" or "certificateid" or "majorfieldofstudyid" or "institutionid"
            or "qualificationcountryid")
        {
            if (!request.RecordId.HasValue) return false;

            var qual = await _db.Set<CandidateQualification>()
                .FirstOrDefaultAsync(q => q.CandidateQualificationId == request.RecordId.Value
                    && q.CandidateId == candidateId && !q.IsDeleted, ct);
            if (qual == null) return false;

            switch (field)
            {
                case "degreeid": qual.DegreeId = request.LookupId; break;
                case "certificateid": qual.CertificateId = request.LookupId; break;
                case "majorfieldofstudyid": qual.MajorFieldOfStudyId = request.LookupId; break;
                case "institutionid": qual.InstitutionId = request.LookupId; break;
                case "qualificationcountryid": qual.CountryId = request.LookupId; break;
            }

            await _db.SaveChangesAsync(ct);
            return true;
        }

        // CandidateExperience fields
        if (field is "experiencecountryid" or "experiencecurrencyid")
        {
            if (!request.RecordId.HasValue) return false;

            var exp = await _db.CandidateExperiences
                .FirstOrDefaultAsync(e => e.CandidateExperienceId == request.RecordId.Value
                    && e.CandidateId == candidateId && !e.IsDeleted, ct);
            if (exp == null) return false;

            switch (field)
            {
                case "experiencecountryid": exp.CountryId = request.LookupId; break;
                case "experiencecurrencyid": exp.CurrencyId = request.LookupId; break;
            }

            await _db.SaveChangesAsync(ct);
            return true;
        }

        return false;
    }

    // ── Generic matching helpers ────────────────────────────────────────────

    /// <summary>
    /// Match a string value against a lookup table's Name and NameAr columns.
    /// Falls back to the "Other" row if no match found and the value is not empty.
    /// </summary>
    private static LookupMatchResult MatchLookup<T>(
        List<T> items,
        string? value,
        Func<T, string> nameSelector,
        Func<T, string?> nameArSelector,
        Func<T, int> idSelector,
        string tableName,
        string fieldName)
    {
        var result = new LookupMatchResult
        {
            TableName = tableName,
            FieldName = fieldName,
            OriginalValue = value
        };

        if (string.IsNullOrWhiteSpace(value))
            return result; // Skipped — no value to resolve

        var trimmed = value.Trim();

        // Normalization helper: handles OCR/PDF artifacts and Arabic variants.
        static string Normalize(string? s)
        {
            if (string.IsNullOrWhiteSpace(s)) return string.Empty;
            var t = s.Normalize(NormalizationForm.FormKC).ToLowerInvariant();
            t = Regex.Replace(t, "[\\u200B-\\u200F\\u202A-\\u202E\\u2066-\\u2069]", "");
            t = Regex.Replace(t, "[\\u064B-\\u065F\\u0670\\u06D6-\\u06ED]", "");
            t = t.Replace("ـ", "");
            t = t.Replace("أ", "ا").Replace("إ", "ا").Replace("آ", "ا").Replace("ٱ", "ا");
            t = t.Replace("ى", "ي").Replace("ؤ", "و").Replace("ئ", "ي").Replace("ة", "ه");
            t = t.Replace("ک", "ك");
            // Common Arabic country/official prefixes that often appear in CVs
            t = t.Replace("المملكة العربية السعودية", "السعودية");
            t = t.Replace("المملكة العربية", "");
            t = t.Replace("المملكة", "");
            t = t.Replace("الجمهورية العربية", "");
            t = t.Replace("العربية", "");
            // collapse multiple spaces
            while (t.Contains("  ")) t = t.Replace("  ", " ");
            return t.Trim();
        }

        static bool IsOtherText(string? s)
        {
            var n = Normalize(s);
            return n is "other" or "اخرى";
        }

        var norm = Normalize(trimmed);
        var compactNorm = norm.Replace(" ", string.Empty);
        var allowOtherDirectMatch = IsOtherText(trimmed);

        // Never auto-match to Other unless parsed value itself is Other.
        var candidates = allowOtherDirectMatch
            ? items
            : items.Where(x => !IsOtherText(nameSelector(x)) && !IsOtherText(nameArSelector(x))).ToList();

        // Try exact match on Name (English, case-insensitive)
        var match = candidates.FirstOrDefault(x =>
            string.Equals(nameSelector(x), trimmed, StringComparison.OrdinalIgnoreCase));

        // Try exact match on NameAr (Arabic, case-insensitive)
        match ??= candidates.FirstOrDefault(x =>
        {
            var ar = nameArSelector(x);
            return ar != null && string.Equals(ar, trimmed, StringComparison.OrdinalIgnoreCase);
        });

        // Try normalized exact match (handles common Arabic variants)
        match ??= candidates.FirstOrDefault(x =>
            Normalize(nameSelector(x)).Equals(norm, StringComparison.OrdinalIgnoreCase)
            || (nameArSelector(x) != null && Normalize(nameArSelector(x)).Equals(norm, StringComparison.OrdinalIgnoreCase)));

        // Try compact exact match (space-insensitive)
        match ??= candidates.FirstOrDefault(x =>
            Normalize(nameSelector(x)).Replace(" ", string.Empty).Equals(compactNorm, StringComparison.OrdinalIgnoreCase)
            || (nameArSelector(x) != null && Normalize(nameArSelector(x)).Replace(" ", string.Empty).Equals(compactNorm, StringComparison.OrdinalIgnoreCase)));

        // Try contains match on Name (partial, e.g. "Bachelor of Science" matches "Bachelor")
        if (match == null && norm.Length >= 3)
        {
            match = candidates.FirstOrDefault(x =>
                trimmed.Contains(nameSelector(x), StringComparison.OrdinalIgnoreCase)
                || nameSelector(x).Contains(trimmed, StringComparison.OrdinalIgnoreCase)
                || Normalize(nameSelector(x)).Contains(norm)
                || norm.Contains(Normalize(nameSelector(x)))
                || Normalize(nameSelector(x)).Replace(" ", string.Empty).Contains(compactNorm)
                || compactNorm.Contains(Normalize(nameSelector(x)).Replace(" ", string.Empty)));
        }

        // Try contains match on NameAr (partial) or normalized Arabic contains
        if (match == null && norm.Length >= 3)
        {
            match = candidates.FirstOrDefault(x =>
            {
                var ar = nameArSelector(x);
                var normAr = Normalize(ar);
                return (ar != null && (trimmed.Contains(ar, StringComparison.OrdinalIgnoreCase)
                    || ar.Contains(trimmed, StringComparison.OrdinalIgnoreCase)))
                    || normAr.Contains(norm)
                    || norm.Contains(normAr)
                    || normAr.Replace(" ", string.Empty).Contains(compactNorm)
                    || compactNorm.Contains(normAr.Replace(" ", string.Empty));
            });
        }

        if (match != null)
        {
            result.ResolvedId = idSelector(match);
            result.MatchedName = nameSelector(match);
            result.IsExactMatch = true;

            // If matched "Other", mark as not exact
            if (result.MatchedName == "Other")
                result.IsExactMatch = false;

            return result;
        }

        // Fallback to "Other" row
        var other = items.FirstOrDefault(x => nameSelector(x) == "Other");
        if (other != null)
        {
            result.ResolvedId = idSelector(other);
            result.MatchedName = "Other";
            result.IsExactMatch = false;
        }

        return result;
    }

    /// <summary>
    /// Match currency by Code, Name, or NameAr.
    /// </summary>
    private static LookupMatchResult MatchCurrency(
        List<Currency> currencies, string? value, string fieldName)
    {
        var result = new LookupMatchResult
        {
            TableName = "Currencies",
            FieldName = fieldName,
            OriginalValue = value
        };

        if (string.IsNullOrWhiteSpace(value))
            return result;

        var trimmed = value.Trim();

        // Match by Code first (e.g. "SAR", "USD")
        var match = currencies.FirstOrDefault(c =>
            string.Equals(c.Code, trimmed, StringComparison.OrdinalIgnoreCase));

        // Then by Name
        match ??= currencies.FirstOrDefault(c =>
            string.Equals(c.Name, trimmed, StringComparison.OrdinalIgnoreCase));

        // Then by NameAr
        match ??= currencies.FirstOrDefault(c =>
            c.NameAr != null && string.Equals(c.NameAr, trimmed, StringComparison.OrdinalIgnoreCase));

        // Partial match
        match ??= currencies.FirstOrDefault(c =>
            c.Name.Contains(trimmed, StringComparison.OrdinalIgnoreCase)
            || trimmed.Contains(c.Name, StringComparison.OrdinalIgnoreCase)
            || trimmed.Contains(c.Code, StringComparison.OrdinalIgnoreCase));

        if (match != null)
        {
            result.ResolvedId = match.CurrencyId;
            result.MatchedName = $"{match.Code} - {match.Name}";
            result.IsExactMatch = match.Name != "Other";
            return result;
        }

        // Fallback to "Other"
        var other = currencies.FirstOrDefault(c => c.Name == "Other");
        if (other != null)
        {
            result.ResolvedId = other.CurrencyId;
            result.MatchedName = "Other";
            result.IsExactMatch = false;
        }

        return result;
    }
}
