using System.ComponentModel.DataAnnotations;

namespace Application.DTOs
{
    public class DealerDriverDTO
    {
        public int DriverID { get; set; }
        public string UserId { get; set; } = string.Empty;
        public int DealerID { get; set; }
        public string? Ln_ID { get; set; }
        public string? IqamaNumber { get; set; }
        public bool IsActive { get; set; }
        
        // User information from ApplicationUser
        public string UserName { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
    }

    public class CreateDealerDriverDTO
    {
        [Required]
        public string UserId { get; set; } = string.Empty;
        // Note: DealerID is automatically set from current user context
        public string? Ln_ID { get; set; }
        public string? IqamaNumber { get; set; }
        public bool IsActive { get; set; }
    }

    public class UpdateDealerDriverDTO : CreateDealerDriverDTO
    {
        [Required]
        public int DriverID { get; set; }
        
        // User information fields for updating the ApplicationUser
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
    }
}