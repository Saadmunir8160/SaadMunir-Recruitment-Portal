using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Domain.Entities
{
    public class DealerNotification : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int NotificationId { get; set; }

        [ForeignKey("Dealer")]
        public int DealerId { get; set; }

        [Required, MaxLength(50)]
        public string Type { get; set; }

        [Required, MaxLength(200)]
        public string Title { get; set; }

        [Required]
        public string Message { get; set; }

        public bool IsRead { get; set; }

        [MaxLength(500)]
        public string? ActionUrl { get; set; }

        [MaxLength(20)]
        public string Priority { get; set; } = "medium";

        [JsonIgnore]
        public virtual Dealer Dealer { get; set; }
    }



    public class DealerFeedback : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int FeedbackId { get; set; }

        [ForeignKey("Dealer")]
        public int DealerId { get; set; }

        [Range(1, 5)]
        public int Rating { get; set; }

        [MaxLength(1000)]
        public string? Feedback { get; set; }

        [MaxLength(50)]
        public string Category { get; set; }

        [JsonIgnore]
        public virtual Dealer Dealer { get; set; }
    }
}