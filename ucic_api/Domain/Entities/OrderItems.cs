using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Domain.Entities
{
    public class OrderItems : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long OrderItemsId { get; set; }

        [ForeignKey("Order")]
        public long OrderId { get; set; }

        [ForeignKey("Product")]
        public long ProductId { get; set; }

        [Required]
        public long Quantity { get; set; }

        [Required]
        public decimal Price { get; set; }

        [Required]
        public int NumberOfTrucks { get; set; }

        public virtual Order Order { get; set; }
        public virtual Product Product { get; set; }
    }

}
