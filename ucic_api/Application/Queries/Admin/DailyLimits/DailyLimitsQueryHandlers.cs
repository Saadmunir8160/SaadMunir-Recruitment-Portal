using MediatR;
using Application.DTOs;

namespace Application.Queries.Admin.DailyLimits
{
    public class GetDealerDailyLimitByIdForAdminQueryHandler : IRequestHandler<GetDealerDailyLimitByIdForAdminQuery, Response<object>>
    {
        public async Task<Response<object>> Handle(GetDealerDailyLimitByIdForAdminQuery request, CancellationToken cancellationToken)
        {
            return new Response<object> { Success = true, Message = "Not implemented yet" };
        }
    }

    public class GetDealerDailyLimitsStatsForAdminQueryHandler : IRequestHandler<GetDealerDailyLimitsStatsForAdminQuery, Response<object>>
    {
        public async Task<Response<object>> Handle(GetDealerDailyLimitsStatsForAdminQuery request, CancellationToken cancellationToken)
        {
            return new Response<object> { Success = true, Message = "Not implemented yet" };
        }
    }
}