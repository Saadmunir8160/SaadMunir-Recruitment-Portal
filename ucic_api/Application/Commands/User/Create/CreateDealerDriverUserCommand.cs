using Application.Common.Interfaces;
using Application.DTOs;
using Domain.Repositories.Command.Base;
using Domain.Repositories.Query.Base;
using MediatR;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;

namespace Application.Commands.User.Create
{
    public class CreateDealerDriverUserCommand : IRequest<Response<CreateDealerDriverUserCommand>>
    {
        [Required(ErrorMessage = "FullName is required")]
        [StringLength(100, ErrorMessage = "FullName cannot exceed 100 characters")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "UserName is required")]
        [StringLength(100, ErrorMessage = "UserName cannot exceed 100 characters")]
        public string UserName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address format.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "PhoneNo is required")]
        [Phone(ErrorMessage = "Invalid Phone Number format.")]
        public string Phone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be between 6 and 100 characters")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "ConfirmationPassword is required")]
        [Compare("Password", ErrorMessage = "Password and confirmation password do not match")]
        public string ConfirmationPassword { get; set; } = string.Empty;

        // DealerDriver-specific properties - DealerID will be set automatically from current user
        [MaxLength(50, ErrorMessage = "Ln_ID cannot exceed 50 characters")]
        public string? Ln_ID { get; set; }
        
        [MaxLength(50, ErrorMessage = "IqamaNumber cannot exceed 50 characters")]
        public string? IqamaNumber { get; set; }
    }

    public class CreateDealerDriverUserCommandHandler : IRequestHandler<CreateDealerDriverUserCommand, Response<CreateDealerDriverUserCommand>>
    {
        private readonly IIdentityService _identityService;
        private readonly ICommandRepository<Domain.Entities.DealerDriver> _dealerDriverRepository;
        private readonly IQueryRepository<Domain.Entities.Dealer> _dealerQueryRepository;
        private readonly ILogger<CreateDealerDriverUserCommandHandler> _logger;

        public CreateDealerDriverUserCommandHandler(
            IIdentityService identityService,
            ICommandRepository<Domain.Entities.DealerDriver> dealerDriverRepository,
            IQueryRepository<Domain.Entities.Dealer> dealerQueryRepository,
            ILogger<CreateDealerDriverUserCommandHandler> logger)
        {
            _identityService = identityService;
            _dealerDriverRepository = dealerDriverRepository;
            _dealerQueryRepository = dealerQueryRepository;
            _logger = logger;
        }

        public async Task<Response<CreateDealerDriverUserCommand>> Handle(CreateDealerDriverUserCommand request, CancellationToken cancellationToken)
        {
            // Get current dealer ID using centralized method
            var currentDealerId = await _identityService.GetCurrentDealerIdAsync();
            if (currentDealerId == null)
            {
                _logger.LogWarning("Current user is not associated with any dealer");
                return new Response<CreateDealerDriverUserCommand>
                {
                    Success = false,
                    Message = "Current user is not associated with any dealer.",
                    Data = request
                };
            }

            _logger.LogInformation($"Creating dealer driver user: {request.UserName} for dealer ID: {currentDealerId}");
            
            // First, verify that the dealer exists
            var dealer = await _dealerQueryRepository.GetByIdAsync(currentDealerId.Value);
            if (dealer == null)
            {
                _logger.LogWarning($"Dealer with ID {currentDealerId} does not exist");
                return new Response<CreateDealerDriverUserCommand>
                {
                    Success = false,
                    Message = $"Dealer with ID {currentDealerId} does not exist.",
                    Data = request
                };
            }

            _logger.LogInformation($"Found dealer: {dealer.DealerName} with ID: {dealer.DealerId}");

            const string dealerDriverRole = "DealerDriver";
            
            // Create user with DealerDriver role
            _logger.LogInformation($"Creating user with role: {dealerDriverRole}");
            var userResult = await _identityService.CreateUserAsync(
                request.UserName, 
                request.Password, 
                request.Email, 
                request.FullName, 
                request.Phone, 
                dealerDriverRole);

            if (!userResult.isSucceed)
            {
                _logger.LogError($"Failed to create user {request.UserName}: {userResult.userId}");
                return new Response<CreateDealerDriverUserCommand>
                {
                    Success = false,
                    Message = $"Failed to create user: {userResult.userId}",
                    Data = request
                };
            }

            _logger.LogInformation($"Successfully created user with ID: {userResult.userId}");

            try
            {
                // Create dealer driver entity
                var dealerDriver = new Domain.Entities.DealerDriver
                {
                    UserId = userResult.userId,
                    DealerID = currentDealerId.Value,
                    Ln_ID = request.Ln_ID,
                    IqamaNumber = request.IqamaNumber,
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = request.UserName,
                    IsActive = true,
                    IsDeleted = false
                };

                _logger.LogInformation($"Creating dealer driver entity for user: {userResult.userId}");
                var driverResult = await _dealerDriverRepository.AddAsync(dealerDriver);
                
                if (driverResult <= 0)
                {
                    _logger.LogError($"Failed to create dealer driver entity for user: {userResult.userId}");
                    
                    // Delete the user if dealer driver creation fails
                    await _identityService.DeleteUserAsync(userResult.userId);
                    _logger.LogInformation($"Deleted user {userResult.userId} due to dealer driver creation failure");

                    return new Response<CreateDealerDriverUserCommand>
                    {
                        Success = false,
                        Message = "Failed to create dealer driver entity. User creation has been rolled back.",
                        Data = request
                    };
                }

                _logger.LogInformation($"Successfully created dealer driver with ID: {dealerDriver.DriverID} for user: {userResult.userId}");

                return new Response<CreateDealerDriverUserCommand>
                {
                    Success = true,
                    Message = $"Dealer driver user created successfully with User ID: {userResult.userId} and Driver ID: {dealerDriver.DriverID} for Dealer: {dealer.DealerName}",
                    Data = request
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Exception while creating dealer driver for user: {userResult.userId}");
                
                // Delete the user if any error occurs
                try
                {
                    await _identityService.DeleteUserAsync(userResult.userId);
                    _logger.LogInformation($"Successfully rolled back user creation for: {userResult.userId}");
                }
                catch (Exception rollbackEx)
                {
                    _logger.LogError(rollbackEx, $"Failed to rollback user creation for: {userResult.userId}");
                }
                
                return new Response<CreateDealerDriverUserCommand>
                {
                    Success = false,
                    Message = $"An error occurred while creating dealer driver: {ex.Message}. User creation has been rolled back.",
                    Data = request
                };
            }
        }
    }
}