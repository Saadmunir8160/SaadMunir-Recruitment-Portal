using Application.DTOs;
using Application.DTOs.DealerOrder;
using Domain.Repositories.Command.Base;
using Domain.Repositories.Query.Base;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Commands.DealerEntities.Create
{
    public class CreateDealerOrderItemCommand : IRequest<Response<int>>
    {
        public Application.DTOs.DealerOrder.CreateDealerOrderItemDTO DealerOrderItem { get; set; }
    }

    public class CreateDealerOrderItemCommandHandler : IRequestHandler<CreateDealerOrderItemCommand, Response<int>>
    {
        private readonly ICommandRepository<Domain.Entities.DealerOrderItem> _repository;
        private readonly IQueryRepository<Domain.Entities.DealerOrderItem> _queryRepository;

        public CreateDealerOrderItemCommandHandler(
            ICommandRepository<Domain.Entities.DealerOrderItem> repository,
            IQueryRepository<Domain.Entities.DealerOrderItem> queryRepository)
        {
            _repository = repository;
            _queryRepository = queryRepository;
        }

        public async Task<Response<int>> Handle(CreateDealerOrderItemCommand request, CancellationToken cancellationToken)
        {
            if (!string.IsNullOrEmpty(request.DealerOrderItem.Product_LnCode))
            {
                var itemExists = await _queryRepository.ValueExistsAsync(nameof(Domain.Entities.DealerOrderItem.Product_LnCode), request.DealerOrderItem.Product_LnCode);
                if (itemExists)
                {
                    return new Response<int>
                    {
                        Success = false,
                        Message = $"Product LN Code '{request.DealerOrderItem.Product_LnCode}' already exists."
                    };
                }
            }

            var entity = new Domain.Entities.DealerOrderItem
            {
                DealerOrderID = request.DealerOrderItem.DealerOrderID,
                DealerProductID = request.DealerOrderItem.DealerProductID,
                Product_LnCode = request.DealerOrderItem.Product_LnCode,
                ProductDescription = request.DealerOrderItem.ProductDescription,
                Quantity = request.DealerOrderItem.Quantity,
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            };

            await _repository.AddAsync(entity);
            return new Response<int> { Success = true, Data = entity.OrderItemID, Message = "Dealer order item created successfully." };
        }
    }
}