using Application.Commands.Transporter.Create;
using Application.Commands.Transporter.Delete;
using Application.Commands.Transporter.Update;
using Application.Queries.Transporter;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TransporterController : Controller
    {
        private readonly IMediator _mediator;

        public TransporterController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("Create")]
        //[Authorize(Roles = "Admin")]
        public async Task<ActionResult> CreateTransporter([FromBody] CreateTransporterCommand command)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            return Ok(await _mediator.Send(command));
        }

        [HttpGet("GetAll")]
        public async Task<ActionResult> GetAllTransporters([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var query = new GetAllTransportersQuery
            {
                PageNumber = pageNumber,
                PageSize = pageSize
            };
            return Ok(await _mediator.Send(query));
        }

        [HttpGet("GetById/{id}")]
        public async Task<ActionResult> GetTransporterById(int id)
        {
            return Ok(await _mediator.Send(new GetTransporterByIdQuery { Id = id }));
        }

        [HttpPut("Update/{id}")]
        public async Task<ActionResult> UpdateTransporter(int id, [FromBody] UpdateTransporterCommand command)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            command.Id = id;
            return Ok(await _mediator.Send(command));
        }

        [HttpDelete("Delete/{id}")]
        public async Task<ActionResult> DeleteTransporter(int id)
        {
            return Ok(await _mediator.Send(new DeleteTransporterCommand { Id = id }));
        }
    }
} 