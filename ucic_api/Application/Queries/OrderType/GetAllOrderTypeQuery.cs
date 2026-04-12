using Application.DTOs;
using Domain.Repositories.Query.Base;
using MediatR;

namespace Application.Queries.OrderType
{
    public class GetAllOrderTypeQuery : IRequest<List<OrderTypeDTO>> { }

    public class GetAllOrderTypeQueryHandler : IRequestHandler<GetAllOrderTypeQuery, List<OrderTypeDTO>>
    {
        private readonly IQueryRepository<Domain.Entities.OrderType> _queryRepository;

        public GetAllOrderTypeQueryHandler(IQueryRepository<Domain.Entities.OrderType> queryRepository)
        {
            _queryRepository = queryRepository;
        }

        public async Task<List<OrderTypeDTO>> Handle(GetAllOrderTypeQuery request, CancellationToken cancellationToken)
        {
            var departments = await _queryRepository.GetAllAsync();
            if (departments == null)
                throw new KeyNotFoundException("Order Type not found");
            return departments.Select(dept => new OrderTypeDTO
            {
                OrderTypeId = dept.OrderTypeId,
                Name = dept.Name
            }).ToList();
        }
    }
}
