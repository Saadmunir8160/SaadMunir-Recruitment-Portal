using Application.Commands.User.Create;
using Application.Commands.User.Update;
using Application.Commands.User.Delete;
using Application.Commands.User.Login;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DealerUserController : ControllerBase
    {
        private readonly IMediator _mediator;

        public DealerUserController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Creates a new dealer user account
        /// </summary>
        /// <param name="command">Dealer user creation data</param>
        /// <returns>Response indicating success or failure</returns>
        [HttpPost("Create")]
        [AllowAnonymous] // Allow registration without authentication
        public async Task<ActionResult> CreateDealerUser([FromBody] CreateDealerUserCommand command)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var response = await _mediator.Send(command);
            
            if (response.Success)
                return Ok(response);
            else
                return BadRequest(response);
        }

        /// <summary>
        /// Creates a new dealer driver user account
        /// </summary>
        /// <param name="command">Dealer driver user creation data</param>
        /// <returns>Response indicating success or failure</returns>
        [HttpPost("CreateDriver")]
        [Authorize(Roles = "Admin,Dealer")] // Restored proper authorization
        public async Task<ActionResult> CreateDealerDriverUser([FromBody] CreateDealerDriverUserCommand command)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var response = await _mediator.Send(command);
            
            if (response.Success)
                return Ok(response);
            else
                return BadRequest(response);
        }

        /// <summary>
        /// Login for dealer users
        /// </summary>
        /// <param name="command">Login credentials</param>
        /// <returns>Dealer login response with profile information</returns>
        [HttpPost("Login")]
        [AllowAnonymous]
        public async Task<ActionResult> LoginDealer([FromBody] LoginDealerCommand command)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var response = await _mediator.Send(command);
            
            if (response.Success)
                return Ok(response);
            else
                return Unauthorized(response);
        }

        /// <summary>
        /// Login for dealer driver users
        /// </summary>
        /// <param name="command">Login credentials</param>
        /// <returns>Dealer driver login response with profile information</returns>
        [HttpPost("LoginDriver")]
        [AllowAnonymous]
        public async Task<ActionResult> LoginDealerDriver([FromBody] LoginDealerDriverCommand command)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var response = await _mediator.Send(command);
            
            if (response.Success)
                return Ok(response);
            else
                return Unauthorized(response);
        }

        /// <summary>
        /// Updates an existing dealer user account
        /// </summary>
        /// <param name="id">Dealer ID</param>
        /// <param name="command">Dealer user update data</param>
        /// <returns>Response indicating success or failure</returns>
        [HttpPut("Update/{id}")]
        [Authorize(Roles = "Admin")] // Only admins can update dealer accounts
        public async Task<ActionResult> UpdateDealerUser(int id, [FromBody] UpdateDealerUserCommand command)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Set the dealer ID from the route
            command.DealerId = id;

            var response = await _mediator.Send(command);
            
            if (response.Success)
                return Ok(response);
            else
                return BadRequest(response);
        }

        /// <summary>
        /// Deletes a dealer user account
        /// </summary>
        /// <param name="id">Dealer ID</param>
        /// <returns>Response indicating success or failure</returns>
        [HttpDelete("Delete/{id}")]
        [Authorize(Roles = "Admin")] // Only admins can delete dealer accounts
        public async Task<ActionResult> DeleteDealerUser(int id)
        {
            var command = new DeleteDealerUserCommand { DealerId = id };
            var response = await _mediator.Send(command);
            
            if (response.Success)
                return Ok(response);
            else
                return BadRequest(response);
        }
    }
}