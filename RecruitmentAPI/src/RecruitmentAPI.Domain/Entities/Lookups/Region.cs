using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RecruitmentAPI.Domain.Entities.Lookups;

[Table("Regions")]
public class Region
{
    [Key]
    public int RegionId { get; set; }

    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? NameAr { get; set; }

    public int? CountryId { get; set; }

    public bool IsActive { get; set; } = true;

    public bool IsCustom { get; set; } = false;

    // Navigation
    [ForeignKey(nameof(CountryId))]
    public Country? Country { get; set; }

    public ICollection<City> Cities { get; set; } = [];
}
