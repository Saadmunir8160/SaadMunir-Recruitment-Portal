using Application.DTOs;
using Application.Common.Interfaces;
using Domain.Entities;
using Domain.Repositories.Query.Base;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Queries.DealerVehicle
{
    public class GetDealerVehicleByIdQuery : IRequest<Response<DealerVehicleDTO>>
    {
        public int VehicleID { get; set; }
    }

    public class GetDealerVehicleByIdQueryHandler : IRequestHandler<GetDealerVehicleByIdQuery, Response<DealerVehicleDTO>>
    {
        private readonly IQueryRepository<Domain.Entities.DealerVehicle> _repository;
        private readonly IIdentityService _identityService;

        public GetDealerVehicleByIdQueryHandler(
            IQueryRepository<Domain.Entities.DealerVehicle> repository,
            IIdentityService identityService)
        {
            _repository = repository;
            _identityService = identityService;
        }

        public async Task<Response<DealerVehicleDTO>> Handle(GetDealerVehicleByIdQuery request, CancellationToken cancellationToken)
        {
            // Get current dealer ID
            var currentDealerId = await _identityService.GetCurrentDealerIdAsync();
            if (currentDealerId == null)
            {
                return new Response<DealerVehicleDTO>
                {
                    Success = false,
                    Message = "Dealer not found for current user"
                };
            }

            // Find vehicle that belongs to the current dealer
            var vehicle = await _repository.GetQueryable()
                .Where(v => v.VehicleID == request.VehicleID && v.DealerID == currentDealerId.Value)
                .FirstOrDefaultAsync();

            if (vehicle == null)
                return new Response<DealerVehicleDTO> { Success = false, Message = "Dealer vehicle not found." };

            var dto = new DealerVehicleDTO
            {
                VehicleID = vehicle.VehicleID,
                DealerID = vehicle.DealerID,
                PlateNumber = vehicle.PlateNumber,
                Type = vehicle.Type,
                Capacity = vehicle.Capacity,
                RegistrationDate = vehicle.RegistrationDate,
                RegistrationExpiryDate = vehicle.RegistrationExpiryDate,
                InsuranceExpiryDate = vehicle.InsuranceExpiryDate,
                Ln_ID = vehicle.Ln_ID,
                RegistrationNumber = vehicle.RegistrationNumber,
                VehicleType = vehicle.VehicleType,
                IsActive = vehicle.IsActive,
                CreatedAt = vehicle.CreatedDate,
                UpdatedAt = vehicle.ModifiedDate ?? vehicle.CreatedDate
            };
            return new Response<DealerVehicleDTO> { Success = true, Data = dto, Message = "Dealer vehicle retrieved successfully." };
        }
    }
}
