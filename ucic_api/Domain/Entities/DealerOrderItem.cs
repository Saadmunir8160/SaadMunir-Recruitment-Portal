using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    public class DealerOrderItem : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int OrderItemID { get; set; }
        [ForeignKey("DealerOrder")]
        public int DealerOrderID { get; set; }
        [ForeignKey("DealerProduct")]
        public int DealerProductID { get; set; }
        [MaxLength(50)]
        public string? Product_LnCode { get; set; }
        public string? ProductDescription { get; set; }
        public decimal? Quantity { get; set; }
        
        [Required, MaxLength(10)]
        public string Unit { get; set; } = "BAG"; // Unit of measurement: BAG, TON, EA, TRK, PCS
        
        public virtual DealerOrder DealerOrder { get; set; }
        public virtual DealerProduct DealerProduct { get; set; }
    }
}