using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Customer : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long CustomerId { get; set; }

        [Required]
        [ForeignKey("User")]
        public string UserId { get; set; }

        [MaxLength(255)]
        public string? CR_No { get; set; }

        [MaxLength(255)]
        public string? VAT_ID { get; set; }

        [MaxLength(255)]
        public string? ContactPerson { get; set; }

        [MaxLength(255)]
        public string? Location { get; set; }

        public virtual ICollection<Order> Orders { get; set; }
        public virtual ICollection<Location> Locations { get; set; }
    }
}
