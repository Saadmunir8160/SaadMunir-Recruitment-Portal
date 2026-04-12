using Application.Common.Interfaces;
using Application.DTOs;
using Domain.Repositories.Query.Base;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Queries.Admin.DealerDriver
{
    public class GetAllDealerDriversForAdminQuery : IRequest<PaginatedResponse<DealerDriverForAdminDTO>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? Search { get; set; }
        public int? DealerId { get; set; }
        public bool? IsActive { get; set; }
    }

    public class GetAllDealerDriversForAdminQueryHandler : IRequestHandler<GetAllDealerDriversForAdminQuery, PaginatedResponse<DealerDriverForAdminDTO>>
    {
        private readonly IQueryRepository<Domain.Entities.DealerDriver> _driverRepository;
        private readonly IQueryRepository<Domain.Entities.Dealer> _dealerRepository;
        private readonly IIdentityService _identityService;

        public GetAllDealerDriversForAdminQueryHandler(
            IQueryRepository<Domain.Entities.DealerDriver> driverRepository,
            IQueryRepository<Domain.Entities.Dealer> dealerRepository,
            IIdentityService identityService)
        {
            _driverRepository = driverRepository;
            _dealerRepository = dealerRepository;
            _identityService = identityService;
        }

        public async Task<PaginatedResponse<DealerDriverForAdminDTO>> Handle(GetAllDealerDriversForAdminQuery request, CancellationToken cancellationToken)
        {
            var query = _driverRepository.GetQueryable();

            // Apply filters that can be done at database level
            if (request.DealerId.HasValue)
            {
                query = query.Where(d => d.DealerID == request.DealerId.Value);
            }

            if (request.IsActive.HasValue)
            {
                query = query.Where(d => d.IsActive == request.IsActive.Value);
            }

            // Apply database-level search on fields that exist in the entity
            if (!string.IsNullOrEmpty(request.Search))
            {
                var searchLower = request.Search.ToLower();
                query = query.Where(d => 
                    (d.Ln_ID != null && d.Ln_ID.ToLower().Contains(searchLower)) ||
                    (d.IqamaNumber != null && d.IqamaNumber.ToLower().Contains(searchLower)));
            }

            var drivers = await query
                .OrderBy(d => d.UserId)
                .ToListAsync(cancellationToken);

            // Get dealer information
            var dealerIds = drivers.Select(d => d.DealerID).Distinct().ToList();
            var dealers = await _dealerRepository.GetQueryable()
                .Where(d => dealerIds.Contains(d.DealerId))
                .ToListAsync(cancellationToken);

            var dealerLookup = dealers.ToDictionary(d => d.DealerId, d => d.DealerName);

            // Fetch user details for all drivers
            var driverDTOs = new List<DealerDriverForAdminDTO>();
            
            foreach (var driver in drivers)
            {
                var userDetails = await _identityService.GetUserDetailsAsync(driver.UserId);
                
                driverDTOs.Add(new DealerDriverForAdminDTO
                {
                    DriverID = driver.DriverID,
                    DealerID = driver.DealerID,
                    DealerName = dealerLookup.GetValueOrDefault(driver.DealerID, "Unknown Dealer"),
                    DriverName = !string.IsNullOrWhiteSpace(userDetails.fullName) ? 
                        userDetails.fullName : "",
                    FullName = userDetails.fullName,
                    UserName = userDetails.UserName,
                    Ln_ID = driver.Ln_ID,
                    IqamaNumber = driver.IqamaNumber,
                    PhoneNumber = userDetails.phoneNumber ?? "N/A",
                    Email = userDetails.email ?? "N/A",
                    IsActive = driver.IsActive,
                    CreatedDate = driver.CreatedDate,
                    ModifiedDate = driver.ModifiedDate ?? DateTime.UtcNow,
                    TotalDeliveries = 0, // Would need to calculate from actual deliveries
                    LastActiveDate = null, // Would need to track from activity logs
                    AssignedVehicle = null // Would need to get from vehicle assignments
                });
            }

            // Apply in-memory search filter on user details (name, email, phone)
            if (!string.IsNullOrEmpty(request.Search))
            {
                var searchLower = request.Search.ToLower();
                driverDTOs = driverDTOs.Where(d =>
                    (!string.IsNullOrEmpty(d.DriverName) && d.DriverName.ToLower().Contains(searchLower)) ||
                    (!string.IsNullOrEmpty(d.FullName) && d.FullName.ToLower().Contains(searchLower)) ||
                    (!string.IsNullOrEmpty(d.UserName) && d.UserName.ToLower().Contains(searchLower)) ||
                    (!string.IsNullOrEmpty(d.Email) && d.Email.ToLower().Contains(searchLower)) ||
                    (!string.IsNullOrEmpty(d.PhoneNumber) && d.PhoneNumber.Contains(searchLower)) ||
                    (!string.IsNullOrEmpty(d.Ln_ID) && d.Ln_ID.ToLower().Contains(searchLower)) ||
                    (!string.IsNullOrEmpty(d.IqamaNumber) && d.IqamaNumber.ToLower().Contains(searchLower))
                ).ToList();
            }

            var totalRecords = driverDTOs.Count;
            
            // Apply pagination after filtering
            driverDTOs = driverDTOs
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            var totalPages = (int)Math.Ceiling((double)totalRecords / request.PageSize);

            return new PaginatedResponse<DealerDriverForAdminDTO>
            {
                Data = driverDTOs,
                Metadata = new PaginationMetadata
                {
                    CurrentPage = request.PageNumber,
                    PageSize = request.PageSize,
                    TotalCount = totalRecords,
                    TotalPages = totalPages
                }
            };
        }
    }

    public class GetDealerDriverByIdForAdminQuery : IRequest<DealerDriverForAdminDTO?>
    {
        public int Id { get; set; }
    }

    public class GetDealerDriverByIdForAdminQueryHandler : IRequestHandler<GetDealerDriverByIdForAdminQuery, DealerDriverForAdminDTO?>
    {
        private readonly IQueryRepository<Domain.Entities.DealerDriver> _driverRepository;
        private readonly IQueryRepository<Domain.Entities.Dealer> _dealerRepository;
        private readonly IIdentityService _identityService;

        public GetDealerDriverByIdForAdminQueryHandler(
            IQueryRepository<Domain.Entities.DealerDriver> driverRepository,
            IQueryRepository<Domain.Entities.Dealer> dealerRepository,
            IIdentityService identityService)
        {
            _driverRepository = driverRepository;
            _dealerRepository = dealerRepository;
            _identityService = identityService;
        }

        public async Task<DealerDriverForAdminDTO?> Handle(GetDealerDriverByIdForAdminQuery request, CancellationToken cancellationToken)
        {
            var driver = await _driverRepository.GetByIdAsync(request.Id);
            if (driver == null)
                return null;

            var dealer = await _dealerRepository.GetByIdAsync(driver.DealerID);
            var userDetails = await _identityService.GetUserDetailsAsync(driver.UserId);

            return new DealerDriverForAdminDTO
            {
                DriverID = driver.DriverID,
                DealerID = driver.DealerID,
                DealerName = dealer?.DealerName ?? "Unknown Dealer",
                DriverName = !string.IsNullOrWhiteSpace(userDetails.fullName) ? 
                    userDetails.fullName : 
                    $"Driver {driver.DriverID}",
                FullName = userDetails.fullName,
                UserName = userDetails.UserName,
                Ln_ID = driver.Ln_ID,
                IqamaNumber = driver.IqamaNumber,
                PhoneNumber = userDetails.phoneNumber ?? "N/A",
                Email = userDetails.email ?? "N/A",
                IsActive = driver.IsActive,
                CreatedDate = driver.CreatedDate,
                ModifiedDate = driver.ModifiedDate ?? DateTime.UtcNow,
                TotalDeliveries = 0,
                LastActiveDate = null,
                AssignedVehicle = null
            };
        }
    }

    public class GetDealerDriversStatsForAdminQuery : IRequest<DealerDriversStatsForAdminDTO>
    {
        public int? DealerId { get; set; }
    }

    public class GetDealerDriversStatsForAdminQueryHandler : IRequestHandler<GetDealerDriversStatsForAdminQuery, DealerDriversStatsForAdminDTO>
    {
        private readonly IQueryRepository<Domain.Entities.DealerDriver> _driverRepository;

        public GetDealerDriversStatsForAdminQueryHandler(IQueryRepository<Domain.Entities.DealerDriver> driverRepository)
        {
            _driverRepository = driverRepository;
        }

        public async Task<DealerDriversStatsForAdminDTO> Handle(GetDealerDriversStatsForAdminQuery request, CancellationToken cancellationToken)
        {
            var query = _driverRepository.GetQueryable();

            if (request.DealerId.HasValue)
            {
                query = query.Where(d => d.DealerID == request.DealerId.Value);
            }

            var drivers = await query.ToListAsync(cancellationToken);

            return new DealerDriversStatsForAdminDTO
            {
                TotalDrivers = drivers.Count,
                ActiveDrivers = drivers.Count(d => d.IsActive),
                InactiveDrivers = drivers.Count(d => !d.IsActive),
                DriversWithVehicles = 0, // Would need vehicle assignment data
                DriversWithoutVehicles = drivers.Count, // Placeholder
                TotalDeliveries = 0, // Would need delivery data
                DeliveriesToday = 0, // Would need delivery data
                DeliveriesThisWeek = 0 // Would need delivery data
            };
        }
    }
}