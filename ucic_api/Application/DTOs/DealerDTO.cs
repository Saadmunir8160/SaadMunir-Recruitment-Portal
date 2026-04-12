using System.ComponentModel.DataAnnotations;

namespace Application.DTOs
{
    public class DealerDTO
    {
        public int DealerId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string DealerName { get; set; } = string.Empty;
        public decimal? CreditLimit { get; set; }
        public decimal? CurrentBalance { get; set; }
        public string? Ln_ID { get; set; }
        public bool IsActive { get; set; }
        
        // User information from Identity
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? FullName { get; set; }
        public string? UserName { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
    }

    public class CreateDealerDTO
    {
        [Required]
        public string UserId { get; set; } = string.Empty;
        [Required]
        public string DealerName { get; set; } = string.Empty;
        public decimal? CreditLimit { get; set; }
        public decimal? CurrentBalance { get; set; }
        public string? Ln_ID { get; set; }
        public bool IsActive { get; set; }
    }

    public class UpdateDealerDTO : CreateDealerDTO
    {
        [Required]
        public int DealerId { get; set; }
    }
}