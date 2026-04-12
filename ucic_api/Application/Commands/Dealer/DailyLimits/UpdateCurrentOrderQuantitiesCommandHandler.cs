using MediatR;
using Application.DTOs;
using Application.Common.Interfaces;
using Application.Commands.Dealer.DailyLimits;
using Application.Queries.Dealer.DailyLimits;
using Microsoft.Extensions.Logging;

namespace Application.Commands.Dealer.DailyLimits
{
    public class UpdateCurrentOrderQuantitiesCommandHandler : IRequestHandler<UpdateCurrentOrderQuantitiesCommand, Response<DealerDailyLimitsDTO>>
    {
        private readonly IMediator _mediator;
        private readonly IIdentityService _identityService;
        private readonly ILogger<UpdateCurrentOrderQuantitiesCommandHandler> _logger;

        public UpdateCurrentOrderQuantitiesCommandHandler(
            IMediator mediator,
            IIdentityService identityService,
            ILogger<UpdateCurrentOrderQuantitiesCommandHandler> logger)
        {
            _mediator = mediator;
            _identityService = identityService;
            _logger = logger;
        }

        public async Task<Response<DealerDailyLimitsDTO>> Handle(UpdateCurrentOrderQuantitiesCommand request, CancellationToken cancellationToken)
        {
            try
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

                // Get current daily limits
                var limitsQuery = new GetDealerDailyLimitsQuery();
                var limitsResponse = await _mediator.Send(limitsQuery, cancellationToken);

                if (!limitsResponse.Success || limitsResponse.Data == null)
                {
                    return new Response<DealerDailyLimitsDTO>
                    {
                        Success = false,
                        Message = "Unable to retrieve current daily limits"
                    };
                }

                // Update current order quantities
                limitsResponse.Data.CurrentOrderTons = request.CurrentOrderTons;
                limitsResponse.Data.CurrentOrderBags = request.CurrentOrderBags;
                limitsResponse.Data.LastUpdated = DateTime.UtcNow;

                // Validate that current order + used today doesn't exceed limits
                var totalTonsAfterOrder = limitsResponse.Data.UsedTodayTons + request.CurrentOrderTons;
                var totalBagsAfterOrder = limitsResponse.Data.UsedTodayBags + request.CurrentOrderBags;

                var validationMessages = new List<string>();

                if (totalTonsAfterOrder > limitsResponse.Data.TotalLimitTons)
                {
                    validationMessages.Add($"Order would exceed daily tons limit. Available: {limitsResponse.Data.RemainingTodayTons} tons");
                }

                if (totalBagsAfterOrder > limitsResponse.Data.TotalLimitBags)
                {
                    validationMessages.Add($"Order would exceed daily bags limit. Available: {limitsResponse.Data.RemainingTodayBags} bags");
                }

                if (validationMessages.Any())
                {
                    return new Response<DealerDailyLimitsDTO>
                    {
                        Success = false,
                        Message = string.Join("; ", validationMessages),
                        Data = limitsResponse.Data
                    };
                }

                return new Response<DealerDailyLimitsDTO>
                {
                    Success = true,
                    Message = "Current order quantities updated successfully",
                    Data = limitsResponse.Data
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating current order quantities for dealer");
                return new Response<DealerDailyLimitsDTO>
                {
                    Success = false,
                    Message = "Error updating current order quantities"
                };
            }
        }
    }
}