using MediatR;
using Application.DTOs;

namespace Application.Commands.Admin.DealerAddress
{
    public class CreateDealerAddressCommandHandler : IRequestHandler<CreateDealerAddressCommand, Response<object>>
    {
        public async Task<Response<object>> Handle(CreateDealerAddressCommand request, CancellationToken cancellationToken)
        {
            // TODO: Implement address creation logic
            return new Response<object>
            {
                Success = true,
                Message = "Address creation not yet implemented"
            };
        }
    }
}