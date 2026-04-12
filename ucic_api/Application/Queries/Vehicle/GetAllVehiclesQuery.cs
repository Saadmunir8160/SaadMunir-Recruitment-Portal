using Application.Common.Exceptions;
using Application.Common.Extensions;
using Application.DTOs;
using Domain.Repositories.Query.Base;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Application.Queries.Vehicle
{
    public class GetAllVehiclesQuery : IRequest<PaginatedResponse<Domain.Entities.Vehicle>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class GetAllVehiclesQueryHandler : IRequestHandler<GetAllVehiclesQuery, PaginatedResponse<Domain.Entities.Vehicle>>
    {
        private readonly IQueryRepository<Domain.Entities.Vehicle> _queryRepository;
        private readonly ILogger<GetAllVehiclesQueryHandler> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public GetAllVehiclesQueryHandler(
            IQueryRepository<Domain.Entities.Vehicle> queryRepository,
            ILogger<GetAllVehiclesQueryHandler> logger,
            IHttpContextAccessor httpContextAccessor)
        {
            _queryRepository = queryRepository;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<PaginatedResponse<Domain.Entities.Vehicle>> Handle(GetAllVehiclesQuery request, CancellationToken cancellationToken)
        {
            var loggedInUser = _httpContextAccessor.HttpContext?.User?.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value ?? "Anonymous";
            
            _logger.LogInformation("Fetching vehicles - User: {User}, Page: {PageNumber}, Size: {PageSize}", 
                loggedInUser, request.PageNumber, request.PageSize);

            var parameters = new PaginationParameters
            {
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };

            try
            {
                var query = _queryRepository.GetQueryable();
                if (query == null || !query.Any())
                {
                    return null; // Return null if no vehicles found, allowing ToPaginatedResponseAsync to handle it gracefully
                }
                // Let ToPaginatedResponseAsync handle empty results gracefully
                var result = await query.ToPaginatedResponseAsync(parameters);
                
                _logger.LogInformation("Successfully retrieved {Count} vehicles out of {Total} total vehicles", 
                    result.Data.Count, result.Metadata.TotalCount);
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching vehicles for user: {User}", loggedInUser);
                throw;
            }
        }
    }
} 