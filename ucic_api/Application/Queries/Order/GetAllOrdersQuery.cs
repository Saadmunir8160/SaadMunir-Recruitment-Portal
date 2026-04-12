using Application.Common.Exceptions;
using Application.Common.Extensions;
using Application.DTOs;
using Domain.Repositories.Query.Base;
using MediatR;

namespace Application.Queries.Order
{
    public class GetAllOrdersQuery : IRequest<PaginatedResponse<OrderDTO>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class GetAllOrdersQueryHandler : IRequestHandler<GetAllOrdersQuery, PaginatedResponse<OrderDTO>>
    {
        private readonly IQueryRepository<Domain.Entities.Order> _queryRepository;

        public GetAllOrdersQueryHandler(IQueryRepository<Domain.Entities.Order> queryRepository)
        {
            _queryRepository = queryRepository;
        }

        public async Task<PaginatedResponse<OrderDTO>> Handle(GetAllOrdersQuery request, CancellationToken cancellationToken)
        {
            var parameters = new PaginationParameters
            {
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };

            var query = _queryRepository.GetQueryable();
            if (query == null || !query.Any())
            {
                throw new NotFoundException("No Orders found.");
            }

            var ordersQuery = query
                .OrderByDescending(x => x.CreatedDate)
                .Select(order => new OrderDTO
                {
                    OrderId = order.OrderId,
                    CustomerId = order.CustomerId,
                    CustomerName = order.Customer.CreatedBy,
                    LocationId = order.LocationId,
                    LocationAddress = order.Location.Address,
                    PromotionId = order.PromotionId > 0 ? (long)order.PromotionId : 0,
                    TotalQuantity = order.TotalQuantity,
                    TotalPrice = order.TotalPrice,
                    TotalVat = order.TotalVat,
                    ShipingCost = order.ShipingCost,
                    CouponDiscount = order.CouponDiscount,
                    Status = order.Status.ToString(),
                    TrackingID = order.TrackingId,
                    CreatedDate = order.CreatedDate,
                    PaymentFilePath = Path.GetFileName(order.PaymentFilePath),
                    PaymentSubmittedDate = order.PaymentSubmittedDate,
                    PaymentConfirmedDate = order.PaymentConfirmedDate,
                    PaymentConfirmedBy = order.PaymentConfirmedBy,
                    OrderConfirmedBy = order.OrderConfirmedBy,
                    OrderConfirmedDate = order.OrderConfirmedDate,
                    PaymentTransactionId=order.PaymentTransactionId,
                    Latitude = order.GpsLocation.Latitude,
                    Longitude = order.GpsLocation.Longitude,
                    OrderItems = order.OrderItems.Select(oi => new OrderItemsDTO
                    {
                        OrderItemsId = oi.OrderItemsId,
                        ProductId = oi.ProductId,
                        ProductName = oi.Product.Name,
                        Quantity = oi.Quantity,
                        Price = oi.Price,
                        NumberOfTrucks = oi.NumberOfTrucks,
                        ProductImage = oi.Product.ImageUrl
                    }).ToList()
                });

            return await ordersQuery.ToPaginatedResponseAsync(parameters);
        }
    }
}
