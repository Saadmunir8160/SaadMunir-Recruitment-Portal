using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    public class Delivery : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int DeliveryID { get; set; }

        [MaxLength(50)]
        public string? LnOrderNumber { get; set; }

        [MaxLength(50)]
        public string? CustomerOrder { get; set; }

        [MaxLength(50)]
        public string? IQN { get; set; }

        [MaxLength(50)]
        public string? InternalSalesRepresentative { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal? QuantityShipped { get; set; }

        [MaxLength(500)]
        public string? ItemDescription { get; set; }

        public DateTime? DateOut { get; set; }

        public DateTime? DateIN { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal? WeightIN { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal? WeightOut { get; set; }

        [MaxLength(50)]
        public string? ProductionOrder { get; set; }

        [MaxLength(50)]
        public string? Item { get; set; }

        [MaxLength(50)]
        public string? Line { get; set; }

        [MaxLength(50)]
        public string? Shipment { get; set; }

        [MaxLength(50)]
        public string? ShipmentLine { get; set; }

        [MaxLength(200)]
        public string? WarehouseDescription { get; set; }

        [MaxLength(200)]
        public string? DriverName { get; set; }

        [MaxLength(100)]
        public string? Car { get; set; }

        [MaxLength(100)]
        public string? DeliveryMeans { get; set; }

        [MaxLength(500)]
        public string? CustomerName { get; set; }

        [MaxLength(200)]
        public string? TransporterName { get; set; }

        [MaxLength(50)]
        public string? Area { get; set; }

        [MaxLength(200)]
        public string? AreaDescription { get; set; }
    }
}

