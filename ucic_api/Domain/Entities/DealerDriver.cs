using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace Domain.Entities
{
    public class DealerDriver : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int DriverID { get; set; }

        [Required]
        [ForeignKey("User")]
        [MaxLength(450)]
        public string UserId { get; set; } = string.Empty; // Maps to ApplicationUserId column in database
        
        [ForeignKey("Dealer")]
        public int DealerID { get; set; }
        
        [MaxLength(50)]
        public string? Ln_ID { get; set; }
        [MaxLength(50)]
        public string? IqamaNumber { get; set; }
        
        public virtual Dealer Dealer { get; set; } = null!;
        public virtual ICollection<DealerOrder> DealerOrders { get; set; } = new List<DealerOrder>();
        public virtual ICollection<DealerDriverLog> DealerDriverLogs { get; set; } = new List<DealerDriverLog>();
    }
}