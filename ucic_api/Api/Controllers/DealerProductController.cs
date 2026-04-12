using Application.Commands.DealerProduct.Create;
using Application.Commands.DealerProduct.Delete;
using Application.Commands.DealerProduct.Update;
using Application.Queries.DealerProduct;
using Application.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Route("api/Dealer/Products")]
    [ApiController]
    [Authorize(Roles = "Admin,Dealer")]
    public class DealerProductController : Controller
    {
        private readonly IMediator _mediator;

        public DealerProductController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("Create")]
        [Authorize(Roles = "Admin,Dealer")]
        public async Task<ActionResult> CreateDealerProduct([FromBody] CreateDealerProductCommand command)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            return Ok(await _mediator.Send(command));
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Dealer")]
        public async Task<ActionResult> GetProducts([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, [FromQuery] string? search = null)
        {
            var query = new GetAllDealerProductsQuery
            {
                PageNumber = pageNumber,
                PageSize = pageSize
            };
            return Ok(await _mediator.Send(query));
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Dealer")]
        public async Task<ActionResult> GetProductById(int id)
        {
            return Ok(await _mediator.Send(new GetDealerProductByIdQuery { Id = id }));
        }

        [HttpPut("Update/{id}")]
        public async Task<ActionResult> UpdateDealerProduct(int id, [FromBody] UpdateDealerProductCommand command)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            command.Id = id;
            return Ok(await _mediator.Send(command));
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeleteDealerProduct(int id)
        {
            return Ok(await _mediator.Send(new DeleteDealerProductCommand { Id = id }));
        }

        /// <summary>
        /// Get available product categories
        /// </summary>
        [HttpGet("categories")]
        [Authorize(Roles = "Admin,Dealer")]
        public async Task<ActionResult> GetCategories()
        {
            var result = await _mediator.Send(new GetProductCategoriesQuery());
            return Ok(result);
        }

        /// <summary>
        /// Check product availability
        /// </summary>
        [HttpPost("{id}/check-availability")]
        [Authorize(Roles = "Admin,Dealer")]
        public async Task<ActionResult> CheckProductAvailability(int id, [FromBody] CheckProductAvailabilityRequest request)
        {
            var query = new CheckProductAvailabilityQuery 
            { 
                ProductId = id, 
                RequestedQuantity = request.Quantity 
            };
            
            var result = await _mediator.Send(query);
            return Ok(result);
        }
    }
} 