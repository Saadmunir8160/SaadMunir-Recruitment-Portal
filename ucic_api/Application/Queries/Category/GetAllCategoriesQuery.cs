using Application.Common.Extensions;
using Application.DTOs;
using Domain.Repositories.Query.Base;
using MediatR;

namespace Application.Queries.Category
{
    public class GetAllCategoriesQuery : IRequest<PaginatedResponse<CategoryDto>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class GetAllCategoriesQueryHandler : IRequestHandler<GetAllCategoriesQuery, PaginatedResponse<CategoryDto>>
    {
        private readonly IQueryRepository<Domain.Entities.Category> _queryRepository;

        public GetAllCategoriesQueryHandler(IQueryRepository<Domain.Entities.Category> queryRepository)
        {
            _queryRepository = queryRepository;
        }

        public async Task<PaginatedResponse<CategoryDto>> Handle(GetAllCategoriesQuery request, CancellationToken cancellationToken)
        {
            var parameters = new PaginationParameters
            {
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };

            var query = _queryRepository.GetQueryable();
            if (query == null || !query.Any())
                throw new KeyNotFoundException("Category not found");

            var categoriesQuery = query.Select(cat => new CategoryDto
            {
                CategoryId = cat.CategoryId,
                Name = cat.Name
            });

            return await categoriesQuery.ToPaginatedResponseAsync(parameters);
        }
    }
}
