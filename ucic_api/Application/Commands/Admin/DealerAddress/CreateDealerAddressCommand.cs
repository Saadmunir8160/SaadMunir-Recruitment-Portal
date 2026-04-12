using MediatR;
using Application.DTOs;
using System.ComponentModel.DataAnnotations;

namespace Application.Commands.Admin.DealerAddress
{
    public class CreateDealerAddressCommand : IRequest<Response<object>>
    {
        [Required]
        public int DealerId { get; set; }
        
        [Required]
        [MaxLength(200)]
        public string? AddressLine1 { get; set; }
        
        [MaxLength(200)]
        public string? AddressLine2 { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string? City { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string? State { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string? Country { get; set; }
        
        [Required]
        [MaxLength(20)]
        public string? PostalCode { get; set; }
        
        public string? AddressType { get; set; }
        
        public bool IsDefault { get; set; }
    }
}