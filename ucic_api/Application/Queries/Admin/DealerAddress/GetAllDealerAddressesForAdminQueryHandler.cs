using MediatR;
using Application.DTOs;
using Application.Common.Interfaces;
using Domain.Repositories.Query.Base;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace Application.Queries.Admin.DealerAddress
{
    public class GetAllDealerAddressesForAdminQueryHandler : IRequestHandler<GetAllDealerAddressesForAdminQuery, Response<object>>
    {
        private readonly IQueryRepository<Domain.Entities.DealerShippingAddress> _addressRepository;
        private readonly ILogger<GetAllDealerAddressesForAdminQueryHandler> _logger;

        public GetAllDealerAddressesForAdminQueryHandler(
            IQueryRepository<Domain.Entities.DealerShippingAddress> addressRepository,
            ILogger<GetAllDealerAddressesForAdminQueryHandler> logger)
        {
            _addressRepository = addressRepository;
            _logger = logger;
        }

        public async Task<Response<object>> Handle(GetAllDealerAddressesForAdminQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var query = _addressRepository.GetQueryable()
                    .Where(addr => addr.IsActive);

                // Apply dealer filter
                if (request.DealerId.HasValue)
                {
                    query = query.Where(addr => addr.DealerID == request.DealerId.Value);
                }

                // Apply search filter
                if (!string.IsNullOrEmpty(request.Search))
                {
                    query = query.Where(addr => 
                        addr.AddressLine1!.Contains(request.Search) ||
                        addr.AddressLine2!.Contains(request.Search) ||
                        addr.City!.Contains(request.Search) ||
                        addr.State!.Contains(request.Search) ||
                        (addr.Dealer != null && addr.Dealer.DealerName.Contains(request.Search)));
                }

                // Include dealer information
                query = query.Include(addr => addr.Dealer);

                // Get total count
                var totalCount = await query.CountAsync(cancellationToken);

                // Apply pagination
                var addresses = await query
                    .OrderByDescending(addr => addr.CreatedDate)
                    .Skip((request.PageNumber - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .Select(addr => new
                    {
                        AddressId = addr.AddressID,
                        addr.DealerID,
                        DealerName = addr.Dealer != null ? addr.Dealer.DealerName : "Unknown Dealer",
                        addr.AddressLine1,
                        addr.AddressLine2,
                        addr.City,
                        addr.State,
                        addr.Country,
                        PostalCode = addr.PostalCode,
                        AddressType = "Shipping", // Default since this is shipping address table
                        IsDefault = false, // You may need to add this field to the entity
                        addr.IsActive,
                        addr.CreatedDate,
                        ModifiedDate = addr.ModifiedDate
                    })
                    .ToListAsync(cancellationToken);

                var totalPages = (int)Math.Ceiling((double)totalCount / request.PageSize);

                var result = new
                {
                    Data = addresses,
                    Metadata = new
                    {
                        CurrentPage = request.PageNumber,
                        PageSize = request.PageSize,
                        TotalCount = totalCount,
                        TotalPages = totalPages,
                        HasNext = request.PageNumber < totalPages,
                        HasPrevious = request.PageNumber > 1
                    }
                };

                return new Response<object>
                {
                    Success = true,
                    Data = result,
                    Message = "Dealer addresses retrieved successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving dealer addresses for admin");
                return new Response<object>
                {
                    Success = false,
                    Message = "An error occurred while retrieving addresses"
                };
            }
        }
    }
}