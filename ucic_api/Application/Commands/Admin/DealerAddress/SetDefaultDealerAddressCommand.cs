using MediatR;
using Application.DTOs;

namespace Application.Commands.Admin.DealerAddress
{
    public class SetDefaultDealerAddressCommand : IRequest<Response<object>>
    {
        public int AddressId { get; set; }
        public int DealerId { get; set; }
    }
}