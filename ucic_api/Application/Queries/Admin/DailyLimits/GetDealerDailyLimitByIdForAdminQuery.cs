using MediatR;
using Application.DTOs;

namespace Application.Queries.Admin.DailyLimits
{
    public class GetDealerDailyLimitByIdForAdminQuery : IRequest<Response<object>>
    {
        public int Id { get; set; }
    }
}