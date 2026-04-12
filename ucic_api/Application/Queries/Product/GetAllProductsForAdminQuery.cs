using Application.Common.Exceptions;
using Application.DTOs;
using Domain.Repositories.Query.Base;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Application.Queries.Product
{
    public class GetAllProductsForAdminQuery : IRequest<List<ProductDTO>>
    {
    }

    public class GetAllProductsForAdminQueryHandler : IRequestHandler<GetAllProductsForAdminQuery,List<ProductDTO>>
    {
        private readonly IQueryRepository<Domain.Entities.Product> _queryRepository;

        private readonly IHttpContextAccessor _httpContextAccessor;

        public GetAllProductsForAdminQueryHandler(IQueryRepository<Domain.Entities.Product> queryRepository, IHttpContextAccessor httpContextAccessor)
        {
            _queryRepository = queryRepository;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<List<ProductDTO>> Handle(GetAllProductsForAdminQuery request, CancellationToken cancellationToken)
        {

            var products = await _queryRepository.GetAllAsync();
            if (products == null || !products.Any())
            {
                throw new NotFoundException("No products found.");
            }
            var results = products
                .OrderByDescending(product => product.CreatedDate)
                .Select(product => new ProductDTO
                {
                    ProductId = product.ProductId,
                    CoverageAreaId = product.CoverageAreaId,
                    CoverageAreaName = product.CoverageArea?.Name ?? string.Empty,
                    ProductName = product.Name ?? string.Empty,
                    ArabicName = product.ArabicName ?? string.Empty,
                    ProductDescription = product.Description ?? string.Empty,
                    ArabicDescription = product.ArabicDescription ?? string.Empty,
                    Code = product.Code ?? string.Empty,
                    Currency = product.Currency ?? string.Empty,
                    DiscountPercentage = product.DiscountPercentage,
                    ImageUrl = product.ImageUrl ?? string.Empty,
                    Price = product.Price,
                    ShippingCostPercentage = product.ShippingCostPercentage,
                    Sku = product.Sku ?? string.Empty,
                    Type = product.Type ?? string.Empty,
                    VatPercentage = product.VatPercentage
                })
                .ToList();


            return results;

        }
    }
}
