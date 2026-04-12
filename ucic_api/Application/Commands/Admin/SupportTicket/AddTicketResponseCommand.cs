using MediatR;
using Application.DTOs;

namespace Application.Commands.Admin.SupportTicket
{
    public class AddTicketResponseCommand : IRequest<Response<object>>
    {
        public int TicketId { get; set; }
        public string? Message { get; set; }
        public bool IsInternal { get; set; }
        public string? NewStatus { get; set; }
    }
}