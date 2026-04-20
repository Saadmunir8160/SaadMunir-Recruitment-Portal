using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RecruitmentAPI.Domain.Entities.Lookups;

[Table("Currencies")]
public class Currency
{
    [Key]
    public int CurrencyId { get; set; }

    [Required, MaxLength(3)]
    public string Code { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? NameAr { get; set; }

    [MaxLength(5)]
    public string? Symbol { get; set; }

    public bool IsActive { get; set; } = true;

    public bool IsCustom { get; set; } = false;
}
