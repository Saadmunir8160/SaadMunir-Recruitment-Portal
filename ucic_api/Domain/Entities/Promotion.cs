using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    public class Promotion : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long PromotionId { get; set; }

        [ForeignKey("CoverageArea")]
        public long CoverageAreaId { get; set; }

        [Required, MaxLength(50)]
        public string? Code { get; set; }

        [Required]
        public decimal DiscountPercentage { get; set; }

        [Required]
        public DateTime ValidFrom { get; set; }

        [Required]
        public DateTime ValidTo { get; set; }

        // Navigation property
        public virtual CoverageArea CoverageArea { get; set; }
        public virtual ICollection<Order> Orders { get; set; }
    }
}
