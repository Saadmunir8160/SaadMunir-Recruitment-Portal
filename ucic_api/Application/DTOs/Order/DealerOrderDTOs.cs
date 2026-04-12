using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Order
{
    public class CreateDealerOrderDTO
    {
        [Required]
        public List<CreateOrderItemDTO> Items { get; set; } = new();
        
        [Required]
        public BillingInfoDTO BillingInfo { get; set; }
        
        [Required]
        public DeliveryAddressDTO DeliveryAddress { get; set; }
        
        [Required]
        public string PaymentMethod { get; set; }
        
        public string? Notes { get; set; }
    }

    public class CreateOrderItemDTO
    {
        [Required]
        public long ProductId { get; set; }
        
        [Required]
        public int Quantity { get; set; }
        
        [Required]
        public decimal UnitPrice { get; set; }
        
        public Dictionary<string, string>? Specifications { get; set; }
    }

    public class BillingInfoDTO
    {
        [Required]
        public string CompanyName { get; set; }
        
        [Required]
        public string GstNumber { get; set; }
        
        [Required]
        public AddressDTO BillingAddress { get; set; }
    }

    public class DeliveryAddressDTO
    {
        [Required]
        public string ContactPerson { get; set; }
        
        [Required]
        public string Phone { get; set; }
        
        [Required]
        public string Email { get; set; }
        
        [Required]
        public AddressDTO Address { get; set; }
        
        public string? DeliveryInstructions { get; set; }
    }

    public class AddressDTO
    {
        [Required]
        public string Street { get; set; }
        
        [Required]
        public string City { get; set; }
        
        [Required]
        public string State { get; set; }
        
        [Required]
        public string Pincode { get; set; }
        
        [Required]
        public string Country { get; set; }
    }

    public class CreateOrderResponseDTO
    {
        public int OrderId { get; set; }
        public string OrderNumber { get; set; }
        public string Message { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime? EstimatedDelivery { get; set; }
    }

    public class DealerOrderDTO
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; }
        public string? CustomerOrderNumber { get; set; }
        public DateTime OrderDate { get; set; }
        public string Status { get; set; }
        public decimal TotalAmount { get; set; }
        public int ItemCount { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public string? TrackingNumber { get; set; }
        public string PaymentStatus { get; set; }
        public List<OrderItemDTO> Items { get; set; } = new();
    }

    public class OrderItemDTO
    {
        public long ProductId { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public Dictionary<string, string>? Specifications { get; set; }
    }

    public class DealerOrderDetailDTO : DealerOrderDTO
    {
        public BillingInfoDTO BillingInfo { get; set; }
        public DeliveryAddressDTO DeliveryAddress { get; set; }
        public string PaymentMethod { get; set; }
        public string? Notes { get; set; }
        public List<OrderHistoryDTO> OrderHistory { get; set; } = new();
    }

    public class OrderHistoryDTO
    {
        public string Status { get; set; }
        public DateTime Timestamp { get; set; }
        public string Description { get; set; }
    }

    public class OrdersResponseDTO
    {
        public List<DealerOrderDTO> Orders { get; set; } = new();
        public int TotalOrders { get; set; }
        public int TotalPages { get; set; }
        public int CurrentPage { get; set; }
    }

    public class UpdateOrderStatusDTO
    {
        [Required]
        public string Status { get; set; }
    }

    public class CancelOrderDTO
    {
        [Required]
        public string Reason { get; set; }
    }

    public class CancelOrderResponseDTO
    {
        public string Message { get; set; }
        public decimal RefundAmount { get; set; }
        public string RefundStatus { get; set; }
    }

    public class ReorderResponseDTO
    {
        public int OrderId { get; set; }
        public string OrderNumber { get; set; }
        public string Message { get; set; }
        public decimal TotalAmount { get; set; }
    }
}