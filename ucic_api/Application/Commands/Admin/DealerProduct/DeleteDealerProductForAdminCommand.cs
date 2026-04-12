using Application.DTOs;
using Domain.Repositories.Command.Base;
using Domain.Repositories.Query.Base;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Commands.Admin.DealerProduct
{
    public class DeleteDealerProductForAdminCommand : IRequest<Response<string>>
    {
        public int Id { get; set; }
    }

    public class DeleteDealerProductForAdminCommandHandler : IRequestHandler<DeleteDealerProductForAdminCommand, Response<string>>
    {
        private readonly ICommandRepository<Domain.Entities.DealerProduct> _commandRepository;
        private readonly IQueryRepository<Domain.Entities.DealerProduct> _queryRepository;
        private readonly IQueryRepository<Domain.Entities.DealerOrderItem> _orderItemQueryRepository;

        public DeleteDealerProductForAdminCommandHandler(
            ICommandRepository<Domain.Entities.DealerProduct> commandRepository,
            IQueryRepository<Domain.Entities.DealerProduct> queryRepository,
            IQueryRepository<Domain.Entities.DealerOrderItem> orderItemQueryRepository)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _orderItemQueryRepository = orderItemQueryRepository;
        }

        public async Task<Response<string>> Handle(DeleteDealerProductForAdminCommand request, CancellationToken cancellationToken)
        {
            // Admin can delete any product - no dealer check needed
            var dealerProduct = await _queryRepository.GetByIdAsync(request.Id);

            if (dealerProduct == null)
            {
                return new Response<string>
                {
                    Success = false,
                    Message = "Dealer Product not found.",
                    Data = "Failed"
                };
            }

            try
            {
                // Check if this product is used in any order items (including soft-deleted orders)
                var isUsedInOrders = await _orderItemQueryRepository.GetQueryable()
                    .AnyAsync(oi => oi.DealerProductID == request.Id, cancellationToken);

                if (isUsedInOrders)
                {
                    return new Response<string>
                    {
                        Success = false,
                        Message = "Cannot delete this product because it is referenced in one or more orders. Please remove the product from all orders before deleting.",
                        Data = "Failed"
                    };
                }

                // Use hard delete to permanently remove the product
                await _commandRepository.HardDeleteAsync(dealerProduct);

                return new Response<string>
                {
                    Success = true,
                    Message = "Successfully deleted dealer product.",
                    Data = "success"
                };
            }
            catch (Exception ex)
            {
                return new Response<string>
                {
                    Success = false,
                    Message = $"Failed to delete dealer product: {ex.Message}",
                    Data = "Failed"
                };
            }
        }
    }
}
