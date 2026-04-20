using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RecruitmentAPI.Domain.Entities.Lookups;

[Table("DistrictCodes")]
public class DistrictCode
{
    [Key]
    public int DistrictCodeId { get; set; }

    [Required, MaxLength(50)]
    public string Code { get; set; } = string.Empty;

    public int DistrictId { get; set; }

    public bool IsActive { get; set; } = true;

    public bool IsCustom { get; set; } = false;

    // Navigation
    [ForeignKey(nameof(DistrictId))]
    public District District { get; set; } = null!;
}
