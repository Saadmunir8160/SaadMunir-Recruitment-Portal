using Application.DTOs;
using Application.Common.Interfaces;
using Domain.Entities;
using Domain.Repositories.Command.Base;
using Domain.Repositories.Query.Base;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Commands.DealerVehicle
{
    public class UpdateDealerVehicleCommand : IRequest<Response<bool>>
    {
        public required UpdateDealerVehicleDTO DealerVehicle { get; set; }
    }

    public class UpdateDealerVehicleCommandHandler : IRequestHandler<UpdateDealerVehicleCommand, Response<bool>>
    {
        private readonly ICommandRepository<Domain.Entities.DealerVehicle> _commandRepository;
        private readonly IQueryRepository<Domain.Entities.DealerVehicle> _queryRepository;
        private readonly IIdentityService _identityService;

        public UpdateDealerVehicleCommandHandler(
            ICommandRepository<Domain.Entities.DealerVehicle> commandRepository, 
            IQueryRepository<Domain.Entities.DealerVehicle> queryRepository,
            IIdentityService identityService)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _identityService = identityService;
        }

        public async Task<Response<bool>> Handle(UpdateDealerVehicleCommand request, CancellationToken cancellationToken)
        {
            // Get current dealer ID
            var currentDealerId = await _identityService.GetCurrentDealerIdAsync();
            if (currentDealerId == null)
            {
                return new Response<bool>
                {
                    Success = false,
                    Message = "Dealer not found for current user"
                };
            }

            // Find vehicle that belongs to current dealer
            var entity = await _queryRepository.GetQueryable()
                .Where(v => v.VehicleID == request.DealerVehicle.VehicleID && v.DealerID == currentDealerId.Value)
                .FirstOrDefaultAsync();

            if (entity == null)
                return new Response<bool> { Success = false, Message = "Dealer vehicle not found." };

            // Update vehicle properties (except DealerID which should remain current dealer)
            entity.PlateNumber = request.DealerVehicle.PlateNumber;
            entity.Type = request.DealerVehicle.Type;
            entity.Capacity = request.DealerVehicle.Capacity;
            entity.RegistrationDate = request.DealerVehicle.RegistrationDate;
            entity.RegistrationExpiryDate = request.DealerVehicle.RegistrationExpiryDate;
            entity.InsuranceExpiryDate = request.DealerVehicle.InsuranceExpiryDate;
            entity.Ln_ID = request.DealerVehicle.Ln_ID;
            entity.RegistrationNumber = request.DealerVehicle.RegistrationNumber;
            entity.VehicleType = request.DealerVehicle.Type; // Map to legacy field for backward compatibility
            entity.IsActive = request.DealerVehicle.IsActive;
            entity.ModifiedDate = DateTime.UtcNow;
            entity.ModifiedBy = _identityService.GetCurrentUserId();

            await _commandRepository.UpdateAsync(entity);
            return new Response<bool> { Success = true, Data = true, Message = "Dealer vehicle updated successfully." };
        }
    }
}
