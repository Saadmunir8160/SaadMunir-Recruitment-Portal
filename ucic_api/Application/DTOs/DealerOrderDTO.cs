using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs
{
    public class DealerOrderDTO
    {
        public int DealerOrderID { get; set; }
        public int DealerID { get; set; }
        public int? DriverID { get; set; }
        public string? DriverName { get; set; }
        public int? VehicleID { get; set; }
        public string? VehicleName { get; set; }
        public int? AddressID { get; set; }
        public int? AreaID { get; set; }
        // Human-friendly delivery area name (from DealerArea)
        public string? DeliveryAreaName { get; set; }
        public string? DeliveryAreaCode { get; set; }
        public string? CustomerOrderNumber { get; set; }
        public string? Ln_OrderNumber { get; set; }
        public string? PortalOrderNumber { get; set; }
        public string? TransporterName { get; set; }
        public DateTime? OrderDate { get; set; }
        public string? Status { get; set; }
        public decimal? TotalAmount { get; set; }
        public bool IsActive { get; set; }
        
        // BaseEntity properties for queries
        public DateTime CreatedDate { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string? ModifiedBy { get; set; }
        
        public List<DealerOrderItemDTO> DealerOrderItems { get; set; } = new List<DealerOrderItemDTO>();
        public DealerShippingAddressDTO? ShippingAddress { get; set; }
    }

    public class DealerOrderItemDTO
    {
        public int OrderItemID { get; set; }
        public int DealerOrderID { get; set; }
        public int DealerProductID { get; set; }
        public string? Product_LnCode { get; set; }
        public string? ProductDescription { get; set; }
        public decimal? Quantity { get; set; }
        public required string Unit { get; set; } // Unit of measurement: bags, tons, pieces, meters, kg, liters, etc.
        public bool IsActive { get; set; }
    }

    public class CreateDealerOrderDTO
    {
        [Required]
        public int DealerID { get; set; }
        public int? DriverID { get; set; }
        public int? VehicleID { get; set; }
        public int? AddressID { get; set; }
        public int? AreaID { get; set; }
        public string? Status { get; set; }
        public decimal? TotalAmount { get; set; }
        public bool IsActive { get; set; }
        public List<CreateDealerOrderItemDTO> OrderItems { get; set; } = new List<CreateDealerOrderItemDTO>();
    }

    public class UpdateDealerOrderDTO : CreateDealerOrderDTO
    {
        [Required]
        public int DealerOrderID { get; set; }
    }

    public class CreateDealerOrderItemDTO
    {
        [Required]
        public int DealerProductID { get; set; }
        public string? Product_LnCode { get; set; }
        public string? ProductDescription { get; set; }
        [Required]
        public decimal Quantity { get; set; }
        [Required]
        [StringLength(10, ErrorMessage = "Unit cannot exceed 10 characters")]
        public required string Unit { get; set; } = "bags";
    }

    public class UpdateDealerOrderItemDTO : CreateDealerOrderItemDTO
    {
        [Required]
        public int OrderItemID { get; set; }
        [Required]
        public int DealerOrderID { get; set; }
    }
}