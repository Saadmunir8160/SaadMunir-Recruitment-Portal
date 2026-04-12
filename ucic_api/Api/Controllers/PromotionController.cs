using Application.Commands.Promotion.Create;
using Application.Commands.Promotion.Delete;
using Application.Commands.Promotion.Update;
using Application.Queries.Promotion;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PromotionController : Controller
    {
        private readonly IMediator _mediator;
        public PromotionController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("GetAll")]
        public async Task<ActionResult> GetAllPromotions([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var query = new GetAllPromotionQuery
            {
                PageNumber = pageNumber,
                PageSize = pageSize
            };
            return Ok(await _mediator.Send(query));
        }

        [HttpPost("Create")]
        public async Task<ActionResult> CreatePromotion([FromBody] CreatePromotionCommand command)
        {
            return Ok(await _mediator.Send(command));
        }

        [HttpPut("Update")]
        public async Task<ActionResult> UpdatePromotion([FromBody] UpdatePromotionCommand command)
        {
            return Ok(await _mediator.Send(command));
        }

        [HttpDelete("Delete/{id}")]
        public async Task<ActionResult> DeletePromotion(long id)
        {
            return Ok(await _mediator.Send(new DeletePromotionCommand { PromotionId = id }));
        }
    }
} 