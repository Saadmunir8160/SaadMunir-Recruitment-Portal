using Application.Commands.Vendor.Approve;
using Application.Commands.Vendor.Delete;
using Application.Queries.Vendor;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VendorsController : Controller
    {
        private readonly IMediator _mediator;
        public VendorsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("GetAll")]
        public async Task<ActionResult> GetAllVendors([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var query = new GetAllVendorsQuery
            {
                PageNumber = pageNumber,
                PageSize = pageSize
            };
            return Ok(await _mediator.Send(query));
        }

        [HttpDelete("Approve/{jobId}")]
        public async Task<ActionResult> ApproveVendor(long jobId)
        {
            return Ok(await _mediator.Send(new ApproveVendorCommand() { Id = jobId }));
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteVendor(long id)
        {
            return Ok(await _mediator.Send(new DeleteVendorCommand() { Id = id }));
        }
    }
}
