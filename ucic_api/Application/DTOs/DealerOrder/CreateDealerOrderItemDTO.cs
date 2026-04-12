using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.DealerOrder
{
    public class CreateDealerOrderItemDTO
    {
        [Required(ErrorMessage = "DealerOrderID is required")]
        public int DealerOrderID { get; set; }
        
        [Required(ErrorMessage = "DealerProductID is required")]
        public int DealerProductID { get; set; }
        
        [StringLength(50)]
        public string? Product_LnCode { get; set; }
        
        public string? ProductDescription { get; set; }
        
        [Required(ErrorMessage = "Quantity is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
        public decimal Quantity { get; set; }
        
        [Required(ErrorMessage = "Unit is required")]
        [StringLength(10, ErrorMessage = "Unit cannot exceed 10 characters")]
        public required string Unit { get; set; } = "BAG";
    }
}