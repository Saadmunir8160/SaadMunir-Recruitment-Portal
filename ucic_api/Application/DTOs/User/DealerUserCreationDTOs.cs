using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.User
{
    public class CreateDealerUserDTO
    {
        [Required(ErrorMessage = "FullName is required")]
        [StringLength(100, ErrorMessage = "FullName cannot exceed 100 characters")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "UserName is required")]
        [StringLength(100, ErrorMessage = "UserName cannot exceed 100 characters")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address format.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "PhoneNo is required")]
        [Phone(ErrorMessage = "Invalid Phone Number format.")]
        public string Phone { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be between 6 and 100 characters")]
        public string Password { get; set; }

        [Required(ErrorMessage = "ConfirmationPassword is required")]
        [Compare("Password", ErrorMessage = "Password and confirmation password do not match")]
        public string ConfirmationPassword { get; set; }

        [Required(ErrorMessage = "DealerName is required")]
        [StringLength(100, ErrorMessage = "DealerName cannot exceed 100 characters")]
        public string DealerName { get; set; }
        
        [Range(0, double.MaxValue, ErrorMessage = "CreditLimit must be a positive number")]
        public decimal? CreditLimit { get; set; }
        
        [Range(0, double.MaxValue, ErrorMessage = "CurrentBalance must be a positive number")]
        public decimal? CurrentBalance { get; set; }
        
        [MaxLength(50, ErrorMessage = "Ln_ID cannot exceed 50 characters")]
        public string? Ln_ID { get; set; }
    }

    public class CreateDealerDriverUserDTO
    {
        [Required(ErrorMessage = "FullName is required")]
        [StringLength(100, ErrorMessage = "FullName cannot exceed 100 characters")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "UserName is required")]
        [StringLength(100, ErrorMessage = "UserName cannot exceed 100 characters")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address format.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "PhoneNo is required")]
        [Phone(ErrorMessage = "Invalid Phone Number format.")]
        public string Phone { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be between 6 and 100 characters")]
        public string Password { get; set; }

        [Required(ErrorMessage = "ConfirmationPassword is required")]
        [Compare("Password", ErrorMessage = "Password and confirmation password do not match")]
        public string ConfirmationPassword { get; set; }

        [Required(ErrorMessage = "DealerID is required")]
        public int DealerID { get; set; }
        
        [MaxLength(50, ErrorMessage = "Ln_ID cannot exceed 50 characters")]
        public string? Ln_ID { get; set; }
        
        [MaxLength(50, ErrorMessage = "IqamaNumber cannot exceed 50 characters")]
        public string? IqamaNumber { get; set; }
    }

    public class DealerUserResponseDTO
    {
        public string UserId { get; set; }
        public int DealerId { get; set; }
        public string DealerName { get; set; }
        public string FullName { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public decimal? CreditLimit { get; set; }
        public decimal? CurrentBalance { get; set; }
        public string? Ln_ID { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class DealerDriverUserResponseDTO
    {
        public string UserId { get; set; }
        public int DriverID { get; set; }
        public int DealerID { get; set; }
        public string DealerName { get; set; }
        public string FullName { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string? Ln_ID { get; set; }
        public string? IqamaNumber { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}