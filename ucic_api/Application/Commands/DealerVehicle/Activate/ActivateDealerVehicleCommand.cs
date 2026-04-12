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
    public class ActivateDealerVehicleCommand : IRequest<Response<bool>>
    {
        public int VehicleID { get; set; }
    }

    public class ActivateDealerVehicleCommandHandler : IRequestHandler<ActivateDealerVehicleCommand, Response<bool>>
    {
        private readonly ICommandRepository<Domain.Entities.DealerVehicle> _commandRepository;
        private readonly IQueryRepository<Domain.Entities.DealerVehicle> _queryRepository;
        private readonly IIdentityService _identityService;

        public ActivateDealerVehicleCommandHandler(
            ICommandRepository<Domain.Entities.DealerVehicle> commandRepository,
            IQueryRepository<Domain.Entities.DealerVehicle> queryRepository,
            IIdentityService identityService)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _identityService = identityService;
        }

        public async Task<Response<bool>> Handle(ActivateDealerVehicleCommand request, CancellationToken cancellationToken)
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
                .Where(v => v.VehicleID == request.VehicleID && v.DealerID == currentDealerId.Value && !v.IsDeleted)
                .FirstOrDefaultAsync();

            if (entity == null)
                return new Response<bool> { Success = false, Message = "Dealer vehicle not found." };

            if (entity.IsActive)
                return new Response<bool> { Success = false, Message = "Dealer vehicle is already active." };

            entity.IsActive = true;
            entity.ModifiedDate = DateTime.UtcNow;
            entity.ModifiedBy = _identityService.GetCurrentUserId();

            await _commandRepository.UpdateAsync(entity);
            return new Response<bool> { Success = true, Data = true, Message = "Dealer vehicle activated successfully." };
        }
    }
}