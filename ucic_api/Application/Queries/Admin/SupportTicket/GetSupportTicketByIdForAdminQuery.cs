using MediatR;
using Application.DTOs;

namespace Application.Queries.Admin.SupportTicket
{
    public class GetSupportTicketByIdForAdminQuery : IRequest<Response<object>>
    {
        public int Id { get; set; }
    }
}