using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace Domain.Entities
{
    public class Driver : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required, MaxLength(200)]
        public string Name { get; set; }

        [MaxLength(20)]
        public string? PhoneNumber { get; set; }

        [MaxLength(50)]
        public string? IqamaNumber { get; set; }

        [MaxLength(100)]
        public string? ErpCode { get; set; }

        // Foreign key to ApplicationUser (Dealer role)
        [ForeignKey("User")]
        [MaxLength(450)]
        public string? UserId { get; set; }

        public virtual ICollection<Order> Orders { get; set; }
        // No navigation to DealerOrder or other dealer-related tables as per ERD
    }
}
