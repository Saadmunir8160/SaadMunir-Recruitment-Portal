using Application.Common.Exceptions;
using Application.DTOs;
using Domain.Repositories.Query.Base;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Application.Queries.Order
{
    public class GetMyOrdersQuery : IRequest<Response<List<OrderDTO>>>
    {
    }

    public class GetMyOrdersQueryHandler : IRequestHandler<GetMyOrdersQuery, Response<List<OrderDTO>>>
    {
        private readonly IQueryRepository<Domain.Entities.Order> _queryRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IQueryRepository<Domain.Entities.Customer> _queryRepositoryCustomer;


        public GetMyOrdersQueryHandler(IQueryRepository<Domain.Entities.Order> queryRepository, IHttpContextAccessor httpContextAccessor, IQueryRepository<Domain.Entities.Customer> queryRepositoryCustomer)
        {
            _queryRepository = queryRepository;
            _httpContextAccessor = httpContextAccessor;
            _queryRepositoryCustomer = queryRepositoryCustomer;
        }

        public async Task<Response<List<OrderDTO>>> Handle(GetMyOrdersQuery request, CancellationToken cancellationToken)
        {
            var customerId = await GetCustomerIDInIDentity();

            var filters = new Dictionary<string, object>
              {
                  { nameof(Domain.Entities.Order.CustomerId), customerId }
              };
            var result = await _queryRepository.GetByColumnsAsync(filters);


            if (result == null || !result.Any())
            {
                throw new NotFoundException("No Orders found.");
            }

            result = result
                .Where(x => x.IsActive);

            var orders = result
                .OrderByDescending(order => order.CreatedDate)
                .Select(order => new OrderDTO
                {
                    OrderId = order.OrderId,
                    CustomerId = order.CustomerId,
                    LocationId = order.LocationId,
                    PromotionId = order.PromotionId > 0 ? (long)order.PromotionId : 0,
                    TotalQuantity = order.TotalQuantity,
                    TotalPrice = order.TotalPrice,
                    TotalVat = order.TotalVat,
                    ShipingCost = order.ShipingCost,
                    CouponDiscount = order.CouponDiscount,
                    Status = order.Status.ToString(),
                    TrackingID = order.TrackingId,
                    CreatedDate = order.CreatedDate                   
                })
                .ToList();

            return new Response<List<OrderDTO>>
            {
                Success = true,
                Message = "Orders retrieved successfully.",
                Data = orders
            };
        }

        private async Task<long?> GetCustomerIDInIDentity()
        {
            var uID = _httpContextAccessor.HttpContext?.User?.FindFirst("UserId")?.Value ?? string.Empty;


            var filters = new Dictionary<string, object>
            {
                { nameof(Domain.Entities.Customer.UserId), uID }
            };
            var result = (await _queryRepositoryCustomer.GetByColumnsWithListAsync(filters)).FirstOrDefault();
            return result?.CustomerId;

        }
    }
}
