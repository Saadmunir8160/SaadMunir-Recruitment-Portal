using Application.Commands.Jobs.Delete;
using Application.Commands.Jobs.Update;
using Application.Commands.Order.Create;
using Application.Commands.Product.Create;
using Application.Commands.Product.Delete;
using Application.Commands.Product.Update;
using Application.Commands.User.Create;
using Application.Queries.Order;
using Application.Queries.Product;
using Application.Queries.User;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : Controller
    {
        private readonly IMediator _mediator;
        public ProductController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("Create")]
       // [Authorize(Roles = "Admin")]
        public async Task<ActionResult> CreateProduct([FromForm] CreateProductCommand command)
        {
            if(!ModelState.IsValid)
                return BadRequest(ModelState);
            return Ok(await _mediator.Send(command));
        }


        [HttpGet("GetAll")]
        public async Task<ActionResult> GetAllProducts([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var query = new GetAllProductsQuery
            {
                PageNumber = pageNumber,
                PageSize = pageSize
            };
            return Ok(await _mediator.Send(query));
        }


        [HttpGet("GetAllProductsForAdmin")]
        public async Task<ActionResult> GetAllProductsForAdmin()
        {
            return Ok(await _mediator.Send(new GetAllProductsForAdminQuery()));
        }

        [HttpGet("GetProductByIdForAdmin/{productId}")]
        public async Task<ActionResult> GetProductByIdForAdmin(long productId)
        {
            return Ok(await _mediator.Send(new GetProductByIdForAdminQuery() { productID = productId }));
        }

        [HttpPut("Update/{productId}")]
        public async Task<ActionResult> UpdateProduct(long productId, [FromForm] UpdateProductCommand command)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            command.ProductId = productId;
            return Ok(await _mediator.Send(command));
        }
        [HttpDelete("Delete/{productId}")]
        public async Task<ActionResult> DeleteJob(long productId)
        {
            return Ok(await _mediator.Send(new DeleteProductCommand() { ProductId = productId }));
        }

        [HttpGet("GetProductsByCoverageArea/{CoverageAreaId}")]
        public async Task<ActionResult> GetProductsByCoverageArea(long CoverageAreaId)
        {
            return Ok(await _mediator.Send(new GetProductsByCoverageAreaQuery() { CoverageAreaId = CoverageAreaId }));
        }

        [HttpGet("GetProduct/{productId}")]
        public async Task<ActionResult> GetAllProducts(long productId)
        {
            return Ok(await _mediator.Send(new GetProductByIDQuery() { productID = productId }));
        }

        //[HttpGet("ValidateCoupon/{couponCode}")]
        //public async Task<ActionResult> GetAllProducts(string couponCode)
        //{
        //    return Ok(await _mediator.Send(new ValidateCouponQuery() { couponCode = couponCode }));
        //}



    }
}
