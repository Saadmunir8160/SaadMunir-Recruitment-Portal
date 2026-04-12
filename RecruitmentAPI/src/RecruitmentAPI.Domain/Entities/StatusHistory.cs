using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RecruitmentAPI.Domain.Entities;

[Table("StatusHistories")]
public class StatusHistory
{
    [Key]
    public long StatusHistoryId { get; set; }

    [Required, MaxLength(50)]
    public string EntityType { get; set; } = string.Empty;

    public long EntityId { get; set; }

    public byte? OldStatus { get; set; }

    public byte NewStatus { get; set; }

    [MaxLength(50)]
    public string? OldStatusName { get; set; }

    [MaxLength(50)]
    public string? NewStatusName { get; set; }

    [MaxLength(450)]
    public string? ChangedByUserId { get; set; }

    [MaxLength(200)]
    public string? ChangedByName { get; set; }

    [MaxLength(500)]
    public string? Reason { get; set; }

    public string? AdditionalData { get; set; }

    public DateTime ChangedDate { get; set; } = DateTime.UtcNow;
}
