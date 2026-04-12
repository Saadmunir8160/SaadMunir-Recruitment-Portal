using Application.Common.Extensions;
using Application.DTOs;
using Domain.Repositories.Query.Base;
using MediatR;

namespace Application.Queries.Country
{
    public class GetAllCountriesQuery : IRequest<PaginatedResponse<CountryDto>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class GetAllCountriesQueryHandler : IRequestHandler<GetAllCountriesQuery, PaginatedResponse<CountryDto>>
    {
        private readonly IQueryRepository<Domain.Entities.Country> _queryRepository;

        public GetAllCountriesQueryHandler(IQueryRepository<Domain.Entities.Country> queryRepository)
        {
            _queryRepository = queryRepository;
        }

        public async Task<PaginatedResponse<CountryDto>> Handle(GetAllCountriesQuery request, CancellationToken cancellationToken)
        {
            var parameters = new PaginationParameters
            {
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };

            var query = _queryRepository.GetQueryable();
            if (query == null || !query.Any())
                throw new KeyNotFoundException("Country not found");

            var countriesQuery = query.Select(country => new CountryDto
            {
                CountryId = country.CountryId,
                CountryName = country.CountryCode + "-" + country.CountryName
            });

            return await countriesQuery.ToPaginatedResponseAsync(parameters);
        }
    }
}
