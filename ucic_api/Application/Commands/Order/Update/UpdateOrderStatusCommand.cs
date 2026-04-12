using Application.Common.Interfaces;
using Domain.Repositories.Command.Base;
using Domain.Repositories.Query.Base;
using MediatR;
using System.ComponentModel.DataAnnotations;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Commands.Order.Update
{
    public class UpdateOrderStatusCommand : IRequest<int>
    {
        [Required]
        public string Ln_OrderNumber { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? AvailableCredit { get; set; }

        [MaxLength(50)]
        public string? CarNumber { get; set; }

        [MaxLength(200)]
        public string? TransporterName { get; set; }

        [MaxLength(50)]
        public string? TransporterCode { get; set; }

        [MaxLength(50)]
        public string? IqamaNumber { get; set; }

        [MaxLength(200)]
        public string? DriverName { get; set; }

        [MaxLength(100)]
        public string? Area { get; set; }
    }

    public class UpdateOrderStatusCommandHandler : IRequestHandler<UpdateOrderStatusCommand, int>
    {
        private readonly ICommandRepository<Domain.Entities.DealerOrder> _orderCommandRepository;
        private readonly IQueryRepository<Domain.Entities.DealerOrder> _orderQueryRepository;
        private readonly ICommandRepository<Domain.Entities.DealerVehicle> _vehicleCommandRepository;
        private readonly IQueryRepository<Domain.Entities.DealerVehicle> _vehicleQueryRepository;
        private readonly ICommandRepository<Domain.Entities.DealerDriver> _driverCommandRepository;
        private readonly IQueryRepository<Domain.Entities.DealerDriver> _driverQueryRepository;
        private readonly ICommandRepository<Domain.Entities.DealerArea> _areaCommandRepository;
        private readonly IQueryRepository<Domain.Entities.DealerArea> _areaQueryRepository;
        private readonly IIdentityService _identityService;

        public UpdateOrderStatusCommandHandler(
            ICommandRepository<Domain.Entities.DealerOrder> orderCommandRepository,
            IQueryRepository<Domain.Entities.DealerOrder> orderQueryRepository,
            ICommandRepository<Domain.Entities.DealerVehicle> vehicleCommandRepository,
            IQueryRepository<Domain.Entities.DealerVehicle> vehicleQueryRepository,
            ICommandRepository<Domain.Entities.DealerDriver> driverCommandRepository,
            IQueryRepository<Domain.Entities.DealerDriver> driverQueryRepository,
            ICommandRepository<Domain.Entities.DealerArea> areaCommandRepository,
            IQueryRepository<Domain.Entities.DealerArea> areaQueryRepository,
            IIdentityService identityService)
        {
            _orderCommandRepository = orderCommandRepository;
            _orderQueryRepository = orderQueryRepository;
            _vehicleCommandRepository = vehicleCommandRepository;
            _vehicleQueryRepository = vehicleQueryRepository;
            _driverCommandRepository = driverCommandRepository;
            _driverQueryRepository = driverQueryRepository;
            _areaCommandRepository = areaCommandRepository;
            _areaQueryRepository = areaQueryRepository;
            _identityService = identityService;
        }

        public async Task<int> Handle(UpdateOrderStatusCommand request, CancellationToken cancellationToken)
        {
            // Find DealerOrder by Ln_OrderNumber
            var existingOrder = await _orderQueryRepository.GetQueryable()
                .FirstOrDefaultAsync(x => x.Ln_OrderNumber == request.Ln_OrderNumber && !x.IsDeleted && x.IsActive, cancellationToken);

            if (existingOrder == null)
                throw new KeyNotFoundException($"Dealer order with Ln_OrderNumber '{request.Ln_OrderNumber}' not found");

            var dealerId = existingOrder.DealerID;

            // Task 1: Vehicle Mapping
            if (!string.IsNullOrWhiteSpace(request.CarNumber))
            {
                var vehicle = await _vehicleQueryRepository.GetQueryable()
                    .FirstOrDefaultAsync(v => v.Ln_ID == request.CarNumber && v.DealerID == dealerId && !v.IsDeleted && v.IsActive, cancellationToken);

                int vehicleId;
                if (vehicle != null)
                {
                    vehicleId = vehicle.VehicleID;
                }
                else
                {
                    var defaultDate = DateTime.UtcNow.AddYears(1);
                    var newVehicle = new Domain.Entities.DealerVehicle
                    {
                        DealerID = dealerId,
                        PlateNumber = request.CarNumber,
                        Type = "truck",
                        Capacity = 1,
                        RegistrationDate = DateTime.UtcNow,
                        RegistrationExpiryDate = defaultDate,
                        InsuranceExpiryDate = defaultDate,
                        Ln_ID = request.CarNumber,
                        IsActive = true,
                        IsDeleted = false,
                        CreatedDate = DateTime.UtcNow
                    };
                    await _vehicleCommandRepository.AddAsync(newVehicle);
                    vehicleId = newVehicle.VehicleID;
                }

                existingOrder.VehicleID = vehicleId;
            }

            // Task 2: Transporter Update
            if (!string.IsNullOrWhiteSpace(request.TransporterName))
            {
                existingOrder.TransporterName = request.TransporterName;
            }

            // Task 3: Driver Handling
            if (!string.IsNullOrWhiteSpace(request.IqamaNumber))
            {
                var driver = await _driverQueryRepository.GetQueryable()
                    .FirstOrDefaultAsync(d => d.IqamaNumber == request.IqamaNumber && d.DealerID == dealerId && !d.IsDeleted && d.IsActive, cancellationToken);

                int driverId;
                if (driver != null)
                {
                    driverId = driver.DriverID;
                }
                else
                {
                    var userName = request.IqamaNumber;
                    var fullName = request.DriverName ?? request.IqamaNumber;
                    var email = $"{request.IqamaNumber}@ucic.com";
                    var phone = "0000000000"; // Placeholder - not provided in payload
                    var password = $"Ucic@{request.IqamaNumber}!"; // Default password for API-created drivers

                    var userResult = await _identityService.CreateUserAsync(
                        userName,
                        password,
                        email,
                        fullName,
                        phone,
                        "DealerDriver");

                    if (!userResult.isSucceed)
                        throw new InvalidOperationException($"Failed to create driver user: {userResult.userId}");

                    try
                    {
                        var newDriver = new Domain.Entities.DealerDriver
                        {
                            UserId = userResult.userId,
                            DealerID = dealerId,
                            IqamaNumber = request.IqamaNumber,
                            Ln_ID = request.IqamaNumber,
                            IsActive = true,
                            IsDeleted = false,
                            CreatedDate = DateTime.UtcNow
                        };
                        await _driverCommandRepository.AddAsync(newDriver);
                        driverId = newDriver.DriverID;
                    }
                    catch
                    {
                        await _identityService.DeleteUserAsync(userResult.userId);
                        throw;
                    }
                }

                existingOrder.DriverID = driverId;
            }

            // Task 4: Area Handling
            if (!string.IsNullOrWhiteSpace(request.Area))
            {
                var area = await _areaQueryRepository.GetQueryable()
                    .FirstOrDefaultAsync(a => (a.AreaName == request.Area || a.AreaCode == request.Area) && !a.IsDeleted && a.IsActive, cancellationToken);

                int areaId;
                if (area != null)
                {
                    areaId = area.AreaID;
                }
                else
                {
                    var areaCode = request.Area.Length > 50 ? request.Area[..50] : request.Area;
                    var newArea = new Domain.Entities.DealerArea
                    {
                        AreaName = request.Area.Length > 100 ? request.Area[..100] : request.Area,
                        AreaCode = areaCode,
                        IsActive = true,
                        IsDeleted = false,
                        CreatedDate = DateTime.UtcNow
                    };
                    await _areaCommandRepository.AddAsync(newArea);
                    areaId = newArea.AreaID;
                }

                existingOrder.AreaID = areaId;
            }

            // Update status
            existingOrder.Status = request.Status.ToLower() switch
            {
                "so-free" => "Pending",
                "so-approved" => "Approved",
                "so-inprocess" => "In Process",
                "so-modified" => "Modified",
                "so-closed" => "Closed",
                "so-canceled" => "Canceled",
                "so-blocked" => "Blocked",
                "wh-tobepicked" => "Loading",
                "wh-picked" => "Loading",
                "pdn-released" => "Loading",
                "wh-confirmed" => "Delivered",
                "so-invoiced" => "Invoiced",
                _ => existingOrder.Status ?? request.Status
            };
            existingOrder.ModifiedDate = DateTime.UtcNow;

            await _orderCommandRepository.UpdateAsync(existingOrder);
            return 1;
        }
    }
}
