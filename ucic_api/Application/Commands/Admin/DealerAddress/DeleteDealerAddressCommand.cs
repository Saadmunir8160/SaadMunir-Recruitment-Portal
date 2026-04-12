using MediatR;
using Application.DTOs;

namespace Application.Commands.Admin.DealerAddress
{
    public class DeleteDealerAddressCommand : IRequest<Response<object>>
    {
        public int AddressId { get; set; }
    }
}