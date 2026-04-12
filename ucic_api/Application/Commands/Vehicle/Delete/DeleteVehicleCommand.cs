using Domain.Repositories.Command.Base;
using Domain.Repositories.Query.Base;
using MediatR;

namespace Application.Commands.Vehicle.Delete
{
    public class DeleteVehicleCommand : IRequest<bool>
    {
        public int VehicleId { get; set; }
    }

    public class DeleteVehicleCommandHandler : IRequestHandler<DeleteVehicleCommand, bool>
    {
        private readonly ICommandRepository<Domain.Entities.Vehicle> _commandRepository;
        private readonly IQueryRepository<Domain.Entities.Vehicle> _queryRepository;

        public DeleteVehicleCommandHandler(
            ICommandRepository<Domain.Entities.Vehicle> commandRepository,
            IQueryRepository<Domain.Entities.Vehicle> queryRepository)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
        }

        public async Task<bool> Handle(DeleteVehicleCommand request, CancellationToken cancellationToken)
        {
            var vehicle = await _queryRepository.GetByIdAsync(request.VehicleId);
            if (vehicle == null)
            {
                return false;
            }

            // Use soft delete (DeleteAsync now implements soft delete for BaseEntity)
            await _commandRepository.DeleteAsync(vehicle);

            return true;
        }
    }
} 