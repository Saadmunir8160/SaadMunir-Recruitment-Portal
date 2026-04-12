using Application.DTOs;
using Domain.Entities;
using Domain.Repositories.Command.Base;
using Domain.Repositories.Query.Base;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Commands.DealerEntities
{
    public class DeleteDealerCommand : IRequest<Response<bool>>
    {
        public int DealerId { get; set; }
    }

    public class DeleteDealerCommandHandler : IRequestHandler<DeleteDealerCommand, Response<bool>>
    {
        private readonly ICommandRepository<Domain.Entities.Dealer> _commandRepository;
        private readonly IQueryRepository<Domain.Entities.Dealer> _queryRepository;
        private readonly IQueryRepository<Domain.Entities.DealerOrder> _orderQueryRepository;
        
        public DeleteDealerCommandHandler(
            ICommandRepository<Domain.Entities.Dealer> commandRepository, 
            IQueryRepository<Domain.Entities.Dealer> queryRepository,
            IQueryRepository<Domain.Entities.DealerOrder> orderQueryRepository)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _orderQueryRepository = orderQueryRepository;
        }
        
        public async Task<Response<bool>> Handle(DeleteDealerCommand request, CancellationToken cancellationToken)
        {
            var entity = await _queryRepository.GetByIdAsync(request.DealerId);
            if (entity == null)
                return new Response<bool> { Success = false, Message = "Dealer not found." };
            
            // Check if dealer has any orders (will be deleted by cascade)
            var hasOrders = await _orderQueryRepository.GetQueryable()
                .AnyAsync(o => o.DealerID == request.DealerId, cancellationToken);
            
            if (hasOrders)
            {
                return new Response<bool> 
                { 
                    Success = false, 
                    Message = "Cannot delete dealer with existing orders. Please delete or archive all orders first." 
                };
            }
            
            // Use hard delete - cascade will handle related records
            await _commandRepository.HardDeleteAsync(entity);
            return new Response<bool> { Success = true, Data = true, Message = "Dealer deleted successfully." };
        }
    }
}
