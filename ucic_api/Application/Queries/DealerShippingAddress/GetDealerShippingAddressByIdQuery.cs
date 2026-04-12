using Application.DTOs;
using Application.Common.Interfaces;
using Domain.Entities;
using Domain.Repositories.Query.Base;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Queries.DealerEntities
{
    public class GetDealerShippingAddressByIdQuery : IRequest<Response<DealerShippingAddressDTO>>
    {
        public int AddressID { get; set; }
    }

    public class GetDealerShippingAddressByIdQueryHandler : IRequestHandler<GetDealerShippingAddressByIdQuery, Response<DealerShippingAddressDTO>>
    {
        private readonly IQueryRepository<Domain.Entities.DealerShippingAddress> _repository;
        private readonly IIdentityService _identityService;

        public GetDealerShippingAddressByIdQueryHandler(
            IQueryRepository<Domain.Entities.DealerShippingAddress> repository,
            IIdentityService identityService)
        {
            _repository = repository;
            _identityService = identityService;
        }

        public async Task<Response<DealerShippingAddressDTO>> Handle(GetDealerShippingAddressByIdQuery request, CancellationToken cancellationToken)
        {
            // Get current dealer ID
            var currentDealerId = await _identityService.GetCurrentDealerIdAsync();
            if (currentDealerId == null)
            {
                return new Response<DealerShippingAddressDTO>
                {
                    Success = false,
                    Message = "Dealer not found for current user"
                };
            }

            // Find address that belongs to the current dealer
            var address = await _repository.GetQueryable()
                .Where(a => a.AddressID == request.AddressID && a.DealerID == currentDealerId.Value)
                .FirstOrDefaultAsync();

            if (address == null)
                return new Response<DealerShippingAddressDTO> { Success = false, Message = "Dealer shipping address not found." };

            var dto = new DealerShippingAddressDTO
            {
                AddressID = address.AddressID,
                DealerID = address.DealerID,
                Ln_ID = address.Ln_ID,
                AddressLine1 = address.AddressLine1,
                AddressLine2 = address.AddressLine2,
                City = address.City,
                State = address.State,
                Country = address.Country,
                PostalCode = address.PostalCode,
                IsActive = address.IsActive
            };
            return new Response<DealerShippingAddressDTO> { Success = true, Data = dto, Message = "Dealer shipping address retrieved successfully." };
        }
    }
}
