using Application.Common.Interfaces;
using Application.DTOs;
using Domain.Repositories.Query.Base;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace Application.Commands.User.Login
{
    public class LoginDealerCommand : IRequest<Response<DealerLoginResponseDTO>>
    {
        [Required(ErrorMessage = "UserName is required")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; }
    }

    public class LoginDealerCommandHandler : IRequestHandler<LoginDealerCommand, Response<DealerLoginResponseDTO>>
    {
        private readonly IIdentityService _identityService;
        private readonly IQueryRepository<Domain.Entities.Dealer> _dealerRepository;

        public LoginDealerCommandHandler(
            IIdentityService identityService,
            IQueryRepository<Domain.Entities.Dealer> dealerRepository)
        {
            _identityService = identityService;
            _dealerRepository = dealerRepository;
        }

        public async Task<Response<DealerLoginResponseDTO>> Handle(LoginDealerCommand request, CancellationToken cancellationToken)
        {
            // Authenticate user
            var loginResult = await _identityService.SigninUserAsync(request.UserName, request.Password);
            
            if (!loginResult)
            {
                return new Response<DealerLoginResponseDTO>
                {
                    Success = false,
                    Message = "Invalid username or password.",
                    Data = null
                };
            }

            // Get user details
            var userId = await _identityService.GetUserIdAsync(request.UserName);
            var userDetails = await _identityService.GetUserDetailsAsync(userId);

            // Check if user has Dealer role
            if (!userDetails.roles.Contains("Dealer"))
            {
                return new Response<DealerLoginResponseDTO>
                {
                    Success = false,
                    Message = "User is not authorized as a Dealer.",
                    Data = null
                };
            }

            // Get dealer information
            var filters = new Dictionary<string, object>
            {
                { nameof(Domain.Entities.Dealer.UserId), userId }
            };
            
            var dealers = await _dealerRepository.GetByColumnsWithListAsync(filters);
            var dealer = dealers.FirstOrDefault();

            if (dealer == null)
            {
                return new Response<DealerLoginResponseDTO>
                {
                    Success = false,
                    Message = "Dealer profile not found.",
                    Data = null
                };
            }

            if (!dealer.IsActive)
            {
                return new Response<DealerLoginResponseDTO>
                {
                    Success = false,
                    Message = "Dealer account is inactive.",
                    Data = null
                };
            }

            var response = new DealerLoginResponseDTO
            {
                UserId = userDetails.userId,
                DealerId = dealer.DealerId,
                DealerName = dealer.DealerName,
                FullName = userDetails.fullName,
                UserName = userDetails.UserName,
                Email = userDetails.email,
                CreditLimit = dealer.CreditLimit,
                CurrentBalance = dealer.CurrentBalance,
                Ln_ID = dealer.Ln_ID,
                Roles = userDetails.roles.ToList()
            };

            return new Response<DealerLoginResponseDTO>
            {
                Success = true,
                Message = "Login successful.",
                Data = response
            };
        }
    }

    public class LoginDealerDriverCommand : IRequest<Response<DealerDriverLoginResponseDTO>>
    {
        [Required(ErrorMessage = "UserName is required")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; }
    }

    public class LoginDealerDriverCommandHandler : IRequestHandler<LoginDealerDriverCommand, Response<DealerDriverLoginResponseDTO>>
    {
        private readonly IIdentityService _identityService;
        private readonly IQueryRepository<Domain.Entities.DealerDriver> _dealerDriverRepository;
        private readonly IQueryRepository<Domain.Entities.Dealer> _dealerRepository;

        public LoginDealerDriverCommandHandler(
            IIdentityService identityService,
            IQueryRepository<Domain.Entities.DealerDriver> dealerDriverRepository,
            IQueryRepository<Domain.Entities.Dealer> dealerRepository)
        {
            _identityService = identityService;
            _dealerDriverRepository = dealerDriverRepository;
            _dealerRepository = dealerRepository;
        }

        public async Task<Response<DealerDriverLoginResponseDTO>> Handle(LoginDealerDriverCommand request, CancellationToken cancellationToken)
        {
            // Authenticate user
            var loginResult = await _identityService.SigninUserAsync(request.UserName, request.Password);
            
            if (!loginResult)
            {
                return new Response<DealerDriverLoginResponseDTO>
                {
                    Success = false,
                    Message = "Invalid username or password.",
                    Data = null
                };
            }

            // Get user details
            var userId = await _identityService.GetUserIdAsync(request.UserName);
            var userDetails = await _identityService.GetUserDetailsAsync(userId);

            // Check if user has DealerDriver role
            if (!userDetails.roles.Contains("DealerDriver"))
            {
                return new Response<DealerDriverLoginResponseDTO>
                {
                    Success = false,
                    Message = "User is not authorized as a Dealer Driver.",
                    Data = null
                };
            }

            // Get dealer driver information
            var filters = new Dictionary<string, object>
            {
                { nameof(Domain.Entities.DealerDriver.UserId), userId }
            };
            
            var dealerDrivers = await _dealerDriverRepository.GetByColumnsWithListAsync(filters);
            var dealerDriver = dealerDrivers.FirstOrDefault();

            if (dealerDriver == null)
            {
                return new Response<DealerDriverLoginResponseDTO>
                {
                    Success = false,
                    Message = "Dealer driver profile not found.",
                    Data = null
                };
            }

            if (!dealerDriver.IsActive)
            {
                return new Response<DealerDriverLoginResponseDTO>
                {
                    Success = false,
                    Message = "Dealer driver account is inactive.",
                    Data = null
                };
            }

            // Get dealer information
            var dealer = await _dealerRepository.GetByIdAsync(dealerDriver.DealerID);

            var response = new DealerDriverLoginResponseDTO
            {
                UserId = userDetails.userId,
                DriverId = dealerDriver.DriverID, // Changed from DriverID to DriverId for consistency
                DealerId = dealerDriver.DealerID, // Changed from DealerID to DealerId for consistency
                DealerName = dealer?.DealerName ?? "Unknown",
                FullName = userDetails.fullName,
                UserName = userDetails.UserName,
                Email = userDetails.email,
                Ln_ID = dealerDriver.Ln_ID,
                IqamaNumber = dealerDriver.IqamaNumber,
                Roles = userDetails.roles.ToList()
            };

            return new Response<DealerDriverLoginResponseDTO>
            {
                Success = true,
                Message = "Login successful.",
                Data = response
            };
        }
    }

    // Response DTOs
    public class DealerLoginResponseDTO
    {
        public string UserId { get; set; }
        public int DealerId { get; set; }
        public string DealerName { get; set; }
        public string FullName { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public decimal? CreditLimit { get; set; }
        public decimal? CurrentBalance { get; set; }
        public string? Ln_ID { get; set; }
        public List<string> Roles { get; set; }
    }

    public class DealerDriverLoginResponseDTO
    {
        public string UserId { get; set; }
        public int DriverId { get; set; } // Changed from DriverID to DriverId for consistency
        public int DealerId { get; set; } // Changed from DealerID to DealerId for consistency
        public string DealerName { get; set; }
        public string FullName { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string? Ln_ID { get; set; }
        public string? IqamaNumber { get; set; }
        public List<string> Roles { get; set; }
    }
}