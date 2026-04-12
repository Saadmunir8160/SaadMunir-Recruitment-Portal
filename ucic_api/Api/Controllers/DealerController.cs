using Application.Commands.DealerEntities;
using Application.Commands.Dealer.Update;
using Application.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Application.Queries.Dealer;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DealerController : Controller
    {
        private readonly IMediator _mediator;
        
        public DealerController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // NOTE: Dealer user creation is handled by DealerUserController
        // This controller focuses only on managing existing dealer entities

        [HttpGet("GetAll")]
        [Authorize(Roles = "Admin,Dealer")]
        public async Task<ActionResult> GetAllDealers(
            [FromQuery] int pageNumber = 1, 
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null,
            [FromQuery] bool? isActive = null)
        {
            var query = new GetAllDealersQuery 
            { 
                PageNumber = pageNumber, 
                PageSize = pageSize,
                SearchTerm = searchTerm,
                IsActive = isActive
            };
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpGet("GetById/{id}")]
        [Authorize(Roles = "Admin,Dealer")]
        public async Task<ActionResult> GetDealerById(int id)
        {
            var result = await _mediator.Send(new GetDealerByIdQuery { DealerId = id });
            return Ok(result);
        }

        [HttpPut("Update/{id}")]
        [Authorize(Roles = "Admin,Dealer")]
        public async Task<ActionResult> UpdateDealer(int id, [FromBody] UpdateDealerDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            
            dto.DealerId = id;
            var command = new UpdateDealerCommand { Dealer = dto };
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpDelete("Delete/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeleteDealer(int id)
        {
            var result = await _mediator.Send(new DeleteDealerCommand { DealerId = id });
            return Ok(result);
        }
    }
}
