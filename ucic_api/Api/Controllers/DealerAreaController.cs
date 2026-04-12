using Application.Commands.DealerArea;
using Application.DTOs;
using Application.Queries.DealerArea;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DealerAreaController : Controller
    {
        private readonly IMediator _mediator;
        
        public DealerAreaController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("Create")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> CreateDealerArea([FromBody] CreateDealerAreaCommand command)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
                
            return Ok(await _mediator.Send(command));
        }

        [HttpGet("GetAll")]
        [AllowAnonymous]
        public async Task<ActionResult> GetAllDealerAreas([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var query = new GetAllDealerAreasQuery { PageNumber = pageNumber, PageSize = pageSize };
            return Ok(await _mediator.Send(query));
        }

        [HttpGet("GetById/{id}")]
        [AllowAnonymous]
        public async Task<ActionResult> GetDealerAreaById(int id)
        {
            return Ok(await _mediator.Send(new GetDealerAreaByIdQuery { AreaID = id }));
        }

        [HttpPut("Update/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> UpdateDealerArea(int id, [FromBody] UpdateDealerAreaCommand command)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
                
            command.AreaID = id;
            return Ok(await _mediator.Send(command));
        }

        [HttpDelete("Delete/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeleteDealerArea(int id)
        {
            return Ok(await _mediator.Send(new DeleteDealerAreaCommand { AreaID = id }));
        }
    }
}
