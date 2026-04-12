using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Product : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long ProductId { get; set; }

        [ForeignKey("CoverageArea")]
        public long CoverageAreaId { get; set; }

        [Required, MaxLength(100)]
        public string? Name { get; set; }

        [MaxLength(100)]
        public string? ArabicName { get; set; }

        public string? Description { get; set; }
        public string? ArabicDescription { get; set; }

        [Required, MaxLength(30)]
        public string? Sku { get; set; }

        [Required, MaxLength(100)]
        public string? Code { get; set; }

        public string? Currency { get; set; }

        [MaxLength(1000)]
        public string? ImageUrl { get; set; }

        [Required]
        public decimal Price { get; set; }

        [Required]
        public decimal ShippingCostPercentage { get; set; }

        [Required]
        public decimal VatPercentage { get; set; }

        [Required]
        public decimal DiscountPercentage { get; set; }

        [Required, MaxLength(30)]
        public string? Type { get; set; }


        public virtual CoverageArea CoverageArea { get; set; }
        public virtual ICollection<OrderItems> OrderItems { get; set; }
    }
}
