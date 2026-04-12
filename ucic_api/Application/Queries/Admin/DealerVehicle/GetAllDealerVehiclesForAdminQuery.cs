using Application.DTOs;
using Domain.Repositories.Query.Base;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Queries.Admin.DealerVehicle
{
    public class GetAllDealerVehiclesForAdminQuery : IRequest<PaginatedResponse<DealerVehicleForAdminDTO>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? Search { get; set; }
        public int? DealerId { get; set; }
        public string? VehicleType { get; set; }
        public bool? IsActive { get; set; }
    }

    public class GetAllDealerVehiclesForAdminQueryHandler : IRequestHandler<GetAllDealerVehiclesForAdminQuery, PaginatedResponse<DealerVehicleForAdminDTO>>
    {
        private readonly IQueryRepository<Domain.Entities.DealerVehicle> _vehicleRepository;
        private readonly IQueryRepository<Domain.Entities.Dealer> _dealerRepository;

        public GetAllDealerVehiclesForAdminQueryHandler(
            IQueryRepository<Domain.Entities.DealerVehicle> vehicleRepository,
            IQueryRepository<Domain.Entities.Dealer> dealerRepository)
        {
            _vehicleRepository = vehicleRepository;
            _dealerRepository = dealerRepository;
        }



        public async Task<PaginatedResponse<DealerVehicleForAdminDTO>> Handle(GetAllDealerVehiclesForAdminQuery request, CancellationToken cancellationToken)
        {
            var query = _vehicleRepository.GetQueryable();

            if (request.DealerId.HasValue)
            {
                query = query.Where(v => v.DealerID == request.DealerId.Value);
            }

            if (request.IsActive.HasValue)
            {
                query = query.Where(v => v.IsActive == request.IsActive.Value);
            }

            if (request.VehicleType != null)
            {
                query = query.Where(v => v.Type == request.VehicleType);
            }

            if (!string.IsNullOrEmpty(request.Search))
            {
                var searchLower = request.Search.ToLower();
                query = query.Where(v => 
                    v.PlateNumber.ToLower().Contains(searchLower) ||
                    v.Type.ToLower().Contains(searchLower));
            }

            var totalRecords = await query.CountAsync(cancellationToken);

            var vehicles = await query
                .OrderBy(v => v.PlateNumber)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            var dealerIds = vehicles.Select(v => v.DealerID).Distinct().ToList();
            var dealers = await _dealerRepository.GetQueryable()
                .Where(d => dealerIds.Contains(d.DealerId))
                .ToListAsync(cancellationToken);

            var dealerLookup = dealers.ToDictionary(d => d.DealerId, d => d.DealerName);

            var vehicleDTOs = vehicles.Select(vehicle => new DealerVehicleForAdminDTO
            {
                VehicleID = vehicle.VehicleID,
                DealerID = vehicle.DealerID,
                DealerName = dealerLookup.GetValueOrDefault(vehicle.DealerID, "Unknown Dealer"),
                VehicleName = VehicleHelper.GetVehicleName(vehicle),
                LicensePlate = VehicleHelper.GetLicensePlate(vehicle),
                VehicleType = VehicleHelper.GetVehicleType(vehicle),
                Capacity = vehicle.Capacity,
                IsActive = vehicle.IsActive,
                CreatedDate = vehicle.CreatedDate,
                ModifiedDate = vehicle.ModifiedDate ?? DateTime.UtcNow,
                DriverID = null, // Will need to be populated from relationships
                DriverName = "No Driver", // Default for now
                Status = vehicle.IsActive ? "Active" : "Inactive"
            }).ToList();

            var totalPages = (int)Math.Ceiling((double)totalRecords / request.PageSize);

            return new PaginatedResponse<DealerVehicleForAdminDTO>
            {
                Data = vehicleDTOs,
                Metadata = new PaginationMetadata
                {
                    CurrentPage = request.PageNumber,
                    PageSize = request.PageSize,
                    TotalCount = totalRecords,
                    TotalPages = totalPages
                },
                Success = true,
                Message = $"Retrieved {vehicleDTOs.Count} vehicles successfully"
            };
        }
    }

    public class GetDealerVehicleByIdForAdminQuery : IRequest<DealerVehicleForAdminDTO?>
    {
        public int Id { get; set; }
    }

    public class GetDealerVehicleByIdForAdminQueryHandler : IRequestHandler<GetDealerVehicleByIdForAdminQuery, DealerVehicleForAdminDTO?>
    {
        private readonly IQueryRepository<Domain.Entities.DealerVehicle> _vehicleRepository;
        private readonly IQueryRepository<Domain.Entities.Dealer> _dealerRepository;

        public GetDealerVehicleByIdForAdminQueryHandler(
            IQueryRepository<Domain.Entities.DealerVehicle> vehicleRepository,
            IQueryRepository<Domain.Entities.Dealer> dealerRepository)
        {
            _vehicleRepository = vehicleRepository;
            _dealerRepository = dealerRepository;
        }

        public async Task<DealerVehicleForAdminDTO?> Handle(GetDealerVehicleByIdForAdminQuery request, CancellationToken cancellationToken)
        {
            var vehicle = await _vehicleRepository.GetByIdAsync(request.Id);
            if (vehicle == null)
                return null;

            var dealer = await _dealerRepository.GetByIdAsync(vehicle.DealerID);

            return new DealerVehicleForAdminDTO
            {
                VehicleID = vehicle.VehicleID,
                DealerID = vehicle.DealerID,
                DealerName = dealer?.DealerName ?? "Unknown Dealer",
                VehicleName = VehicleHelper.GetVehicleName(vehicle),
                LicensePlate = VehicleHelper.GetLicensePlate(vehicle),
                VehicleType = VehicleHelper.GetVehicleType(vehicle),
                Capacity = vehicle.Capacity,
                IsActive = vehicle.IsActive,
                CreatedDate = vehicle.CreatedDate,
                ModifiedDate = vehicle.ModifiedDate ?? DateTime.UtcNow,
                Status = vehicle.IsActive ? "Active" : "Inactive"
            };
        }
    }

    public class GetDealerVehiclesStatsForAdminQuery : IRequest<DealerVehiclesStatsForAdminDTO>
    {
        public int? DealerId { get; set; }
    }

    public class GetDealerVehiclesStatsForAdminQueryHandler : IRequestHandler<GetDealerVehiclesStatsForAdminQuery, DealerVehiclesStatsForAdminDTO>
    {
        private readonly IQueryRepository<Domain.Entities.DealerVehicle> _vehicleRepository;

        public GetDealerVehiclesStatsForAdminQueryHandler(IQueryRepository<Domain.Entities.DealerVehicle> vehicleRepository)
        {
            _vehicleRepository = vehicleRepository;
        }

        public async Task<DealerVehiclesStatsForAdminDTO> Handle(GetDealerVehiclesStatsForAdminQuery request, CancellationToken cancellationToken)
        {
            var query = _vehicleRepository.GetQueryable();

            if (request.DealerId.HasValue)
            {
                query = query.Where(v => v.DealerID == request.DealerId.Value);
            }

            var vehicles = await query.ToListAsync(cancellationToken);

            return new DealerVehiclesStatsForAdminDTO
            {
                TotalVehicles = vehicles.Count,
                ActiveVehicles = vehicles.Count(v => v.IsActive),
                InactiveVehicles = vehicles.Count(v => !v.IsActive),
                TotalCapacity = vehicles.Where(v => v.IsActive).Sum(v => v.Capacity)
            };
        }
    }

    public static class VehicleHelper
    {
        public static string GetVehicleName(Domain.Entities.DealerVehicle vehicle)
        {
            // Try different combinations to create a meaningful vehicle name
            if (!string.IsNullOrWhiteSpace(vehicle.PlateNumber))
            {
                var type = GetVehicleType(vehicle);
                return $"{type} - {vehicle.PlateNumber}";
            }
            
            if (!string.IsNullOrWhiteSpace(vehicle.RegistrationNumber))
            {
                var type = GetVehicleType(vehicle);
                return $"{type} - {vehicle.RegistrationNumber}";
            }
            
            return $"Vehicle {vehicle.VehicleID}";
        }

        public static string GetLicensePlate(Domain.Entities.DealerVehicle vehicle)
        {
            // Check PlateNumber first, then RegistrationNumber, then fallback
            if (!string.IsNullOrWhiteSpace(vehicle.PlateNumber))
                return vehicle.PlateNumber;
                
            if (!string.IsNullOrWhiteSpace(vehicle.RegistrationNumber))
                return vehicle.RegistrationNumber;
                
            return $"N/A-{vehicle.VehicleID}";
        }

        public static string GetVehicleType(Domain.Entities.DealerVehicle vehicle)
        {
            // Check Type field first, then VehicleType (legacy), then fallback
            if (!string.IsNullOrWhiteSpace(vehicle.Type))
                return vehicle.Type;
                
            if (!string.IsNullOrWhiteSpace(vehicle.VehicleType))
                return vehicle.VehicleType;
                
            return "Unknown";
        }
    }
}