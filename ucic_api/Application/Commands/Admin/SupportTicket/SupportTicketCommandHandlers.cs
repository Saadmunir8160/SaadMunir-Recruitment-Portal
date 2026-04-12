using MediatR;
using Application.DTOs;
using Domain.Repositories.Command.Base;
using Domain.Repositories.Query.Base;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace Application.Commands.Admin.SupportTicket
{
    public class AddTicketResponseCommandHandler : IRequestHandler<AddTicketResponseCommand, Response<object>>
    {
        private readonly ICommandRepository<Domain.Entities.SupportTicketMessage> _messageCommandRepository;
        private readonly ICommandRepository<Domain.Entities.SupportTicket> _ticketCommandRepository;
        private readonly IQueryRepository<Domain.Entities.SupportTicket> _ticketQueryRepository;
        private readonly ILogger<AddTicketResponseCommandHandler> _logger;
        private readonly IMapper _mapper;

        public AddTicketResponseCommandHandler(
            ICommandRepository<Domain.Entities.SupportTicketMessage> messageCommandRepository,
            ICommandRepository<Domain.Entities.SupportTicket> ticketCommandRepository,
            IQueryRepository<Domain.Entities.SupportTicket> ticketQueryRepository,
            ILogger<AddTicketResponseCommandHandler> logger,
            IMapper mapper)
        {
            _messageCommandRepository = messageCommandRepository;
            _ticketCommandRepository = ticketCommandRepository;
            _ticketQueryRepository = ticketQueryRepository;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<Response<object>> Handle(AddTicketResponseCommand request, CancellationToken cancellationToken)
        {
            // Verify ticket exists
            var ticket = await _ticketQueryRepository.GetByIdAsync(request.TicketId);
            if (ticket == null)
            {
                return new Response<object>
                {
                    Success = false,
                    Message = "Support ticket not found"
                };
            }

            // Add the message
            var message = new Domain.Entities.SupportTicketMessage
            {
                TicketId = request.TicketId,
                Message = request.Message ?? "",
                IsFromDealer = false, // Admin response
                SenderName = "Admin", // You might want to get actual admin name
                SentAt = DateTime.UtcNow,
                IsActive = true
            };

            await _messageCommandRepository.AddAsync(message);

            // Update ticket status if provided
            if (!string.IsNullOrEmpty(request.NewStatus))
            {
                ticket.Status = request.NewStatus;
                ticket.ModifiedDate = DateTime.UtcNow;
                
                if (request.NewStatus.ToLower() == "resolved")
                {
                    ticket.ResolvedAt = DateTime.UtcNow;
                }

                await _ticketCommandRepository.UpdateAsync(ticket);
            }

            _logger.LogInformation("Admin response added to ticket {TicketId}", request.TicketId);

            return new Response<object>
            {
                Success = true,
                Message = "Response added successfully"
            };
        }
    }

    public class UpdateTicketStatusCommandHandler : IRequestHandler<UpdateTicketStatusCommand, Response<object>>
    {
        private readonly ICommandRepository<Domain.Entities.SupportTicket> _commandRepository;
        private readonly IQueryRepository<Domain.Entities.SupportTicket> _queryRepository;
        private readonly ILogger<UpdateTicketStatusCommandHandler> _logger;
        private readonly IMapper _mapper;

        public UpdateTicketStatusCommandHandler(
            ICommandRepository<Domain.Entities.SupportTicket> commandRepository,
            IQueryRepository<Domain.Entities.SupportTicket> queryRepository,
            ILogger<UpdateTicketStatusCommandHandler> logger,
            IMapper mapper)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<Response<object>> Handle(UpdateTicketStatusCommand request, CancellationToken cancellationToken)
        {
            var ticket = await _queryRepository.GetByIdAsync(request.TicketId);
            if (ticket == null)
            {
                return new Response<object>
                {
                    Success = false,
                    Message = "Support ticket not found"
                };
            }

            ticket.Status = request.Status ?? ticket.Status;
            ticket.ModifiedDate = DateTime.UtcNow;

            if (request.Status?.ToLower() == "resolved")
            {
                ticket.ResolvedAt = DateTime.UtcNow;
            }
            else if (request.Status?.ToLower() == "open")
            {
                ticket.ResolvedAt = null; // Reopen ticket
            }

            await _commandRepository.UpdateAsync(ticket);

            _logger.LogInformation("Ticket {TicketId} status updated to {Status}", request.TicketId, request.Status);

            return new Response<object>
            {
                Success = true,
                Message = "Ticket status updated successfully"
            };
        }
    }

    public class AssignTicketCommandHandler : IRequestHandler<AssignTicketCommand, Response<object>>
    {
        private readonly ICommandRepository<Domain.Entities.SupportTicket> _commandRepository;
        private readonly IQueryRepository<Domain.Entities.SupportTicket> _queryRepository;
        private readonly ILogger<AssignTicketCommandHandler> _logger;
        private readonly IMapper _mapper;

        public AssignTicketCommandHandler(
            ICommandRepository<Domain.Entities.SupportTicket> commandRepository,
            IQueryRepository<Domain.Entities.SupportTicket> queryRepository,
            ILogger<AssignTicketCommandHandler> logger,
            IMapper mapper)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<Response<object>> Handle(AssignTicketCommand request, CancellationToken cancellationToken)
        {
            var ticket = await _queryRepository.GetByIdAsync(request.TicketId);
            if (ticket == null)
            {
                return new Response<object>
                {
                    Success = false,
                    Message = "Support ticket not found"
                };
            }

            ticket.AssignedTo = request.AssignedTo;
            ticket.ModifiedDate = DateTime.UtcNow;

            await _commandRepository.UpdateAsync(ticket);

            _logger.LogInformation("Ticket {TicketId} assigned to {AssignedTo}", request.TicketId, request.AssignedTo);

            return new Response<object>
            {
                Success = true,
                Message = "Ticket assigned successfully"
            };
        }
    }

    public class UpdateTicketPriorityCommandHandler : IRequestHandler<UpdateTicketPriorityCommand, Response<object>>
    {
        private readonly ICommandRepository<Domain.Entities.SupportTicket> _commandRepository;
        private readonly IQueryRepository<Domain.Entities.SupportTicket> _queryRepository;
        private readonly ILogger<UpdateTicketPriorityCommandHandler> _logger;
        private readonly IMapper _mapper;

        public UpdateTicketPriorityCommandHandler(
            ICommandRepository<Domain.Entities.SupportTicket> commandRepository,
            IQueryRepository<Domain.Entities.SupportTicket> queryRepository,
            ILogger<UpdateTicketPriorityCommandHandler> logger,
            IMapper mapper)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<Response<object>> Handle(UpdateTicketPriorityCommand request, CancellationToken cancellationToken)
        {
            var ticket = await _queryRepository.GetByIdAsync(request.TicketId);
            if (ticket == null)
            {
                return new Response<object>
                {
                    Success = false,
                    Message = "Support ticket not found"
                };
            }

            ticket.Priority = request.Priority ?? ticket.Priority;
            ticket.ModifiedDate = DateTime.UtcNow;

            await _commandRepository.UpdateAsync(ticket);

            _logger.LogInformation("Ticket {TicketId} priority updated to {Priority}", request.TicketId, request.Priority);

            return new Response<object>
            {
                Success = true,
                Message = "Ticket priority updated successfully"
            };
        }
    }
}