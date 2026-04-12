using Application.DTOs;
using Domain.Repositories.Query.Base;
using MediatR;

namespace Application.Queries.DealerProduct
{
    public class GetProductCategoriesQuery : IRequest<List<string>>
    {
    }

    public class GetProductCategoriesQueryHandler : IRequestHandler<GetProductCategoriesQuery, List<string>>
    {
        private readonly IQueryRepository<Domain.Entities.Category> _categoryRepository;

        public GetProductCategoriesQueryHandler(IQueryRepository<Domain.Entities.Category> categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public async Task<List<string>> Handle(GetProductCategoriesQuery request, CancellationToken cancellationToken)
        {
            var query = _categoryRepository.GetQueryable();
            
            if (query == null)
                return new List<string>();

            var activeCategories = query.Where(c => c.Active).OrderBy(c => c.Name);
            
            return activeCategories.Select(c => c.Name).ToList();
        }
    }
}