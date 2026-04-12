using Application.Common.Exceptions;
using Application.DTOs;
using Domain.Repositories.Query.Base;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Application.Queries.Product
{
    public class GetProductByIdForAdminQuery : IRequest<ProductByIdDTO>
    {

        public long productID { get; set; }
    }

    public class GetProductByIdForAdminQueryHandler : IRequestHandler<GetProductByIdForAdminQuery, ProductByIdDTO>
    {
        private readonly IQueryRepository<Domain.Entities.Product> _queryRepository;

        private readonly IHttpContextAccessor _httpContextAccessor;

        public GetProductByIdForAdminQueryHandler(IQueryRepository<Domain.Entities.Product> queryRepository, IHttpContextAccessor httpContextAccessor)
        {
            _queryRepository = queryRepository;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<ProductByIdDTO> Handle(GetProductByIdForAdminQuery request, CancellationToken cancellationToken)
        {

            var product = await _queryRepository.GetByIdAsync(request.productID);
            if (product == null)
            {
                throw new NotFoundException("No products found.");
            }
            var result =  new ProductByIdDTO
            {
                    ProductId = product.ProductId,
                    CoverageAreaId = product.CoverageAreaId,
                    CoverageAreaName = product.CoverageArea?.Name ?? string.Empty,
                    Name = product.Name ?? string.Empty,
                    ArabicName = product.ArabicName ?? string.Empty,
                    Description = product.Description ?? string.Empty,
                    ArabicDescription = product.ArabicDescription ?? string.Empty,
                    ProductCode = product.Code ?? string.Empty,
                    Currency = product.Currency ?? string.Empty,
                    DiscountPercentage = product.DiscountPercentage,
                    ProductFilePath = product.ImageUrl ?? string.Empty,
                    Price = product.Price,
                    shipingCostPercentage = product.ShippingCostPercentage,
                    Sku = product.Sku ?? string.Empty,
                    Type = product.Type ?? string.Empty,
                    VatPercentage = product.VatPercentage
                };


            return result;

        }
    }
}
