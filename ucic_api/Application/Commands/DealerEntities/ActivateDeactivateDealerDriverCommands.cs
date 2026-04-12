using Application.DTOs;
using Domain.Repositories.Command.Base;
using Domain.Repositories.Query.Base;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Commands.DealerEntities
{
    public class ActivateDealerDriverCommand : IRequest<Response<int>>
    {
        public int DriverID { get; set; }
    }

    public class ActivateDealerDriverCommandHandler : IRequestHandler<ActivateDealerDriverCommand, Response<int>>
    {
        private readonly ICommandRepository<Domain.Entities.DealerDriver> _commandRepository;
        private readonly IQueryRepository<Domain.Entities.DealerDriver> _queryRepository;

        public ActivateDealerDriverCommandHandler(
            ICommandRepository<Domain.Entities.DealerDriver> commandRepository,
            IQueryRepository<Domain.Entities.DealerDriver> queryRepository)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
        }

        public async Task<Response<int>> Handle(ActivateDealerDriverCommand request, CancellationToken cancellationToken)
        {
            var driver = await _queryRepository.GetByIdAsync(request.DriverID);
            if (driver == null)
            {
                return new Response<int> { Success = false, Message = "Driver not found." };
            }

            await _commandRepository.UpdateColumnAsync(request.DriverID, "IsActive", true);
            return new Response<int> { Success = true, Data = request.DriverID, Message = "Driver activated successfully." };
        }
    }

    public class DeactivateDealerDriverCommand : IRequest<Response<int>>
    {
        public int DriverID { get; set; }
    }

    public class DeactivateDealerDriverCommandHandler : IRequestHandler<DeactivateDealerDriverCommand, Response<int>>
    {
        private readonly ICommandRepository<Domain.Entities.DealerDriver> _commandRepository;
        private readonly IQueryRepository<Domain.Entities.DealerDriver> _queryRepository;

        public DeactivateDealerDriverCommandHandler(
            ICommandRepository<Domain.Entities.DealerDriver> commandRepository,
            IQueryRepository<Domain.Entities.DealerDriver> queryRepository)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
        }

        public async Task<Response<int>> Handle(DeactivateDealerDriverCommand request, CancellationToken cancellationToken)
        {
            var driver = await _queryRepository.GetByIdAsync(request.DriverID);
            if (driver == null)
            {
                return new Response<int> { Success = false, Message = "Driver not found." };
            }

            await _commandRepository.UpdateColumnAsync(request.DriverID, "IsActive", false);
            return new Response<int> { Success = true, Data = request.DriverID, Message = "Driver deactivated successfully." };
        }
    }
}