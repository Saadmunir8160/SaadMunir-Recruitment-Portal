using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using RecruitmentAPI.Domain.Enums;

namespace RecruitmentAPI.Domain.Entities;

[Table("Notifications")]
public class Notification
{
    [Key]
    public long NotificationId { get; set; }

    [MaxLength(450)]
    public string? RecipientUserId { get; set; }

    [MaxLength(200)]
    public string? RecipientEmail { get; set; }

    [MaxLength(50)]
    public string? RecipientPhone { get; set; }

    public NotificationChannel Channel { get; set; }

    [MaxLength(300)]
    public string? Subject { get; set; }

    public string? Body { get; set; }

    [MaxLength(50)]
    public string? TemplateCode { get; set; }

    [MaxLength(50)]
    public string? EntityType { get; set; }

    public long? EntityId { get; set; }

    public NotificationStatus Status { get; set; } = NotificationStatus.Pending;

    public DateTime? SentDate { get; set; }

    public DateTime? ReadDate { get; set; }

    [MaxLength(500)]
    public string? ErrorMessage { get; set; }

    public int RetryCount { get; set; } = 0;

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
}
