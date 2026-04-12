using Application.DTOs;
using Domain.Repositories.Query.Base;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Queries.Dealer
{
    public class GetDealerByIdQuery : IRequest<DealerDTO>
    {
        public int DealerId { get; set; }
    }

    public class GetDealerByIdQueryHandler : IRequestHandler<GetDealerByIdQuery, DealerDTO>
    {
        private readonly IQueryRepository<Domain.Entities.Dealer> _repository;
        public GetDealerByIdQueryHandler(IQueryRepository<Domain.Entities.Dealer> repository)
        {
            _repository = repository;
        }

        public async Task<DealerDTO> Handle(GetDealerByIdQuery request, CancellationToken cancellationToken)
        {
            var dealer = await _repository.GetByIdAsync(request.DealerId);

            return new DealerDTO()
            {
                DealerId = dealer.DealerId,
                UserId = dealer.UserId,
                DealerName = dealer.DealerName,
                CreditLimit = dealer.CreditLimit,
                CurrentBalance = dealer.CurrentBalance,
                Ln_ID = dealer.Ln_ID,
                IsActive = dealer.IsActive
            };
        }
    }
}
