using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Dealer
{
    // Profile DTOs
    public class DealerProfileDTO
    {
        public int Id { get; set; }
        public string? DealerCode { get; set; }
        public string? DealerName { get; set; }
        public decimal? CreditLimit { get; set; }
        public decimal? CurrentBalance { get; set; }
        public string? Ln_ID { get; set; }
        public bool IsVerified { get; set; }
        
        // User Information
        public DealerUserInfoDTO UserInfo { get; set; } = new();
        
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class DealerUserInfoDTO 
    {
        public string? UserId { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? UserName { get; set; }
        public IList<string> Roles { get; set; } = new List<string>();
    }

    public class ChangePasswordRequest
    {
        [Required]
        public string CurrentPassword { get; set; } = string.Empty;
        
        [Required]
        [MinLength(8)]
        public string NewPassword { get; set; } = string.Empty;
        
        [Required]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public class AddressesDTO
    {
        public AddressDTO? Registered { get; set; }
        public AddressDTO? Billing { get; set; }
        public AddressDTO? Shipping { get; set; }
    }

    public class AddressDTO
    {
        public string? Street { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? Pincode { get; set; }
        public string? Country { get; set; }
        public string? Landmark { get; set; }
    }

    public class ContactInfoDTO
    {
        public string? PrimaryPhone { get; set; }
        public string? SecondaryPhone { get; set; }
        public string? PrimaryEmail { get; set; }
        public string? SecondaryEmail { get; set; }
    }

    public class BankDetailsDTO
    {
        public string? BankName { get; set; }
        public string? AccountNumber { get; set; }
        public string? IfscCode { get; set; }
        public string? AccountType { get; set; }
        public string? AccountHolderName { get; set; }
    }

    public class DocumentsDTO
    {
        public string? GstCertificate { get; set; }
        public string? PanCard { get; set; }
        public string? IncorporationCertificate { get; set; }
        public string? TradeLicense { get; set; }
        public string? BankStatement { get; set; }
        public string? CancelledCheque { get; set; }
    }

    public class UpdateDealerProfileDTO
    {
        public string? CompanyName { get; set; }
        public string? Website { get; set; }
        public string? Description { get; set; }
        public ContactInfoDTO? ContactInfo { get; set; }
        public AddressesDTO? Addresses { get; set; }
        public BankDetailsDTO? BankDetails { get; set; }
    }

    public class UpdateAddressDTO
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
        public string? Landmark { get; set; }
    }

    public class VerifyBankDetailsDTO
    {
        [Required]
        public string BankName { get; set; }
        [Required]
        public string AccountNumber { get; set; }
        [Required]
        public string IfscCode { get; set; }
        [Required]
        public string AccountType { get; set; }
        [Required]
        public string AccountHolderName { get; set; }
    }

    public class BankVerificationResponseDTO
    {
        public bool IsValid { get; set; }
        public string Message { get; set; }
    }

    public class DocumentUploadResponseDTO
    {
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public DateTime UploadedAt { get; set; }
        public string Message { get; set; }
    }

    public class ProfileCompletionStatusDTO
    {
        public int CompletionPercentage { get; set; }
        public List<string> MissingFields { get; set; } = new();
    }

    public class VerificationRequestResponseDTO
    {
        public string Message { get; set; }
        public string ReferenceNumber { get; set; }
    }

    // Dashboard DTOs
    public class DealerDashboardStatsDTO
    {
        public int TotalOrders { get; set; }
        public int PendingOrders { get; set; }
        public int ConfirmedOrders { get; set; }
        public int DeliveredOrders { get; set; }
        public int CancelledOrders { get; set; }
        public decimal MonthlySpent { get; set; }
        public decimal YearlySpent { get; set; }
        public decimal AverageOrderValue { get; set; }
        public DateTime? LastOrderDate { get; set; }
        public DateTime? NextDeliveryDate { get; set; }
        public double AverageDeliveryTime { get; set; }
        public int TotalProductsOrdered { get; set; }
        public List<string> FavoriteProducts { get; set; } = new();
        public PaymentStatusDTO PaymentStatus { get; set; } = new();
    }

    public class PaymentStatusDTO
    {
        public decimal Outstanding { get; set; }
        public decimal Overdue { get; set; }
        public decimal Paid { get; set; }
    }

    public class RecentOrderDTO
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; }
        public string? Ln_OrderNumber { get; set; }  // NEW: LN Order Number from database
        public DateTime OrderDate { get; set; }
        public string Status { get; set; }
        public decimal TotalAmount { get; set; }
        public int ItemCount { get; set; }
        public DateTime? EstimatedDelivery { get; set; }
        public string? TrackingNumber { get; set; }
    }

    public class DealerNotificationDTO
    {
        public int Id { get; set; }
        public string Type { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? ActionUrl { get; set; }
        public string Priority { get; set; }
    }

    public class MonthlySummaryDTO
    {
        public string Month { get; set; }
        public int Year { get; set; }
        public int OrderCount { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal AverageOrderValue { get; set; }
    }

    public class TopProductDTO
    {
        public long ProductId { get; set; }
        public string ProductName { get; set; }
        public int TotalQuantity { get; set; }
        public decimal TotalValue { get; set; }
        public DateTime? LastOrderDate { get; set; }
    }

    public class PaymentSummaryDTO
    {
        public decimal TotalOutstanding { get; set; }
        public decimal OverdueAmount { get; set; }
        public decimal CurrentMonthSpent { get; set; }
        public decimal CreditLimit { get; set; }
        public decimal AvailableCredit { get; set; }
        public DateTime? NextPaymentDue { get; set; }
    }

    public class OrderTrendsDTO
    {
        public List<string> Labels { get; set; } = new();
        public List<int> OrderCounts { get; set; } = new();
        public List<decimal> OrderValues { get; set; } = new();
    }

    public class CategorySpendingDTO
    {
        public string Category { get; set; }
        public decimal Amount { get; set; }
        public double Percentage { get; set; }
    }

    public class DeliveryMetricsDTO
    {
        public double OnTimeDeliveryRate { get; set; }
        public double AverageDeliveryTime { get; set; }
        public int DelayedDeliveries { get; set; }
        public List<UpcomingDeliveryDTO> UpcomingDeliveries { get; set; } = new();
    }

    public class UpcomingDeliveryDTO
    {
        public string OrderNumber { get; set; }
        public DateTime ExpectedDate { get; set; }
        public string Status { get; set; }
    }

    public class UnreadCountDTO
    {
        public int Count { get; set; }
    }
}