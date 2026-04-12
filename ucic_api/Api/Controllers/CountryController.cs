using Application.Queries.Country;
using Application.Queries.News;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CountryController : Controller
    {
        private readonly IMediator _mediator;
        public CountryController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("GetAll")]
        public async Task<ActionResult> GetAllCountries([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var query = new GetAllCountriesQuery
            {
                PageNumber = pageNumber,
                PageSize = pageSize
            };
            return Ok(await _mediator.Send(query));
        }

        [HttpGet("GetCityByCountry/{countryId}")]
        public async Task<ActionResult> GetCityByCountry(int countryId)
        {
            return Ok(await _mediator.Send(new GetCityByCountryIdQuery() { CountryId = countryId }));
        }

    }
}
