using Application.Common.Exceptions;
using Application.Common.Extensions;
using Application.DTOs;
using Domain.Repositories.Query.Base;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Application.Queries.Driver
{
    public class GetAllDriversQuery : IRequest<PaginatedResponse<Domain.Entities.Driver>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class GetAllDriversQueryHandler : IRequestHandler<GetAllDriversQuery, PaginatedResponse<Domain.Entities.Driver>>
    {
        private readonly IQueryRepository<Domain.Entities.Driver> _queryRepository;
        private readonly ILogger<GetAllDriversQueryHandler> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public GetAllDriversQueryHandler(
            IQueryRepository<Domain.Entities.Driver> queryRepository,
            ILogger<GetAllDriversQueryHandler> logger,
            IHttpContextAccessor httpContextAccessor)
        {
            _queryRepository = queryRepository;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<PaginatedResponse<Domain.Entities.Driver>> Handle(GetAllDriversQuery request, CancellationToken cancellationToken)
        {
            var loggedInUser = _httpContextAccessor.HttpContext?.User?.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value ?? "Anonymous";
            
            _logger.LogInformation("Fetching drivers - User: {User}, Page: {PageNumber}, Size: {PageSize}", 
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
                
                _logger.LogInformation("Successfully retrieved {Count} drivers out of {Total} total drivers", 
                    result.Data.Count, result.Metadata.TotalCount);
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching drivers for user: {User}", loggedInUser);
                throw;
            }
        }
    }
} 