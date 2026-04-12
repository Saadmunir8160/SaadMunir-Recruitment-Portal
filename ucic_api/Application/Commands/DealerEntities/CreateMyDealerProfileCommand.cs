using Application.Common.Interfaces;
using Application.DTOs;
using Domain.Repositories.Command.Base;
using Domain.Repositories.Query.Base;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Commands.DealerEntities
{
    public class CreateMyDealerProfileCommand : IRequest<Response<object>>
    {
        public string? CompanyName { get; set; }
        public string? DealerCode { get; set; }
        public decimal? CreditLimit { get; set; }
    }

    public class CreateMyDealerProfileCommandHandler : IRequestHandler<CreateMyDealerProfileCommand, Response<object>>
    {
        private readonly ICommandRepository<Domain.Entities.Dealer> _dealerCommandRepository;
        private readonly IQueryRepository<Domain.Entities.Dealer> _dealerQueryRepository;
        private readonly IIdentityService _identityService;

        public CreateMyDealerProfileCommandHandler(
            ICommandRepository<Domain.Entities.Dealer> dealerCommandRepository,
            IQueryRepository<Domain.Entities.Dealer> dealerQueryRepository,
            IIdentityService identityService)
        {
            _dealerCommandRepository = dealerCommandRepository;
            _dealerQueryRepository = dealerQueryRepository;
            _identityService = identityService;
        }

        public async Task<Response<object>> Handle(CreateMyDealerProfileCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // Get current user ID
                var currentUserId = _identityService.GetCurrentUserId();
                if (string.IsNullOrEmpty(currentUserId))
                {
                    return new Response<object>
                    {
                        Success = false,
                        Message = "User not authenticated"
                    };
                }

                // Check if user has Dealer role
                var userRoles = await _identityService.GetUserRolesAsync(currentUserId);
                if (!userRoles.Contains("Dealer"))
                {
                    return new Response<object>
                    {
                        Success = false,
                        Message = "User is not authorized as a dealer"
                    };
                }

                // Check if dealer profile already exists
                var existingDealer = await _dealerQueryRepository.GetQueryable()
                    .FirstOrDefaultAsync(d => d.UserId == currentUserId && !d.IsDeleted);

                if (existingDealer != null)
                {
                    return new Response<object>
                    {
                        Success = false,
                        Message = "Dealer profile already exists for this user"
                    };
                }

                // Get user details
                var userDetails = await _identityService.GetUserDetailsAsync(currentUserId);

                // Create dealer entity - simplified for new structure
                var dealer = new Domain.Entities.Dealer
                {
                    UserId = currentUserId,
                    DealerName = userDetails.fullName,
                    // CompanyName removed from simplified entity
                    DealerCode = request.DealerCode,
                    CreditLimit = request.CreditLimit ?? 0,
                    CurrentBalance = 0,
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = currentUserId,
                    IsActive = true,
                    IsDeleted = false
                };

                var result = await _dealerCommandRepository.AddAsync(dealer);

                if (result <= 0)
                {
                    return new Response<object>
                    {
                        Success = false,
                        Message = "Failed to create dealer profile"
                    };
                }

                return new Response<object>
                {
                    Success = true,
                    Message = "Dealer profile created successfully",
                    Data = new { DealerId = dealer.DealerId, DealerName = dealer.DealerName }
                };
            }
            catch (Exception ex)
            {
                return new Response<object>
                {
                    Success = false,
                    Message = $"Error creating dealer profile: {ex.Message}"
                };
            }
        }
    }
}