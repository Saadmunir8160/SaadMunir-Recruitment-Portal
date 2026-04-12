using Application.Commands.Auth;
using Application.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AuthController(IMediator mediator)
        {
            _mediator = mediator;
        }


        [HttpPost("RegisterCandidate")]
        [ProducesDefaultResponseType(typeof(AuthResponseDTO))]
        public async Task<IActionResult> RegisterCandidate([FromBody] RegisterCandidateCommand command)
        {
            return Ok(await _mediator.Send(command));
        }

        [HttpPost("Login")]
        [ProducesDefaultResponseType(typeof(AuthResponseDTO))]
        public async Task<IActionResult> Login([FromBody] AuthCommand command)
        {
            //var result = await _mediator.Send(command);
            //Response.Cookies.Append("authToken", result.Token, new CookieOptions
            //{
            //    HttpOnly = true,
            //    Secure = true, // Use true in production
            //    SameSite = SameSiteMode.Strict
            //});

            return Ok(await _mediator.Send(command));
        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            Response.Cookies.Delete("authToken");
            return Ok(new { Message = "Logout successful" });
        }

        [HttpGet("CheckLogin")]
        [Authorize(Roles = "Admin, User")]
        public IActionResult CheckLogin()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                var expiryClaim = User.Claims.FirstOrDefault(c => c.Type == "exp")?.Value;
                if (expiryClaim != null && long.TryParse(expiryClaim, out var exp))
                {
                    var expiryDate = DateTimeOffset.FromUnixTimeSeconds(exp).UtcDateTime;
                    if (expiryDate > DateTime.UtcNow)
                    {
                        return Ok(true);
                    }
                }

                return Unauthorized(false);
            }

            //if (User.Identity?.IsAuthenticated == true)
            //{
            //    return Ok(true);
            //}
            return Unauthorized(false);
        }

        [HttpPost("verify-captcha")]
        public async Task<IActionResult> VerifyCaptcha([FromBody] VerifyCaptchaCommand command)
        {
            var result = await _mediator.Send(command);

            if (result.Success)
            {
                return Ok(new { success = result.Success, message = result.Message });
            }

            return BadRequest(new { success = result.Success, message = result.ErrorMessage });
        }
    }
}
