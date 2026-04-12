using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RecruitmentAPI.Domain.Entities;

[Table("VacancyRecruiters")]
public class VacancyRecruiter : BaseEntity
{
    [Key]
    public long VacancyRecruiterId { get; set; }

    public long VacancyId { get; set; }

    [Required, MaxLength(450)]
    public string RecruiterUserId { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? RecruiterName { get; set; }

    public bool IsPrimary { get; set; } = false;

    public DateTime AssignedDate { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(VacancyId))]
    public Vacancy Vacancy { get; set; } = null!;
}
