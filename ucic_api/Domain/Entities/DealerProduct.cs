using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace Domain.Entities
{
    public class DealerProduct : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int DealerProductID { get; set; }
        [ForeignKey("Dealer")]
        public int? DealerID { get; set; }
        [Required, MaxLength(100)]
        public string ProductName { get; set; }
        [MaxLength(50)]
        public string? ProductCode { get; set; }
        [MaxLength(50)]
        public string? Product_LnCode { get; set; }
        public string? Description { get; set; }
        
        [Required, MaxLength(10)]
        public string Unit { get; set; } = "BAG"; // Unit of measurement: BAG, TON, EA, TRK, PCS
        
        public virtual Dealer? Dealer { get; set; }
        public virtual ICollection<DealerOrderItem> DealerOrderItems { get; set; } = new List<DealerOrderItem>();
    }
}