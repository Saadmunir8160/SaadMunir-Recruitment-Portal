namespace RecruitmentAPI.Application.DTOs.Lookups;

/// <summary>
/// A single row from any lookup table, suitable for populating frontend dropdowns.
/// </summary>
public class LookupItemDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? NameAr { get; set; }
    public int? ParentId { get; set; }
}

/// <summary>
/// Result of matching a single string value against a lookup table.
/// </summary>
public class LookupMatchResult
{
    public string TableName { get; set; } = string.Empty;
    public string FieldName { get; set; } = string.Empty;
    public string? OriginalValue { get; set; }
    public int? ResolvedId { get; set; }
    public string? MatchedName { get; set; }
    public bool IsExactMatch { get; set; }
}

/// <summary>
/// Complete resolution result for a candidate's CV-parsed data.
/// </summary>
public class CandidateResolutionResultDto
{
    public long CandidateId { get; set; }
    public int TotalFields { get; set; }
    public int ExactMatches { get; set; }
    public int FallbackToOther { get; set; }
    public int Skipped { get; set; }
    public List<LookupMatchResult> Results { get; set; } = [];
}

/// <summary>
/// Request to resolve a single string against a specific lookup table.
/// </summary>
public class ResolveSingleValueRequest
{
    public string TableName { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public int? ParentId { get; set; }
}

/// <summary>
/// Request to manually update a resolved lookup for a candidate.
/// </summary>
public class UpdateCandidateLookupRequest
{
    public string FieldName { get; set; } = string.Empty;                                       
    public int LookupId { get; set; }
    public long? RecordId { get; set; }
}

/// <summary>
/// Represents the personal information of a candidate.
/// </summary>
public class PersonalInfoDto
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Nationality { get; set; } = string.Empty; // Arabic or English string
    public string City { get; set; } = string.Empty; // Arabic or English string
    public string Address { get; set; } = string.Empty;
}

/// <summary>
/// Represents an education record of a candidate.
/// </summary>
public class EducationDto
{
    public string Qualification { get; set; } = string.Empty;
    public string Major { get; set; } = string.Empty;
    public string Institution { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public DateTime? GraduationYear { get; set; } // YYYY-MM-DD or null
    public string? Grade { get; set; } // Nullable grade
}

/// <summary>
/// Represents an experience record of a candidate.
/// </summary>
public class ExperienceDto
{
    public string Employer { get; set; } = string.Empty;
    public string JobTitle { get; set; } = string.Empty;
    public DateTime StartDate { get; set; } // YYYY-MM-DD
    public DateTime? EndDate { get; set; } // YYYY-MM-DD or null
    public bool IsCurrent { get; set; }
    public decimal? Salary { get; set; } // Nullable salary
    public string? Currency { get; set; } // Nullable currency
    public string Description { get; set; } = string.Empty;
    public string? Country { get; set; } // Nullable country
}

/// <summary>
/// Response containing the parsed data from a CV.
/// </summary>
public class ParseCvPreviewResponse
{
    public bool Success { get; set; }
    public CvDataDto Data { get; set; } = null!;
}

/// <summary>
/// Represents the entire CV data transfer object.
/// </summary>
public class CvDataDto
{
    public PersonalInfoDto PersonalInfo { get; set; } = null!;
    public List<EducationDto> Education { get; set; } = new();
    public List<ExperienceDto> Experience { get; set; } = new();
    public List<string> Skills { get; set; } = new();
    public List<LanguageDto> Languages { get; set; } = new();
    public string Summary { get; set; } = string.Empty;
    public int OverallConfidence { get; set; }
}

/// <summary>
/// Represents a language and its proficiency.
/// </summary>
public class LanguageDto
{
    public string Language { get; set; } = string.Empty;
    public string Proficiency { get; set; } = string.Empty;
}
