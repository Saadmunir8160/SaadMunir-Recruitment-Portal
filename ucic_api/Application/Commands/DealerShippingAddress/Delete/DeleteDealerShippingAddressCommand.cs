using Application.DTOs;
using Application.Common.Interfaces;
using Domain.Repositories.Command.Base;
using Domain.Repositories.Query.Base;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Commands.DealerEntities
{
    public class DeleteDealerShippingAddressCommand : IRequest<Response<bool>>
    {
        public int AddressID { get; set; }
    }

    public class DeleteDealerShippingAddressCommandHandler : IRequestHandler<DeleteDealerShippingAddressCommand, Response<bool>>
    {
        private readonly ICommandRepository<Domain.Entities.DealerShippingAddress> _commandRepository;
        private readonly IQueryRepository<Domain.Entities.DealerShippingAddress> _queryRepository;
        private readonly IIdentityService _identityService;

        public DeleteDealerShippingAddressCommandHandler(
            ICommandRepository<Domain.Entities.DealerShippingAddress> commandRepository, 
            IQueryRepository<Domain.Entities.DealerShippingAddress> queryRepository,
            IIdentityService identityService)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _identityService = identityService;
        }

        public async Task<Response<bool>> Handle(DeleteDealerShippingAddressCommand request, CancellationToken cancellationToken)
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
                .Where(a => a.AddressID == request.AddressID && a.DealerID == currentDealerId.Value)
                .FirstOrDefaultAsync();

            if (entity == null)
                return new Response<bool> { Success = false, Message = "Dealer shipping address not found." };

            // Hard delete - will fail if address has orders due to RESTRICT constraint
            try
            {
                await _commandRepository.HardDeleteAsync(entity);
                return new Response<bool> { Success = true, Data = true, Message = "Dealer shipping address deleted successfully." };
            }
            catch (Exception ex)
            {
                return new Response<bool> 
                { 
                    Success = false, 
                    Message = "Cannot delete address with existing orders. Please remove associations first." 
                };
            }
        }
    }
}
