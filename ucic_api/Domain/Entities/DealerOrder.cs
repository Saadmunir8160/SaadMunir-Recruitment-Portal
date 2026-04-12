using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace Domain.Entities
{
    public class DealerOrder : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int DealerOrderID { get; set; }
        [ForeignKey("Dealer")]
        public int DealerID { get; set; }
        [MaxLength(50)]
        public string? CustomerOrderNumber { get; set; }
        [MaxLength(50)]
        public string? Ln_OrderNumber { get; set; }
        [MaxLength(50)]
        public string? PortalOrderNumber { get; set; }
        [MaxLength(50)]
        public string? TransporterName { get; set; }
        public DateTime? OrderDate { get; set; }
        [MaxLength(50)]
        public string? Status { get; set; }
        public decimal? TotalAmount { get; set; }
        [ForeignKey("DealerDriver")]
        public int? DriverID { get; set; }
        [ForeignKey("DealerShippingAddress")]
        public int? AddressID { get; set; }
        [ForeignKey("DealerArea")]
        public int? AreaID { get; set; }
        [ForeignKey("DealerVehicle")]
        public int? VehicleID { get; set; }
        public virtual Dealer Dealer { get; set; }
        public virtual DealerDriver? DealerDriver { get; set; }
        public virtual DealerVehicle? DealerVehicle { get; set; }
        public virtual DealerShippingAddress? DealerShippingAddress { get; set; }
        public virtual DealerArea? DealerArea { get; set; }
        public virtual ICollection<DealerOrderItem> DealerOrderItems { get; set; } = new List<DealerOrderItem>();
        public virtual ICollection<DealerDriverLog> DealerDriverLogs { get; set; } = new List<DealerDriverLog>();
    }
}