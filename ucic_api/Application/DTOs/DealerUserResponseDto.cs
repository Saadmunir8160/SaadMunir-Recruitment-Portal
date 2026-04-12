namespace Application.DTOs
{
    public class DealerUserResponseDto
    {
        public string UserId { get; set; } = string.Empty;
        public int DealerId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string DealerName { get; set; } = string.Empty;
        public decimal? CreditLimit { get; set; }
        public decimal? CurrentBalance { get; set; }
        public string? Ln_ID { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
    }
}