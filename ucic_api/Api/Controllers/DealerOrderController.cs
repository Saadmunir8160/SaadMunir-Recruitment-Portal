using Application.Commands.DealerOrder.Create;
using Application.Commands.DealerOrder.Delete;
using Application.Commands.DealerOrder.Resend;
using Application.Queries.DealerOrder;
using Application.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DealerOrderController : Controller
    {
        private readonly IMediator _mediator;

        public DealerOrderController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("Create")]
        [Authorize(Roles = "Admin, Dealer")]
        public async Task<ActionResult> CreateDealerOrder([FromBody] CreateDealerOrderCommand command)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            else if (User.Identity?.IsAuthenticated == false)
                return Unauthorized();

            return Ok(await _mediator.Send(command));
        }

        [HttpGet("GetAll")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> GetAllDealerOrders([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var query = new GetAllDealerOrdersQuery
            {
                PageNumber = pageNumber,
                PageSize = pageSize
            };
            return Ok(await _mediator.Send(query));
        }

        [HttpGet("GetById/{id}")]
        [Authorize(Roles = "Admin, Dealer")]
        public async Task<ActionResult> GetDealerOrderById(int id)
        {
            return Ok(await _mediator.Send(new GetDealerOrderByIdQuery { DealerOrderId = id }));
        }

        [HttpGet("GetMyOrders")]
        [Authorize(Roles = "Admin, Dealer")]
        public async Task<ActionResult> GetMyDealerOrders()
        {
            return Ok(await _mediator.Send(new GetMyDealerOrdersQuery()));
        }

        [HttpDelete("Delete/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeleteDealerOrder(int id)
        {
            return Ok(await _mediator.Send(new DeleteDealerOrderCommand { DealerOrderId = id }));
        }

        /// <summary>
        /// Resend a dealer order to the external system by customer order number.
        /// Loads the order and its items from the database and posts to CreateSalesOrder API.
        /// </summary>
        /// <param name="request">Request containing CustomerOrderNumber</param>
        /// <returns>Success with Ln_OrderNumber from external system, or error</returns>
        [HttpPost("ResendToExternalApi")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> ResendDealerOrderToExternalApi([FromBody] ResendDealerOrderToExternalApiRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.CustomerOrderNumber))
                return BadRequest(new { Success = false, Message = "Customer order number is required" });
            if (User.Identity?.IsAuthenticated == false)
                return Unauthorized();

            var result = await _mediator.Send(new ResendDealerOrderToExternalApiCommand
            {
                CustomerOrderNumber = request.CustomerOrderNumber
            });

            if (!result.Success)
            {
                if (result.Message?.Contains("not found") == true)
                    return NotFound(result);
                if (result.Message?.Contains("permission") == true)
                    return StatusCode(403, result);
                return BadRequest(result);
            }
            return Ok(result);
        }
    }

    public class ResendDealerOrderToExternalApiRequest
    {
        public string CustomerOrderNumber { get; set; } = string.Empty;
    }
} 