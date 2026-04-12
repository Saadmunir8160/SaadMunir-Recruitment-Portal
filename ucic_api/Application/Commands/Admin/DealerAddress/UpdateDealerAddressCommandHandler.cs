using MediatR;
using Application.DTOs;

namespace Application.Commands.Admin.DealerAddress
{
    public class UpdateDealerAddressCommandHandler : IRequestHandler<UpdateDealerAddressCommand, Response<object>>
    {
        public async Task<Response<object>> Handle(UpdateDealerAddressCommand request, CancellationToken cancellationToken)
        {
            // TODO: Implement address update logic
            return new Response<object>
            {
                Success = true,
                Message = "Address update not yet implemented"
            };
        }
    }
}