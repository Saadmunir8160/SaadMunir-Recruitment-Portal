using System;

namespace Application.DTOs
{
    public class PromotionDto
    {
        public long PromotionId { get; set; }
        public long CoverageAreaId { get; set; }
        public string? Code { get; set; }
        public decimal DiscountPercentage { get; set; }
        public DateTime ValidFrom { get; set; }
        public DateTime ValidTo { get; set; }
    }
} 