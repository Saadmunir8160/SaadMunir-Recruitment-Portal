using Application.Common.Extensions;
using Application.DTOs;
using Domain.Repositories.Query.Base;
using MediatR;

namespace Application.Queries.Promotion
{
    public class GetAllPromotionQuery : IRequest<PaginatedResponse<PromotionDto>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class GetAllPromotionQueryHandler : IRequestHandler<GetAllPromotionQuery, PaginatedResponse<PromotionDto>>
    {
        private readonly IQueryRepository<Domain.Entities.Promotion> _queryRepository;

        public GetAllPromotionQueryHandler(IQueryRepository<Domain.Entities.Promotion> queryRepository)
        {
            _queryRepository = queryRepository;
        }

        public async Task<PaginatedResponse<PromotionDto>> Handle(GetAllPromotionQuery request, CancellationToken cancellationToken)
        {
            var parameters = new PaginationParameters
            {
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };

            var query = _queryRepository.GetQueryable();
            if (query == null || !query.Any())
                throw new KeyNotFoundException("Promotions not found");

            var promotionsQuery = query.Select(promotion => new PromotionDto
            {
                PromotionId = promotion.PromotionId,
                CoverageAreaId = promotion.CoverageAreaId,
                Code = promotion.Code,
                DiscountPercentage = promotion.DiscountPercentage,
                ValidFrom = promotion.ValidFrom,
                ValidTo = promotion.ValidTo
            });

            return await promotionsQuery.ToPaginatedResponseAsync(parameters);
        }
    }
} 