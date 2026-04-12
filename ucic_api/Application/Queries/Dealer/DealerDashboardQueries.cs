using Application.DTOs.Dealer;
using Application.Common.Interfaces;
using Domain.Repositories.Query.Base;
using MediatR;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Application.Queries.Dealer
{
    public class GetDashboardStatsQuery : IRequest<DealerDashboardStatsDTO>
    {
    }

    public class GetDashboardStatsQueryHandler : IRequestHandler<GetDashboardStatsQuery, DealerDashboardStatsDTO>
    {
        private readonly IQueryRepository<Domain.Entities.Dealer> _dealerRepository;
        private readonly IQueryRepository<Domain.Entities.DealerOrder> _dealerOrderRepository;
        private readonly IQueryRepository<Domain.Entities.DealerOrderItem> _dealerOrderItemRepository;
        private readonly IIdentityService _identityService;

        public GetDashboardStatsQueryHandler(
            IQueryRepository<Domain.Entities.Dealer> dealerRepository,
            IQueryRepository<Domain.Entities.DealerOrder> dealerOrderRepository,
            IQueryRepository<Domain.Entities.DealerOrderItem> dealerOrderItemRepository,
            IIdentityService identityService)
        {
            _dealerRepository = dealerRepository;
            _dealerOrderRepository = dealerOrderRepository;
            _dealerOrderItemRepository = dealerOrderItemRepository;
            _identityService = identityService;
        }

        public async Task<DealerDashboardStatsDTO> Handle(GetDashboardStatsQuery request, CancellationToken cancellationToken)
        {
            var currentDealerId = await _identityService.GetCurrentDealerIdAsync();
            if (currentDealerId == null)
                throw new UnauthorizedAccessException("User not authenticated or dealer not found");

            // Get the dealer information for balance data
            var dealerFilters = new Dictionary<string, object>
            {
                { nameof(Domain.Entities.Dealer.DealerId), currentDealerId.Value }
            };

            var dealers = await _dealerRepository.GetByColumnsWithListAsync(dealerFilters);
            var dealer = dealers.FirstOrDefault();

            if (dealer == null)
                throw new Exception("Dealer profile not found");

            var orderFilters = new Dictionary<string, object>
            {
                { nameof(Domain.Entities.DealerOrder.DealerID), currentDealerId.Value }
            };

            var orders = await _dealerOrderRepository.GetByColumnsWithListAsync(orderFilters);

            var now = DateTime.UtcNow;
            var startOfMonth = new DateTime(now.Year, now.Month, 1);
            var startOfYear = new DateTime(now.Year, 1, 1);

            var monthlyOrders = orders.Where(o => o.CreatedDate >= startOfMonth).ToList();
            var yearlyOrders = orders.Where(o => o.CreatedDate >= startOfYear).ToList();

            var totalOrders = orders.Count();
            var pendingOrders = orders.Count(o => o.Status?.ToLower() == "pending");
            var confirmedOrders = orders.Count(o => o.Status?.ToLower() == "confirmed");
            var deliveredOrders = orders.Count(o => o.Status?.ToLower() == "delivered");
            var cancelledOrders = orders.Count(o => o.Status?.ToLower() == "cancelled");

            var monthlySpent = monthlyOrders.Sum(o => o.TotalAmount ?? 0);
            var yearlySpent = yearlyOrders.Sum(o => o.TotalAmount ?? 0);
            var averageOrderValue = totalOrders > 0 ? yearlySpent / totalOrders : 0;

            var lastOrder = orders.OrderByDescending(o => o.CreatedDate).FirstOrDefault();
            var nextDelivery = orders.Where(o => o.Status == "Shipped").OrderBy(o => o.CreatedDate).FirstOrDefault();

            // Get total products ordered by querying order items for each order
            var totalProductsOrdered = 0;
            
            foreach (var order in orders)
            {
                var orderItemFilters = new Dictionary<string, object>
                {
                    { nameof(Domain.Entities.DealerOrderItem.DealerOrderID), order.DealerOrderID }
                };

                var orderItems = await _dealerOrderItemRepository.GetByColumnsWithListAsync(orderItemFilters);
                totalProductsOrdered += (int)orderItems.Sum(item => item.Quantity ?? 0);
            }

            return new DealerDashboardStatsDTO
            {
                TotalOrders = totalOrders,
                PendingOrders = pendingOrders,
                ConfirmedOrders = confirmedOrders,
                DeliveredOrders = deliveredOrders,
                CancelledOrders = cancelledOrders,
                MonthlySpent = monthlySpent,
                YearlySpent = yearlySpent,
                AverageOrderValue = averageOrderValue,
                LastOrderDate = lastOrder?.CreatedDate,
                NextDeliveryDate = nextDelivery?.CreatedDate.AddDays(3), // Mock estimated delivery
                AverageDeliveryTime = 3, // Mock average
                TotalProductsOrdered = totalProductsOrdered,
                FavoriteProducts = new List<string> { "Cement Bag 50kg", "Steel Rods", "Paint" }, // Mock data
                PaymentStatus = new PaymentStatusDTO
                {
                    Outstanding = dealer.CurrentBalance ?? 0,
                    Overdue = 15000, // Mock data
                    Paid = yearlySpent - (dealer.CurrentBalance ?? 0)
                }
            };
        }
    }

    public class GetRecentOrdersQuery : IRequest<List<RecentOrderDTO>>
    {
        public int Limit { get; set; } = 5;
    }

    public class GetRecentOrdersQueryHandler : IRequestHandler<GetRecentOrdersQuery, List<RecentOrderDTO>>
    {
        private readonly IQueryRepository<Domain.Entities.DealerOrder> _dealerOrderRepository;
        private readonly IIdentityService _identityService;

        public GetRecentOrdersQueryHandler(
            IQueryRepository<Domain.Entities.DealerOrder> dealerOrderRepository,
            IIdentityService identityService)
        {
            _dealerOrderRepository = dealerOrderRepository;
            _identityService = identityService;
        }

        public async Task<List<RecentOrderDTO>> Handle(GetRecentOrdersQuery request, CancellationToken cancellationToken)
        {
            var currentDealerId = await _identityService.GetCurrentDealerIdAsync();
            if (currentDealerId == null)
                throw new UnauthorizedAccessException("User not authenticated or dealer not found");

            var orderFilters = new Dictionary<string, object>
            {
                { nameof(Domain.Entities.DealerOrder.DealerID), currentDealerId.Value }
            };

            var orders = await _dealerOrderRepository.GetByColumnsWithListAsync(orderFilters);

            return orders
                .OrderByDescending(o => o.CreatedDate)
                .Take(request.Limit)
                .Select(o => new RecentOrderDTO
                {
                    Id = o.DealerOrderID,
                    OrderNumber = o.PortalOrderNumber ?? "",
                    Ln_OrderNumber = o.Ln_OrderNumber ?? "",  // NEW: Map LN Order Number
                    OrderDate = o.CreatedDate,
                    Status = o.Status ?? "Pending",
                    TotalAmount = o.TotalAmount ?? 0,
                    ItemCount = o.DealerOrderItems?.Count ?? 0,
                    EstimatedDelivery = o.CreatedDate.AddDays(3), // Mock estimated delivery
                    TrackingNumber = o.CustomerOrderNumber ?? "" // Mock tracking
                })
                .ToList();
        }
    }

    // Note: Dealer notifications have been removed - no notification system implemented

    public class GetMonthlySummaryQuery : IRequest<List<MonthlySummaryDTO>>
    {
        public int Months { get; set; } = 12;
    }

    public class GetMonthlySummaryQueryHandler : IRequestHandler<GetMonthlySummaryQuery, List<MonthlySummaryDTO>>
    {
        private readonly IQueryRepository<Domain.Entities.DealerOrder> _dealerOrderRepository;
        private readonly IIdentityService _identityService;

        public GetMonthlySummaryQueryHandler(
            IQueryRepository<Domain.Entities.DealerOrder> dealerOrderRepository,
            IIdentityService identityService)
        {
            _dealerOrderRepository = dealerOrderRepository;
            _identityService = identityService;
        }

        public async Task<List<MonthlySummaryDTO>> Handle(GetMonthlySummaryQuery request, CancellationToken cancellationToken)
        {
            var currentDealerId = await _identityService.GetCurrentDealerIdAsync();
            if (currentDealerId == null)
                throw new UnauthorizedAccessException("User not authenticated or dealer not found");

            var orderFilters = new Dictionary<string, object>
            {
                { nameof(Domain.Entities.DealerOrder.DealerID), currentDealerId.Value }
            };

            var orders = await _dealerOrderRepository.GetByColumnsWithListAsync(orderFilters);

            var now = DateTime.UtcNow;
            var result = new List<MonthlySummaryDTO>();

            for (int i = 0; i < request.Months; i++)
            {
                var monthStart = now.AddMonths(-i);
                var monthEnd = monthStart.AddMonths(1);

                var monthlyOrders = orders.Where(o => o.CreatedDate >= monthStart && o.CreatedDate < monthEnd).ToList();

                var orderCount = monthlyOrders.Count;
                var totalAmount = monthlyOrders.Sum(o => o.TotalAmount ?? 0);

                result.Add(new MonthlySummaryDTO
                {
                    Month = monthStart.ToString("MMMM"),
                    Year = monthStart.Year,
                    OrderCount = orderCount,
                    TotalAmount = totalAmount,
                    AverageOrderValue = orderCount > 0 ? totalAmount / orderCount : 0
                });
            }

            return result.OrderBy(r => r.Year).ThenBy(r => DateTime.ParseExact(r.Month, "MMMM", null).Month).ToList();
        }
    }

    // Note: Unread notification count query removed - no notification system implemented
}