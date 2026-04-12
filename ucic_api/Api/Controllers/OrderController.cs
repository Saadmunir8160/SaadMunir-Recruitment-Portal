using Application.Commands.CoverageArea.Create;
using Application.Commands.Jobs.Update;
using Application.Commands.Order.Create;
using Application.Commands.Order.Delete;
using Application.Commands.Order.Update;
using Application.Commands.Product.Delete;
using Application.Queries.GeneralData;
using Application.Queries.News;
using Application.Queries.Order;
using Application.Queries.Product;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : Controller
    {
        private readonly IMediator _mediator;
        public OrderController(IMediator mediator)
        {
            _mediator = mediator;
        }


        [HttpPost("CreateOrder")]
        //[Authorize(Roles = "Admin, User")]
        public async Task<ActionResult> CreateOrder(CreateOrderCommand command)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            else if (User.Identity?.IsAuthenticated == false)
                return Unauthorized();

            //var ipAddress = GetClientIpAddress();
            //command.IpAddress = ipAddress;
            return Ok(await _mediator.Send(command));
        }

        [HttpPost("CreatePayment")]
        //[Authorize(Roles = "Admin, User")]
        public async Task<ActionResult> CreatePaymentForOrder(CreatePaymentForOrderCommand command)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            else if (User.Identity?.IsAuthenticated == false)
                return Unauthorized();
            return Ok(await _mediator.Send(command));
        }


        [HttpGet("ValidateCoupon")]
        [Authorize(Roles = "Admin, User")]
        public async Task<ActionResult> ValidateCoupon([FromQuery] string couponCode, [FromQuery] long coverageAreaId)
        {
            if (User.Identity?.IsAuthenticated == false)
                return Unauthorized();
            var result = await _mediator.Send(new ValidateCouponQuery
            {
                couponCode = couponCode,
                coverageAreaId = coverageAreaId
            });
            return Ok(result);
        }

        [HttpGet("GetMyOrders")]
        [Authorize(Roles = "Admin, User")]
        public async Task<ActionResult> GetMyOrders()
        {
            return Ok(await _mediator.Send(new GetMyOrdersQuery()));
        }

        [HttpGet("GetAllOrders")]
        public async Task<ActionResult> GetAllOrders([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var query = new GetAllOrdersQuery
            {
                PageNumber = pageNumber,
                PageSize = pageSize
            };
            return Ok(await _mediator.Send(query));
        }

        [HttpGet("GetOrderByOrderId/{orderId}")]
        public async Task<ActionResult> GetOrderByOrderId(long orderId)
        {
            return Ok(await _mediator.Send(new GetOrderByIdQuery() { OrderId = orderId }));
        }


        [HttpPut("VerifyPaymentForOrder/{orderId}")]
        public async Task<ActionResult> UpdateOrderPaymentVerification(long orderId, [FromForm] VerifyPaymentForOrderCommand command)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            command.OrderId = orderId;
            return Ok(await _mediator.Send(command));
        }

        [HttpDelete("Delete/{orderId}")]
        public async Task<ActionResult> DeleteOrder(long orderId)
        {
            return Ok(await _mediator.Send(new DeleteOrderCommand() { OrderId = orderId }));
        }

        [HttpPut("OrderConfirm/{orderId}")]
        public async Task<ActionResult> ConfirmOrder(long orderId, [FromForm] OrderConfirmCommand command)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            command.OrderId = orderId;
            return Ok(await _mediator.Send(command));
        }

        [HttpPut("CancelOrder/{orderId}")]
        public async Task<ActionResult> CancelOrder(long orderId, [FromBody] CancelOrderCommand command)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            command.OrderId = orderId;
            return Ok(await _mediator.Send(command));
        }

        [HttpGet("GetGeneralDataAndCities/{CoverageAreaId}")]
        public async Task<ActionResult> GetGeneralDataAndCities(long CoverageAreaId)
        {
            return Ok(await _mediator.Send(new GetGeneralDataAndCitiesQuery() { CoverageAreaId = CoverageAreaId }));
        }

        private string GetClientIpAddress()
        {
            // This will give you the client IP address
            var ipAddress = Request.HttpContext.Connection.RemoteIpAddress?.ToString();

            // If the request is coming through a proxy (like a load balancer or reverse proxy),
            // you may need to use the X-Forwarded-For header to get the actual client IP.
            if (Request.Headers.ContainsKey("X-Forwarded-For"))
            {
                ipAddress = Request.Headers["X-Forwarded-For"];
            }

            return ipAddress ?? "Unknown IP";  // Fallback if IP is not found
        }
    }
}
