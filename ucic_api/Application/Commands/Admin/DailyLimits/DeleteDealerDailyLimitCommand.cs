using MediatR;
using Application.DTOs;

namespace Application.Commands.Admin.DailyLimits
{
    public class DeleteDealerDailyLimitCommand : IRequest<Response<object>>
    {
        public int DailyLimitID { get; set; }
    }
}