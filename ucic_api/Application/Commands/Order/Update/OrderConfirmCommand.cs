using Application.Common.Exceptions;
using Domain.Repositories.Command.Base;
using Domain.Repositories.Query.Base;
using MediatR;
using System.ComponentModel.DataAnnotations;
using Domain.Entities;

namespace Application.Commands.Order.Update
{
    public class OrderConfirmCommand : IRequest<int>
    {
        public long OrderId { get; set; }
        [MaxLength(450)]
        public string? UserId { get; set; }
        public int? DriverId { get; set; }
        public int? VehicleId { get; set; }
        public string? OrderConfirmedBy { get; set; }
    }

    public class OrderConfirmCommandHandler : IRequestHandler<OrderConfirmCommand, int>
    {
        private readonly ICommandRepository<Domain.Entities.Order> _commandRepository;
        private readonly IQueryRepository<Domain.Entities.Order> _queryRepository;

        public OrderConfirmCommandHandler(
            ICommandRepository<Domain.Entities.Order> commandRepository,
            IQueryRepository<Domain.Entities.Order> queryRepository)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
        }

        public async Task<int> Handle(OrderConfirmCommand request, CancellationToken cancellationToken)
        {
            var existingOrder = await _queryRepository.GetByIdAsync(request.OrderId);
            if (existingOrder == null)
                throw new KeyNotFoundException("Order not found");

            existingOrder.OrderConfirmedDate = DateTime.UtcNow;
            existingOrder.OrderConfirmedBy = request.OrderConfirmedBy;
            existingOrder.UserId = request.UserId;
            existingOrder.DriverId = request.DriverId;
            existingOrder.VehicleId = request.VehicleId;
            existingOrder.Status = OrderStatus.Confirmed;
            existingOrder.ModifiedDate = DateTime.UtcNow;

            await _commandRepository.UpdateAsync(existingOrder);
            return 1;
        }
    }
} 