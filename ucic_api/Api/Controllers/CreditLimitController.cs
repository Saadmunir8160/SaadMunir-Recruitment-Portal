using Application.Queries.Dealer;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Dealer")]
    public class CreditLimitController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CreditLimitController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Get the latest credit limit from LN API for the current authenticated dealer
        /// </summary>
        /// <returns>Available credit amount</returns>
        [HttpGet]
        public async Task<ActionResult> GetCreditLimit()
        {
            var result = await _mediator.Send(new GetCreditLimitQuery());
            
            if (!result.Success)
            {
                return BadRequest(result);
            }
            
            return Ok(result);
        }
    }
}

