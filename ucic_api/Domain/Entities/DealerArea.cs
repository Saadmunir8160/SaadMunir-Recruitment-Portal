using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace Domain.Entities
{
    public class DealerArea : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int AreaID { get; set; }

        [Required]
        [MaxLength(100)]
        public required string AreaName { get; set; }

        [Required]
        [MaxLength(50)]
        public required string AreaCode { get; set; }

        public virtual ICollection<DealerOrder> DealerOrders { get; set; } = new List<DealerOrder>();
    }
}
