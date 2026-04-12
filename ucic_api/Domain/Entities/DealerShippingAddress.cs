using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace Domain.Entities
{
    public class DealerShippingAddress : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int AddressID { get; set; }
        [ForeignKey("Dealer")]
        public int DealerID { get; set; }
        [MaxLength(50)]
        public string? Ln_ID { get; set; }
        [MaxLength(200)]
        public string? AddressLine1 { get; set; }
        [MaxLength(200)]
        public string? AddressLine2 { get; set; }
        [MaxLength(100)]
        public string? City { get; set; }
        [MaxLength(100)]
        public string? State { get; set; }
        [MaxLength(100)]
        public string? Country { get; set; }
        [MaxLength(20)]
        public string? PostalCode { get; set; }
        public virtual Dealer? Dealer { get; set; }
        public virtual ICollection<DealerOrder> DealerOrders { get; set; } = new List<DealerOrder>();
    }
}