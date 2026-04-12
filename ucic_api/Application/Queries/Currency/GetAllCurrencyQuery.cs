using Application.Common.Extensions;
using Application.DTOs;
using Domain.Repositories.Query.Base;
using MediatR;

namespace Application.Queries.Currency
{
    public class GetAllCurrencyQuery : IRequest<PaginatedResponse<CurrencyDto>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class GetAllCurrencyQueryHandler : IRequestHandler<GetAllCurrencyQuery, PaginatedResponse<CurrencyDto>>
    {
        private readonly IQueryRepository<Domain.Entities.Currency> _queryRepository;

        public GetAllCurrencyQueryHandler(IQueryRepository<Domain.Entities.Currency> queryRepository)
        {
            _queryRepository = queryRepository;
        }

        public async Task<PaginatedResponse<CurrencyDto>> Handle(GetAllCurrencyQuery request, CancellationToken cancellationToken)
        {
            var parameters = new PaginationParameters
            {
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };

            var query = _queryRepository.GetQueryable();
            if (query == null || !query.Any())
                throw new KeyNotFoundException("Currency not found");

            var currenciesQuery = query.Select(currency => new CurrencyDto
            {
                CurrencyId = currency.CurrencyId,
                CurrencyCode = currency.CurrencyCode!
            });

            return await currenciesQuery.ToPaginatedResponseAsync(parameters);
        }
    }
}
