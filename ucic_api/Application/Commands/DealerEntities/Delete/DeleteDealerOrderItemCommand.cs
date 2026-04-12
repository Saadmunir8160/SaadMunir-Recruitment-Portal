using Application.DTOs;
using Application.DTOs.DealerOrder;
using Domain.Repositories.Command.Base;
using Domain.Repositories.Query.Base;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Commands.DealerEntities
{
    public class DeleteDealerOrderItemCommand : IRequest<Response<bool>>
    {
        public int OrderItemID { get; set; }
    }

    public class DeleteDealerOrderItemCommandHandler : IRequestHandler<DeleteDealerOrderItemCommand, Response<bool>>
    {
        private readonly ICommandRepository<Domain.Entities.DealerOrderItem> _commandRepository;
        private readonly IQueryRepository<Domain.Entities.DealerOrderItem> _queryRepository;

        public DeleteDealerOrderItemCommandHandler(ICommandRepository<Domain.Entities.DealerOrderItem> commandRepository, IQueryRepository<Domain.Entities.DealerOrderItem> queryRepository)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
        }

        public async Task<Response<bool>> Handle(DeleteDealerOrderItemCommand request, CancellationToken cancellationToken)
        {
            var entity = await _queryRepository.GetByIdAsync(request.OrderItemID);
            if (entity == null)
                return new Response<bool> { Success = false, Message = "Dealer order item not found." };

            entity.IsActive = false;
            entity.IsDeleted = true;
            entity.ModifiedDate = DateTime.UtcNow;

            await _commandRepository.UpdateAsync(entity);
            return new Response<bool> { Success = true, Data = true, Message = "Dealer order item deleted successfully." };
        }
    }
}
