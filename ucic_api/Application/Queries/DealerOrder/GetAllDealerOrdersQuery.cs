using Application.Common.Interfaces;
using Application.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Queries.DealerOrder
{
    public class GetAllDealerOrdersQuery : IRequest<PaginatedResponse<DealerOrderDTO>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class GetAllDealerOrdersQueryHandler : IRequestHandler<GetAllDealerOrdersQuery, PaginatedResponse<DealerOrderDTO>>
    {
        private readonly Domain.Repositories.Query.Base.IQueryRepository<Domain.Entities.DealerOrder> _dealerOrderRepository;
        private readonly Application.Common.Interfaces.IIdentityService _identityService;

        public GetAllDealerOrdersQueryHandler(
            Domain.Repositories.Query.Base.IQueryRepository<Domain.Entities.DealerOrder> dealerOrderRepository,
            Application.Common.Interfaces.IIdentityService identityService)
        {
            _dealerOrderRepository = dealerOrderRepository;
            _identityService = identityService;
        }

        public async Task<PaginatedResponse<DealerOrderDTO>> Handle(GetAllDealerOrdersQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var parameters = new PaginationParameters
                {
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize
                };

                var query = _dealerOrderRepository.GetQueryable()
                    .Include(x => x.DealerOrderItems)
                        .ThenInclude(x => x.DealerProduct)
                    .OrderByDescending(x => x.CreatedDate);

                // You may need to implement ToPaginatedResponseAsync for DealerOrder
                var dealerOrders = await query.ToListAsync();
                var pagedOrders = dealerOrders.Skip((parameters.PageNumber - 1) * parameters.PageSize).Take(parameters.PageSize).ToList();
                
                // Convert to DTOs
                var dealerOrderDTOs = pagedOrders.Select(order => new DealerOrderDTO
                {
                    DealerOrderID = order.DealerOrderID,
                    DealerID = order.DealerID,
                    DriverID = order.DriverID,
                    VehicleID = order.VehicleID,
                    AddressID = order.AddressID,
                    Status = order.Status,
                    TotalAmount = order.TotalAmount,
                    CreatedDate = order.CreatedDate,
                    CreatedBy = order.CreatedBy,
                    ModifiedDate = order.ModifiedDate,
                    ModifiedBy = order.ModifiedBy,
                    DealerOrderItems = order.DealerOrderItems?.Select(item => new DealerOrderItemDTO
                    {
                        OrderItemID = item.OrderItemID,
                        DealerOrderID = item.DealerOrderID,
                        DealerProductID = item.DealerProductID,
                        Product_LnCode = item.Product_LnCode,
                        ProductDescription = item.ProductDescription,
                        Quantity = item.Quantity,
                        Unit = item.Unit
                    }).ToList()
                }).ToList();

                var result = new PaginatedResponse<DealerOrderDTO>
                {
                    Data = dealerOrderDTOs,
                    Metadata = new PaginationMetadata
                    {
                        TotalCount = dealerOrders.Count,
                        PageSize = parameters.PageSize,
                        CurrentPage = parameters.PageNumber,
                        TotalPages = (int)Math.Ceiling(dealerOrders.Count / (double)parameters.PageSize)
                    },
                    Success = true,
                    Message = "Dealer orders retrieved successfully"
                };

                return result;
            }
            catch (Exception ex)
            {
                return new PaginatedResponse<DealerOrderDTO>
                {
                    Success = false,
                    Message = $"Error retrieving dealer orders: {ex.Message}"
                };
            }
        }
    }
}