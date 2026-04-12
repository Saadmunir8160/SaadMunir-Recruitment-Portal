using MediatR;
using Application.DTOs;
using Application.Common.Interfaces;
using Application.Queries.Dealer.DailyLimits;
using Domain.Repositories.Query.Base;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace Application.Queries.Dealer.DailyLimits
{
    public class GetDealerDailyLimitsQueryHandler : IRequestHandler<GetDealerDailyLimitsQuery, Response<DealerDailyLimitsDTO>>
    {
        private readonly IQueryRepository<Domain.Entities.DealerDailyLimit> _dailyLimitRepository;
        private readonly IQueryRepository<Domain.Entities.DealerOrder> _orderRepository;
        private readonly IIdentityService _identityService;
        private readonly ILogger<GetDealerDailyLimitsQueryHandler> _logger;

        public GetDealerDailyLimitsQueryHandler(
            IQueryRepository<Domain.Entities.DealerDailyLimit> dailyLimitRepository,
            IQueryRepository<Domain.Entities.DealerOrder> orderRepository,
            IIdentityService identityService,
            ILogger<GetDealerDailyLimitsQueryHandler> logger)
        {
            _dailyLimitRepository = dailyLimitRepository;
            _orderRepository = orderRepository;
            _identityService = identityService;
            _logger = logger;
        }

        public async Task<Response<DealerDailyLimitsDTO>> Handle(GetDealerDailyLimitsQuery request, CancellationToken cancellationToken)
        {
            // Get current dealer ID
            var currentDealerId = await _identityService.GetCurrentDealerIdAsync();
            if (currentDealerId == null)
            {
                return new Response<DealerDailyLimitsDTO>
                {
                    Success = false,
                    Message = "Dealer not found for current user"
                };
            }

            // Get dealer's daily limits configuration
            var allLimits = await _dailyLimitRepository
                .GetQueryable()
                .Where(dl => (dl.DealerID == currentDealerId.Value || dl.DealerID == null) && dl.IsActive)
                .ToListAsync(cancellationToken);

            // Helper function to get effective limit (dealer-specific first, then general fallback)
            decimal GetEffectiveLimit(string limitType)
            {
                // First try dealer-specific limit
                var dealerSpecific = allLimits.FirstOrDefault(dl => dl.DealerID == currentDealerId.Value && dl.LimitType == limitType);
                if (dealerSpecific != null && dealerSpecific.LimitValue.HasValue)
                {
                    return dealerSpecific.LimitValue.Value;
                }

                // Fallback to general limit (NULL DealerID)
                var generalLimit = allLimits.FirstOrDefault(dl => dl.DealerID == null && dl.LimitType == limitType);
                if (generalLimit != null && generalLimit.LimitValue.HasValue)
                {
                    return generalLimit.LimitValue.Value;
                }

                // Final fallback to hardcoded defaults
                return limitType == "DAILY_TONS_LIMIT" ? 50.0m : 1000.0m;
            }

            // Get effective limits using the new logic
            decimal totalLimitTons = GetEffectiveLimit("DAILY_TONS_LIMIT");
            int totalLimitBags = (int)GetEffectiveLimit("DAILY_BAGS_LIMIT");

            // Calculate today's usage from completed orders
            // Use UTC date to match order creation time (orders are created with DateTime.UtcNow)
            var todayUtc = DateTime.UtcNow.Date;
            var todayOrders = await _orderRepository
                .GetQueryable()
                .Where(o => o.DealerID == currentDealerId.Value &&
                           o.CreatedDate.Date == todayUtc 
                           /*&& (o.Status == "Delivered" || o.Status == "Completed")*/)
                .Include(o => o.DealerOrderItems)
                .ToListAsync(cancellationToken);

            // Calculate usage totals from today's orders
            decimal usedTodayTons = 0;
            int usedTodayBags = 0;

            foreach (var order in todayOrders)
            {
                foreach (var item in order.DealerOrderItems ?? new List<Domain.Entities.DealerOrderItem>())
                {
                    if (item.Quantity.HasValue && !string.IsNullOrEmpty(item.Unit))
                    {
                        if (item.Unit.ToLower() == "tons")
                        {
                            usedTodayTons += item.Quantity.Value;
                        }
                        else if (item.Unit.ToLower() == "bags")
                        {
                            usedTodayBags += (int)item.Quantity.Value;
                            // Bag-based products only count toward bag limits, not ton limits
                        }
                    }
                }
            }

            // Calculate remaining amounts
            var remainingTodayTons = Math.Max(0, totalLimitTons - usedTodayTons);
            var remainingTodayBags = Math.Max(0, totalLimitBags - usedTodayBags);

            var result = new DealerDailyLimitsDTO
            {
                DealerId = currentDealerId.Value,
                TotalLimitTons = totalLimitTons,
                UsedTodayTons = usedTodayTons,
                RemainingTodayTons = remainingTodayTons,
                TotalLimitBags = totalLimitBags,
                UsedTodayBags = usedTodayBags,
                RemainingTodayBags = remainingTodayBags,
                CurrentOrderTons = 0, // This will be updated by frontend when cart changes
                CurrentOrderBags = 0, // This will be updated by frontend when cart changes
                LastUpdated = DateTime.UtcNow
            };

            return new Response<DealerDailyLimitsDTO>
            {
                Success = true,
                Message = "Daily limits retrieved successfully",
                Data = result
            };
        }
    }
}