using Application.DTOs;
using Application.Common.Interfaces;
using Domain.Entities;
using Domain.Repositories.Query.Base;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Queries.DealerEntities
{
    public class GetAllDealerShippingAddressesQuery : IRequest<Response<List<DealerShippingAddressDTO>>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class GetAllDealerShippingAddressesQueryHandler : IRequestHandler<GetAllDealerShippingAddressesQuery, Response<List<DealerShippingAddressDTO>>>
    {
        private readonly IQueryRepository<Domain.Entities.DealerShippingAddress> _repository;
        private readonly IIdentityService _identityService;

        public GetAllDealerShippingAddressesQueryHandler(
            IQueryRepository<Domain.Entities.DealerShippingAddress> repository,
            IIdentityService identityService)
        {
            _repository = repository;
            _identityService = identityService;
        }

        public async Task<Response<List<DealerShippingAddressDTO>>> Handle(GetAllDealerShippingAddressesQuery request, CancellationToken cancellationToken)
        {
            // Get current dealer ID
            var currentDealerId = await _identityService.GetCurrentDealerIdAsync();
            if (currentDealerId == null)
            {
                return new Response<List<DealerShippingAddressDTO>>
                {
                    Success = false,
                    Message = "Dealer not found for current user",
                    Data = new List<DealerShippingAddressDTO>()
                };
            }

            // Filter by current dealer
            var addresses = await _repository.GetQueryable()
                .Where(a => a.DealerID == currentDealerId.Value && a.IsActive && !a.IsDeleted)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();

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
                Message = "Dealer shipping addresses retrieved successfully." 
            };
        }
    }
}
