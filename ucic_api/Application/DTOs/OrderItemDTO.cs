using System.ComponentModel.DataAnnotations;

namespace Application.DTOs
{
    public class OrderItemDTO
    {
        [Required(ErrorMessage = "ProductId is required")]
        public long ProductId { get; set; }

        [Required(ErrorMessage = "Quantity is required")]
        public long Quantity { get; set; }

        [Required(ErrorMessage = "Price is required")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "NumberOfTrucks is required")]
        public int NumberOfTrucks { get; set; }
    }
}
