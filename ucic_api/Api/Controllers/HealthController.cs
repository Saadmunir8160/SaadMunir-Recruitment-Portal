using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HealthController : Controller
    {
        private readonly IMediator _mediator;
        public HealthController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("CheckStatus")]
        public async Task<ActionResult> CheckStatus()
        {
            return Ok("server is running");
        }

       
    }
}
