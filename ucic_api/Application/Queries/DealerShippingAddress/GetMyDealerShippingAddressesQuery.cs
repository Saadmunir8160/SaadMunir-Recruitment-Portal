using Application.Common.Interfaces;
using Application.DTOs;
using Domain.Entities;
using Domain.Repositories.Query.Base;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Queries.DealerEntities
{
    public class GetMyDealerShippingAddressesQuery : IRequest<Response<List<DealerShippingAddressDTO>>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class GetMyDealerShippingAddressesQueryHandler : IRequestHandler<GetMyDealerShippingAddressesQuery, Response<List<DealerShippingAddressDTO>>>
    {
        private readonly IQueryRepository<Domain.Entities.DealerShippingAddress> _repository;
        private readonly ILogger<GetMyDealerShippingAddressesQueryHandler> _logger;
        private readonly IIdentityService _identityService;

        public GetMyDealerShippingAddressesQueryHandler(
            IQueryRepository<Domain.Entities.DealerShippingAddress> repository,
            ILogger<GetMyDealerShippingAddressesQueryHandler> logger,
            IIdentityService identityService)
        {
            _repository = repository;
            _logger = logger;
            _identityService = identityService;
        }

        public async Task<Response<List<DealerShippingAddressDTO>>> Handle(GetMyDealerShippingAddressesQuery request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Starting to fetch dealer shipping addresses");

                // Get current dealer ID directly - this handles authentication internally
                var currentDealerId = await _identityService.GetCurrentDealerIdAsync();
                _logger.LogInformation("Current Dealer ID: {DealerId}", currentDealerId?.ToString() ?? "NULL");

                if (currentDealerId == null)
                {
                    _logger.LogWarning("No dealer found for current authenticated user");
                    return new Response<List<DealerShippingAddressDTO>>
                    {
                        Success = false,
                        Message = "No dealer profile found for current user",
                        Data = new List<DealerShippingAddressDTO>()
                    };
                }

                // Get addresses for this specific dealer
                var addresses = await _repository.GetQueryable()
                    .Where(x => x.DealerID == currentDealerId.Value && !x.IsDeleted && x.IsActive)
                    .OrderByDescending(x => x.CreatedDate)
                    .Skip((request.PageNumber - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .ToListAsync();

                _logger.LogInformation("Found {Count} addresses for dealer {DealerId}", addresses.Count, currentDealerId.Value);

                var result = addresses.Select(d => new DealerShippingAddressDTO
                {
                    AddressID = d.AddressID,
                    DealerID = d.DealerID,
                    Ln_ID = d.Ln_ID,
                    AddressLine1 = d.AddressLine1,
                    AddressLine2 = d.AddressLine2,
                    City = d.City,
                    State = d.State,
                    Country = d.Country,
                    PostalCode = d.PostalCode,
                    IsActive = d.IsActive
                }).ToList();

                return new Response<List<DealerShippingAddressDTO>>
                {
                    Success = true,
                    Data = result,
                    Message = "Addresses retrieved successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving addresses");
                return new Response<List<DealerShippingAddressDTO>>
                {
                    Success = false,
                    Message = $"Error retrieving addresses: {ex.Message}",
                    Data = new List<DealerShippingAddressDTO>()
                };
            }
        }
    }
}