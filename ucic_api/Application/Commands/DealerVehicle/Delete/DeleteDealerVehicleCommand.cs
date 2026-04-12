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
    public class DeleteDealerVehicleCommand : IRequest<Response<bool>>
    {
        public int VehicleID { get; set; }
    }

    public class DeleteDealerVehicleCommandHandler : IRequestHandler<DeleteDealerVehicleCommand, Response<bool>>
    {
        private readonly ICommandRepository<Domain.Entities.DealerVehicle> _commandRepository;
        private readonly IQueryRepository<Domain.Entities.DealerVehicle> _queryRepository;
        private readonly IIdentityService _identityService;

        public DeleteDealerVehicleCommandHandler(
            ICommandRepository<Domain.Entities.DealerVehicle> commandRepository, 
            IQueryRepository<Domain.Entities.DealerVehicle> queryRepository,
            IIdentityService identityService)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _identityService = identityService;
        }

        public async Task<Response<bool>> Handle(DeleteDealerVehicleCommand request, CancellationToken cancellationToken)
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
                .Where(v => v.VehicleID == request.VehicleID && v.DealerID == currentDealerId.Value)
                .FirstOrDefaultAsync();

            if (entity == null)
                return new Response<bool> { Success = false, Message = "Dealer vehicle not found." };

            // Hard delete - will fail if vehicle has orders due to RESTRICT constraint
            try
            {
                await _commandRepository.HardDeleteAsync(entity);
                return new Response<bool> { Success = true, Data = true, Message = "Dealer vehicle deleted successfully." };
            }
            catch (Exception ex)
            {
                return new Response<bool> 
                { 
                    Success = false, 
                    Message = "Cannot delete vehicle with existing orders. Please remove associations first." 
                };
            }
        }
    }
}
