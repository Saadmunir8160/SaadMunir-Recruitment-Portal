using MediatR;
using Application.DTOs;

namespace Application.Commands.Admin.SupportTicket
{
    public class UpdateTicketPriorityCommand : IRequest<Response<object>>
    {
        public int TicketId { get; set; }
        public string? Priority { get; set; }
    }
}