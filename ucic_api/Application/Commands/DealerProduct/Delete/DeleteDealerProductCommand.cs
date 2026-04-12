using Application.Common.Interfaces;
using Domain.Repositories.Command.Base;
using Domain.Repositories.Query.Base;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Commands.DealerProduct.Delete
{
    public class DeleteDealerProductCommand : IRequest<bool>
    {
        public int Id { get; set; }
    }

    public class DeleteDealerProductCommandHandler : IRequestHandler<DeleteDealerProductCommand, bool>
    {
        private readonly ICommandRepository<Domain.Entities.DealerProduct> _commandRepository;
        private readonly IQueryRepository<Domain.Entities.DealerProduct> _queryRepository;
        private readonly IIdentityService _identityService;

        public DeleteDealerProductCommandHandler(
            ICommandRepository<Domain.Entities.DealerProduct> commandRepository,
            IQueryRepository<Domain.Entities.DealerProduct> queryRepository,
            IIdentityService identityService)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _identityService = identityService;
        }

        public async Task<bool> Handle(DeleteDealerProductCommand request, CancellationToken cancellationToken)
        {
            // Get current dealer ID
            var currentDealerId = await _identityService.GetCurrentDealerIdAsync();
            if (currentDealerId == null)
            {
                return false; // No dealer found for current user
            }

            // Find product that belongs to current dealer
            var dealerProduct = await _queryRepository.GetQueryable()
                .Where(p => p.DealerProductID == request.Id && p.DealerID == currentDealerId.Value)
                .FirstOrDefaultAsync();

            if (dealerProduct == null)
            {
                return false;
            }

            // Hard delete will fail if product is used in orders (RESTRICT constraint)
            try
            {
                await _commandRepository.HardDeleteAsync(dealerProduct);
                return true;
            }
            catch
            {
                // Product is referenced in order items
                return false;
            }
        }
    }
} 