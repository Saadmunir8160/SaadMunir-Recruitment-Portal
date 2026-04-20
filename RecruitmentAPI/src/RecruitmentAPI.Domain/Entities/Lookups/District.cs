using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RecruitmentAPI.Domain.Entities.Lookups;

[Table("Districts")]
public class District
{
    [Key]
    public int DistrictId { get; set; }

    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? NameAr { get; set; }

    public int CityId { get; set; }

    public bool IsActive { get; set; } = true;

    public bool IsCustom { get; set; } = false;

    // Navigation
    [ForeignKey(nameof(CityId))]
    public City City { get; set; } = null!;

    public ICollection<DistrictCode> DistrictCodes { get; set; } = [];
}
