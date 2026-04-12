using MediatR;
using Application.DTOs;
using Application.Queries.Admin.SupportTicket;
using Domain.Repositories.Query.Base;
using Microsoft.EntityFrameworkCore;
using Domain.Entities;
using AutoMapper;
using Microsoft.Extensions.Logging;

namespace Application.Queries.Admin.SupportTicket
{
    public class GetAllSupportTicketsForAdminQueryHandler : IRequestHandler<GetAllSupportTicketsForAdminQuery, Response<object>>
    {
        private readonly IQueryRepository<Domain.Entities.SupportTicket> _queryRepository;
        private readonly IQueryRepository<Domain.Entities.Dealer> _dealerRepository;
        private readonly ILogger<GetAllSupportTicketsForAdminQueryHandler> _logger;
        private readonly IMapper _mapper;

        public GetAllSupportTicketsForAdminQueryHandler(
            IQueryRepository<Domain.Entities.SupportTicket> queryRepository,
            IQueryRepository<Domain.Entities.Dealer> dealerRepository,
            ILogger<GetAllSupportTicketsForAdminQueryHandler> logger,
            IMapper mapper)
        {
            _queryRepository = queryRepository;
            _dealerRepository = dealerRepository;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<Response<object>> Handle(GetAllSupportTicketsForAdminQuery request, CancellationToken cancellationToken)
        {
            var query = _queryRepository.GetQueryable()
                .Where(st => !st.IsDeleted && st.IsActive);

            // Apply filters
            if (!string.IsNullOrEmpty(request.Search))
            {
                query = query.Where(st => 
                    st.Subject.Contains(request.Search) || 
                    st.Description.Contains(request.Search) ||
                    st.TicketNumber.Contains(request.Search));
            }

            if (request.DealerId.HasValue)
            {
                query = query.Where(st => st.DealerId == request.DealerId.Value);
            }

            if (!string.IsNullOrEmpty(request.Status))
            {
                query = query.Where(st => st.Status == request.Status);
            }

            if (!string.IsNullOrEmpty(request.Priority))
            {
                query = query.Where(st => st.Priority == request.Priority);
            }

            if (!string.IsNullOrEmpty(request.Category))
            {
                query = query.Where(st => st.Category == request.Category);
            }

            var totalCount = await query.CountAsync(cancellationToken);
            var totalPages = (int)Math.Ceiling((double)totalCount / request.PageSize);

            // Get dealers for dealer names
            var dealers = await _dealerRepository.GetAllAsync();
            var dealerLookup = dealers.ToDictionary(d => d.DealerId, d => d.DealerName);

            var tickets = await query
                .OrderByDescending(st => st.CreatedDate)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Include(st => st.Messages)
                .Select(st => new
                {
                    TicketId = st.TicketId,
                    TicketNumber = st.TicketNumber,
                    Title = st.Subject,
                    Description = st.Description,
                    DealerId = st.DealerId,
                    DealerName = dealerLookup.ContainsKey(st.DealerId) ? dealerLookup[st.DealerId] : "Unknown",
                    Category = st.Category,
                    Priority = st.Priority,
                    Status = st.Status,
                    AssignedTo = st.AssignedTo,
                    CreatedDate = st.CreatedDate,
                    UpdatedDate = st.ModifiedDate,
                    ResolvedDate = st.ResolvedAt,
                    Responses = st.Messages.Select(m => new
                        {
                            ResponseId = m.MessageId,
                            TicketId = m.TicketId,
                            Message = m.Message,
                            CreatedBy = m.SenderName,
                            CreatedDate = m.SentAt,
                            IsInternal = !m.IsFromDealer
                        }).ToList()
                    })
                    .ToListAsync(cancellationToken);

            var result = new
            {
                Data = tickets,
                Metadata = new
                {
                    CurrentPage = request.PageNumber,
                    PageSize = request.PageSize,
                    TotalCount = totalCount,
                    TotalPages = totalPages,
                    HasNext = request.PageNumber < totalPages,
                    HasPrevious = request.PageNumber > 1
                }
            };

            return new Response<object>
            {
                Success = true,
                Data = result,
                Message = "Support tickets retrieved successfully"
            };
        }
    }

    public class GetSupportTicketByIdForAdminQueryHandler : IRequestHandler<GetSupportTicketByIdForAdminQuery, Response<object>>
    {
        private readonly IQueryRepository<Domain.Entities.SupportTicket> _queryRepository;
        private readonly IQueryRepository<Domain.Entities.Dealer> _dealerRepository;
        private readonly ILogger<GetSupportTicketByIdForAdminQueryHandler> _logger;
        private readonly IMapper _mapper;

        public GetSupportTicketByIdForAdminQueryHandler(
            IQueryRepository<Domain.Entities.SupportTicket> queryRepository,
            IQueryRepository<Domain.Entities.Dealer> dealerRepository,
            ILogger<GetSupportTicketByIdForAdminQueryHandler> logger,
            IMapper mapper)
        {
            _queryRepository = queryRepository;
            _dealerRepository = dealerRepository;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<Response<object>> Handle(GetSupportTicketByIdForAdminQuery request, CancellationToken cancellationToken)
        {
            // Get dealers for dealer names
            var dealers = await _dealerRepository.GetAllAsync();
            var dealerLookup = dealers.ToDictionary(d => d.DealerId, d => d.DealerName);

            var ticket = await _queryRepository.GetQueryable()
                .Where(st => st.TicketId == request.Id && !st.IsDeleted && st.IsActive)
                .Include(st => st.Messages)
                .Select(st => new
                {
                    TicketId = st.TicketId,
                    TicketNumber = st.TicketNumber,
                    Title = st.Subject,
                    Description = st.Description,
                    DealerId = st.DealerId,
                    DealerName = dealerLookup.ContainsKey(st.DealerId) ? dealerLookup[st.DealerId] : "Unknown",
                    Category = st.Category,
                    Priority = st.Priority,
                    Status = st.Status,
                    AssignedTo = st.AssignedTo,
                    CreatedDate = st.CreatedDate,
                    UpdatedDate = st.ModifiedDate,
                    ResolvedDate = st.ResolvedAt,
                    Responses = st.Messages.Select(m => new
                    {
                        ResponseId = m.MessageId,
                        TicketId = m.TicketId,
                        Message = m.Message,
                        CreatedBy = m.SenderName,
                        CreatedDate = m.SentAt,
                        IsInternal = !m.IsFromDealer
                    }).OrderBy(m => m.CreatedDate).ToList()
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (ticket == null)
            {
                return new Response<object>
                {
                    Success = false,
                    Message = "Support ticket not found"
                };
            }

            return new Response<object>
            {
                Success = true,
                Data = ticket,
                Message = "Support ticket retrieved successfully"
            };
        }
    }

    public class ExportSupportTicketsForAdminQueryHandler : IRequestHandler<ExportSupportTicketsForAdminQuery, ExportFileResult>
    {
        public async Task<ExportFileResult> Handle(ExportSupportTicketsForAdminQuery request, CancellationToken cancellationToken)
        {
            return new ExportFileResult
            {
                FileContent = new byte[0],
                ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                FileName = "support_tickets.xlsx"
            };
        }
    }
}