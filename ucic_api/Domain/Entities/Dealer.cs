using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Domain.Entities
{
    public class Dealer : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int DealerId { get; set; }

        [Required]
        [ForeignKey("User")]
        [MaxLength(450)]
        public string UserId { get; set; }

        [Required, MaxLength(100)]
        public string DealerName { get; set; }
        
        // Existing fields
        public decimal? CreditLimit { get; set; }
        public decimal? CurrentBalance { get; set; }
        [MaxLength(50)]
        public string? Ln_ID { get; set; }

        // Extended profile fields
        [MaxLength(50)]
        public string? DealerCode { get; set; }
        
        // Verification
        public bool IsVerified { get; set; }

        // Navigation properties - Add JsonIgnore to prevent circular references
        [JsonIgnore]
        public virtual ICollection<DealerProduct> DealerProducts { get; set; } = new List<DealerProduct>();
        
        [JsonIgnore]
        public virtual ICollection<DealerOrder> DealerOrders { get; set; } = new List<DealerOrder>();
        
        [JsonIgnore]
        public virtual ICollection<DealerDriver> DealerDrivers { get; set; } = new List<DealerDriver>();
        
        [JsonIgnore]
        public virtual ICollection<DealerVehicle> DealerVehicles { get; set; } = new List<DealerVehicle>();
        
        [JsonIgnore]
        public virtual ICollection<DealerShippingAddress> DealerShippingAddresses { get; set; } = new List<DealerShippingAddress>();
        
        [JsonIgnore]
        public virtual ICollection<DealerDailyLimit> DealerDailyLimits { get; set; } = new List<DealerDailyLimit>();
        
        // Note: DealerAddress, DealerNotification, and SupportTicket navigation properties removed
        // These entities have been removed from the system
    }
}