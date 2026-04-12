using Application.Common.Exceptions;
using Application.DTOs;
using Domain.Repositories.Query.Base;
using MediatR;
using Application.Common.Interfaces;

namespace Application.Queries.Order
{
    public class GetOrderByIdQuery : IRequest<Response<OrderDTO>>
    {
        public long OrderId { get; set; }
    }

    public class GetOrderByIdQueryHandler : IRequestHandler<GetOrderByIdQuery, Response<OrderDTO>>
    {
        private readonly IQueryRepository<Domain.Entities.Order> _queryRepository;
        private readonly IIdentityService _identityService;

        public GetOrderByIdQueryHandler(
            IQueryRepository<Domain.Entities.Order> queryRepository,
            IIdentityService identityService)
        {
            _queryRepository = queryRepository;
            _identityService = identityService;
        }

        public async Task<Response<OrderDTO>> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.GetByIdAsync(request.OrderId);

            if (result == null)
            {
                throw new NotFoundException("No Orders found.");
            }

            var userDetails = await _identityService.GetUserDetailsAsync(result.Customer.UserId);

            var order =  new OrderDTO
            {
                OrderId = result.OrderId,
                CustomerId = result.CustomerId,
                CustomerName = userDetails.fullName,
                LocationId = result.LocationId,
                LocationAddress = result.Location?.Address,
                PromotionId = result.PromotionId > 0 ? (long)result.PromotionId : 0,
                TotalQuantity = result.TotalQuantity,
                TotalPrice = result.TotalPrice,
                TotalVat = result.TotalVat,
                ShipingCost = result.ShipingCost,
                CouponDiscount = result.CouponDiscount,
                Status = result.Status.ToString(),
                TrackingID = result.TrackingId,
                CreatedDate = result.CreatedDate,
                PaymentFilePath = !string.IsNullOrEmpty(result.PaymentFilePath) ? Path.GetFileName(result.PaymentFilePath) : null,
                PaymentSubmittedDate = result.PaymentSubmittedDate,
                PaymentConfirmedDate = result.PaymentConfirmedDate,
                PaymentConfirmedBy = result.PaymentConfirmedBy,
                OrderConfirmedBy = result.OrderConfirmedBy,
                OrderConfirmedDate = result.OrderConfirmedDate,
                PaymentTransactionId = result.PaymentTransactionId,
                Latitude = result.GpsLocation?.Latitude,
                Longitude = result.GpsLocation?.Longitude,
                OrderItems = result.OrderItems?.Select(oi => new OrderItemsDTO
                {
                    OrderItemsId = oi.OrderItemsId,
                    ProductId = oi.ProductId,
                    ProductName = oi.Product?.Name,
                    Quantity = oi.Quantity,
                    Price = oi.Price,
                    NumberOfTrucks = oi.NumberOfTrucks,
                    ProductImage = oi.Product?.ImageUrl
                }).ToList()
            };

            return new Response<OrderDTO>
            {
                Success = true,
                Message = "Order retrieved successfully.",
                Data = order!
            };
        }
    }
}
