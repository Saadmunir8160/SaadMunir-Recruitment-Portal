using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RecruitmentAPI.Domain.Entities.Lookups;

[Table("Cities")]
public class City
{
    [Key]
    public int CityId { get; set; }

    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? NameAr { get; set; }

    public int CountryId { get; set; }

    public int? RegionId { get; set; }

    public bool IsActive { get; set; } = true;

    public bool IsCustom { get; set; } = false;

    // Navigation
    [ForeignKey(nameof(CountryId))]
    public Country Country { get; set; } = null!;

    [ForeignKey(nameof(RegionId))]
    public Region? Region { get; set; }

    public ICollection<District> Districts { get; set; } = [];
}
