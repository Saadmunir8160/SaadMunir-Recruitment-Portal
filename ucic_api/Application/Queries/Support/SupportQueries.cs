using MediatR;
using Application.DTOs;
using Application.DTOs.Support;

namespace Application.Queries.Support
{
    public class GetDealerSupportTicketsQuery : IRequest<Response<IEnumerable<SupportTicketDTO>>>
    {
        // Note: DealerId is automatically retrieved from current user context
        public string? Status { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class GetSupportTicketByIdQuery : IRequest<Response<SupportTicketDetailDTO>>
    {
        public int TicketId { get; set; }
        // Note: DealerId is automatically retrieved from current user context
    }


}