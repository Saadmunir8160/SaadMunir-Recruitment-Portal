using Application.Common.Interfaces;
using Application.DTOs;
using Domain.Entities;
using Domain.Repositories.Command.Base;
using Domain.Repositories.Query.Base;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Commands.Admin.DealerVehicle
{
    public class UpdateDealerVehicleForAdminCommand : IRequest<Response<bool>>
    {
        public int Id { get; set; }
        public string VehicleName { get; set; } = string.Empty;
        public string LicensePlate { get; set; } = string.Empty;
        public string VehicleType { get; set; } = string.Empty;
        public decimal Capacity { get; set; }
        public bool IsActive { get; set; }
    }

    public class UpdateDealerVehicleForAdminCommandHandler : IRequestHandler<UpdateDealerVehicleForAdminCommand, Response<bool>>
    {
        private readonly ICommandRepository<Domain.Entities.DealerVehicle> _commandRepository;
        private readonly IQueryRepository<Domain.Entities.DealerVehicle> _queryRepository;
        private readonly IIdentityService _identityService;
        private readonly ILogger<UpdateDealerVehicleForAdminCommandHandler> _logger;

        public UpdateDealerVehicleForAdminCommandHandler(
            ICommandRepository<Domain.Entities.DealerVehicle> commandRepository,
            IQueryRepository<Domain.Entities.DealerVehicle> queryRepository,
            IIdentityService identityService,
            ILogger<UpdateDealerVehicleForAdminCommandHandler> logger)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _identityService = identityService;
            _logger = logger;
        }

        public async Task<Response<bool>> Handle(UpdateDealerVehicleForAdminCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var entity = await _queryRepository.GetByIdAsync(request.Id);
                if (entity == null)
                {
                    return new Response<bool>
                    {
                        Success = false,
                        Message = "Vehicle not found."
                    };
                }

                // Update vehicle properties
                // Note: VehicleName is computed from PlateNumber and Type, so we don't store it directly
                entity.PlateNumber = request.LicensePlate;
                entity.Type = request.VehicleType;
                entity.VehicleType = request.VehicleType; // Legacy field for backward compatibility
                entity.Capacity = request.Capacity;
                entity.IsActive = request.IsActive;
                entity.ModifiedDate = DateTime.UtcNow;
                entity.ModifiedBy = _identityService.GetCurrentUserId();

                await _commandRepository.UpdateAsync(entity);

                _logger.LogInformation("Admin updated vehicle {VehicleId} successfully", request.Id);

                return new Response<bool>
                {
                    Success = true,
                    Data = true,
                    Message = "Vehicle updated successfully."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating vehicle {VehicleId}", request.Id);
                return new Response<bool>
                {
                    Success = false,
                    Message = $"Error updating vehicle: {ex.Message}"
                };
            }
        }
    }
}
