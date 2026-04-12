using Application.Common.Exceptions;
using Application.Common.Extensions;
using Application.DTOs;
using Domain.Repositories.Query.Base;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Application.Queries.DealerProduct
{
    public class GetAllDealerProductsQuery : IRequest<PaginatedResponse<DealerProductDTO>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class GetAllDealerProductsQueryHandler : IRequestHandler<GetAllDealerProductsQuery, PaginatedResponse<DealerProductDTO>>
    {
        private readonly IQueryRepository<Domain.Entities.DealerProduct> _queryRepository;
        private readonly ILogger<GetAllDealerProductsQueryHandler> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public GetAllDealerProductsQueryHandler(
            IQueryRepository<Domain.Entities.DealerProduct> queryRepository,
            ILogger<GetAllDealerProductsQueryHandler> logger,
            IHttpContextAccessor httpContextAccessor)
        {
            _queryRepository = queryRepository;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<PaginatedResponse<DealerProductDTO>> Handle(GetAllDealerProductsQuery request, CancellationToken cancellationToken)
        {
            var loggedInUser = _httpContextAccessor.HttpContext?.User?.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value ?? "Anonymous";
            
            _logger.LogInformation("Fetching dealer products - User: {User}, Page: {PageNumber}, Size: {PageSize}", 
                loggedInUser, request.PageNumber, request.PageSize);

            var parameters = new PaginationParameters
            {
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };

            try
            {
                var query = _queryRepository.GetQueryable();
                
                // Get entities first
                var entityResult = await query.ToPaginatedResponseAsync(parameters);
                
                // Map entities to DTOs
                var dtoData = entityResult.Data.Select(entity => new DealerProductDTO
                {
                    dealerProductID = entity.DealerProductID,
                    productName = entity.ProductName,
                    description = entity.Description,
                    shortDescription = !string.IsNullOrEmpty(entity.Description) && entity.Description.Length > 100 
                        ? entity.Description.Substring(0, 100) + "..." 
                        : entity.Description,
                    unit = entity.Unit, // Map the unit property from entity
                    ERPItemCode = entity.ProductCode,
                    ItemDescription = entity.Product_LnCode,
                    createdDate = entity.CreatedDate,
                    updatedDate = entity.ModifiedDate,
                    CreatedBy = entity.CreatedBy,
                    ModifiedBy = entity.ModifiedBy,
                    isActive = entity.IsActive
                }).ToList();

                var result = new PaginatedResponse<DealerProductDTO>
                {
                    Data = dtoData,
                    Metadata = entityResult.Metadata
                };
                
                _logger.LogInformation("Successfully retrieved {Count} dealer products out of {Total} total dealer products", 
                    result.Data.Count, result.Metadata.TotalCount);
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching dealer products for user: {User}", loggedInUser);
                throw;
            }
        }
    }
} 