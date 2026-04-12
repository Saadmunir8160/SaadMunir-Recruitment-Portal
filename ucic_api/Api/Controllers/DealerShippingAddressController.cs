using Application.Commands.DealerEntities;
using Application.Commands.DealerShippingAddress;
using Application.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Application.Queries.DealerEntities;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DealerShippingAddressController : Controller
    {
        private readonly IMediator _mediator;
        
        public DealerShippingAddressController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("Create")]
        public async Task<ActionResult> CreateDealerShippingAddress([FromBody] CreateDealerShippingAddressDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
                
            var command = new CreateDealerShippingAddressCommand { DealerShippingAddress = dto };
            return Ok(await _mediator.Send(command));
        }

        [HttpGet("GetAll")]
        public async Task<ActionResult> GetAllDealerShippingAddresses([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var query = new GetAllDealerShippingAddressesQuery { PageNumber = pageNumber, PageSize = pageSize };
            return Ok(await _mediator.Send(query));
        }

        [HttpGet("GetMyAddresses")]
        [Authorize(Roles = "Dealer")]
        public async Task<ActionResult> GetMyDealerShippingAddresses([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var query = new GetMyDealerShippingAddressesQuery { PageNumber = pageNumber, PageSize = pageSize };
            return Ok(await _mediator.Send(query));
        }

        [HttpGet("GetById/{id}")]
        public async Task<ActionResult> GetDealerShippingAddressById(int id)
        {
            return Ok(await _mediator.Send(new GetDealerShippingAddressByIdQuery { AddressID = id }));
        }

        [HttpPut("Update/{id}")]
        public async Task<ActionResult> UpdateDealerShippingAddress(int id, [FromBody] UpdateDealerShippingAddressDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
                
            dto.AddressID = id;
            var command = new UpdateDealerShippingAddressCommand { DealerShippingAddress = dto };
            return Ok(await _mediator.Send(command));
        }

        [HttpDelete("Delete/{id}")]
        public async Task<ActionResult> DeleteDealerShippingAddress(int id)
        {
            return Ok(await _mediator.Send(new DeleteDealerShippingAddressCommand { AddressID = id }));
        }
    }
}
