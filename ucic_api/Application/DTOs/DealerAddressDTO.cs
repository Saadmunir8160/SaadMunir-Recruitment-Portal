using System.ComponentModel.DataAnnotations;

namespace Application.DTOs
{
    public class DealerAddressDTO
    {
        public int AddressId { get; set; }
        public int DealerId { get; set; }
        public string Name { get; set; }
        public string ContactPerson { get; set; }
        public string Street { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string PostalCode { get; set; }
        public string Country { get; set; }
        public string? Phone { get; set; }
        public bool IsDefault { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
    }

    public class CreateDealerAddressDTO
    {
        [Required]
        public int DealerId { get; set; }
        
        [Required, MaxLength(100)]
        public string Name { get; set; }
        
        [Required, MaxLength(100)]
        public string ContactPerson { get; set; }
        
        [Required, MaxLength(500)]
        public string Street { get; set; }
        
        [Required, MaxLength(100)]
        public string City { get; set; }
        
        [Required, MaxLength(100)]
        public string State { get; set; }
        
        [Required, MaxLength(20)]
        public string PostalCode { get; set; }
        
        [Required, MaxLength(100)]
        public string Country { get; set; }
        
        [MaxLength(20)]
        public string? Phone { get; set; }
        
        public bool IsDefault { get; set; } = false;
        public bool IsActive { get; set; } = true;
    }

    public class UpdateDealerAddressDTO : CreateDealerAddressDTO
    {
        [Required]
        public int AddressId { get; set; }
    }
}