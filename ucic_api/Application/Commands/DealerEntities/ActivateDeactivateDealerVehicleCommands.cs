using Application.DTOs;
using Domain.Repositories.Command.Base;
using Domain.Repositories.Query.Base;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Commands.DealerEntities
{
    public class ActivateDealerVehicleCommand : IRequest<Response<int>>
    {
        public int VehicleID { get; set; }
    }

    public class ActivateDealerVehicleCommandHandler : IRequestHandler<ActivateDealerVehicleCommand, Response<int>>
    {
        private readonly ICommandRepository<Domain.Entities.DealerVehicle> _commandRepository;
        private readonly IQueryRepository<Domain.Entities.DealerVehicle> _queryRepository;

        public ActivateDealerVehicleCommandHandler(
            ICommandRepository<Domain.Entities.DealerVehicle> commandRepository,
            IQueryRepository<Domain.Entities.DealerVehicle> queryRepository)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
        }

        public async Task<Response<int>> Handle(ActivateDealerVehicleCommand request, CancellationToken cancellationToken)
        {
            var vehicle = await _queryRepository.GetByIdAsync(request.VehicleID);
            if (vehicle == null)
            {
                return new Response<int> { Success = false, Message = "Vehicle not found." };
            }

            await _commandRepository.UpdateColumnAsync(request.VehicleID, "IsActive", true);
            return new Response<int> { Success = true, Data = request.VehicleID, Message = "Vehicle activated successfully." };
        }
    }

    public class DeactivateDealerVehicleCommand : IRequest<Response<int>>
    {
        public int VehicleID { get; set; }
    }

    public class DeactivateDealerVehicleCommandHandler : IRequestHandler<DeactivateDealerVehicleCommand, Response<int>>
    {
        private readonly ICommandRepository<Domain.Entities.DealerVehicle> _commandRepository;
        private readonly IQueryRepository<Domain.Entities.DealerVehicle> _queryRepository;

        public DeactivateDealerVehicleCommandHandler(
            ICommandRepository<Domain.Entities.DealerVehicle> commandRepository,
            IQueryRepository<Domain.Entities.DealerVehicle> queryRepository)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
        }

        public async Task<Response<int>> Handle(DeactivateDealerVehicleCommand request, CancellationToken cancellationToken)
        {
            var vehicle = await _queryRepository.GetByIdAsync(request.VehicleID);
            if (vehicle == null)
            {
                return new Response<int> { Success = false, Message = "Vehicle not found." };
            }

            await _commandRepository.UpdateColumnAsync(request.VehicleID, "IsActive", false);
            return new Response<int> { Success = true, Data = request.VehicleID, Message = "Vehicle deactivated successfully." };
        }
    }
}