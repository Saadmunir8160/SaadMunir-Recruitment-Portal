using Domain.Repositories.Command.Base;
using MediatR;
using Microsoft.Extensions.Logging;
using Application.DTOs;
using Application.DTOs.Support;
using Application.Common.Interfaces;
using AutoMapper;

namespace Application.Commands.Support.Create
{
    public class CreateSupportTicketCommandHandler : IRequestHandler<CreateSupportTicketCommand, Response<SupportTicketDTO>>
    {
        private readonly ICommandRepository<Domain.Entities.SupportTicket> _commandRepository;
        private readonly IIdentityService _identityService;
        private readonly ILogger<CreateSupportTicketCommandHandler> _logger;
        private readonly IMapper _mapper;

        public CreateSupportTicketCommandHandler(
            ICommandRepository<Domain.Entities.SupportTicket> commandRepository, 
            IIdentityService identityService, 
            ILogger<CreateSupportTicketCommandHandler> logger,
            IMapper mapper)
        {
            _commandRepository = commandRepository;
            _identityService = identityService;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<Response<SupportTicketDTO>> Handle(CreateSupportTicketCommand request, CancellationToken cancellationToken)
        {
            // Get current dealer ID using centralized method
            var currentDealerId = await _identityService.GetCurrentDealerIdAsync();
            if (currentDealerId == null)
            {
                return new Response<SupportTicketDTO>
                {
                    Success = false,
                    Message = "Dealer not found for current user",
                    Data = new SupportTicketDTO()
                };
            }

            // Generate unique ticket number
            var ticketNumber = $"TKT-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..6].ToUpper()}";

            var supportTicket = new Domain.Entities.SupportTicket
            {
                TicketNumber = ticketNumber,
                Subject = request.Subject,
                Description = request.Description,
                Category = request.Category,
                Priority = request.Priority,
                Status = "Open",
                DealerId = currentDealerId.Value, // Use centralized dealer ID
                OrderId = request.OrderId,
                ProductId = request.ProductId,
                CreatedDate = DateTime.UtcNow,
                IsActive = true
            };

            var ticketId = await _commandRepository.AddAsync(supportTicket);
            supportTicket.TicketId = ticketId;

            var responseDto = _mapper.Map<SupportTicketDTO>(supportTicket);

            _logger.LogInformation("Support ticket {TicketNumber} created successfully for dealer {DealerId}", 
                ticketNumber, currentDealerId.Value);

            return new Response<SupportTicketDTO>
            {
                Success = true,
                Message = "Support ticket created successfully",
                Data = responseDto
            };
        }
    }
}