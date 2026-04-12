using Application.Common.Interfaces;
using Application.DTOs;
using Application.Services;
using Domain.Entities;
using Domain.Repositories.Command.Base;
using Domain.Repositories.Query.Base;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Commands.DealerEntities
{
    public class UpdateDealerDriverCommand : IRequest<Response<bool>>
    {
        public UpdateDealerDriverDTO DealerDriver { get; set; } = null!;
    }

    public class UpdateDealerDriverCommandHandler : IRequestHandler<UpdateDealerDriverCommand, Response<bool>>
    {
        private readonly ICommandRepository<DealerDriver> _commandRepository;
        private readonly IQueryRepository<DealerDriver> _queryRepository;
        private readonly IUserUpdateService _userUpdateService;
        private readonly IIdentityService _identityService;
        private readonly ILogger<UpdateDealerDriverCommandHandler> _logger;
        
        public UpdateDealerDriverCommandHandler(
            ICommandRepository<DealerDriver> commandRepository, 
            IQueryRepository<DealerDriver> queryRepository,
            IUserUpdateService userUpdateService,
            IIdentityService identityService,
            ILogger<UpdateDealerDriverCommandHandler> logger)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _userUpdateService = userUpdateService;
            _identityService = identityService;
            _logger = logger;
        }
        public async Task<Response<bool>> Handle(UpdateDealerDriverCommand request, CancellationToken cancellationToken)
        {
            // Get current dealer ID using centralized method
            var currentDealerId = await _identityService.GetCurrentDealerIdAsync();
            if (currentDealerId == null)
            {
                return new Response<bool>
                {
                    Success = false,
                    Message = "Dealer not found for current user"
                };
            }

            var entity = await _queryRepository.GetByIdAsync(request.DealerDriver.DriverID);
            if (entity == null)
                return new Response<bool> { Success = false, Message = "Dealer driver not found." };

            // Validate that the driver belongs to the current dealer
            if (entity.DealerID != currentDealerId.Value)
            {
                return new Response<bool>
                {
                    Success = false,
                    Message = "Access denied. Driver does not belong to current dealer."
                };
            }
            
            // Update user information if provided
            if (!string.IsNullOrEmpty(request.DealerDriver.FullName) || 
                !string.IsNullOrEmpty(request.DealerDriver.Email) || 
                !string.IsNullOrEmpty(request.DealerDriver.PhoneNumber))
            {
                var userUpdateSuccess = await _userUpdateService.UpdateUserAsync(
                    entity.UserId,
                    request.DealerDriver.FullName,
                    request.DealerDriver.Email,
                    request.DealerDriver.PhoneNumber);
                
                if (!userUpdateSuccess)
                {
                    return new Response<bool> { Success = false, Message = "Failed to update user information." };
                }
            }
            
            // Update dealer driver information - DealerID remains the same (no need to update from request)
            entity.UserId = request.DealerDriver.UserId;
            entity.Ln_ID = request.DealerDriver.Ln_ID;
            entity.IqamaNumber = request.DealerDriver.IqamaNumber;
            entity.IsActive = request.DealerDriver.IsActive;
            entity.ModifiedDate = DateTime.UtcNow;
            entity.ModifiedBy = _identityService.GetCurrentUserId();

            await _commandRepository.UpdateAsync(entity);

            _logger.LogInformation("Dealer driver {DriverId} updated successfully for dealer {DealerId}", 
                request.DealerDriver.DriverID, currentDealerId.Value);

            return new Response<bool> 
            { 
                Success = true, 
                Data = true, 
                Message = "Dealer driver updated successfully." 
            };
        }
    }
}
