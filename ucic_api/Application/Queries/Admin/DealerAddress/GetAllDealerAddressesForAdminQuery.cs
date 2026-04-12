using MediatR;
using Application.DTOs;

namespace Application.Queries.Admin.DealerAddress
{
    public class GetAllDealerAddressesForAdminQuery : IRequest<Response<object>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? Search { get; set; }
        public int? DealerId { get; set; }
        public string? AddressType { get; set; }
    }
}