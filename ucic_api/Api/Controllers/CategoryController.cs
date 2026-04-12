using Application.Queries.Category;
using Application.Queries.OrderType;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : Controller
    {
        private readonly IMediator _mediator;
        public CategoryController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("GetAll")]
        public async Task<ActionResult> GetAllCategories([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var query = new GetAllCategoriesQuery
            {
                PageNumber = pageNumber,
                PageSize = pageSize
            };
            return Ok(await _mediator.Send(query));
        }

        [HttpGet("GetAllOrderType")]
        public async Task<ActionResult> GetAllOrderType()
        {
            return Ok(await _mediator.Send(new GetAllOrderTypeQuery()));
        }
    }
}
