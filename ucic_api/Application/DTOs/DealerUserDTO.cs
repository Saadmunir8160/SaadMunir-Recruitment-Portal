using System.ComponentModel.DataAnnotations;

namespace Application.DTOs
{
    public class DealerUserDTO
    {
        public string Id { get; set; }
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public string? FullName { get; set; }
        public string? CreditLimit { get; set; }
        public string? LnId { get; set; }
        public int? UserTypeId { get; set; }
        public string? UserTypeName { get; set; }
    }
} 