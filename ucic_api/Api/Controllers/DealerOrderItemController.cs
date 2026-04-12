using Application.Commands.DealerEntities.Create;
using Application.Commands.DealerEntities;
using Application.DTOs.DealerOrder;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Application.Queries.DealerEntities;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DealerOrderItemController : ControllerBase
    {
        private readonly IMediator _mediator;
        
        public DealerOrderItemController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("Create")]
        public async Task<ActionResult> CreateDealerOrderItem([FromBody] CreateDealerOrderItemDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
                
            var command = new CreateDealerOrderItemCommand { DealerOrderItem = dto };
            return Ok(await _mediator.Send(command));
        }

        [HttpGet("GetAll")]
        public async Task<ActionResult> GetAllDealerOrderItems([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var query = new GetAllDealerOrderItemsQuery { PageNumber = pageNumber, PageSize = pageSize };
            return Ok(await _mediator.Send(query));
        }

        [HttpGet("GetById/{id}")]
        public async Task<ActionResult> GetDealerOrderItemById(int id)
        {
            var query = new GetDealerOrderItemByIdQuery { OrderItemID = id };
            return Ok(await _mediator.Send(query));
        }

        [HttpPut("Update/{id}")]
        public async Task<ActionResult> UpdateDealerOrderItem(int id, [FromBody] UpdateDealerOrderItemDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
                
            dto.OrderItemID = id;
            var command = new UpdateDealerOrderItemCommand { DealerOrderItem = dto };
            return Ok(await _mediator.Send(command));
        }

        [HttpDelete("Delete/{id}")]
        public async Task<ActionResult> DeleteDealerOrderItem(int id)
        {
            var command = new DeleteDealerOrderItemCommand { OrderItemID = id };
            return Ok(await _mediator.Send(command));
        }
    }
}
