using Application.DTOs;
using Domain.Entities;
using Domain.Repositories.Query.Base;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Queries.DealerEntities
{
    public class GetAllDealerOrderItemsQuery : IRequest<Response<List<DealerOrderItemDTO>>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class GetAllDealerOrderItemsQueryHandler : IRequestHandler<GetAllDealerOrderItemsQuery, Response<List<DealerOrderItemDTO>>>
    {
        private readonly IQueryRepository<DealerOrderItem> _repository;
        public GetAllDealerOrderItemsQueryHandler(IQueryRepository<DealerOrderItem> repository)
        {
            _repository = repository;
        }
        public async Task<Response<List<DealerOrderItemDTO>>> Handle(GetAllDealerOrderItemsQuery request, CancellationToken cancellationToken)
        {
            var all = await _repository.GetAllAsync();
            var paged = all.Skip((request.PageNumber - 1) * request.PageSize).Take(request.PageSize).ToList();
            var result = paged.Select(d => new DealerOrderItemDTO
            {
                OrderItemID = d.OrderItemID,
                DealerOrderID = d.DealerOrderID,
                DealerProductID = d.DealerProductID,
                Product_LnCode = d.Product_LnCode,
                ProductDescription = d.ProductDescription,
                Quantity = d.Quantity,
                Unit = d.Unit,
                IsActive = d.IsActive
            }).ToList();
            return new Response<List<DealerOrderItemDTO>> { Success = true, Data = result, Message = "Dealer order items retrieved successfully." };
        }
    }

    public class GetDealerOrderItemByIdQuery : IRequest<Response<DealerOrderItemDTO>>
    {
        public int OrderItemID { get; set; }
    }

    public class GetDealerOrderItemByIdQueryHandler : IRequestHandler<GetDealerOrderItemByIdQuery, Response<DealerOrderItemDTO>>
    {
        private readonly IQueryRepository<DealerOrderItem> _repository;
        public GetDealerOrderItemByIdQueryHandler(IQueryRepository<DealerOrderItem> repository)
        {
            _repository = repository;
        }
        public async Task<Response<DealerOrderItemDTO>> Handle(GetDealerOrderItemByIdQuery request, CancellationToken cancellationToken)
        {
            var d = await _repository.GetByIdAsync(request.OrderItemID);
            if (d == null)
                return new Response<DealerOrderItemDTO> { Success = false, Message = "Dealer order item not found." };
            var dto = new DealerOrderItemDTO
            {
                OrderItemID = d.OrderItemID,
                DealerOrderID = d.DealerOrderID,
                DealerProductID = d.DealerProductID,
                Product_LnCode = d.Product_LnCode,
                ProductDescription = d.ProductDescription,
                Quantity = d.Quantity,
                Unit = d.Unit ?? "bags"
            };
            return new Response<DealerOrderItemDTO> { Success = true, Data = dto, Message = "Dealer order item retrieved successfully." };
        }
    }
}
