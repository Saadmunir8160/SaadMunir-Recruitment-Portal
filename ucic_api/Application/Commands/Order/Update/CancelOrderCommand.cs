using Application.Common.Exceptions;
using Domain.Repositories.Command.Base;
using Domain.Repositories.Query.Base;
using MediatR;
using System.ComponentModel.DataAnnotations;
using Domain.Entities;

namespace Application.Commands.Order.Update
{
    public class CancelOrderCommand : IRequest<int>
    {
        public long OrderId { get; set; }
        [MaxLength(500)]
        public string? Reason { get; set; }
    }

    public class CancelOrderCommandHandler : IRequestHandler<CancelOrderCommand, int>
    {
        private readonly ICommandRepository<Domain.Entities.Order> _commandRepository;
        private readonly IQueryRepository<Domain.Entities.Order> _queryRepository;

        public CancelOrderCommandHandler(
            ICommandRepository<Domain.Entities.Order> commandRepository,
            IQueryRepository<Domain.Entities.Order> queryRepository)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
        }

        public async Task<int> Handle(CancelOrderCommand request, CancellationToken cancellationToken)
        {
            var existingOrder = await _queryRepository.GetByIdAsync(request.OrderId);
            if (existingOrder == null)
                throw new KeyNotFoundException("Order not found");

            // Only allow cancellation if order is in a cancellable state
            if (existingOrder.Status != OrderStatus.Pending)
            {
                throw new InvalidOperationException("Order cannot be cancelled in its current state");
            }

            existingOrder.Status = OrderStatus.Cancelled;
            existingOrder.CancelledDate = DateTime.UtcNow;
            existingOrder.OrderCancelledRemarks = request.Reason;
            existingOrder.ModifiedDate = DateTime.UtcNow;

            await _commandRepository.UpdateAsync(existingOrder);
            return 1;
        }
    }
}
