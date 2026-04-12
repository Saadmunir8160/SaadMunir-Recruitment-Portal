using System.ComponentModel.DataAnnotations;

namespace Application.DTOs
{
    public class DealerVehicleDTO
    {
        public int VehicleID { get; set; }
        public int DealerID { get; set; }
        public string PlateNumber { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public decimal Capacity { get; set; }
        public DateTime RegistrationDate { get; set; }
        public DateTime RegistrationExpiryDate { get; set; }
        public DateTime InsuranceExpiryDate { get; set; }
        public string? Ln_ID { get; set; }
        public string? RegistrationNumber { get; set; }
        public string? VehicleType { get; set; } // Legacy field
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class CreateDealerVehicleDTO
    {
        [Required]
        public int DealerID { get; set; }
        
        [Required]
        [MaxLength(50)]
        public string PlateNumber { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(20)]
        public string Type { get; set; } = string.Empty;
        
        [Required]
        [Range(0.1, 100)]
        public decimal Capacity { get; set; }
        
        [Required]
        public DateTime RegistrationDate { get; set; }
        
        [Required]
        public DateTime RegistrationExpiryDate { get; set; }
        
        [Required]
        public DateTime InsuranceExpiryDate { get; set; }
        
        [MaxLength(50)]
        public string? Ln_ID { get; set; }
        
        [MaxLength(50)]
        public string? RegistrationNumber { get; set; }
        
        public bool IsActive { get; set; } = true;
    }

    public class UpdateDealerVehicleDTO : CreateDealerVehicleDTO
    {
        [Required]
        public int VehicleID { get; set; }
    }

    // For frontend compatibility
    public class VehicleResponseDTO
    {
        public string Id { get; set; } = string.Empty;
        public string DealerId { get; set; } = string.Empty;
        public string PlateNumber { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public decimal Capacity { get; set; }
        public DateTime RegistrationDate { get; set; }
        public DateTime RegistrationExpiryDate { get; set; }
        public DateTime InsuranceExpiryDate { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}