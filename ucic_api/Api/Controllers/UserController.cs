using Application.Commands.ContactUs.Create;
using Application.Commands.User.Create;
using Application.Commands.User.Delete;
using Application.Commands.User.Update;
using Application.Commands.User.ChangePassword;
using Application.Commands.Vendor.Create;
using Application.DTOs;
using Application.Queries.User;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IMediator _mediator;

        public UserController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("Create")]
        [ProducesDefaultResponseType(typeof(int))]
        public async Task<ActionResult> CreateUser(CreateUserCommand command)
        {
            return Ok(await _mediator.Send(command));
        }

        [HttpGet("GetAll")]
        //[Authorize(Roles = "Admin")]
        [ProducesDefaultResponseType(typeof(List<UserResponseDTO>))]
        public async Task<IActionResult> GetAllUserAsync([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var query = new Application.Queries.User.GetUserQuery
            {
                PageNumber = pageNumber,
                PageSize = pageSize
            };
            return Ok(await _mediator.Send(query));
        }

        [HttpGet("GetByRole/{roleName}")]
        //[Authorize(Roles = "Admin")]
        [ProducesDefaultResponseType(typeof(List<UserDropdownDTO>))]
        public async Task<IActionResult> GetUsersByRoleAsync(string roleName)
        {
            var query = new Application.Queries.User.GetUsersByRoleQuery
            {
                RoleName = roleName
            };
            return Ok(await _mediator.Send(query));
        }

        [HttpDelete("Delete/{userId}")]
        //[Authorize(Roles = "Admin")]
        [ProducesDefaultResponseType(typeof(int))]
        public async Task<IActionResult> DeleteUser(string userId)
        {
            var result = await _mediator.Send(new DeleteUserCommand() { Id = userId });
            return Ok(result);
        }

        [HttpGet("GetUserDetails/{userId}")]
        //[Authorize(Roles = "Admin")]
        [ProducesDefaultResponseType(typeof(UserDetailsResponseDTO))]
        public async Task<IActionResult> GetUserDetails(string userId)
        {
            var result = await _mediator.Send(new GetUserDetailsQuery() { UserId = userId });
            return Ok(result);
        }

        [HttpGet("GetUserDetailsByUserName/{userName}")]
        //[Authorize(Roles = "Admin")]
        [ProducesDefaultResponseType(typeof(UserDetailsResponseDTO))]
        public async Task<IActionResult> GetUserDetailsByUserName(string userName)
        {
            var result = await _mediator.Send(new GetUserDetailsByUserNameQuery() { UserName = userName });
            return Ok(result);
        }

        [HttpPost("AssignRoles")]
        //[Authorize(Roles = "Admin")]
        [ProducesDefaultResponseType(typeof(int))]

        public async Task<ActionResult> AssignRoles(AssignUsersRoleCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpPut("EditUserRoles")]
        //[Authorize(Roles = "Admin")]
        [ProducesDefaultResponseType(typeof(int))]

        public async Task<ActionResult> EditUserRoles(UpdateUserRolesCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpGet("GetAllUserDetails")]
        //[Authorize(Roles = "Admin")]
        [ProducesDefaultResponseType(typeof(UserDetailsResponseDTO))]
        public async Task<IActionResult> GetAllUserDetails()
        {
            var result = await _mediator.Send(new GetAllUsersDetailsQuery());
            return Ok(result);
        }


        [HttpPut("EditUserProfile/{id}")]
        //[Authorize(Roles = "Admin")]
        [ProducesDefaultResponseType(typeof(int))]
        public async Task<ActionResult> EditUserProfile(string id, [FromBody] EditUserProfileCommand command)
        {
            if (id == command.Id)
            {
                var result = await _mediator.Send(command);
                return Ok(result);
            }
            else
            {
                return BadRequest();
            }
        }

        [HttpPost("ChangePassword")]
        [Authorize]
        [ProducesDefaultResponseType(typeof(int))]
        public async Task<ActionResult> ChangePassword([FromBody] ChangePasswordCommand command)
        {
            var currentUserId = User.FindFirst("UserId")?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            
            if (string.IsNullOrEmpty(currentUserId) || currentUserId != command.UserId)
            {
                return Unauthorized("You can only change your own password");
            }

            var result = await _mediator.Send(command);
            return Ok(result);
        }


        [HttpGet("GetUserWithLocations/{coverageAreaId}")]
        //[Authorize(Roles = "Admin, User")]
        [ProducesDefaultResponseType(typeof(GetUserWithLocationsQuery))]
        public async Task<IActionResult> GetUserWithLocations(long coverageAreaId)
        {
            var userId = User.FindFirst("UserId")?.Value;
            var result = await _mediator.Send(new GetUserWithLocationsQuery() { UserId = userId!, coverageAreaId = coverageAreaId });
            return Ok(result);
        }


        [HttpPost("CreateVendor")]
        [ProducesDefaultResponseType(typeof(int))]
        public async Task<ActionResult> CreateVendor([FromForm] CreateVendorCommand command)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            return Ok(await _mediator.Send(command));
        }

        [HttpPost("CreateContactUsRequest")]
        [ProducesDefaultResponseType(typeof(int))]
        public async Task<ActionResult> CreateContactUsRequest(CreateContactUsCommand command)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            return Ok(await _mediator.Send(command));
        }

    }
}
