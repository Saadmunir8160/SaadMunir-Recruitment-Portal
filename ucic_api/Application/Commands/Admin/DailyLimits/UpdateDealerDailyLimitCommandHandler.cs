using MediatR;
using Application.DTOs;
using Application.Common.Interfaces;
using Domain.Repositories.Command.Base;
using Domain.Repositories.Query.Base;
using Microsoft.Extensions.Logging;

namespace Application.Commands.Admin.DailyLimits
{
    public class UpdateDealerDailyLimitCommandHandler : IRequestHandler<UpdateDealerDailyLimitCommand, Response<object>>
    {
        private readonly ICommandRepository<Domain.Entities.DealerDailyLimit> _commandRepository;
        private readonly IQueryRepository<Domain.Entities.DealerDailyLimit> _queryRepository;
        private readonly IIdentityService _identityService;
        private readonly ILogger<UpdateDealerDailyLimitCommandHandler> _logger;

        public UpdateDealerDailyLimitCommandHandler(
            ICommandRepository<Domain.Entities.DealerDailyLimit> commandRepository,
            IQueryRepository<Domain.Entities.DealerDailyLimit> queryRepository,
            IIdentityService identityService,
            ILogger<UpdateDealerDailyLimitCommandHandler> logger)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _identityService = identityService;
            _logger = logger;
        }

        public async Task<Response<object>> Handle(UpdateDealerDailyLimitCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var existingLimit = await _queryRepository.GetByIdAsync(request.DailyLimitID);
                if (existingLimit == null)
                {
                    return new Response<object>
                    {
                        Success = false,
                        Message = "Daily limit not found"
                    };
                }

                var currentUserId = _identityService.GetCurrentUserId();
                
                existingLimit.DealerID = request.DealerID;
                existingLimit.LimitType = request.LimitType;
                existingLimit.LimitValue = request.LimitValue;
                existingLimit.EffectiveDate = request.EffectiveDate;
                if (request.IsActive.HasValue)
                    existingLimit.IsActive = request.IsActive.Value;
                existingLimit.ModifiedDate = DateTime.UtcNow;
                existingLimit.ModifiedBy = currentUserId;

                await _commandRepository.UpdateAsync(existingLimit);

                return new Response<object>
                {
                    Success = true,
                    Message = "Daily limit updated successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating dealer daily limit");
                return new Response<object>
                {
                    Success = false,
                    Message = "An error occurred while updating the daily limit"
                };
            }
        }
    }
}