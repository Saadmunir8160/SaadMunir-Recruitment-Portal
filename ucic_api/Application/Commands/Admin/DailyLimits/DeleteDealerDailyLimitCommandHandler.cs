using MediatR;
using Application.DTOs;
using Application.Common.Interfaces;
using Domain.Repositories.Command.Base;
using Domain.Repositories.Query.Base;
using Microsoft.Extensions.Logging;

namespace Application.Commands.Admin.DailyLimits
{
    public class DeleteDealerDailyLimitCommandHandler : IRequestHandler<DeleteDealerDailyLimitCommand, Response<object>>
    {
        private readonly ICommandRepository<Domain.Entities.DealerDailyLimit> _commandRepository;
        private readonly IQueryRepository<Domain.Entities.DealerDailyLimit> _queryRepository;
        private readonly IIdentityService _identityService;
        private readonly ILogger<DeleteDealerDailyLimitCommandHandler> _logger;

        public DeleteDealerDailyLimitCommandHandler(
            ICommandRepository<Domain.Entities.DealerDailyLimit> commandRepository,
            IQueryRepository<Domain.Entities.DealerDailyLimit> queryRepository,
            IIdentityService identityService,
            ILogger<DeleteDealerDailyLimitCommandHandler> logger)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _identityService = identityService;
            _logger = logger;
        }

        public async Task<Response<object>> Handle(DeleteDealerDailyLimitCommand request, CancellationToken cancellationToken)
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
                
                // Soft delete
                existingLimit.IsDeleted = true;
                existingLimit.IsActive = false;
                existingLimit.ModifiedDate = DateTime.UtcNow;
                existingLimit.ModifiedBy = currentUserId;

                await _commandRepository.UpdateAsync(existingLimit);

                return new Response<object>
                {
                    Success = true,
                    Message = "Daily limit deleted successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting dealer daily limit");
                return new Response<object>
                {
                    Success = false,
                    Message = "An error occurred while deleting the daily limit"
                };
            }
        }
    }
}