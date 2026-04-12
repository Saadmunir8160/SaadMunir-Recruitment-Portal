using Domain.Repositories.Query.Base;
using MediatR;
using Application.DTOs;
using Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Application.Queries.DealerProduct
{
    public class GetDealerProductByIdQuery : IRequest<DealerProductDTO?>
    {
        public int Id { get; set; }
    }

    public class GetDealerProductByIdQueryHandler : IRequestHandler<GetDealerProductByIdQuery, DealerProductDTO?>
    {
        private readonly IQueryRepository<Domain.Entities.DealerProduct> _queryRepository;
        private readonly IIdentityService _identityService;

        public GetDealerProductByIdQueryHandler(
            IQueryRepository<Domain.Entities.DealerProduct> queryRepository,
            IIdentityService identityService)
        {
            _queryRepository = queryRepository;
            _identityService = identityService;
        }

        public async Task<DealerProductDTO?> Handle(GetDealerProductByIdQuery request, CancellationToken cancellationToken)
        {
            // Get current dealer ID for authentication (but don't filter by it)
            var currentDealerId = await _identityService.GetCurrentDealerIdAsync();
            if (currentDealerId == null)
            {
                return null; // No dealer found for current user
            }

            // Find any active product (not filtered by dealer) - products are catalog items
            var entity = await _queryRepository.GetQueryable()
                .Where(p => p.DealerProductID == request.Id && p.IsActive && !p.IsDeleted)
                .FirstOrDefaultAsync();
            
            if (entity == null)
                return null;

            return new DealerProductDTO
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
            };
        }
    }
} 