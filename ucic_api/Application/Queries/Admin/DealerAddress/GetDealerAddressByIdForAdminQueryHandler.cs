using MediatR;
using Application.DTOs;

namespace Application.Queries.Admin.DealerAddress
{
    public class GetDealerAddressByIdForAdminQueryHandler : IRequestHandler<GetDealerAddressByIdForAdminQuery, Response<object>>
    {
        public async Task<Response<object>> Handle(GetDealerAddressByIdForAdminQuery request, CancellationToken cancellationToken)
        {
            return new Response<object> { Success = true, Message = "Not implemented yet" };
        }
    }
}