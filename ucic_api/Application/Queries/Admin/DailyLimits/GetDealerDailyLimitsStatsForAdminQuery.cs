using MediatR;
using Application.DTOs;

namespace Application.Queries.Admin.DailyLimits
{
    public class GetDealerDailyLimitsStatsForAdminQuery : IRequest<Response<object>>
    {
        public int? DealerId { get; set; }
    }
}