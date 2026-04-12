using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RecruitmentAPI.Application.Queries.ScreeningTasks;

namespace RecruitmentAPI.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class ScreeningTasksController : ControllerBase
{
    private readonly IMediator _mediator;

    public ScreeningTasksController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("application/{applicationId}")]
    [Authorize(Roles = RecruitmentPortalRoles.Staff)]
    public async Task<IActionResult> GetByApplication(long applicationId)
    {
        var result = await _mediator.Send(new GetScreeningTasksByApplicationQuery { ApplicationId = applicationId });
        return Ok(result);
    }
}
