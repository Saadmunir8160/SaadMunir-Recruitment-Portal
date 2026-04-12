using Application.Common.Interfaces;
using Application.DTOs;
using Domain.Repositories.Command.Base;
using Domain.Repositories.Query.Base;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace Application.Commands.User.Update
{
    public class UpdateDealerUserCommand : IRequest<Response<UpdateDealerUserCommand>>
    {
        [Required(ErrorMessage = "DealerId is required")]
        public int DealerId { get; set; }

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

        // Dealer-specific properties
        [Required(ErrorMessage = "DealerName is required")]
        [StringLength(100, ErrorMessage = "DealerName cannot exceed 100 characters")]
        public string DealerName { get; set; } = string.Empty;
        
        public decimal? CreditLimit { get; set; }
        public decimal? CurrentBalance { get; set; }
        
        [MaxLength(50, ErrorMessage = "Ln_ID cannot exceed 50 characters")]
        public string? Ln_ID { get; set; }
        
        public bool? IsActive { get; set; }
    }

    public class UpdateDealerUserCommandHandler : IRequestHandler<UpdateDealerUserCommand, Response<UpdateDealerUserCommand>>
    {
        private readonly IIdentityService _identityService;
        private readonly IQueryRepository<Domain.Entities.Dealer> _dealerQueryRepository;
        private readonly ICommandRepository<Domain.Entities.Dealer> _dealerCommandRepository;

        public UpdateDealerUserCommandHandler(
            IIdentityService identityService,
            IQueryRepository<Domain.Entities.Dealer> dealerQueryRepository,
            ICommandRepository<Domain.Entities.Dealer> dealerCommandRepository)
        {
            _identityService = identityService;
            _dealerQueryRepository = dealerQueryRepository;
            _dealerCommandRepository = dealerCommandRepository;
        }

        public async Task<Response<UpdateDealerUserCommand>> Handle(UpdateDealerUserCommand request, CancellationToken cancellationToken)
        {
            // Get the dealer entity
            var dealer = await _dealerQueryRepository.GetByIdAsync(request.DealerId);
            if (dealer == null)
            {
                return new Response<UpdateDealerUserCommand>
                {
                    Success = false,
                    Message = "Dealer not found",
                    Data = request
                };
            }

            // Check if LN_ID already exists (excluding current dealer)
            if (!string.IsNullOrWhiteSpace(request.Ln_ID))
            {
                var existingDealers = _dealerQueryRepository.GetQueryable()
                    .Where(d => d.Ln_ID == request.Ln_ID && d.DealerId != request.DealerId)
                    .ToList();
                
                if (existingDealers.Any())
                {
                    return new Response<UpdateDealerUserCommand>
                    {
                        Success = false,
                        Message = $"LN-ID '{request.Ln_ID}' is already in use by another dealer. Please use a unique LN-ID.",
                        Data = request
                    };
                }
            }

            // Get current user roles to maintain them
            var currentUserRoles = await _identityService.GetUserRolesAsync(dealer.UserId);

            // Update user information using Identity Service
            var userUpdateResult = await _identityService.UpdateUserProfile(
                dealer.UserId,
                request.FullName,
                request.Email,
                request.Phone,
                currentUserRoles);

            if (!userUpdateResult)
            {
                return new Response<UpdateDealerUserCommand>
                {
                    Success = false,
                    Message = "Failed to update user profile",
                    Data = request
                };
            }

            // Update dealer entity
            dealer.DealerName = request.DealerName;
            dealer.CreditLimit = request.CreditLimit;
            dealer.CurrentBalance = request.CurrentBalance;
            dealer.Ln_ID = request.Ln_ID;
            if (request.IsActive.HasValue)
            {
                dealer.IsActive = request.IsActive.Value;
            }
            dealer.ModifiedDate = DateTime.UtcNow;
            dealer.ModifiedBy = request.UserName;

            await _dealerCommandRepository.UpdateAsync(dealer);

            var responseData = new UpdateDealerUserCommand
            {
                DealerId = request.DealerId,
                FullName = request.FullName,
                UserName = request.UserName,
                Email = request.Email,
                Phone = request.Phone,
                DealerName = request.DealerName,
                CreditLimit = request.CreditLimit,
                CurrentBalance = request.CurrentBalance,
                Ln_ID = request.Ln_ID,
                IsActive = request.IsActive
            };

            return new Response<UpdateDealerUserCommand>
            {
                Success = true,
                Message = $"Dealer user updated successfully for ID: {request.DealerId}",
                Data = responseData
            };
        }
    }
}