using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RecruitmentAPI.Domain.Entities.Lookups;

[Table("Countries")]
public class Country
{
    [Key]
    public int CountryId { get; set; }

    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? NameAr { get; set; }

    [Required, MaxLength(3)]
    public string Iso3Code { get; set; } = string.Empty;

    [MaxLength(2)]
    public string? Iso2Code { get; set; }

    [MaxLength(10)]
    public string? PhoneCode { get; set; }

    public bool IsActive { get; set; } = true;

    public bool IsCustom { get; set; } = false;

    // Navigation
    public ICollection<Region> Regions { get; set; } = [];
    public ICollection<City> Cities { get; set; } = [];
    public ICollection<Nationality> Nationalities { get; set; } = [];
    public ICollection<Institution> Institutions { get; set; } = [];
}
