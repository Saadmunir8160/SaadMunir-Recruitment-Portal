using Application.Common.Exceptions;
using Application.Common.Extensions;
using Application.DTOs;
using Domain.Repositories.Query.Base;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Application.Queries.Transporter
{
    public class GetAllTransportersQuery : IRequest<PaginatedResponse<Domain.Entities.Transporter>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class GetAllTransportersQueryHandler : IRequestHandler<GetAllTransportersQuery, PaginatedResponse<Domain.Entities.Transporter>>
    {
        private readonly IQueryRepository<Domain.Entities.Transporter> _queryRepository;
        private readonly ILogger<GetAllTransportersQueryHandler> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public GetAllTransportersQueryHandler(
            IQueryRepository<Domain.Entities.Transporter> queryRepository,
            ILogger<GetAllTransportersQueryHandler> logger,
            IHttpContextAccessor httpContextAccessor)
        {
            _queryRepository = queryRepository;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<PaginatedResponse<Domain.Entities.Transporter>> Handle(GetAllTransportersQuery request, CancellationToken cancellationToken)
        {
            var loggedInUser = _httpContextAccessor.HttpContext?.User?.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value ?? "Anonymous";
            
            _logger.LogInformation("Fetching transporters - User: {User}, Page: {PageNumber}, Size: {PageSize}", 
                loggedInUser, request.PageNumber, request.PageSize);

            var parameters = new PaginationParameters
            {
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };

            try
            {
                var query = _queryRepository.GetQueryable();
                
                // Let ToPaginatedResponseAsync handle empty results gracefully
                var result = await query.ToPaginatedResponseAsync(parameters);
                
                _logger.LogInformation("Successfully retrieved {Count} transporters out of {Total} total transporters", 
                    result.Data.Count, result.Metadata.TotalCount);
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching transporters for user: {User}", loggedInUser);
                throw;
            }
        }
    }
} 