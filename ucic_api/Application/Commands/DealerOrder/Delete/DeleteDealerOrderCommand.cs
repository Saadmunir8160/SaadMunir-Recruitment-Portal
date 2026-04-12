using Application.Common.Interfaces;
using Application.DTOs;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace Application.Commands.DealerOrder.Delete
{
    public class DeleteDealerOrderCommand : IRequest<Response<string>>
    {
        [Required(ErrorMessage = "DealerOrderId is required")]
        public int DealerOrderId { get; set; }
    }

    public class DeleteDealerOrderCommandHandler : IRequestHandler<DeleteDealerOrderCommand, Response<string>>
    {
        private readonly Domain.Repositories.Command.Base.ICommandRepository<Domain.Entities.DealerOrder> _dealerOrderRepository;
        private readonly Domain.Repositories.Command.Base.ICommandRepository<Domain.Entities.DealerOrderItem> _dealerOrderItemRepository;
        private readonly Domain.Repositories.Query.Base.IQueryRepository<Domain.Entities.DealerOrder> _dealerOrderQueryRepository;
        private readonly Domain.Repositories.Query.Base.IQueryRepository<Domain.Entities.DealerOrderItem> _dealerOrderItemQueryRepository;
        private readonly Application.Common.Interfaces.IIdentityService _identityService;

        public DeleteDealerOrderCommandHandler(
            Domain.Repositories.Command.Base.ICommandRepository<Domain.Entities.DealerOrder> dealerOrderRepository,
            Domain.Repositories.Command.Base.ICommandRepository<Domain.Entities.DealerOrderItem> dealerOrderItemRepository,
            Domain.Repositories.Query.Base.IQueryRepository<Domain.Entities.DealerOrder> dealerOrderQueryRepository,
            Domain.Repositories.Query.Base.IQueryRepository<Domain.Entities.DealerOrderItem> dealerOrderItemQueryRepository,
            Application.Common.Interfaces.IIdentityService identityService)
        {
            _dealerOrderRepository = dealerOrderRepository;
            _dealerOrderItemRepository = dealerOrderItemRepository;
            _dealerOrderQueryRepository = dealerOrderQueryRepository;
            _dealerOrderItemQueryRepository = dealerOrderItemQueryRepository;
            _identityService = identityService;
        }

        public async Task<Response<string>> Handle(DeleteDealerOrderCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // Get the dealer order
                var dealerOrder = await _dealerOrderQueryRepository.GetByIdAsync(request.DealerOrderId);
                if (dealerOrder == null)
                {
                    return new Response<string>
                    {
                        Success = false,
                        Message = "Dealer order not found"
                    };
                }

                // Hard delete the dealer order - cascade will automatically delete order items
                await _dealerOrderRepository.HardDeleteAsync(dealerOrder);

                return new Response<string>
                {
                    Success = true,
                    Message = "Dealer order deleted successfully"
                };
            }
            catch (Exception ex)
            {
                return new Response<string>
                {
                    Success = false,
                    Message = $"Error deleting dealer order: {ex.Message}"
                };
            }
        }
    }
}