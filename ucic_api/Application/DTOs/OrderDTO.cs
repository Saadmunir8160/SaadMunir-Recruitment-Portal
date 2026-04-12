using System.ComponentModel.DataAnnotations;
using Domain.Entities;

namespace Application.DTOs
{
    public class OrderDTO
    {
        public long OrderId { get; set; }
        public long CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public long LocationId { get; set; }
        public string? LocationAddress { get; set; }
        public long PromotionId { get; set; }
        public long TotalQuantity { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal TotalVat { get; set; }
        public decimal ShipingCost { get; set; }
        public decimal CouponDiscount { get; set; }
        public string? Status { get; set; }
        public string? TrackingID { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? PaymentSubmittedDate { get; set; }
        public DateTime? PaymentConfirmedDate { get; set; }
        public string? PaymentTransactionId { get; set; }

        [MaxLength(50)]
        public string? PaymentConfirmedBy { get; set; }

        public string? PaymentFilePath { get; set; }

        public DateTime? OrderConfirmedDate { get; set; }
        public string? OrderConfirmedBy { get; set; }

        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        // New property to hold order items
        public ICollection<OrderItemsDTO>? OrderItems { get; set; }
        public virtual GpsLocation? GpsLocation { get; set; }
    }

    public class OrderItemsDTO
    {
        public long OrderItemsId { get; set; }
        public long ProductId { get; set; }
        public string? ProductName { get; set; } // Optional if you want to include product info
        public long Quantity { get; set; }
        public decimal Price { get; set; }
        public int NumberOfTrucks { get; set; }
        public string? ProductImage { get; set; }
    }

}
