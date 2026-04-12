using Application.DTOs;
using Domain.Repositories.Query.Base;
using MediatR;

namespace Application.Queries.DealerProduct
{
    public class CheckProductAvailabilityQuery : IRequest<bool>
    {
        public int ProductId { get; set; }
        public int RequestedQuantity { get; set; }
    }

    public class CheckProductAvailabilityQueryHandler : IRequestHandler<CheckProductAvailabilityQuery, bool>
    {
        private readonly IQueryRepository<Domain.Entities.DealerProduct> _productRepository;

        public CheckProductAvailabilityQueryHandler(IQueryRepository<Domain.Entities.DealerProduct> productRepository)
        {
            _productRepository = productRepository;
        }

        public async Task<bool> Handle(CheckProductAvailabilityQuery request, CancellationToken cancellationToken)
        {
            // Check if the product exists and is active
            var product = await _productRepository.GetByIdAsync(request.ProductId);
            
            if (product == null || !product.IsActive)
                return false;

            // For now, since DealerProduct doesn't have stock quantity field,
            // we'll return true if product exists and is active
            // In future, this can be enhanced with proper inventory management
            return true;
        }
    }
}