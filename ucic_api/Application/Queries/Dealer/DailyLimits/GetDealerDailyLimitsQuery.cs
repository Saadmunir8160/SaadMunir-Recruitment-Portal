using MediatR;
using Application.DTOs;

namespace Application.Queries.Dealer.DailyLimits
{
    public class GetDealerDailyLimitsQuery : IRequest<Response<DealerDailyLimitsDTO>>
    {
        // No parameters needed - uses current authenticated dealer
    }

    public class DealerDailyLimitsDTO
    {
        public decimal TotalLimitTons { get; set; }
        public decimal UsedTodayTons { get; set; }
        public decimal RemainingTodayTons { get; set; }
        public int TotalLimitBags { get; set; }
        public int UsedTodayBags { get; set; }
        public int RemainingTodayBags { get; set; }
        public decimal CurrentOrderTons { get; set; }
        public int CurrentOrderBags { get; set; }
        public DateTime LastUpdated { get; set; }
        public int DealerId { get; set; }
    }
}