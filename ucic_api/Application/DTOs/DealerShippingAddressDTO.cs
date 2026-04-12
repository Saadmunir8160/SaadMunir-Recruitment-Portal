using System.ComponentModel.DataAnnotations;

namespace Application.DTOs
{
    public class DealerShippingAddressDTO
    {
        public int AddressID { get; set; }
        public int DealerID { get; set; }
        public string? Ln_ID { get; set; }
        public string? AddressLine1 { get; set; }
        public string? AddressLine2 { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? Country { get; set; }
        public string? PostalCode { get; set; }
        public bool IsActive { get; set; }
    }

    public class CreateDealerShippingAddressDTO
    {
        [Required]
        public int DealerID { get; set; }
        public string? Ln_ID { get; set; }
        public string? AddressLine1 { get; set; }
        public string? AddressLine2 { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? Country { get; set; }
        public string? PostalCode { get; set; }
        public bool IsActive { get; set; }
    }

    public class UpdateDealerShippingAddressDTO : CreateDealerShippingAddressDTO
    {
        [Required]
        public int AddressID { get; set; }
    }
}