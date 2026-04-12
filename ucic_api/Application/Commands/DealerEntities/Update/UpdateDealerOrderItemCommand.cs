using Application.DTOs;
using Application.DTOs.DealerOrder;
using Domain.Repositories.Command.Base;
using Domain.Repositories.Query.Base;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Commands.DealerEntities
{
    public class UpdateDealerOrderItemCommand : IRequest<Response<bool>>
    {
        public Application.DTOs.DealerOrder.UpdateDealerOrderItemDTO DealerOrderItem { get; set; }
    }

    public class UpdateDealerOrderItemCommandHandler : IRequestHandler<UpdateDealerOrderItemCommand, Response<bool>>
    {
        private readonly ICommandRepository<Domain.Entities.DealerOrderItem> _commandRepository;
        private readonly IQueryRepository<Domain.Entities.DealerOrderItem> _queryRepository;

        public UpdateDealerOrderItemCommandHandler(ICommandRepository<Domain.Entities.DealerOrderItem> commandRepository, IQueryRepository<Domain.Entities.DealerOrderItem> queryRepository)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
        }

        public async Task<Response<bool>> Handle(UpdateDealerOrderItemCommand request, CancellationToken cancellationToken)
        {
            var entity = await _queryRepository.GetByIdAsync(request.DealerOrderItem.OrderItemID);
            if (entity == null)
                return new Response<bool> { Success = false, Message = "Dealer order item not found." };

            entity.DealerOrderID = request.DealerOrderItem.DealerOrderID;
            entity.DealerProductID = request.DealerOrderItem.DealerProductID;
            entity.Product_LnCode = request.DealerOrderItem.Product_LnCode;
            entity.ProductDescription = request.DealerOrderItem.ProductDescription;
            entity.Quantity = request.DealerOrderItem.Quantity;
            entity.ModifiedDate = DateTime.UtcNow;

            await _commandRepository.UpdateAsync(entity);
            return new Response<bool> { Success = true, Data = true, Message = "Dealer order item updated successfully." };
        }
    }
}
