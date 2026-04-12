using Application.DTOs;
using Application.Common.Interfaces;
using Domain.Entities;
using Domain.Repositories.Command.Base;
using Domain.Repositories.Query.Base;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Commands.DealerShippingAddress
{
    public class UpdateDealerShippingAddressCommand : IRequest<Response<bool>>
    {
        public required UpdateDealerShippingAddressDTO DealerShippingAddress { get; set; }
    }

    public class UpdateDealerShippingAddressCommandHandler : IRequestHandler<UpdateDealerShippingAddressCommand, Response<bool>>
    {
        private readonly ICommandRepository<Domain.Entities.DealerShippingAddress> _commandRepository;
        private readonly IQueryRepository<Domain.Entities.DealerShippingAddress> _queryRepository;
        private readonly IIdentityService _identityService;

        public UpdateDealerShippingAddressCommandHandler(
            ICommandRepository<Domain.Entities.DealerShippingAddress> commandRepository, 
            IQueryRepository<Domain.Entities.DealerShippingAddress> queryRepository,
            IIdentityService identityService)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _identityService = identityService;
        }

        public async Task<Response<bool>> Handle(UpdateDealerShippingAddressCommand request, CancellationToken cancellationToken)
        {
            // Get current dealer ID
            var currentDealerId = await _identityService.GetCurrentDealerIdAsync();
            if (currentDealerId == null)
            {
                return new Response<bool>
                {
                    Success = false,
                    Message = "Dealer not found for current user"
                };
            }

            // Find address that belongs to current dealer
            var entity = await _queryRepository.GetQueryable()
                .Where(a => a.AddressID == request.DealerShippingAddress.AddressID && a.DealerID == currentDealerId.Value)
                .FirstOrDefaultAsync();

            if (entity == null)
                return new Response<bool> { Success = false, Message = "Dealer shipping address not found." };

            // Update address properties (except DealerID which should remain current dealer)
            entity.Ln_ID = request.DealerShippingAddress.Ln_ID;
            entity.AddressLine1 = request.DealerShippingAddress.AddressLine1;
            entity.AddressLine2 = request.DealerShippingAddress.AddressLine2;
            entity.City = request.DealerShippingAddress.City;
            entity.State = request.DealerShippingAddress.State;
            entity.Country = request.DealerShippingAddress.Country;
            entity.PostalCode = request.DealerShippingAddress.PostalCode;
            entity.IsActive = request.DealerShippingAddress.IsActive;
            entity.ModifiedDate = DateTime.UtcNow;
            entity.ModifiedBy = _identityService.GetCurrentUserId();

            await _commandRepository.UpdateAsync(entity);
            return new Response<bool> { Success = true, Data = true, Message = "Dealer shipping address updated successfully." };
        }
    }
}
