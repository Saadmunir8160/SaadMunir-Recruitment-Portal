using Application.DTOs;
using Domain.Repositories.Query.Base;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Application.Queries.Admin.DealerProduct
{
    public class GetAllDealerProductsForAdminQuery : IRequest<PaginatedResponse<DealerProductForAdminDTO>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? Search { get; set; }
        public int? DealerId { get; set; }
        public bool? IsActive { get; set; }
    }

    public class GetAllDealerProductsForAdminQueryHandler : IRequestHandler<GetAllDealerProductsForAdminQuery, PaginatedResponse<DealerProductForAdminDTO>>
    {
        private readonly IQueryRepository<Domain.Entities.DealerProduct> _dealerProductRepository;
        private readonly IQueryRepository<Domain.Entities.Dealer> _dealerRepository;

        public GetAllDealerProductsForAdminQueryHandler(
            IQueryRepository<Domain.Entities.DealerProduct> dealerProductRepository,
            IQueryRepository<Domain.Entities.Dealer> dealerRepository)
        {
            _dealerProductRepository = dealerProductRepository;
            _dealerRepository = dealerRepository;
        }

        public async Task<PaginatedResponse<DealerProductForAdminDTO>> Handle(GetAllDealerProductsForAdminQuery request, CancellationToken cancellationToken)
        {
            var query = _dealerProductRepository.GetQueryable();

            // Apply filters
            if (request.DealerId.HasValue)
            {
                query = query.Where(dp => dp.DealerID == request.DealerId.Value);
            }

            if (request.IsActive.HasValue)
            {
                query = query.Where(dp => dp.IsActive == request.IsActive.Value);
            }

            if (!string.IsNullOrEmpty(request.Search))
            {
                var searchLower = request.Search.ToLower();
                query = query.Where(dp => 
                    dp.ProductName.ToLower().Contains(searchLower) ||
                    (dp.Description != null && dp.Description.ToLower().Contains(searchLower)));
            }

            // Get total count for pagination
            var totalRecords = await query.CountAsync(cancellationToken);

            // Apply pagination - Order by LN Product Code descending
            var products = await query
                .OrderByDescending(dp => dp.Product_LnCode)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            // Get dealer information
            var dealerIds = products.Select(p => p.DealerID).Distinct().ToList();
            var dealers = await _dealerRepository.GetQueryable()
                .Where(d => dealerIds.Contains(d.DealerId))
                .ToListAsync(cancellationToken);

            var dealerLookup = dealers.ToDictionary(d => d.DealerId, d => d.DealerName);

            // Map to DTOs
            var productDTOs = products.Select(product => new DealerProductForAdminDTO
            {
                DealerProductID = product.DealerProductID,
                DealerID = product.DealerID ?? 0,
                DealerName = product.DealerID.HasValue ? dealerLookup.GetValueOrDefault(product.DealerID.Value, "Unknown Dealer") : "No Dealer Assigned",
                ProductID = 0, // DealerProduct doesn't have ProductID - it's a standalone entity
                ProductName = product.ProductName,
                ProductDescription = product.Description,
                Product_LnCode = product.Product_LnCode,
                PricePerUnit = 0, // DealerProduct doesn't have PricePerUnit - would need to be added
                AvailableQuantity = 0, // DealerProduct doesn't have AvailableQuantity - would need to be added
                UnitOfMeasure = product.Unit,
                IsActive = product.IsActive,
                CreatedDate = product.CreatedDate,
                ModifiedDate = product.ModifiedDate ?? DateTime.UtcNow
            }).ToList();

            var totalPages = (int)Math.Ceiling((double)totalRecords / request.PageSize);

            return new PaginatedResponse<DealerProductForAdminDTO>
            {
                Data = productDTOs,
                Metadata = new PaginationMetadata
                {
                    CurrentPage = request.PageNumber,
                    PageSize = request.PageSize,
                    TotalCount = totalRecords,
                    TotalPages = totalPages
                }
            };
        }
    }

    public class GetDealerProductByIdForAdminQuery : IRequest<DealerProductForAdminDTO?>
    {
        public int Id { get; set; }
    }

    public class GetDealerProductByIdForAdminQueryHandler : IRequestHandler<GetDealerProductByIdForAdminQuery, DealerProductForAdminDTO?>
    {
        private readonly IQueryRepository<Domain.Entities.DealerProduct> _dealerProductRepository;
        private readonly IQueryRepository<Domain.Entities.Dealer> _dealerRepository;

        public GetDealerProductByIdForAdminQueryHandler(
            IQueryRepository<Domain.Entities.DealerProduct> dealerProductRepository,
            IQueryRepository<Domain.Entities.Dealer> dealerRepository)
        {
            _dealerProductRepository = dealerProductRepository;
            _dealerRepository = dealerRepository;
        }

        public async Task<DealerProductForAdminDTO?> Handle(GetDealerProductByIdForAdminQuery request, CancellationToken cancellationToken)
        {
            var product = await _dealerProductRepository.GetByIdAsync(request.Id);
            if (product == null)
                return null;

            Domain.Entities.Dealer? dealer = null;
            if (product.DealerID.HasValue)
            {
                dealer = await _dealerRepository.GetByIdAsync(product.DealerID.Value);
            }

            return new DealerProductForAdminDTO
            {
                DealerProductID = product.DealerProductID,
                DealerID = product.DealerID ?? 0,
                DealerName = dealer?.DealerName ?? (product.DealerID.HasValue ? "Unknown Dealer" : "No Dealer Assigned"),
                ProductID = 0, // DealerProduct doesn't have ProductID
                ProductName = product.ProductName,
                ProductDescription = product.Description,
                Product_LnCode = product.Product_LnCode,
                PricePerUnit = 0, // DealerProduct doesn't have PricePerUnit
                AvailableQuantity = 0, // DealerProduct doesn't have AvailableQuantity
                UnitOfMeasure = product.Unit,
                IsActive = product.IsActive,
                CreatedDate = product.CreatedDate,
                ModifiedDate = product.ModifiedDate ?? DateTime.UtcNow
            };
        }
    }

    public class GetDealerProductsStatsForAdminQuery : IRequest<DealerProductsStatsForAdminDTO>
    {
        public int? DealerId { get; set; }
    }

    public class GetDealerProductsStatsForAdminQueryHandler : IRequestHandler<GetDealerProductsStatsForAdminQuery, DealerProductsStatsForAdminDTO>
    {
        private readonly IQueryRepository<Domain.Entities.DealerProduct> _dealerProductRepository;

        public GetDealerProductsStatsForAdminQueryHandler(IQueryRepository<Domain.Entities.DealerProduct> dealerProductRepository)
        {
            _dealerProductRepository = dealerProductRepository;
        }

        public async Task<DealerProductsStatsForAdminDTO> Handle(GetDealerProductsStatsForAdminQuery request, CancellationToken cancellationToken)
        {
            var query = _dealerProductRepository.GetQueryable();

            if (request.DealerId.HasValue)
            {
                query = query.Where(dp => dp.DealerID == request.DealerId.Value);
            }

            var products = await query.ToListAsync(cancellationToken);

            return new DealerProductsStatsForAdminDTO
            {
                TotalProducts = products.Count,
                ActiveProducts = products.Count(p => p.IsActive),
                InactiveProducts = products.Count(p => !p.IsActive),
                TotalInventoryValue = 0, // Need to add inventory value calculation when PricePerUnit/AvailableQuantity are added
                LowStockProducts = 0, // Need to add when AvailableQuantity is added to DealerProduct entity
                OutOfStockProducts = 0 // Need to add when AvailableQuantity is added to DealerProduct entity
            };
        }
    }
}