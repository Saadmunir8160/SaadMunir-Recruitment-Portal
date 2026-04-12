using System.Text.Json;
using System.Text.Json.Serialization;

namespace RecruitmentAPI.Application.Common.Interfaces;

/// <summary>
/// Same as NullableIntFromStringConverter but for nullable decimal.
/// </summary>
internal sealed class NullableDecimalFromStringConverter : JsonConverter<decimal?>
{
    public override decimal? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
            return null;

        if (reader.TokenType == JsonTokenType.Number)
            return reader.GetDecimal();

        if (reader.TokenType == JsonTokenType.String)
        {
            var s = reader.GetString();
            if (string.IsNullOrWhiteSpace(s)) return null;
            if (decimal.TryParse(s, System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out var parsed))
                return parsed;
            return null;
        }

        throw new JsonException($"Cannot convert token type '{reader.TokenType}' to decimal?.");
    }

    public override void Write(Utf8JsonWriter writer, decimal? value, JsonSerializerOptions options)
    {
        if (value is null) writer.WriteNullValue();
        else writer.WriteNumberValue(value.Value);
    }
}


internal sealed class NullableIntFromStringConverter : JsonConverter<int?>
{
    public override int? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
            return null;

        if (reader.TokenType == JsonTokenType.Number)
            return reader.GetInt32();

        if (reader.TokenType == JsonTokenType.String)
        {
            var s = reader.GetString();
            if (string.IsNullOrWhiteSpace(s)) return null;
            if (int.TryParse(s, out var parsed)) return parsed;
            return null;
        }

        throw new JsonException($"Cannot convert token type '{reader.TokenType}' to int?.");
    }

    public override void Write(Utf8JsonWriter writer, int? value, JsonSerializerOptions options)
    {
        if (value is null) writer.WriteNullValue();
        else writer.WriteNumberValue(value.Value);
    }
}

/// <summary>
/// Structured data returned by Claude after parsing a CV.
/// Matches the JSON schema in the Claude prompt.
/// </summary>
public class CvParseResult
{
    [JsonPropertyName("personalInfo")]
    public CvPersonalInfo? PersonalInfo { get; set; }

    [JsonPropertyName("education")]
    public List<CvEducation> Education { get; set; } = [];

    [JsonPropertyName("experience")]
    public List<CvExperience> Experience { get; set; } = [];

    [JsonPropertyName("skills")]
    public List<string> Skills { get; set; } = [];

    [JsonPropertyName("certifications")]
    public List<CvCertification> Certifications { get; set; } = [];

    [JsonPropertyName("languages")]
    public List<CvLanguage> Languages { get; set; } = [];

    [JsonPropertyName("summary")]
    public string? Summary { get; set; }

    [JsonPropertyName("totalYearsOfExperience")]
    [JsonConverter(typeof(NullableDecimalFromStringConverter))]
    public decimal? TotalYearsOfExperience { get; set; }

    [JsonPropertyName("overallConfidence")]
    public decimal OverallConfidence { get; set; }

    /// <summary>Raw JSON string from Claude — stored in CandidateDocuments.AiRawResponseJson</summary>
    public string? RawJson { get; set; }
}

public class CvPersonalInfo
{
    [JsonPropertyName("fullName")]
    public string? FullName { get; set; }

    [JsonPropertyName("email")]
    public string? Email { get; set; }

    [JsonPropertyName("phone")]
    public string? Phone { get; set; }

    [JsonPropertyName("nationality")]
    public string? Nationality { get; set; }

    [JsonPropertyName("dateOfBirth")]
    public string? DateOfBirth { get; set; }

    [JsonPropertyName("nationalId")]
    public string? NationalId { get; set; }

    [JsonPropertyName("address")]
    public string? Address { get; set; }

    [JsonPropertyName("city")]
    public string? City { get; set; }

    [JsonPropertyName("confidence")]
    public decimal Confidence { get; set; }
}

public class CvEducation
{
    [JsonPropertyName("qualification")]
    public string? Qualification { get; set; }

    [JsonPropertyName("major")]
    public string? Major { get; set; }

    [JsonPropertyName("institution")]
    public string? Institution { get; set; }

    [JsonPropertyName("graduationYear")]
    [JsonConverter(typeof(NullableIntFromStringConverter))]
    public int? GraduationYear { get; set; }

    [JsonPropertyName("grade")]
    public string? Grade { get; set; }

    [JsonPropertyName("country")]
    public string? Country { get; set; }

    [JsonPropertyName("confidence")]
    public decimal Confidence { get; set; }
}

public class CvExperience
{
    [JsonPropertyName("employer")]
    public string? Employer { get; set; }

    [JsonPropertyName("jobTitle")]
    public string? JobTitle { get; set; }

    [JsonPropertyName("startDate")]
    public string? StartDate { get; set; }

    [JsonPropertyName("endDate")]
    public string? EndDate { get; set; }

    [JsonPropertyName("isCurrent")]
    public bool IsCurrent { get; set; }

    [JsonPropertyName("salary")]
    [JsonConverter(typeof(NullableDecimalFromStringConverter))]
    public decimal? Salary { get; set; }

    [JsonPropertyName("currency")]
    public string? Currency { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("country")]
    public string? Country { get; set; }

    [JsonPropertyName("confidence")]
    public decimal Confidence { get; set; }
}

public class CvCertification
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("issuer")]
    public string? Issuer { get; set; }

    [JsonPropertyName("date")]
    public string? Date { get; set; }
}

public class CvLanguage
{
    [JsonPropertyName("language")]
    public string? Language { get; set; }

    [JsonPropertyName("proficiency")]
    public string? Proficiency { get; set; }
}
