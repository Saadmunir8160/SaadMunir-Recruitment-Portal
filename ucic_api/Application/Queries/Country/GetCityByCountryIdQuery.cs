using Application.DTOs;
using Domain.Repositories.Query.Base;
using MediatR;

namespace Application.Queries.Country
{
    public class GetCityByCountryIdQuery : IRequest<List<CityDTO>>
    {
        public int CountryId { get; set; }
    }

    public class GetCityByCountryIdQueryHandler : IRequestHandler<GetCityByCountryIdQuery, List<CityDTO>>
    {
        private readonly IQueryRepository<Domain.Entities.CitiesByCountry> _queryRepository;

        public GetCityByCountryIdQueryHandler(IQueryRepository<Domain.Entities.CitiesByCountry> queryRepository)
        {
            _queryRepository = queryRepository;
        }

        public async Task<List<CityDTO>> Handle(GetCityByCountryIdQuery request, CancellationToken cancellationToken)
        {
            var allCities = await _queryRepository.GetAllAsync();
            if (allCities == null)
                throw new KeyNotFoundException("Cities not found");


            var filteredCities = allCities
                .Where(x => x.CountryId == request.CountryId && x.IsActive)
                .Select(city => new CityDTO
                {
                    CityId = city.CityId,
                    Name = city.CityName
                })
                .ToList();


            return filteredCities;

        }
    }

}
