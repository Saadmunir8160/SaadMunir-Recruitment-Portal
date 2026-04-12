using Application.Common.Interfaces;
using Application.DTOs;
using Application.DTOs.DealerOrder;
using Domain.Repositories.Command.Base;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Commands.DealerShippingAddress
{
    public class CreateDealerShippingAddressCommand : IRequest<Response<int>>
    {
        public CreateDealerShippingAddressDTO DealerShippingAddress { get; set; }
    }

    public class CreateDealerShippingAddressCommandHandler : IRequestHandler<CreateDealerShippingAddressCommand, Response<int>>
    {
        private readonly ICommandRepository<Domain.Entities.DealerShippingAddress> _repository;
        private readonly IIdentityService _identityService;
        
        public CreateDealerShippingAddressCommandHandler(
            ICommandRepository<Domain.Entities.DealerShippingAddress> repository,
            IIdentityService identityService)
        {
            _repository = repository;
            _identityService = identityService;
        }
        
        public async Task<Response<int>> Handle(CreateDealerShippingAddressCommand request, CancellationToken cancellationToken)
        {
            // Get current dealer ID using centralized method
            var currentDealerId = await _identityService.GetCurrentDealerIdAsync();
            if (currentDealerId == null)
            {
                return new Response<int>
                {
                    Success = false,
                    Message = "Dealer not found for current user"
                };
            }

            var entity = new Domain.Entities.DealerShippingAddress
            {
                DealerID = currentDealerId.Value, // Use centralized dealer ID
                Ln_ID = request.DealerShippingAddress.Ln_ID,
                AddressLine1 = request.DealerShippingAddress.AddressLine1,
                AddressLine2 = request.DealerShippingAddress.AddressLine2,
                City = request.DealerShippingAddress.City,
                State = request.DealerShippingAddress.State,
                Country = request.DealerShippingAddress.Country,
                PostalCode = request.DealerShippingAddress.PostalCode,
                IsActive = request.DealerShippingAddress.IsActive,
                CreatedDate = DateTime.UtcNow
            };
            
            await _repository.AddAsync(entity);
            return new Response<int> 
            { 
                Success = true, 
                Data = entity.AddressID, 
                Message = "Dealer shipping address created successfully." 
            };
        }
    }
}
