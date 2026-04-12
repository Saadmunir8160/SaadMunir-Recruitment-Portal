using Application.Common.Exceptions;
using Application.Common.Extensions;
using Application.DTOs;
using Domain.Repositories.Query.Base;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Application.Queries.Product
{
    public class GetAllProductsQuery : IRequest<PaginatedResponse<Domain.Entities.Product>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class GetAllProductsQueryHandler : IRequestHandler<GetAllProductsQuery, PaginatedResponse<Domain.Entities.Product>>
    {
        private readonly IQueryRepository<Domain.Entities.Product> _queryRepository;
        private readonly ILogger<GetAllProductsQueryHandler> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public GetAllProductsQueryHandler(IQueryRepository<Domain.Entities.Product> queryRepository, ILogger<GetAllProductsQueryHandler> logger, IHttpContextAccessor httpContextAccessor)
        {
            _queryRepository = queryRepository;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<PaginatedResponse<Domain.Entities.Product>> Handle(GetAllProductsQuery request, CancellationToken cancellationToken)
        {
            var parameters = new PaginationParameters
            {
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };

            var query = _queryRepository.GetQueryable();
            
            if (query == null || !query.Any())
            {
                throw new NotFoundException("No products found.");
            }

            return await query.ToPaginatedResponseAsync(parameters);
        }
    }
}
