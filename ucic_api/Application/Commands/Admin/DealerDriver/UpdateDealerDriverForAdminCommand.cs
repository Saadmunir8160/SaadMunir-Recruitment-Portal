using Application.Common.Interfaces;
using Application.DTOs;
using Application.Services;
using Domain.Entities;
using Domain.Repositories.Command.Base;
using Domain.Repositories.Query.Base;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Commands.Admin.DealerDriver
{
    public class UpdateDealerDriverForAdminCommand : IRequest<Response<bool>>
    {
        public int Id { get; set; }
        public string DriverName { get; set; } = string.Empty;
        public string? Ln_ID { get; set; }
        public string? IqamaNumber { get; set; }
        public string PhoneNumber { get; set; } = string.Empty;
        public string? Email { get; set; }
        public bool IsActive { get; set; }
    }

    public class UpdateDealerDriverForAdminCommandHandler : IRequestHandler<UpdateDealerDriverForAdminCommand, Response<bool>>
    {
        private readonly ICommandRepository<Domain.Entities.DealerDriver> _commandRepository;
        private readonly IQueryRepository<Domain.Entities.DealerDriver> _queryRepository;
        private readonly IUserUpdateService _userUpdateService;
        private readonly IIdentityService _identityService;
        private readonly ILogger<UpdateDealerDriverForAdminCommandHandler> _logger;

        public UpdateDealerDriverForAdminCommandHandler(
            ICommandRepository<Domain.Entities.DealerDriver> commandRepository,
            IQueryRepository<Domain.Entities.DealerDriver> queryRepository,
            IUserUpdateService userUpdateService,
            IIdentityService identityService,
            ILogger<UpdateDealerDriverForAdminCommandHandler> logger)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _userUpdateService = userUpdateService;
            _identityService = identityService;
            _logger = logger;
        }

        public async Task<Response<bool>> Handle(UpdateDealerDriverForAdminCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var entity = await _queryRepository.GetByIdAsync(request.Id);
                if (entity == null)
                {
                    return new Response<bool>
                    {
                        Success = false,
                        Message = "Driver not found."
                    };
                }

                // Update user information (FullName, Email, PhoneNumber are stored in ApplicationUser)
                if (!string.IsNullOrEmpty(request.DriverName) || 
                    !string.IsNullOrEmpty(request.Email) || 
                    !string.IsNullOrEmpty(request.PhoneNumber))
                {
                    var userUpdateSuccess = await _userUpdateService.UpdateUserAsync(
                        entity.UserId,
                        request.DriverName,
                        request.Email,
                        request.PhoneNumber);
                    
                    if (!userUpdateSuccess)
                    {
                        return new Response<bool> 
                        { 
                            Success = false, 
                            Message = "Failed to update user information." 
                        };
                    }
                }

                // Update driver entity properties (Ln_ID and IqamaNumber)
                entity.Ln_ID = request.Ln_ID;
                entity.IqamaNumber = request.IqamaNumber;
                entity.IsActive = request.IsActive;
                entity.ModifiedDate = DateTime.UtcNow;
                entity.ModifiedBy = _identityService.GetCurrentUserId();

                await _commandRepository.UpdateAsync(entity);

                _logger.LogInformation("Admin updated driver {DriverId} successfully", request.Id);

                return new Response<bool>
                {
                    Success = true,
                    Data = true,
                    Message = "Driver updated successfully."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating driver {DriverId}", request.Id);
                return new Response<bool>
                {
                    Success = false,
                    Message = $"Error updating driver: {ex.Message}"
                };
            }
        }
    }
}
