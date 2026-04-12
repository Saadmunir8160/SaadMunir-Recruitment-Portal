using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Support
{
    public class CreateSupportTicketDTO
    {
        [Required]
        public string Subject { get; set; }
        
        [Required]
        public string Description { get; set; }
        
        [Required]
        public string Category { get; set; }
        
        [Required]
        public string Priority { get; set; }
        
        public long? OrderId { get; set; }
        public long? ProductId { get; set; }
    }

    public class SupportTicketDTO
    {
        public int Id { get; set; }
        public string TicketNumber { get; set; }
        public string Subject { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public string Priority { get; set; }
        public string Status { get; set; }
        public int DealerId { get; set; }
        public string? AssignedTo { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? ResolvedAt { get; set; }
        public long? OrderId { get; set; }
        public long? ProductId { get; set; }
        public List<SupportTicketAttachmentDTO> Attachments { get; set; } = new();
        public int MessageCount { get; set; }
    }

    public class SupportTicketDetailDTO : SupportTicketDTO
    {
        public List<SupportTicketMessageDTO> Messages { get; set; } = new();
    }

    public class SupportTicketAttachmentDTO
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public long FileSize { get; set; }
        public DateTime UploadedAt { get; set; }
    }

    public class SupportTicketMessageDTO
    {
        public int Id { get; set; }
        public int TicketId { get; set; }
        public string Message { get; set; }
        public bool IsFromDealer { get; set; }
        public string SenderName { get; set; }
        public DateTime SentAt { get; set; }
        public List<SupportTicketAttachmentDTO> Attachments { get; set; } = new();
    }

    public class AddMessageToTicketDTO
    {
        [Required]
        public string Message { get; set; }
    }

    public class UpdateTicketStatusDTO
    {
        [Required]
        public string Status { get; set; }
    }

    public class CreateTicketResponseDTO
    {
        public int TicketId { get; set; }
        public string TicketNumber { get; set; }
        public string Message { get; set; }
    }

    public class AddMessageResponseDTO
    {
        public int MessageId { get; set; }
        public string Message { get; set; }
    }

    public class SupportContactInfoDTO
    {
        public string SupportEmail { get; set; } = "support@ucicautoparts.com";
        public string SupportPhone { get; set; } = "+91-124-4567890";
        public string EmergencyPhone { get; set; } = "+91-98765-43210";
        public string BusinessHours { get; set; } = "Mon-Fri: 9:00 AM - 6:00 PM";
        public string Address { get; set; } = "UCIC Auto Parts, Industrial Area, Gurgaon";
    }

    public class SupportMetadataDTO
    {
        public List<CategoryOptionDTO> Categories { get; set; } = new();
        public List<PriorityOptionDTO> Priorities { get; set; } = new();
        public List<StatusOptionDTO> Statuses { get; set; } = new();
    }

    public class CategoryOptionDTO
    {
        public string Value { get; set; }
        public string Label { get; set; }
    }

    public class PriorityOptionDTO
    {
        public string Value { get; set; }
        public string Label { get; set; }
        public string Color { get; set; }
    }

    public class StatusOptionDTO
    {
        public string Value { get; set; }
        public string Label { get; set; }
        public string Color { get; set; }
    }
}