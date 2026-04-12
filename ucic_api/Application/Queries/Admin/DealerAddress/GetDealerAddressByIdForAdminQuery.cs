using MediatR;
using Application.DTOs;

namespace Application.Queries.Admin.DealerAddress
{
    public class GetDealerAddressByIdForAdminQuery : IRequest<Response<object>>
    {
        public int Id { get; set; }
    }
}