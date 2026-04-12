using System;

namespace Application.DTOs
{
    public class DeliveryDTO
    {
        public int DeliveryID { get; set; }
        public string? LnOrderNumber { get; set; }
        public string? CustomerOrder { get; set; }
        public string? IQN { get; set; }
        public string? InternalSalesRepresentative { get; set; }
        public decimal? QuantityShipped { get; set; }
        public string? ItemDescription { get; set; }
        public DateTime? DateOut { get; set; }
        public DateTime? DateIN { get; set; }
        public decimal? WeightIN { get; set; }
        public decimal? WeightOut { get; set; }
        public string? ProductionOrder { get; set; }
        public string? Item { get; set; }
        public string? Line { get; set; }
        public string? Shipment { get; set; }
        public string? ShipmentLine { get; set; }
        public string? WarehouseDescription { get; set; }
        public string? DriverName { get; set; }
        public string? Car { get; set; }
        public string? DeliveryMeans { get; set; }
        public string? CustomerName { get; set; }
        public string? TransporterName { get; set; }
        public string? Area { get; set; }
        public string? AreaDescription { get; set; }
        
        // BaseEntity properties
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime CreatedDate { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string? ModifiedBy { get; set; }
    }
}

