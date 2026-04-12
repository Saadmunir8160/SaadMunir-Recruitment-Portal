using Application.Commands.DealerEntities;
using Application.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Application.Queries.DealerEntities;

namespace Api.Controllers
{
    [Route("api/dealer/drivers")]
    [ApiController]
    public class DealerDriverController : Controller
    {
        private readonly IMediator _mediator;
        public DealerDriverController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("")]
        [Authorize(Roles = "Admin,Dealer")]
        public async Task<ActionResult> GetAllDealerDrivers([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var query = new GetAllDealerDriversQuery { PageNumber = pageNumber, PageSize = pageSize };
            return Ok(await _mediator.Send(query));
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Dealer,DealerDriver")]
        public async Task<ActionResult> GetDealerDriverById(int id)
        {
            return Ok(await _mediator.Send(new GetDealerDriverByIdQuery { DriverID = id }));
        }

        [HttpPost("")]
        [Authorize(Roles = "Admin,Dealer")]
        public async Task<ActionResult> CreateDealerDriver([FromBody] CreateDealerDriverDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
                
            var command = new CreateDealerDriverCommand { DealerDriver = dto };
            return Ok(await _mediator.Send(command));
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Dealer")]
        public async Task<ActionResult> UpdateDealerDriver(int id, [FromBody] UpdateDealerDriverDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            
            var command = new UpdateDealerDriverCommand { DealerDriver = dto };
            command.DealerDriver.DriverID = id;
            return Ok(await _mediator.Send(command));
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Dealer")]
        public async Task<ActionResult> DeleteDealerDriver(int id)
        {
            return Ok(await _mediator.Send(new DeleteDealerDriverCommand { DriverID = id }));
        }

        [HttpPatch("{id}/activate")]
        [Authorize(Roles = "Admin,Dealer")]
        public async Task<ActionResult> ActivateDealerDriver(int id)
        {
            var command = new ActivateDealerDriverCommand { DriverID = id };
            return Ok(await _mediator.Send(command));
        }

        [HttpPatch("{id}/deactivate")]
        [Authorize(Roles = "Admin,Dealer")]
        public async Task<ActionResult> DeactivateDealerDriver(int id)
        {
            var command = new DeactivateDealerDriverCommand { DriverID = id };
            return Ok(await _mediator.Send(command));
        }
    }
}
