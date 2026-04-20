using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RecruitmentAPI.Domain.Entities.Lookups;

[Table("MajorFieldsOfStudy")]
public class MajorFieldOfStudy
{
    [Key]
    public int MajorFieldOfStudyId { get; set; }

    [Required, MaxLength(300)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(300)]
    public string? NameAr { get; set; }

    public bool IsActive { get; set; } = true;

    public bool IsCustom { get; set; } = false;
}
