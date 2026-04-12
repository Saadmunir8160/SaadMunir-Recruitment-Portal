using Domain.Repositories.Query.Base;
using MediatR;

namespace Application.Queries.Transporter
{
    public class GetTransporterByIdQuery : IRequest<Domain.Entities.Transporter>
    {
        public int Id { get; set; }
    }

    public class GetTransporterByIdQueryHandler : IRequestHandler<GetTransporterByIdQuery, Domain.Entities.Transporter>
    {
        private readonly IQueryRepository<Domain.Entities.Transporter> _queryRepository;

        public GetTransporterByIdQueryHandler(IQueryRepository<Domain.Entities.Transporter> queryRepository)
        {
            _queryRepository = queryRepository;
        }

        public async Task<Domain.Entities.Transporter> Handle(GetTransporterByIdQuery request, CancellationToken cancellationToken)
        {
            return await _queryRepository.GetByIdAsync(request.Id);
        }
    }
} 