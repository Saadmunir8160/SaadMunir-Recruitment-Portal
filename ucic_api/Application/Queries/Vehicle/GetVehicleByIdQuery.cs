using Domain.Repositories.Query.Base;
using MediatR;

namespace Application.Queries.Vehicle
{
    public class GetVehicleByIdQuery : IRequest<Domain.Entities.Vehicle>
    {
        public int VehicleId { get; set; }
    }

    public class GetVehicleByIdQueryHandler : IRequestHandler<GetVehicleByIdQuery, Domain.Entities.Vehicle>
    {
        private readonly IQueryRepository<Domain.Entities.Vehicle> _queryRepository;

        public GetVehicleByIdQueryHandler(IQueryRepository<Domain.Entities.Vehicle> queryRepository)
        {
            _queryRepository = queryRepository;
        }

        public async Task<Domain.Entities.Vehicle> Handle(GetVehicleByIdQuery request, CancellationToken cancellationToken)
        {
            return await _queryRepository.GetByIdAsync(request.VehicleId);
        }
    }
} 