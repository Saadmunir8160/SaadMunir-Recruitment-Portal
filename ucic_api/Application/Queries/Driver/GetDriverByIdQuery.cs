using Domain.Repositories.Query.Base;
using MediatR;

namespace Application.Queries.Driver
{
    public class GetDriverByIdQuery : IRequest<Domain.Entities.Driver>
    {
        public int Id { get; set; }
    }

    public class GetDriverByIdQueryHandler : IRequestHandler<GetDriverByIdQuery, Domain.Entities.Driver>
    {
        private readonly IQueryRepository<Domain.Entities.Driver> _queryRepository;

        public GetDriverByIdQueryHandler(IQueryRepository<Domain.Entities.Driver> queryRepository)
        {
            _queryRepository = queryRepository;
        }

        public async Task<Domain.Entities.Driver> Handle(GetDriverByIdQuery request, CancellationToken cancellationToken)
        {
            return await _queryRepository.GetByIdAsync(request.Id);
        }
    }
} 