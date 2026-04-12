using MediatR;
using Application.DTOs;
using Application.Queries.Dealer.DailyLimits;

namespace Application.Commands.Dealer.DailyLimits
{
    public class UpdateCurrentOrderQuantitiesCommand : IRequest<Response<DealerDailyLimitsDTO>>
    {
        public decimal CurrentOrderTons { get; set; }
        public int CurrentOrderBags { get; set; }
    }
}