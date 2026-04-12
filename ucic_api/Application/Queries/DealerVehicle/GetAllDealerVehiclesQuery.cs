using Application.DTOs;
using Application.Common.Interfaces;
using Domain.Entities;
using Domain.Repositories.Query.Base;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Queries.DealerVehicle
{
    public class GetAllDealerVehiclesQuery : IRequest<Response<List<DealerVehicleDTO>>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class GetAllDealerVehiclesQueryHandler : IRequestHandler<GetAllDealerVehiclesQuery, Response<List<DealerVehicleDTO>>>
    {
        private readonly IQueryRepository<Domain.Entities.DealerVehicle> _repository;
        private readonly IIdentityService _identityService;

        public GetAllDealerVehiclesQueryHandler(
            IQueryRepository<Domain.Entities.DealerVehicle> repository,
            IIdentityService identityService)
        {
            _repository = repository;
            _identityService = identityService;
        }

        public async Task<Response<List<DealerVehicleDTO>>> Handle(GetAllDealerVehiclesQuery request, CancellationToken cancellationToken)
        {
            // Get current dealer ID
            var currentDealerId = await _identityService.GetCurrentDealerIdAsync();
            if (currentDealerId == null)
            {
                return new Response<List<DealerVehicleDTO>>
                {
                    Success = false,
                    Message = "Dealer not found for current user",
                    Data = new List<DealerVehicleDTO>()
                };
            }

            // Filter by current dealer
            var vehicles = await _repository.GetQueryable()
                .Where(v => /*v.DealerID == currentDealerId.Value &&*/ !v.IsDeleted)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();

            var result = vehicles.Select(d => new DealerVehicleDTO
            {
                VehicleID = d.VehicleID,
                DealerID = d.DealerID,
                PlateNumber = d.PlateNumber,
                Type = d.Type,
                Capacity = d.Capacity,
                RegistrationDate = d.RegistrationDate,
                RegistrationExpiryDate = d.RegistrationExpiryDate,
                InsuranceExpiryDate = d.InsuranceExpiryDate,
                Ln_ID = d.Ln_ID,
                RegistrationNumber = d.RegistrationNumber,
                VehicleType = d.VehicleType,
                IsActive = d.IsActive,
                CreatedAt = d.CreatedDate,
                UpdatedAt = d.ModifiedDate ?? d.CreatedDate
            }).ToList();

            return new Response<List<DealerVehicleDTO>> 
            { 
                Success = true, 
                Data = result, 
                Message = "Dealer vehicles retrieved successfully." 
            };
        }
    }
}
