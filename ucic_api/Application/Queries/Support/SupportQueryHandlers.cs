using Application.Common.Interfaces;
using Application.DTOs;
using Application.DTOs.Support;
using Domain.Repositories.Query.Base;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;

namespace Application.Queries.Support
{
    public class GetDealerSupportTicketsQueryHandler : IRequestHandler<GetDealerSupportTicketsQuery, Response<IEnumerable<SupportTicketDTO>>>
    {
        private readonly IQueryRepository<Domain.Entities.SupportTicket> _queryRepository;
        private readonly IIdentityService _identityService;
        private readonly ILogger<GetDealerSupportTicketsQueryHandler> _logger;
        private readonly IMapper _mapper;

        public GetDealerSupportTicketsQueryHandler(
            IQueryRepository<Domain.Entities.SupportTicket> queryRepository,
            IIdentityService identityService,
            ILogger<GetDealerSupportTicketsQueryHandler> logger,
            IMapper mapper)
        {
            _queryRepository = queryRepository;
            _identityService = identityService;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<Response<IEnumerable<SupportTicketDTO>>> Handle(GetDealerSupportTicketsQuery request, CancellationToken cancellationToken)
        {
            // Get current dealer ID using centralized method
            var currentDealerId = await _identityService.GetCurrentDealerIdAsync();
            if (currentDealerId == null)
            {
                return new Response<IEnumerable<SupportTicketDTO>>
                {
                    Success = false,
                    Message = "Dealer not found for current user"
                };
            }

            var query = _queryRepository.GetQueryable()
                .Where(x => x.DealerId == currentDealerId.Value && !x.IsDeleted && x.IsActive);

            // Filter by status if provided
            if (!string.IsNullOrEmpty(request.Status))
            {
                query = query.Where(x => x.Status == request.Status);
            }

            // Apply pagination
            var tickets = await query
                .OrderByDescending(x => x.CreatedDate)
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            var ticketDTOs = _mapper.Map<IEnumerable<SupportTicketDTO>>(tickets);

            return new Response<IEnumerable<SupportTicketDTO>>
            {
                Success = true,
                Message = "Support tickets retrieved successfully",
                Data = ticketDTOs
            };
        }
    }

    public class GetSupportTicketByIdQueryHandler : IRequestHandler<GetSupportTicketByIdQuery, Response<SupportTicketDetailDTO>>
    {
        private readonly IQueryRepository<Domain.Entities.SupportTicket> _queryRepository;
        private readonly IIdentityService _identityService;
        private readonly ILogger<GetSupportTicketByIdQueryHandler> _logger;
        private readonly IMapper _mapper;

        public GetSupportTicketByIdQueryHandler(
            IQueryRepository<Domain.Entities.SupportTicket> queryRepository,
            IIdentityService identityService,
            ILogger<GetSupportTicketByIdQueryHandler> logger,
            IMapper mapper)
        {
            _queryRepository = queryRepository;
            _identityService = identityService;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<Response<SupportTicketDetailDTO>> Handle(GetSupportTicketByIdQuery request, CancellationToken cancellationToken)
        {
            // Get current dealer ID using centralized method
            var currentDealerId = await _identityService.GetCurrentDealerIdAsync();
            if (currentDealerId == null)
            {
                return new Response<SupportTicketDetailDTO>
                {
                    Success = false,
                    Message = "Dealer not found for current user"
                };
            }

            var ticket = await _queryRepository.GetQueryable()
                .Include(x => x.Messages)
                .Include(x => x.Attachments)
                .FirstOrDefaultAsync(x => x.TicketId == request.TicketId 
                                        && x.DealerId == currentDealerId.Value 
                                        && !x.IsDeleted 
                                        && x.IsActive, cancellationToken);

            if (ticket == null)
            {
                return new Response<SupportTicketDetailDTO>
                {
                    Success = false,
                    Message = "Support ticket not found"
                };
            }

            var ticketDTO = _mapper.Map<SupportTicketDetailDTO>(ticket);

            return new Response<SupportTicketDetailDTO>
            {
                Success = true,
                Message = "Support ticket retrieved successfully",
                Data = ticketDTO
            };
        }
    }


}