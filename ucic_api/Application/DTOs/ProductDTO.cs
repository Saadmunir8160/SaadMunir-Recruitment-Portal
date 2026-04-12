using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs
{
    public class ProductDTO
    {
        public long ProductId { get; set; }
        public long CoverageAreaId { get; set; }
        public string? CoverageAreaName { get; set; }
        public string? ProductName { get; set; }
        public string? ArabicName { get; set; }
        public string? ProductDescription { get; set; }
        public string? ArabicDescription { get; set; }
        public string? Sku { get; set; }
        public string? Code { get; set; }
        public string? Currency { get; set; }
        public string? ImageUrl { get; set; }
        public decimal Price { get; set; }
        public decimal ShippingCostPercentage { get; set; }
        public decimal VatPercentage { get; set; }
        public decimal DiscountPercentage { get; set; }
        public string? Type { get; set; }
    }
}
