using MediatR;
using Application.DTOs;

namespace Application.Commands.Admin.SupportTicket
{
    public class UpdateTicketStatusCommand : IRequest<Response<object>>
    {
        public int TicketId { get; set; }
        public string? Status { get; set; }
    }
}