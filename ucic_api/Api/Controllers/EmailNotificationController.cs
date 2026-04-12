using Application.Commands.DepartmentNotificationRecipient.Create;
using Application.Commands.DepartmentNotificationRecipient.Delete;
using Application.Commands.DepartmentNotificationRecipient.Update;
using Application.Queries.DepartmentNotificationRecipient;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmailNotificationController : Controller
    {
        private readonly IMediator _mediator;
        public EmailNotificationController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("Create")]
        public async Task<ActionResult> CreateEmailNotification([FromForm] CreateEmailNotificationCommand command)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            else if (User.Identity?.IsAuthenticated == false)
                return Unauthorized();

            return Ok(await _mediator.Send(command));
        }


        [HttpGet("GetAll")]
        public async Task<ActionResult> GetAllDepartmentNotificationRecipients([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var query = new DepartmentNotificationRecipientQuery
            {
                PageNumber = pageNumber,
                PageSize = pageSize
            };
            return Ok(await _mediator.Send(query));
        }

        [HttpGet("GetById/{id}")]
        public async Task<ActionResult> GetDepartmentNotificationRecipientsById(long id)
        {
            return Ok(await _mediator.Send(new DepartmentNotificationRecipientByIdQuery() { Id = id }));
        }

        [HttpDelete("Delete/{id}")]
        public async Task<ActionResult> DeleteJob(long id)
        {
            return Ok(await _mediator.Send(new DeleteDepartmentNotificationRecipientCommand() { Id = id }));
        }

        [HttpPut("Update/{id}")]
        public async Task<ActionResult> UpdateJob(long id, [FromForm] UpdateDepartmentNotificationRecipientCommand command)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            command.Id = id;
            return Ok(await _mediator.Send(command));
        }
    }
}
