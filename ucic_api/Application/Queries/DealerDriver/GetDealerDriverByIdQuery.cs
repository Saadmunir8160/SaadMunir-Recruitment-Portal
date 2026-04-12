using Application.DTOs;
using Application.Services;
using Domain.Entities;
using Domain.Repositories.Query.Base;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Queries.DealerEntities
{
    public class GetDealerDriverByIdQuery : IRequest<Response<DealerDriverDTO>>
    {
        public int DriverID { get; set; }
    }

    public class GetDealerDriverByIdQueryHandler : IRequestHandler<GetDealerDriverByIdQuery, Response<DealerDriverDTO>>
    {
        private readonly IQueryRepository<DealerDriver> _repository;
        private readonly IUserQueryService _userQueryService;
        
        public GetDealerDriverByIdQueryHandler(IQueryRepository<DealerDriver> repository, IUserQueryService userQueryService)
        {
            _repository = repository;
            _userQueryService = userQueryService;
        }
        public async Task<Response<DealerDriverDTO>> Handle(GetDealerDriverByIdQuery request, CancellationToken cancellationToken)
        {
            var driver = await _repository.GetByIdAsync(request.DriverID);
            if (driver == null)
                return new Response<DealerDriverDTO> { Success = false, Message = "Dealer driver not found." };
            
            // Get user information
            var userInfo = await _userQueryService.GetUserInfoAsync(driver.UserId);
            
            var result = new DealerDriverDTO
            {
                DriverID = driver.DriverID,
                UserId = driver.UserId,
                DealerID = driver.DealerID,
                Ln_ID = driver.Ln_ID,
                IqamaNumber = driver.IqamaNumber,
                IsActive = driver.IsActive,
                
                // User information from ApplicationUser
                UserName = userInfo?.UserName ?? "",
                FullName = userInfo?.FullName ?? "",
                Email = userInfo?.Email ?? "",
                PhoneNumber = userInfo?.PhoneNumber ?? "",
                CreatedDate = driver.CreatedDate,
                UpdatedDate = driver.ModifiedDate ?? driver.CreatedDate
            };
            return new Response<DealerDriverDTO> { Success = true, Data = result, Message = "Dealer driver retrieved successfully." };
        }
    }

    // Note: DealerAddress queries removed - using DealerShippingAddress instead
}
