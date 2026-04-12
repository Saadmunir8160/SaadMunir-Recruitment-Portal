using MediatR;
using Application.DTOs;

namespace Application.Commands.Admin.DealerAddress
{
    public class DeleteDealerAddressCommandHandler : IRequestHandler<DeleteDealerAddressCommand, Response<object>>
    {
        public async Task<Response<object>> Handle(DeleteDealerAddressCommand request, CancellationToken cancellationToken)
        {
            return new Response<object> { Success = true, Message = "Not implemented yet" };
        }
    }

    public class SetDefaultDealerAddressCommandHandler : IRequestHandler<SetDefaultDealerAddressCommand, Response<object>>
    {
        public async Task<Response<object>> Handle(SetDefaultDealerAddressCommand request, CancellationToken cancellationToken)
        {
            return new Response<object> { Success = true, Message = "Not implemented yet" };
        }
    }
}