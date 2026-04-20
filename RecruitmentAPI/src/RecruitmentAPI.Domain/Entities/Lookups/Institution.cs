using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RecruitmentAPI.Domain.Entities.Lookups;

[Table("Institutions")]
public class Institution
{
    [Key]
    public int InstitutionId { get; set; }

    [Required, MaxLength(400)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(400)]
    public string? NameAr { get; set; }

    public int CountryId { get; set; }

    public bool IsActive { get; set; } = true;

    public bool IsCustom { get; set; } = false;

    // Navigation
    [ForeignKey(nameof(CountryId))]
    public Country Country { get; set; } = null!;
}
