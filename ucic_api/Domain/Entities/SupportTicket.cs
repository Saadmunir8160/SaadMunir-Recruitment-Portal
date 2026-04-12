using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Domain.Entities
{
    public class SupportTicket : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int TicketId { get; set; }

        [Required, MaxLength(50)]
        public string TicketNumber { get; set; }

        [Required, MaxLength(200)]
        public string Subject { get; set; }

        [Required]
        public string Description { get; set; }

        [Required, MaxLength(50)]
        public string Category { get; set; }

        [Required, MaxLength(20)]
        public string Priority { get; set; }

        [Required, MaxLength(50)]
        public string Status { get; set; }

        [ForeignKey("Dealer")]
        public int DealerId { get; set; }

        [MaxLength(100)]
        public string? AssignedTo { get; set; }

        public DateTime? ResolvedAt { get; set; }

        [ForeignKey("Order")]
        public long? OrderId { get; set; }

        [ForeignKey("Product")]
        public long? ProductId { get; set; }

        [JsonIgnore]
        public virtual Dealer Dealer { get; set; }
        
        [JsonIgnore]
        public virtual Order? Order { get; set; }
        
        [JsonIgnore]
        public virtual Product? Product { get; set; }
        
        public virtual ICollection<SupportTicketAttachment> Attachments { get; set; } = new List<SupportTicketAttachment>();
        public virtual ICollection<SupportTicketMessage> Messages { get; set; } = new List<SupportTicketMessage>();
    }

    public class SupportTicketAttachment : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int AttachmentId { get; set; }

        [ForeignKey("SupportTicket")]
        public int TicketId { get; set; }

        [Required, MaxLength(255)]
        public string FileName { get; set; }

        [Required, MaxLength(500)]
        public string FilePath { get; set; }

        public long FileSize { get; set; }

        public DateTime UploadedAt { get; set; }

        [JsonIgnore]
        public virtual SupportTicket SupportTicket { get; set; }
    }

    public class SupportTicketMessage : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MessageId { get; set; }

        [ForeignKey("SupportTicket")]
        public int TicketId { get; set; }

        [Required]
        public string Message { get; set; }

        public bool IsFromDealer { get; set; }

        [Required, MaxLength(100)]
        public string SenderName { get; set; }

        public DateTime SentAt { get; set; }

        [JsonIgnore]
        public virtual SupportTicket SupportTicket { get; set; }
        
        public virtual ICollection<SupportTicketMessageAttachment> Attachments { get; set; } = new List<SupportTicketMessageAttachment>();
    }

    public class SupportTicketMessageAttachment : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int AttachmentId { get; set; }

        [ForeignKey("SupportTicketMessage")]
        public int MessageId { get; set; }

        [Required, MaxLength(255)]
        public string FileName { get; set; }

        [Required, MaxLength(500)]
        public string FilePath { get; set; }

        public long FileSize { get; set; }

        public DateTime UploadedAt { get; set; }

        [JsonIgnore]
        public virtual SupportTicketMessage SupportTicketMessage { get; set; }
    }
}