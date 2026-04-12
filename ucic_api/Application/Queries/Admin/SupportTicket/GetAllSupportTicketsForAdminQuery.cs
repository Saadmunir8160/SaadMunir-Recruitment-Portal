using MediatR;
using Application.DTOs;

namespace Application.Queries.Admin.SupportTicket
{
    public class GetAllSupportTicketsForAdminQuery : IRequest<Response<object>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? Search { get; set; }
        public int? DealerId { get; set; }
        public string? Category { get; set; }
        public string? Status { get; set; }
        public string? Priority { get; set; }
    }
}