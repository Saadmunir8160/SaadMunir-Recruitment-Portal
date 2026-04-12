using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace Domain.Entities
{
    public class DealerVehicle : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int VehicleID { get; set; }
        
        [ForeignKey("Dealer")]
        public int DealerID { get; set; }
        
        [Required]
        [MaxLength(50)]
        public required string PlateNumber { get; set; }
        
        [Required]
        [MaxLength(20)]
        public required string Type { get; set; } // truck, van, pickup, trailer
        
        [Column(TypeName = "decimal(8,2)")]
        public decimal Capacity { get; set; } // in tons
        
        public DateTime RegistrationDate { get; set; }
        public DateTime RegistrationExpiryDate { get; set; }
        public DateTime InsuranceExpiryDate { get; set; }
        
        [MaxLength(50)]
        public string? Ln_ID { get; set; } // License ID
        
        [MaxLength(50)]
        public string? RegistrationNumber { get; set; }
        
        [MaxLength(50)]
        public string? VehicleType { get; set; } // Legacy field, can be removed later
        
        public virtual Dealer? Dealer { get; set; }
        public virtual ICollection<DealerOrder> DealerOrders { get; set; } = new List<DealerOrder>();
    }
}