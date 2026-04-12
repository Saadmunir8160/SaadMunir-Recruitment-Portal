using Application.Commands.Vehicle.Create;
using Application.Commands.Vehicle.Delete;
using Application.Commands.Vehicle.Update;
using Application.Queries.Vehicle;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VehicleController : Controller
    {
        private readonly IMediator _mediator;

        public VehicleController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("Create")]
        //[Authorize(Roles = "Admin")]
        public async Task<ActionResult> CreateVehicle([FromBody] CreateVehicleCommand command)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            return Ok(await _mediator.Send(command));
        }

        [HttpGet("GetAll")]
        public async Task<ActionResult> GetAllVehicles([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var query = new GetAllVehiclesQuery
            {
                PageNumber = pageNumber,
                PageSize = pageSize
            };
            return Ok(await _mediator.Send(query));
        }

        [HttpGet("GetById/{vehicleId}")]
        public async Task<ActionResult> GetVehicleById(int vehicleId)
        {
            return Ok(await _mediator.Send(new GetVehicleByIdQuery { VehicleId = vehicleId }));
        }

        [HttpPut("Update/{vehicleId}")]
        public async Task<ActionResult> UpdateVehicle(int vehicleId, [FromBody] UpdateVehicleCommand command)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            command.VehicleId = vehicleId;
            return Ok(await _mediator.Send(command));
        }

        [HttpDelete("Delete/{vehicleId}")]
        public async Task<ActionResult> DeleteVehicle(int vehicleId)
        {
            return Ok(await _mediator.Send(new DeleteVehicleCommand { VehicleId = vehicleId }));
        }
    }
} 