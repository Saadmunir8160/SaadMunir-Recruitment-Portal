namespace Application.DTOs.DealerProfile
{
    public class DealerProfileDto
    {
        public int DealerId { get; set; }
        public string DealerName { get; set; }
        public decimal? CreditLimit { get; set; }
        public decimal? CurrentBalance { get; set; }
        public string? Ln_ID { get; set; }
        public string? DealerCode { get; set; }
        public bool IsVerified { get; set; }
        
        // User information for display
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
    }
}