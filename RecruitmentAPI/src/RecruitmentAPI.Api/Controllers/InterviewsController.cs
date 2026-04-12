using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RecruitmentAPI.Application.Queries.Interviews;

namespace RecruitmentAPI.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class InterviewsController : ControllerBase
{
    private readonly IMediator _mediator;

    public InterviewsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [Authorize(Roles = RecruitmentPortalRoles.Staff)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await _mediator.Send(new GetAllInterviewsQuery
        {
            PageNumber = pageNumber,
            PageSize = pageSize
        });
        return Ok(result);
    }

    [HttpGet("application/{applicationId}")]
    [Authorize(Roles = RecruitmentPortalRoles.Staff)]
    public async Task<IActionResult> GetByApplication(long applicationId)
    {
        var result = await _mediator.Send(new GetInterviewsByApplicationQuery { ApplicationId = applicationId });
        return Ok(result);
    }
}
