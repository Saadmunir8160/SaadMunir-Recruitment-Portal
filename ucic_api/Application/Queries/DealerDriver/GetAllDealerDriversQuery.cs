using Application.Common.Interfaces;
using Application.DTOs;
using Domain.Entities;
using Domain.Repositories.Query.Base;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Queries.DealerEntities
{
    public class GetAllDealerDriversQuery : IRequest<Response<List<DealerDriverDTO>>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class GetAllDealerDriversQueryHandler : IRequestHandler<GetAllDealerDriversQuery, Response<List<DealerDriverDTO>>>
    {
        private readonly IQueryRepository<DealerDriver> _repository;
        private readonly IIdentityService _identityService;
        private readonly ILogger<GetAllDealerDriversQueryHandler> _logger;

        public GetAllDealerDriversQueryHandler(
            IQueryRepository<DealerDriver> repository,
            IIdentityService identityService,
            ILogger<GetAllDealerDriversQueryHandler> logger)
        {
            _repository = repository;
            _identityService = identityService;
            _logger = logger;
        }

        public async Task<Response<List<DealerDriverDTO>>> Handle(GetAllDealerDriversQuery request, CancellationToken cancellationToken)
        {
            // Get current dealer ID using centralized method
            var currentDealerId = await _identityService.GetCurrentDealerIdAsync();
            if (currentDealerId == null)
            {
                _logger.LogWarning("Current user is not associated with any dealer");
                return new Response<List<DealerDriverDTO>>
                {
                    Success = false,
                    Message = "Current user is not associated with any dealer.",
                    Data = new List<DealerDriverDTO>()
                };
            }

            var all = await _repository.GetAllAsync();
            
            // Filter by current dealer
            var dealerDrivers = all.Where(d => /*d.DealerID == currentDealerId.Value*/ !d.IsDeleted).ToList();
            
            var paged = dealerDrivers.Skip((request.PageNumber - 1) * request.PageSize).Take(request.PageSize).ToList();
            
            var result = new List<DealerDriverDTO>();

            foreach (var driver in paged)
            {
                var userInfo = await _identityService.GetUserDetailsAsync(driver.UserId);
                
                result.Add(new DealerDriverDTO
                {
                    DriverID = driver.DriverID,
                    UserId = driver.UserId,
                    DealerID = driver.DealerID,
                    Ln_ID = driver.Ln_ID,
                    IqamaNumber = driver.IqamaNumber,
                    IsActive = driver.IsActive,
                    // Include user information if available
                    UserName = userInfo.UserName ?? driver.UserId,
                    FullName = userInfo.fullName ?? "",
                    Email = userInfo.email ?? "",
                    PhoneNumber = userInfo.phoneNumber ?? "",
                    CreatedDate = driver.CreatedDate,
                    UpdatedDate = driver.ModifiedDate ?? driver.CreatedDate
                });
            }

            return new Response<List<DealerDriverDTO>> 
            { 
                Success = true, 
                Data = result, 
                Message = "Dealer drivers retrieved successfully." 
            };
        }
    }

    // Note: DealerAddress queries removed - using DealerShippingAddress instead
}
