using Application.Commands.DealerVehicle;
using Application.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Application.Queries.DealerVehicle;

namespace Api.Controllers
{
    [Route("api/dealer/vehicles")]
    [ApiController]
    public class DealerVehicleController : Controller
    {
        private readonly IMediator _mediator;
        
        public DealerVehicleController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("")]
        [Authorize(Roles = "Admin,Dealer")]
        public async Task<ActionResult> GetAllDealerVehicles([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var query = new GetAllDealerVehiclesQuery { PageNumber = pageNumber, PageSize = pageSize };
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Dealer")]
        public async Task<ActionResult> GetDealerVehicleById(int id)
        {
            var result = await _mediator.Send(new GetDealerVehicleByIdQuery { VehicleID = id });
            return Ok(result);
        }

        [HttpPost("")]
        [Authorize(Roles = "Admin,Dealer")]
        public async Task<ActionResult> CreateDealerVehicle([FromBody] CreateDealerVehicleDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
                
            var command = new CreateDealerVehicleCommand { DealerVehicle = dto };
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Dealer")]
        public async Task<ActionResult> UpdateDealerVehicle(int id, [FromBody] UpdateDealerVehicleDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
                
            dto.VehicleID = id;
            var command = new UpdateDealerVehicleCommand { DealerVehicle = dto };
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Dealer")]
        public async Task<ActionResult> DeleteDealerVehicle(int id)
        {
            var result = await _mediator.Send(new DeleteDealerVehicleCommand { VehicleID = id });
            return Ok(result);
        }

        [HttpPatch("{id}/activate")]
        [Authorize(Roles = "Admin,Dealer")]
        public async Task<ActionResult> ActivateDealerVehicle(int id)
        {
            var command = new ActivateDealerVehicleCommand { VehicleID = id };
            return Ok(await _mediator.Send(command));
        }

        [HttpPatch("{id}/deactivate")]
        [Authorize(Roles = "Admin,Dealer")]
        public async Task<ActionResult> DeactivateDealerVehicle(int id)
        {
            var command = new DeactivateDealerVehicleCommand { VehicleID = id };
            return Ok(await _mediator.Send(command));
        }
    }
}
