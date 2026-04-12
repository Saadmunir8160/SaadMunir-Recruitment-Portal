using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Domain.Entities
{
    public class Order : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long OrderId { get; set; }

        [Required, MaxLength(150)]
        public string TrackingId { get; set; }

        [ForeignKey("Customer")]
        public long CustomerId { get; set; }

        [ForeignKey("Location")]
        public long LocationId { get; set; }

        [Required]
        public long TotalQuantity { get; set; }

        [Required]
        public decimal TotalPrice { get; set; }
        public decimal PaymentAmount { get; set; }

        [Required]
        public decimal TotalVat { get; set; }

        [Required]
        public decimal ShipingCost { get; set; }

        [Required]
        public decimal CouponDiscount { get; set; }

        public string? PaymentTransactionId { get; set; }
        public string? OrderLnNumber { get; set; }
        public string? ShipmentNumber { get; set; }
        public string? PaymentRemarks { get; set; }
        public string? OrderCancelledRemarks { get; set; }
        public DateTime? PaymentSubmittedDate { get; set; }
        public DateTime? PaymentConfirmedDate { get; set; }

        [MaxLength(50)]
        public string? PaymentConfirmedBy { get; set; }

        public string? PaymentFilePath { get; set; }

        public DateTime? OrderConfirmedDate { get; set; }
        public string? OrderConfirmedBy { get; set; }
        public DateTime? ShipmentDate { get; set; }

        public DateTime? CancelledDate { get; set; }

        [MaxLength(50)]
        public string? CancelledBy { get; set; }

        public DateTime? RefundedDate { get; set; }

        [MaxLength(50)]
        public string? RefundedBy { get; set; }

        [MaxLength(20)]
        public string? TranspoterName { get; set; }

        [MaxLength(50)]
        public string? CustomerOrderNumber { get; set; }
        public DateTime? DateAssignedToDealer { get; set; }
        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        [ForeignKey("Promotion")]
        public long? PromotionId { get; set; }

        [ForeignKey("Vehicle")]
        public int? VehicleId { get; set; }

        [ForeignKey("Driver")]
        public int? DriverId { get; set; }

        [ForeignKey("OrderType")]
        public int? OrderTypeId { get; set; }

        [MaxLength(450)]
        public string? UserId { get; set; }

        public virtual Customer Customer { get; set; }
        public virtual Location Location { get; set; }
        public virtual Promotion Promotion { get; set; }
        public virtual Driver Driver { get; set; }
        public virtual Vehicle Vehicle { get; set; }
        public virtual OrderType OrderType { get; set; }
        public virtual GpsLocation GpsLocation { get; set; }
        public virtual ICollection<OrderItems> OrderItems { get; set; }
    }

    public enum OrderStatus
    {
        Pending,
        PaymentSubmitted,
        PaymentConfirmed,
        Confirmed,
        Shipped,
        Cancelled,
        Refunded
    }
}
