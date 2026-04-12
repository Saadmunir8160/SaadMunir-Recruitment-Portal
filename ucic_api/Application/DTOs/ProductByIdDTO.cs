
namespace Application.DTOs
{
    public class ProductByIdDTO
    {
        public long ProductId { get; set; }
        public long CoverageAreaId { get; set; }
        public string? CoverageAreaName { get; set; }
        public string? Name { get; set; }
        public string? ArabicName { get; set; }
        public string? Description { get; set; }
        public string? ArabicDescription { get; set; }
        public string? Sku { get; set; }
        public string? ProductCode { get; set; }
        public string? Currency { get; set; }
        public string? ProductFilePath { get; set; }
        public decimal Price { get; set; }
        public decimal shipingCostPercentage { get; set; }
        public decimal VatPercentage { get; set; }
        public decimal DiscountPercentage { get; set; }
        public string? Type { get; set; }
    }
}
