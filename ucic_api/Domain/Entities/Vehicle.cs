using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace Domain.Entities
{
    public class Vehicle : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int VehicleId { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; }

        public string Type { get; set; }
        [Required, MaxLength(50)]
        public string RegistrationNo { get; set; }

        [MaxLength(100)]
        public string? Model { get; set; }

        [ForeignKey("User")]
        [MaxLength(450)]
        public string? UserId { get; set; }

        [MaxLength(100)]
        public string? ErpCode { get; set; }

        public virtual ICollection<Order> Orders { get; set; }
    }
}
