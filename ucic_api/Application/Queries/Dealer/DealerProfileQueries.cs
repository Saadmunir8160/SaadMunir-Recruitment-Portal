using Application.DTOs.Dealer;
using Application.DTOs;
using Application.Common.Interfaces;
using Domain.Repositories.Query.Base;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Queries.Dealer
{
    public class GetDealerProfileQuery : IRequest<Response<DealerProfileDTO>>
    {
    }

    public class GetDealerProfileQueryHandler : IRequestHandler<GetDealerProfileQuery, Response<DealerProfileDTO>>
    {
        private readonly IQueryRepository<Domain.Entities.Dealer> _dealerRepository;
        private readonly IIdentityService _identityService;

        public GetDealerProfileQueryHandler(
            IQueryRepository<Domain.Entities.Dealer> dealerRepository,
            IIdentityService identityService)
        {
            _dealerRepository = dealerRepository;
            _identityService = identityService;
        }

        public async Task<Response<DealerProfileDTO>> Handle(GetDealerProfileQuery request, CancellationToken cancellationToken)
        {
                // Get current dealer ID
                var currentDealerId = await _identityService.GetCurrentDealerIdAsync();
                if (currentDealerId == null)
                {
                    return new Response<DealerProfileDTO>
                    {
                        Success = false,
                        Message = "User not authenticated or dealer not found"
                    };
                }

                // Get dealer information
                var dealer = await _dealerRepository
                    .GetQueryable()
                    .FirstOrDefaultAsync(d => d.DealerId == currentDealerId.Value, cancellationToken);

                if (dealer == null)
                {
                    return new Response<DealerProfileDTO>
                    {
                        Success = false,
                        Message = "Dealer profile not found"
                    };
                }

                // Get current user ID and user details
                var currentUserId = _identityService.GetCurrentUserId();
                var userInfo = new DealerUserInfoDTO();
                
                if (!string.IsNullOrEmpty(currentUserId))
                {
                    var userDetails = await _identityService.GetUserDetailsAsync(currentUserId);
                    userInfo.UserId = userDetails.userId;
                    userInfo.FullName = userDetails.fullName;
                    userInfo.Email = userDetails.email;
                    userInfo.PhoneNumber = userDetails.phoneNumber;
                    userInfo.UserName = userDetails.UserName;
                    userInfo.Roles = userDetails.roles;
                }

                var dealerProfile = new DealerProfileDTO
                {
                    Id = dealer.DealerId,
                    DealerCode = dealer.DealerCode,
                    DealerName = dealer.DealerName,
                    CreditLimit = dealer.CreditLimit,
                    CurrentBalance = dealer.CurrentBalance,
                    Ln_ID = dealer.Ln_ID,
                    IsVerified = dealer.IsVerified,
                    UserInfo = userInfo,
                    CreatedAt = dealer.CreatedDate,
                    //UpdatedAt = dealer.updatedDate
                };

                return new Response<DealerProfileDTO>
                {
                    Success = true,
                    Data = dealerProfile,
                    Message = "Dealer profile retrieved successfully"
                };
        }
    }
}