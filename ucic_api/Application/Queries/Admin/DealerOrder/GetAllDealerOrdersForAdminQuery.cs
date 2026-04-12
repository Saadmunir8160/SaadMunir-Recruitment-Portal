using Application.DTOs;
using Domain.Repositories.Query.Base;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Queries.Admin.DealerOrder
{
    public class GetAllDealerOrdersForAdminQuery : IRequest<PaginatedResponse<DealerOrderForAdminDTO>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? Search { get; set; }
        public int? DealerId { get; set; }
        public string? Status { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    public class GetAllDealerOrdersForAdminQueryHandler : IRequestHandler<GetAllDealerOrdersForAdminQuery, PaginatedResponse<DealerOrderForAdminDTO>>
    {
        private readonly IQueryRepository<Domain.Entities.DealerOrder> _dealerOrderRepository;
        private readonly IQueryRepository<Domain.Entities.Dealer> _dealerRepository;
        private readonly IQueryRepository<Domain.Entities.DealerDriver> _driverRepository;
        private readonly IQueryRepository<Domain.Entities.DealerVehicle> _vehicleRepository;
        private readonly IQueryRepository<Domain.Entities.DealerOrderItem> _orderItemRepository;
        private readonly IQueryRepository<Domain.Entities.DealerArea> _areaRepository;
        private readonly Application.Common.Interfaces.IIdentityService _identityService;

        public GetAllDealerOrdersForAdminQueryHandler(
            IQueryRepository<Domain.Entities.DealerOrder> dealerOrderRepository,
            IQueryRepository<Domain.Entities.Dealer> dealerRepository,
            IQueryRepository<Domain.Entities.DealerDriver> driverRepository,
            IQueryRepository<Domain.Entities.DealerVehicle> vehicleRepository,
            IQueryRepository<Domain.Entities.DealerOrderItem> orderItemRepository,
            IQueryRepository<Domain.Entities.DealerArea> areaRepository,
            Application.Common.Interfaces.IIdentityService identityService)
        {
            _dealerOrderRepository = dealerOrderRepository;
            _dealerRepository = dealerRepository;
            _driverRepository = driverRepository;
            _vehicleRepository = vehicleRepository;
            _orderItemRepository = orderItemRepository;
            _areaRepository = areaRepository;
            _identityService = identityService;
        }

        public async Task<PaginatedResponse<DealerOrderForAdminDTO>> Handle(GetAllDealerOrdersForAdminQuery request, CancellationToken cancellationToken)
        {
            var query = _dealerOrderRepository.GetQueryable();

            if (request.DealerId.HasValue)
            {
                query = query.Where(o => o.DealerID == request.DealerId.Value);
            }

            if (!string.IsNullOrEmpty(request.Status))
            {
                query = query.Where(o => o.Status == request.Status);
            }

            if (request.StartDate.HasValue)
            {
                query = query.Where(o => o.OrderDate >= request.StartDate.Value);
            }

            if (request.EndDate.HasValue)
            {
                query = query.Where(o => o.OrderDate <= request.EndDate.Value);
            }

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var searchLower = request.Search.ToLower();
                query = query.Where(o => 
                    (o.CustomerOrderNumber != null && o.CustomerOrderNumber.ToLower().Contains(searchLower)) ||
                    (o.PortalOrderNumber != null && o.PortalOrderNumber.ToLower().Contains(searchLower)) ||
                    (o.Ln_OrderNumber != null && o.Ln_OrderNumber.ToLower().Contains(searchLower)) ||
                    (o.TransporterName != null && o.TransporterName.ToLower().Contains(searchLower)) ||
                    (o.Dealer != null && o.Dealer.DealerName != null && o.Dealer.DealerName.ToLower().Contains(searchLower)) ||
                    (o.Dealer != null && o.Dealer.Ln_ID != null && o.Dealer.Ln_ID.ToLower().Contains(searchLower)));
            }

            var totalRecords = await query.CountAsync(cancellationToken);

            var orders = await query
                .OrderByDescending(o => o.OrderDate)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            var dealerIds = orders.Select(o => o.DealerID).Distinct().ToList();
            var driverIds = orders.Where(o => o.DriverID.HasValue).Select(o => o.DriverID!.Value).Distinct().ToList();
            var vehicleIds = orders.Where(o => o.VehicleID.HasValue).Select(o => o.VehicleID!.Value).Distinct().ToList();
            var areaIds = orders.Where(o => o.AreaID.HasValue).Select(o => o.AreaID!.Value).Distinct().ToList();
            var orderIds = orders.Select(o => o.DealerOrderID).ToList();

            var dealers = await _dealerRepository.GetQueryable()
                .Where(d => dealerIds.Contains(d.DealerId))
                .ToListAsync(cancellationToken);

            var drivers = await _driverRepository.GetQueryable()
                .Where(d => driverIds.Contains(d.DriverID))
                .ToListAsync(cancellationToken);

            var vehicles = await _vehicleRepository.GetQueryable()
                .Where(v => vehicleIds.Contains(v.VehicleID))
                .ToListAsync(cancellationToken);

            var areas = await _areaRepository.GetQueryable()
                .Where(a => areaIds.Contains(a.AreaID))
                .ToListAsync(cancellationToken);

            var orderItems = await _orderItemRepository.GetQueryable()
                .Where(oi => orderIds.Contains(oi.DealerOrderID))
                .ToListAsync(cancellationToken);

            var dealerLookup = dealers.ToDictionary(d => d.DealerId, d => new { d.DealerName, d.Ln_ID });
            
            // Fetch driver names from users table
            var driverLookup = new Dictionary<int, string>();
            foreach (var driver in drivers)
            {
                var userDetails = await _identityService.GetUserDetailsAsync(driver.UserId);
                var driverName = !string.IsNullOrWhiteSpace(userDetails.fullName) 
                    ? userDetails.fullName 
                    : userDetails.UserName ?? $"Driver {driver.DriverID}";
                driverLookup[driver.DriverID] = driverName;
            }
            
            var vehicleLookup = vehicles.ToDictionary(v => v.VehicleID, v => 
                !string.IsNullOrWhiteSpace(v.PlateNumber) ? v.PlateNumber : $"Vehicle {v.VehicleID}");
            var areaLookup = areas.ToDictionary(a => a.AreaID, a => new { a.AreaName, a.AreaCode });
            var orderItemsLookup = orderItems.GroupBy(oi => oi.DealerOrderID)
                .ToDictionary(g => g.Key, g => g.ToList());

            var orderDTOs = orders.Select(order => new DealerOrderForAdminDTO
            {
                DealerOrderID = order.DealerOrderID,
                DealerID = order.DealerID,
                DealerName = dealerLookup.ContainsKey(order.DealerID) ? dealerLookup[order.DealerID].DealerName : "Unknown Dealer",
                DealerLN_ID = dealerLookup.ContainsKey(order.DealerID) ? dealerLookup[order.DealerID].Ln_ID : null,
                CustomerOrderNumber = order.CustomerOrderNumber,
                PortalOrderNumber = order.PortalOrderNumber,
                Ln_OrderNumber = order.Ln_OrderNumber ?? "",
                TransporterName = order.TransporterName,
                OrderDate = order.OrderDate ?? DateTime.UtcNow,
                Status = order.Status ?? "Unknown",
                TotalAmount = order.TotalAmount ?? 0,
                DriverID = order.DriverID,
                DriverName = order.DriverID.HasValue ? driverLookup.GetValueOrDefault(order.DriverID.Value, "Unknown Driver") : null,
                VehicleID = order.VehicleID,
                VehicleName = order.VehicleID.HasValue ? vehicleLookup.GetValueOrDefault(order.VehicleID.Value, "Unknown Vehicle") : null,
                AddressID = order.AddressID,
                AreaID = order.AreaID,
                AreaName = order.AreaID.HasValue && areaLookup.ContainsKey(order.AreaID.Value) ? areaLookup[order.AreaID.Value].AreaName : null,
                AreaCode = order.AreaID.HasValue && areaLookup.ContainsKey(order.AreaID.Value) ? areaLookup[order.AreaID.Value].AreaCode : null,
                IsActive = order.IsActive,
                CreatedDate = order.CreatedDate,
                ModifiedDate = order.ModifiedDate ?? DateTime.UtcNow,
                OrderItems = orderItemsLookup.GetValueOrDefault(order.DealerOrderID, new List<Domain.Entities.DealerOrderItem>())
                    .Select(oi => new DealerOrderItemForAdminDTO
                    {
                        OrderItemID = oi.OrderItemID,
                        DealerOrderID = oi.DealerOrderID,
                        DealerProductID = oi.DealerProductID,
                        ProductName = oi.ProductDescription ?? "Unknown Product",
                        Quantity = (int)(oi.Quantity ?? 0),
                        UnitPrice = 0,
                        TotalPrice = 0,
                        Notes = oi.Unit ?? "Unknown Unit"
                    }).ToList()
            }).ToList();

            var totalPages = (int)Math.Ceiling((double)totalRecords / request.PageSize);

            return new PaginatedResponse<DealerOrderForAdminDTO>
            {
                Data = orderDTOs,
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

    public class GetDealerOrdersStatsForAdminQuery : IRequest<DealerOrdersStatsForAdminDTO>
    {
        public int? DealerId { get; set; }
    }

    public class GetDealerOrdersStatsForAdminQueryHandler : IRequestHandler<GetDealerOrdersStatsForAdminQuery, DealerOrdersStatsForAdminDTO>
    {
        private readonly IQueryRepository<Domain.Entities.DealerOrder> _dealerOrderRepository;

        public GetDealerOrdersStatsForAdminQueryHandler(IQueryRepository<Domain.Entities.DealerOrder> dealerOrderRepository)
        {
            _dealerOrderRepository = dealerOrderRepository;
        }

        public async Task<DealerOrdersStatsForAdminDTO> Handle(GetDealerOrdersStatsForAdminQuery request, CancellationToken cancellationToken)
        {
            var query = _dealerOrderRepository.GetQueryable();

            if (request.DealerId.HasValue)
            {
                query = query.Where(o => o.DealerID == request.DealerId.Value);
            }

            var orders = await query.ToListAsync(cancellationToken);
            var today = DateTime.Today;
            var weekStart = today.AddDays(-(int)today.DayOfWeek);
            var monthStart = new DateTime(today.Year, today.Month, 1);

            var totalOrderValue = orders.Where(o => o.TotalAmount.HasValue).Sum(o => o.TotalAmount!.Value);
            var averageOrderValue = orders.Any(o => o.TotalAmount.HasValue) 
                ? orders.Where(o => o.TotalAmount.HasValue).Average(o => o.TotalAmount!.Value) 
                : 0;

            return new DealerOrdersStatsForAdminDTO
            {
                TotalOrders = orders.Count,
                PendingOrders = orders.Count(o => o.Status == "Pending" || o.Status == "Processing"),
                CompletedOrders = orders.Count(o => o.Status == "Completed" || o.Status == "Delivered"),
                CancelledOrders = orders.Count(o => o.Status == "Cancelled"),
                TotalOrderValue = totalOrderValue,
                AverageOrderValue = averageOrderValue,
                OrdersToday = orders.Count(o => o.OrderDate.HasValue && o.OrderDate.Value.Date == today),
                OrdersThisWeek = orders.Count(o => o.OrderDate.HasValue && o.OrderDate.Value.Date >= weekStart),
                OrdersThisMonth = orders.Count(o => o.OrderDate.HasValue && o.OrderDate.Value.Date >= monthStart)
            };
        }
    }

    public class GetDealerOrderByIdForAdminQuery : IRequest<DealerOrderForAdminDTO?>
    {
        public int Id { get; set; }
    }

    public class GetDealerOrderByIdForAdminQueryHandler : IRequestHandler<GetDealerOrderByIdForAdminQuery, DealerOrderForAdminDTO?>
    {
        private readonly IQueryRepository<Domain.Entities.DealerOrder> _dealerOrderRepository;
        private readonly IQueryRepository<Domain.Entities.Dealer> _dealerRepository;
        private readonly IQueryRepository<Domain.Entities.DealerDriver> _driverRepository;
        private readonly IQueryRepository<Domain.Entities.DealerVehicle> _vehicleRepository;
        private readonly IQueryRepository<Domain.Entities.DealerOrderItem> _orderItemRepository;
        private readonly IQueryRepository<Domain.Entities.DealerArea> _areaRepository;
        private readonly Application.Common.Interfaces.IIdentityService _identityService;

        public GetDealerOrderByIdForAdminQueryHandler(
            IQueryRepository<Domain.Entities.DealerOrder> dealerOrderRepository,
            IQueryRepository<Domain.Entities.Dealer> dealerRepository,
            IQueryRepository<Domain.Entities.DealerDriver> driverRepository,
            IQueryRepository<Domain.Entities.DealerVehicle> vehicleRepository,
            IQueryRepository<Domain.Entities.DealerOrderItem> orderItemRepository,
            IQueryRepository<Domain.Entities.DealerArea> areaRepository,
            Application.Common.Interfaces.IIdentityService identityService)
        {
            _dealerOrderRepository = dealerOrderRepository;
            _dealerRepository = dealerRepository;
            _driverRepository = driverRepository;
            _vehicleRepository = vehicleRepository;
            _orderItemRepository = orderItemRepository;
            _areaRepository = areaRepository;
            _identityService = identityService;
        }

        public async Task<DealerOrderForAdminDTO?> Handle(GetDealerOrderByIdForAdminQuery request, CancellationToken cancellationToken)
        {
            var order = await _dealerOrderRepository.GetByIdAsync(request.Id);
            if (order == null)
                return null;

            var dealer = await _dealerRepository.GetByIdAsync(order.DealerID);
            var driver = order.DriverID.HasValue ? await _driverRepository.GetByIdAsync(order.DriverID.Value) : null;
            var vehicle = order.VehicleID.HasValue ? await _vehicleRepository.GetByIdAsync(order.VehicleID.Value) : null;
            var area = order.AreaID.HasValue ? await _areaRepository.GetByIdAsync(order.AreaID.Value) : null;
            
            // Fetch driver name from users table
            string? driverName = null;
            if (driver != null)
            {
                var userDetails = await _identityService.GetUserDetailsAsync(driver.UserId);
                driverName = !string.IsNullOrWhiteSpace(userDetails.fullName) 
                    ? userDetails.fullName 
                    : userDetails.UserName ?? $"Driver {driver.DriverID}";
            }

            var orderItems = await _orderItemRepository.GetQueryable()
                .Where(oi => oi.DealerOrderID == order.DealerOrderID)
                .ToListAsync(cancellationToken);

            return new DealerOrderForAdminDTO
            {
                DealerOrderID = order.DealerOrderID,
                DealerID = order.DealerID,
                DealerName = dealer?.DealerName ?? "Unknown Dealer",
                CustomerOrderNumber = order.CustomerOrderNumber,
                PortalOrderNumber = order.PortalOrderNumber,
                Ln_OrderNumber = order.Ln_OrderNumber,
                TransporterName = order.TransporterName,
                OrderDate = order.OrderDate ?? DateTime.UtcNow,
                Status = order.Status ?? "Unknown",
                TotalAmount = order.TotalAmount ?? 0,
                DriverID = order.DriverID,
                DriverName = driverName,
                VehicleID = order.VehicleID,
                VehicleName = vehicle?.PlateNumber ?? "Unknown Vehicle",
                AddressID = order.AddressID,
                AreaID = order.AreaID,
                AreaName = area?.AreaName,
                AreaCode = area?.AreaCode,
                IsActive = order.IsActive,
                CreatedDate = order.CreatedDate,
                ModifiedDate = order.ModifiedDate ?? DateTime.UtcNow,
                OrderItems = orderItems.Select(oi => new DealerOrderItemForAdminDTO
                {
                    OrderItemID = oi.OrderItemID,
                    DealerOrderID = oi.DealerOrderID,
                    DealerProductID = oi.DealerProductID,
                    ProductName = oi.ProductDescription ?? "Unknown Product",
                    Quantity = (int)(oi.Quantity ?? 0),
                    UnitPrice = 0,
                    TotalPrice = 0,
                    Notes = oi.Unit ?? "Unknown Unit"
                }).ToList()
            };
        }
    }
}