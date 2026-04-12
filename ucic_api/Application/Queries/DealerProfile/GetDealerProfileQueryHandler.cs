using Application.DTOs.DealerProfile;
using Application.Common.Interfaces;
using Application.Queries.DealerProfile;
using Domain.Entities;
using Domain.Repositories.Query.Base;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Queries.DealerProfile
{
    public class GetDealerProfileQueryHandler : IRequestHandler<GetDealerProfileQuery, DealerProfileDto>
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

        public async Task<DealerProfileDto> Handle(GetDealerProfileQuery request, CancellationToken cancellationToken)
        {
            var currentDealerId = await _identityService.GetCurrentDealerIdAsync();
            
            var dealer = await _dealerRepository.GetQueryable()
                .Where(d => d.DealerId == currentDealerId)
                .FirstOrDefaultAsync(cancellationToken);

            if (dealer == null)
            {
                throw new Exception("Dealer not found");
            }

            // Get user information using the IdentityService
            var userInfo = await _identityService.GetUserDetailsAsync(dealer.UserId);

            return new DealerProfileDto
            {
                DealerId = dealer.DealerId,
                DealerName = dealer.DealerName,
                CreditLimit = dealer.CreditLimit,
                CurrentBalance = dealer.CurrentBalance,
                Ln_ID = dealer.Ln_ID,
                DealerCode = dealer.DealerCode,
                IsVerified = dealer.IsVerified,
                FullName = userInfo.fullName ?? "",
                Email = userInfo.email ?? "",
                PhoneNumber = userInfo.phoneNumber ?? ""
            };
        }
    }
}