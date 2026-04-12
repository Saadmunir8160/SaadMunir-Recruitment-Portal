using Application.Common.Interfaces;
using Application.DTOs;
using Domain.Repositories.Command.Base;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace Application.Commands.User.Create
{
    public class CreateDealerUserCommand : IRequest<Response<CreateDealerUserCommand>>
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

        // Dealer-specific properties
        [Required(ErrorMessage = "DealerName is required")]
        [StringLength(100, ErrorMessage = "DealerName cannot exceed 100 characters")]
        public string DealerName { get; set; } = string.Empty;
        
        public decimal? CreditLimit { get; set; }
        public decimal? CurrentBalance { get; set; }
        
        [MaxLength(50, ErrorMessage = "Ln_ID cannot exceed 50 characters")]
        public string? Ln_ID { get; set; }
    }

    public class CreateDealerUserCommandHandler : IRequestHandler<CreateDealerUserCommand, Response<CreateDealerUserCommand>>
    {
        private readonly IIdentityService _identityService;
        private readonly ICommandRepository<Domain.Entities.Dealer> _dealerRepository;
        private readonly Domain.Repositories.Query.Base.IQueryRepository<Domain.Entities.Dealer> _dealerQueryRepository;

        public CreateDealerUserCommandHandler(
            IIdentityService identityService,
            ICommandRepository<Domain.Entities.Dealer> dealerRepository,
            Domain.Repositories.Query.Base.IQueryRepository<Domain.Entities.Dealer> dealerQueryRepository)
        {
            _identityService = identityService;
            _dealerRepository = dealerRepository;
            _dealerQueryRepository = dealerQueryRepository;
        }

        public async Task<Response<CreateDealerUserCommand>> Handle(CreateDealerUserCommand request, CancellationToken cancellationToken)
        {
            // Check if LN_ID already exists
            if (!string.IsNullOrWhiteSpace(request.Ln_ID))
            {
                var lnIdExists = await _dealerQueryRepository.ValueExistsAsync("Ln_ID", request.Ln_ID);
                if (lnIdExists)
                {
                    return new Response<CreateDealerUserCommand>
                    {
                        Success = false,
                        Message = $"LN-ID '{request.Ln_ID}' is already in use. Please use a unique LN-ID.",
                        Data = request
                    };
                }
            }

            const string dealerRole = "Dealer";
            
            // Create user with Dealer role
            var userResult = await _identityService.CreateUserAsync(
                request.UserName, 
                request.Password, 
                request.Email, 
                request.FullName, 
                request.Phone, 
                dealerRole);

            if (!userResult.isSucceed)
            {
                return new Response<CreateDealerUserCommand>
                {
                    Success = false,
                    Message = $"Failed to create user: {userResult.userId}",
                    Data = request
                };
            }

            // Create dealer entity
            var dealer = new Domain.Entities.Dealer
            {
                UserId = userResult.userId,
                DealerName = request.DealerName,
                CreditLimit = request.CreditLimit,
                CurrentBalance = request.CurrentBalance ?? 0,
                Ln_ID = request.Ln_ID,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = request.UserName,
                IsActive = true,
                IsDeleted = false
            };

            var dealerResult = await _dealerRepository.AddAsync(dealer);
            
            if (dealerResult <= 0)
            {
                // Rollback: Delete the user if dealer creation fails
                await _identityService.DeleteUserAsync(userResult.userId);

                return new Response<CreateDealerUserCommand>
                {
                    Success = false,
                    Message = "Failed to create dealer profile. User creation has been rolled back.",
                    Data = request
                };
            }

            var responseData = new CreateDealerUserCommand
            {
                FullName = request.FullName,
                UserName = request.UserName,
                Email = request.Email,
                Phone = request.Phone,
                DealerName = request.DealerName,
                CreditLimit = request.CreditLimit,
                CurrentBalance = request.CurrentBalance,
                Ln_ID = request.Ln_ID
                // Note: Password and ConfirmationPassword are intentionally omitted
            };

            return new Response<CreateDealerUserCommand>
            {
                Success = true,
                Message = $"Dealer user created successfully with ID: {userResult.userId}",
                Data = responseData
            };
        }
    }
}