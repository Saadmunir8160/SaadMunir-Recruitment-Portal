using MediatR;
using Application.DTOs;
using Application.Common.Interfaces;
using Domain.Repositories.Command.Base;
using Microsoft.Extensions.Logging;

namespace Application.Commands.Admin.DailyLimits
{
    public class CreateDealerDailyLimitCommandHandler : IRequestHandler<CreateDealerDailyLimitCommand, Response<object>>
    {
        private readonly ICommandRepository<Domain.Entities.DealerDailyLimit> _dailyLimitRepository;
        private readonly IIdentityService _identityService;
        private readonly ILogger<CreateDealerDailyLimitCommandHandler> _logger;

        public CreateDealerDailyLimitCommandHandler(
            ICommandRepository<Domain.Entities.DealerDailyLimit> dailyLimitRepository,
            IIdentityService identityService,
            ILogger<CreateDealerDailyLimitCommandHandler> logger)
        {
            _dailyLimitRepository = dailyLimitRepository;
            _identityService = identityService;
            _logger = logger;
        }

        public async Task<Response<object>> Handle(CreateDealerDailyLimitCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var currentUserId = _identityService.GetCurrentUserId();
                
                var dailyLimit = new Domain.Entities.DealerDailyLimit
                {
                    DealerID = request.DealerID,
                    LimitType = request.LimitType,
                    LimitValue = request.LimitValue,
                    EffectiveDate = request.EffectiveDate,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = currentUserId,
                    ModifiedDate = DateTime.UtcNow,
                    ModifiedBy = currentUserId
                };

                await _dailyLimitRepository.AddAsync(dailyLimit);

                return new Response<object>
                {
                    Success = true,
                    Data = new { dailyLimit.DailyLimitID },
                    Message = "Daily limit created successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating dealer daily limit");
                return new Response<object>
                {
                    Success = false,
                    Message = "An error occurred while creating the daily limit"
                };
            }
        }
    }
}