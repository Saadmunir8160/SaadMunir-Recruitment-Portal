using Application.Commands.CoverageArea.Create;
using Application.Commands.CoverageArea.Update;
using Application.Commands.CoverageArea.Delete;
using Application.Queries.CoverageArea;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CoverageAreaController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CoverageAreaController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("Create")]
        public async Task<ActionResult> CreateCoverageArea(CreateCoverageAreaCommand command)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            return Ok(await _mediator.Send(command));
        }

        [HttpPut("Update")]
        public async Task<ActionResult> UpdateCoverageArea(UpdateCoverageAreaCommand command)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            return Ok(await _mediator.Send(command));
        }

        [HttpDelete("Delete/{id}")]
        public async Task<ActionResult> DeleteCoverageArea(long id)
        {
            var command = new DeleteCoverageAreaCommand { CoverageAreaId = id };
            return Ok(await _mediator.Send(command));
        }

        [HttpGet("GetAll")]
        public async Task<ActionResult> GetAllCoverageAreas([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var query = new GetAllCoverageAreasQuery
            {
                PageNumber = pageNumber,
                PageSize = pageSize
            };
            return Ok(await _mediator.Send(query));
        }
    }
}
