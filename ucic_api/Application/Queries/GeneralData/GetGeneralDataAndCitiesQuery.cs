using Application.Common.Exceptions;
using Application.DTOs;
using Domain.Repositories.Query.Base;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Queries.GeneralData
{
    public class GetGeneralDataAndCitiesQuery : IRequest<GeneralSettingAndCitiesDTO>
    {
        public long CoverageAreaId { get; set; }
    }

    public class GetGeneralDataAndCitiesQueryHandler : IRequestHandler<GetGeneralDataAndCitiesQuery, GeneralSettingAndCitiesDTO>
    {
        private readonly IQueryRepository<Domain.Entities.GeneralSettings> _queryRepository;
        private readonly IQueryRepository<Domain.Entities.Cities> _queryRepositoryCities;

        public GetGeneralDataAndCitiesQueryHandler(IQueryRepository<Domain.Entities.GeneralSettings> queryRepository, IQueryRepository<Domain.Entities.Cities> queryRepositoryCities)
        {
            _queryRepository = queryRepository;
            _queryRepositoryCities = queryRepositoryCities;
        }

        public async Task<GeneralSettingAndCitiesDTO> Handle(GetGeneralDataAndCitiesQuery request, CancellationToken cancellationToken)
        {
            //var filters = new Dictionary<string, object>
            //  {
            //      { nameof(Domain.Entities.GeneralSettings.SettingsKey), new[] { "VATPercentage", "Country" } },
            //      //{ nameof(Domain.Entities.GeneralSettings.SettingsKey), "Country" }
            //  };
            var result = await _queryRepository.GetAllAsync();

            var filtersCities = new Dictionary<string, object>
              {
                  { nameof(Domain.Entities.Cities.CoverageAreaId), request.CoverageAreaId }
              };
            var resultCities = await _queryRepositoryCities.GetByColumnsAsync(filtersCities);

            string vatPercentage = result.FirstOrDefault(r => r.SettingsKey == "VATPercentage")?.SettingsValue ?? "15";
            string country = result.FirstOrDefault(r => r.SettingsKey == "Country")?.SettingsValue ?? "Saudi Arabia";

            return new GeneralSettingAndCitiesDTO()
            {
                VatPercentage = vatPercentage,
                Country = country,
                cities = resultCities.Select(city => new CitiesDTO
                {
                    CitiesId = city.CitiesId,
                    CityName = city.CityName,
                }).ToList()
            };
        }
    }
}
