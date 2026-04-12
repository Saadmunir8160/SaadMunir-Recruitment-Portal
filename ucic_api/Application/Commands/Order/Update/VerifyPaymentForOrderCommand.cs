using Domain.Entities;
using Domain.Repositories.Command.Base;
using Domain.Repositories.Query.Base;
using MediatR;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace Application.Commands.Order.Update
{
    public class VerifyPaymentForOrderCommand : IRequest<bool>
    {
        [Required(ErrorMessage = "Order ID is required")]
        public long OrderId { get; set; }
    }

    public class VerifyPaymentForOrderCommandHandler : IRequestHandler<VerifyPaymentForOrderCommand, bool>
    {
        private readonly ICommandRepository<Domain.Entities.Order> _commandRepository;
        private readonly IQueryRepository<Domain.Entities.Order> _queryRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public VerifyPaymentForOrderCommandHandler(
            ICommandRepository<Domain.Entities.Order> commandRepository,
            IQueryRepository<Domain.Entities.Order> queryRepository,
            IHttpContextAccessor httpContextAccessor)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<bool> Handle(VerifyPaymentForOrderCommand request, CancellationToken cancellationToken)
        {
            var userName = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty;
            var existingOrder = await _queryRepository.GetByIdAsync(request.OrderId);
            if (existingOrder == null)
            {
                return false;
            }

            existingOrder.PaymentConfirmedBy = userName;
            existingOrder.PaymentConfirmedDate = DateTime.Now;
            existingOrder.Status = OrderStatus.PaymentConfirmed;

            await _commandRepository.UpdateAsync(existingOrder); // UpdateAsync returns void, so no assignment is needed.

            return true;
        }

    }
} 