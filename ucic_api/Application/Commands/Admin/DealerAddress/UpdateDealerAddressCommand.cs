using MediatR;
using Application.DTOs;

namespace Application.Commands.Admin.DealerAddress
{
    public class UpdateDealerAddressCommand : IRequest<Response<object>>
    {
        public int AddressId { get; set; }
        public int DealerId { get; set; }
        public string? AddressLine1 { get; set; }
        public string? AddressLine2 { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? Country { get; set; }
        public string? PostalCode { get; set; }
        public string? AddressType { get; set; }
        public bool? IsDefault { get; set; }
        public bool? IsActive { get; set; }
    }
}