using Application.Common.Interfaces;
using Application.DTOs;
using Domain.Repositories.Query.Base;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Queries.DealerOrder
{
    public class GetDealerOrderByIdQuery : IRequest<Response<DealerOrderForAdminDTO>>
    {
        public int DealerOrderId { get; set; }
    }

    public class GetDealerOrderByIdQueryHandler : IRequestHandler<GetDealerOrderByIdQuery, Response<DealerOrderForAdminDTO>>
    {
        private readonly IQueryRepository<Domain.Entities.DealerOrder> _dealerOrderRepository;
        private readonly IQueryRepository<Domain.Entities.Dealer> _dealerRepository;
        private readonly IQueryRepository<Domain.Entities.DealerDriver> _driverRepository;
        private readonly IQueryRepository<Domain.Entities.DealerVehicle> _vehicleRepository;
        private readonly IQueryRepository<Domain.Entities.DealerOrderItem> _orderItemRepository;
        private readonly IQueryRepository<Domain.Entities.DealerArea> _areaRepository;
        private readonly IIdentityService _identityService;

        public GetDealerOrderByIdQueryHandler(
            IQueryRepository<Domain.Entities.DealerOrder> dealerOrderRepository,
            IQueryRepository<Domain.Entities.Dealer> dealerRepository,
            IQueryRepository<Domain.Entities.DealerDriver> driverRepository,
            IQueryRepository<Domain.Entities.DealerVehicle> vehicleRepository,
            IQueryRepository<Domain.Entities.DealerOrderItem> orderItemRepository,
            IQueryRepository<Domain.Entities.DealerArea> areaRepository,
            IIdentityService identityService)
        {
            _dealerOrderRepository = dealerOrderRepository;
            _dealerRepository = dealerRepository;
            _driverRepository = driverRepository;
            _vehicleRepository = vehicleRepository;
            _orderItemRepository = orderItemRepository;
            _areaRepository = areaRepository;
            _identityService = identityService;
        }

        public async Task<Response<DealerOrderForAdminDTO>> Handle(GetDealerOrderByIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var order = await _dealerOrderRepository.GetByIdAsync(request.DealerOrderId);
                if (order == null)
                {
                    return new Response<DealerOrderForAdminDTO>
                    {
                        Success = false,
                        Message = "Dealer order not found"
                    };
                }

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

                var orderDTO = new DealerOrderForAdminDTO
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
                    VehicleName = vehicle?.PlateNumber ?? (vehicle != null ? $"Vehicle {vehicle.VehicleID}" : null),
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

                return new Response<DealerOrderForAdminDTO>
                {
                    Success = true,
                    Message = "Dealer order retrieved successfully",
                    Data = orderDTO
                };
            }
            catch (Exception ex)
            {
                return new Response<DealerOrderForAdminDTO>
                {
                    Success = false,
                    Message = $"Error retrieving dealer order: {ex.Message}"
                };
            }
        }
    }
}