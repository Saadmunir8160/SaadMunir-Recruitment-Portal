using Domain.Repositories.Command.Base;
using Domain.Repositories.Query.Base;
using MediatR;

namespace Application.Commands.Order.Delete
{
    public class DeleteOrderCommand : IRequest<bool>
    {
        public long OrderId { get; set; }
    }

    public class DeleteOrderCommandHandler : IRequestHandler<DeleteOrderCommand, bool>
    {
        private readonly ICommandRepository<Domain.Entities.Order> _commandRepository;
        private readonly IQueryRepository<Domain.Entities.Order> _queryRepository;

        public DeleteOrderCommandHandler(ICommandRepository<Domain.Entities.Order> commandRepository, IQueryRepository<Domain.Entities.Order> queryRepository)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
        }
        public async Task<bool> Handle(DeleteOrderCommand request, CancellationToken cancellationToken)
        {
            var order = await _queryRepository.GetByIdAsync(request.OrderId);
            if (order == null)
            {
                return false;

            }

            order.IsActive = false;

            await _commandRepository.UpdateAsync(order);

            return true;
        }
    }
}
