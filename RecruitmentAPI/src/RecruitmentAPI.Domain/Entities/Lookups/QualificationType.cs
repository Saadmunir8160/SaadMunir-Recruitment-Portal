using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RecruitmentAPI.Domain.Entities.Lookups;

[Table("QualificationTypes")]
public class QualificationType
{
    [Key]
    public int QualificationTypeId { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? NameAr { get; set; }

    public bool IsActive { get; set; } = true;

    // Navigation
    public ICollection<Degree> Degrees { get; set; } = [];
    public ICollection<Certificate> Certificates { get; set; } = [];
}
