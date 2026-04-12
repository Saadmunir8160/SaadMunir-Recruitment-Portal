using MediatR;
using Application.DTOs;

namespace Application.Commands.Admin.SupportTicket
{
    public class AssignTicketCommand : IRequest<Response<object>>
    {
        public int TicketId { get; set; }
        public string? AssignedTo { get; set; }
    }
}