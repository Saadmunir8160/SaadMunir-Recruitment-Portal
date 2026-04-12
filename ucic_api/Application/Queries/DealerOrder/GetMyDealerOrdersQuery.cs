using Application.Common.Interfaces;
using Application.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Queries.DealerOrder
{
    public class GetMyDealerOrdersQuery : IRequest<Response<List<DealerOrderDTO>>>
    {
    }

    public class GetMyDealerOrdersQueryHandler : IRequestHandler<GetMyDealerOrdersQuery, Response<List<DealerOrderDTO>>>
    {
        private readonly Domain.Repositories.Query.Base.IQueryRepository<Domain.Entities.DealerOrder> _dealerOrderRepository;
        private readonly Domain.Repositories.Query.Base.IQueryRepository<Domain.Entities.DealerDriver> _driverRepository;
        private readonly Domain.Repositories.Query.Base.IQueryRepository<Domain.Entities.DealerVehicle> _vehicleRepository;
        private readonly Application.Common.Interfaces.IIdentityService _identityService;

        public GetMyDealerOrdersQueryHandler(
            Domain.Repositories.Query.Base.IQueryRepository<Domain.Entities.DealerOrder> dealerOrderRepository,
            Domain.Repositories.Query.Base.IQueryRepository<Domain.Entities.DealerDriver> driverRepository,
            Domain.Repositories.Query.Base.IQueryRepository<Domain.Entities.DealerVehicle> vehicleRepository,
            Application.Common.Interfaces.IIdentityService identityService)
        {
            _dealerOrderRepository = dealerOrderRepository;
            _driverRepository = driverRepository;
            _vehicleRepository = vehicleRepository;
            _identityService = identityService;
        }

        public async Task<Response<List<DealerOrderDTO>>> Handle(GetMyDealerOrdersQuery request, CancellationToken cancellationToken)
        {
            try
            {
                // Get current dealer ID using the centralized method
                var currentDealerId = await _identityService.GetCurrentDealerIdAsync();
                if (currentDealerId == null)
                {
                    return new Response<List<DealerOrderDTO>>
                    {
                        Success = false,
                        Message = "Dealer not found for current user"
                    };
                }

                var dealerOrders = await _dealerOrderRepository.GetQueryable()
                    .Include(x => x.DealerOrderItems)
                        .ThenInclude(x => x.DealerProduct)
                    .Include(x => x.DealerShippingAddress)
                    .Include(x => x.DealerArea)
                    .Where(x => x.DealerID == currentDealerId.Value && !x.IsDeleted && x.IsActive)
                    .OrderByDescending(x => x.CreatedDate)
                    .ToListAsync();

                // Fetch driver and vehicle data for all orders
                var driverIds = dealerOrders.Where(o => o.DriverID.HasValue).Select(o => o.DriverID!.Value).Distinct().ToList();
                var vehicleIds = dealerOrders.Where(o => o.VehicleID.HasValue).Select(o => o.VehicleID!.Value).Distinct().ToList();

                var drivers = await _driverRepository.GetQueryable()
                    .Where(d => driverIds.Contains(d.DriverID))
                    .ToListAsync();

                var vehicles = await _vehicleRepository.GetQueryable()
                    .Where(v => vehicleIds.Contains(v.VehicleID))
                    .ToListAsync();

                // Build lookup dictionaries for drivers (fetch names from identity service)
                var driverLookup = new Dictionary<int, string>();
                foreach (var driver in drivers)
                {
                    var userDetails = await _identityService.GetUserDetailsAsync(driver.UserId);
                    var driverName = !string.IsNullOrWhiteSpace(userDetails.fullName)
                        ? userDetails.fullName
                        : userDetails.UserName ?? $"Driver {driver.DriverID}";
                    driverLookup[driver.DriverID] = driverName;
                }

                // Build lookup dictionary for vehicles
                var vehicleLookup = vehicles.ToDictionary(v => v.VehicleID, v =>
                    !string.IsNullOrWhiteSpace(v.PlateNumber) ? v.PlateNumber : $"Vehicle {v.VehicleID}");

                var dealerOrderDTOs = new List<DealerOrderDTO>();

                foreach (var order in dealerOrders)
                {
                    var dealerOrderDTO = new DealerOrderDTO
                    {
                        DealerOrderID = order.DealerOrderID,
                        DealerID = order.DealerID,
                        DriverID = order.DriverID,
                        DriverName = order.DriverID.HasValue ? driverLookup.GetValueOrDefault(order.DriverID.Value, null) : null,
                        VehicleID = order.VehicleID,
                        VehicleName = order.VehicleID.HasValue ? vehicleLookup.GetValueOrDefault(order.VehicleID.Value, null) : null,
                        AddressID = order.AddressID,
                        CustomerOrderNumber = order.CustomerOrderNumber,
                        Ln_OrderNumber = order.Ln_OrderNumber,
                        PortalOrderNumber = order.PortalOrderNumber,
                        TransporterName = order.TransporterName,
                        OrderDate = order.OrderDate,
                        Status = order.Status,
                        TotalAmount = order.TotalAmount,
                        IsActive = order.IsActive,
                        CreatedDate = order.CreatedDate,
                        CreatedBy = order.CreatedBy,
                        ModifiedDate = order.ModifiedDate,
                        ModifiedBy = order.ModifiedBy,
                        DealerOrderItems = order.DealerOrderItems?.Select(item => new DealerOrderItemDTO
                        {
                            OrderItemID = item.OrderItemID,
                            DealerOrderID = item.DealerOrderID,
                            DealerProductID = item.DealerProductID,
                            Product_LnCode = item.Product_LnCode,
                            ProductDescription = item.ProductDescription,
                            Quantity = item.Quantity,
                            Unit = item.Unit // Map the unit from the entity with fallback
                        }).ToList(),
                        ShippingAddress = order.DealerShippingAddress != null ? new DealerShippingAddressDTO
                        {
                            AddressID = order.DealerShippingAddress.AddressID,
                            DealerID = order.DealerShippingAddress.DealerID,
                            Ln_ID = order.DealerShippingAddress.Ln_ID,
                            AddressLine1 = order.DealerShippingAddress.AddressLine1,
                            AddressLine2 = order.DealerShippingAddress.AddressLine2,
                            City = order.DealerShippingAddress.City,
                            State = order.DealerShippingAddress.State,
                            Country = order.DealerShippingAddress.Country,
                            PostalCode = order.DealerShippingAddress.PostalCode,
                            IsActive = order.DealerShippingAddress.IsActive
                        } : null,
                        DeliveryAreaName = order.DealerArea != null ? order.DealerArea.AreaName : null,
                        DeliveryAreaCode = order.DealerArea != null ? order.DealerArea.AreaCode : null
                    };

                    dealerOrderDTOs.Add(dealerOrderDTO);
                }

                return new Response<List<DealerOrderDTO>>
                {
                    Success = true,
                    Message = "My dealer orders retrieved successfully",
                    Data = dealerOrderDTOs
                };
            }
            catch (Exception ex)
            {
                return new Response<List<DealerOrderDTO>>
                {
                    Success = false,
                    Message = $"Error retrieving my dealer orders: {ex.Message}"
                };
            }
        }
    }
}