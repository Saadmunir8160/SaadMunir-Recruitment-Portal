using Application.Commands.Driver.Create;
using Application.Commands.Driver.Delete;
using Application.Commands.Driver.Update;
using Application.Queries.Driver;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DriverController : Controller
    {
        private readonly IMediator _mediator;

        public DriverController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("Create")]
        //[Authorize(Roles = "Admin")]
        public async Task<ActionResult> CreateDriver([FromBody] CreateDriverCommand command)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            return Ok(await _mediator.Send(command));
        }

        [HttpGet("GetAll")]
        public async Task<ActionResult> GetAllDrivers([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var query = new GetAllDriversQuery
            {
                PageNumber = pageNumber,
                PageSize = pageSize
            };
            return Ok(await _mediator.Send(query));
        }

        [HttpGet("GetById/{id}")]
        public async Task<ActionResult> GetDriverById(int id)
        {
            return Ok(await _mediator.Send(new GetDriverByIdQuery { Id = id }));
        }

        [HttpPut("Update/{id}")]
        public async Task<ActionResult> UpdateDriver(int id, [FromBody] UpdateDriverCommand command)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            command.Id = id;
            return Ok(await _mediator.Send(command));
        }

        [HttpDelete("Delete/{id}")]
        public async Task<ActionResult> DeleteDriver(int id)
        {
            return Ok(await _mediator.Send(new DeleteDriverCommand { Id = id }));
        }
    }
} 