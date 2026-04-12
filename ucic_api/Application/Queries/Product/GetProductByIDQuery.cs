using Domain.Repositories.Query.Base;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Queries.Product
{
    public class GetProductByIDQuery : IRequest<Domain.Entities.Product>
    {
        public long productID { get; set; }
    }

    public class GetProductByIDQueryHandler : IRequestHandler<GetProductByIDQuery, Domain.Entities.Product>
    {
        private readonly IQueryRepository<Domain.Entities.Product> _queryRepository;

        public GetProductByIDQueryHandler(IQueryRepository<Domain.Entities.Product> queryRepository)
        {
            _queryRepository = queryRepository;
        }

        public async Task<Domain.Entities.Product> Handle(GetProductByIDQuery request, CancellationToken cancellationToken)
        {
            return await _queryRepository.GetByIdAsync(request.productID);
        }
    }
}
