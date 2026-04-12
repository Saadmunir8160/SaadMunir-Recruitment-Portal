using Domain.Repositories.Command.Base;
using MediatR;
using System.ComponentModel.DataAnnotations;
using Application.DTOs;
using Application.DTOs.Support;
using Application.Common.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace Application.Commands.Support.AddMessage
{
    public class AddMessageToTicketCommand : IRequest<Response<AddMessageResponseDTO>>
    {
        public int TicketId { get; set; }

        [Required(ErrorMessage = "Message is required")]
        public string Message { get; set; } = string.Empty;
        // Note: DealerId is automatically retrieved from current user context
    }

    public class AddMessageToTicketCommandHandler : IRequestHandler<AddMessageToTicketCommand, Response<AddMessageResponseDTO>>
    {
        private readonly ICommandRepository<Domain.Entities.SupportTicketMessage> _messageRepository;
        private readonly Domain.Repositories.Query.Base.IQueryRepository<Domain.Entities.SupportTicket> _ticketRepository;
        private readonly IIdentityService _identityService;
        private readonly ILogger<AddMessageToTicketCommandHandler> _logger;

        public AddMessageToTicketCommandHandler(
            ICommandRepository<Domain.Entities.SupportTicketMessage> messageRepository,
            Domain.Repositories.Query.Base.IQueryRepository<Domain.Entities.SupportTicket> ticketRepository,
            IIdentityService identityService,
            ILogger<AddMessageToTicketCommandHandler> logger)
        {
            _messageRepository = messageRepository;
            _ticketRepository = ticketRepository;
            _identityService = identityService;
            _logger = logger;
        }

        public async Task<Response<AddMessageResponseDTO>> Handle(AddMessageToTicketCommand request, CancellationToken cancellationToken)
        {
            // Get current dealer ID using centralized method
            var currentDealerId = await _identityService.GetCurrentDealerIdAsync();
            if (currentDealerId == null)
            {
                return new Response<AddMessageResponseDTO>
                {
                    Success = false,
                    Message = "Dealer not found for current user",
                    Data = new AddMessageResponseDTO()
                };
            }

            // Verify ticket belongs to current dealer
            var ticket = await _ticketRepository.GetQueryable()
                .Where(x => x.TicketId == request.TicketId && x.DealerId == currentDealerId.Value)
                .FirstOrDefaultAsync();

            if (ticket == null)
            {
                return new Response<AddMessageResponseDTO>
                {
                    Success = false,
                    Message = "Support ticket not found or access denied",
                    Data = new AddMessageResponseDTO()
                };
            }

            // Check if ticket is resolved or closed
            if (ticket.Status == "Resolved" || ticket.Status == "Closed")
            {
                return new Response<AddMessageResponseDTO>
                {
                    Success = false,
                    Message = "Cannot add messages to resolved or closed tickets",
                    Data = new AddMessageResponseDTO()
                };
            }

            var message = new Domain.Entities.SupportTicketMessage
            {
                TicketId = request.TicketId,
                Message = request.Message,
                IsFromDealer = true,
                SenderName = "Dealer", // Could be enhanced to get actual dealer name
                SentAt = DateTime.UtcNow,
                CreatedDate = DateTime.UtcNow,
                IsActive = true
            };

            var messageId = await _messageRepository.AddAsync(message);

            var response = new AddMessageResponseDTO
            {
                MessageId = messageId,
                Message = "Message added successfully to support ticket"
            };

            _logger.LogInformation("Message added to support ticket {TicketId} by dealer {DealerId}", 
                request.TicketId, currentDealerId.Value);

            return new Response<AddMessageResponseDTO>
            {
                Success = true,
                Message = "Message added successfully",
                Data = response
            };
        }
    }
}