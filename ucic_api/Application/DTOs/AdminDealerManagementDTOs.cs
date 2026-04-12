namespace Application.DTOs
{
    // Dealer Product DTOs for Admin
    public class DealerProductForAdminDTO
    {
        public int DealerProductID { get; set; }
        public int DealerID { get; set; }
        public string DealerName { get; set; } = string.Empty;
        public int ProductID { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string? ProductDescription { get; set; }
        public string? Product_LnCode { get; set; }
        public decimal PricePerUnit { get; set; }
        public int AvailableQuantity { get; set; }
        public string? UnitOfMeasure { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
    }

    public class DealerProductsStatsForAdminDTO
    {
        public int TotalProducts { get; set; }
        public int ActiveProducts { get; set; }
        public int InactiveProducts { get; set; }
        public decimal TotalInventoryValue { get; set; }
        public int LowStockProducts { get; set; }
        public int OutOfStockProducts { get; set; }
    }

    // Dealer Order DTOs for Admin
    public class DealerOrderForAdminDTO
    {
        public int DealerOrderID { get; set; }
        public int DealerID { get; set; }
        public string DealerName { get; set; } = string.Empty;
        public string? DealerLN_ID { get; set; }
        public string? CustomerOrderNumber { get; set; }
        public string? PortalOrderNumber { get; set; }
        public string? Ln_OrderNumber { get; set; }
        public string? TransporterName { get; set; }
        public DateTime OrderDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public int? DriverID { get; set; }
        public string? DriverName { get; set; }
        public int? VehicleID { get; set; }
        public string? VehicleName { get; set; }
        public int? AddressID { get; set; }
        public int? AreaID { get; set; }
        public string? AreaName { get; set; }
        public string? AreaCode { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public List<DealerOrderItemForAdminDTO> OrderItems { get; set; } = new List<DealerOrderItemForAdminDTO>();
    }

    public class DealerOrderItemForAdminDTO
    {
        public int OrderItemID { get; set; }
        public int DealerOrderID { get; set; }
        public int DealerProductID { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public string? Notes { get; set; }
    }

    public class DealerOrdersStatsForAdminDTO
    {
        public int TotalOrders { get; set; }
        public int PendingOrders { get; set; }
        public int CompletedOrders { get; set; }
        public int CancelledOrders { get; set; }
        public decimal TotalOrderValue { get; set; }
        public decimal AverageOrderValue { get; set; }
        public int OrdersToday { get; set; }
        public int OrdersThisWeek { get; set; }
        public int OrdersThisMonth { get; set; }
    }

    // Dealer Driver DTOs for Admin
    public class DealerDriverForAdminDTO
    {
        public int DriverID { get; set; }
        public int DealerID { get; set; }
        public string DealerName { get; set; } = string.Empty;
        public string DriverName { get; set; } = string.Empty;
        public string? Ln_ID { get; set; }
        public string? IqamaNumber { get; set; }
        public string PhoneNumber { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? FullName { get; set; }
        public string? UserName { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public int TotalDeliveries { get; set; }
        public DateTime? LastActiveDate { get; set; }
        public string? AssignedVehicle { get; set; }
    }

    public class DealerDriversStatsForAdminDTO
    {
        public int TotalDrivers { get; set; }
        public int ActiveDrivers { get; set; }
        public int InactiveDrivers { get; set; }
        public int DriversWithVehicles { get; set; }
        public int DriversWithoutVehicles { get; set; }
        public int TotalDeliveries { get; set; }
        public int DeliveriesToday { get; set; }
        public int DeliveriesThisWeek { get; set; }
    }

    // Dealer Vehicle DTOs for Admin
    public class DealerVehicleForAdminDTO
    {
        public int VehicleID { get; set; }
        public int DealerID { get; set; }
        public string DealerName { get; set; } = string.Empty;
        public string VehicleName { get; set; } = string.Empty;
        public string LicensePlate { get; set; } = string.Empty;
        public string VehicleType { get; set; } = string.Empty;
        public decimal Capacity { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public int? DriverID { get; set; }
        public string? DriverName { get; set; }
        public string? Status { get; set; }
        public DateTime? LastUsedDate { get; set; }
    }

    public class DealerVehiclesStatsForAdminDTO
    {
        public int TotalVehicles { get; set; }
        public int ActiveVehicles { get; set; }
        public int InactiveVehicles { get; set; }
        public int VehiclesWithDrivers { get; set; }
        public int VehiclesWithoutDrivers { get; set; }
        public decimal TotalCapacity { get; set; }
        public int VehiclesInUse { get; set; }
        public int VehiclesAvailable { get; set; }
    }

    // Dashboard DTOs for Admin
    public class DealerManagementDashboardStatsDTO
    {
        public int TotalDealers { get; set; }
        public int ActiveDealers { get; set; }
        public DealerProductsStatsForAdminDTO ProductStats { get; set; } = new DealerProductsStatsForAdminDTO();
        public DealerOrdersStatsForAdminDTO OrderStats { get; set; } = new DealerOrdersStatsForAdminDTO();
        public DealerDriversStatsForAdminDTO DriverStats { get; set; } = new DealerDriversStatsForAdminDTO();
        public DealerVehiclesStatsForAdminDTO VehicleStats { get; set; } = new DealerVehiclesStatsForAdminDTO();
    }

    public class DealerSummaryForAdminDTO
    {
        public int DealerId { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public int TotalProducts { get; set; }
        public int TotalOrders { get; set; }
        public int TotalDrivers { get; set; }
        public int TotalVehicles { get; set; }
        public decimal TotalOrderValue { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? LastOrderDate { get; set; }
    }

    // Dealer Area DTOs for Admin
    public class DealerAreaForAdminDTO
    {
        public int AreaID { get; set; }
        public string AreaName { get; set; } = string.Empty;
        public string AreaCode { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public int OrderCount { get; set; }
    }

    public class DealerAreasStatsForAdminDTO
    {
        public int TotalAreas { get; set; }
        public int ActiveAreas { get; set; }
        public int InactiveAreas { get; set; }
    }
}