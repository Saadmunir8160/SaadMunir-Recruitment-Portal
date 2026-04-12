using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RecruitmentAPI.Application.Commands.Applications.Create;
using RecruitmentAPI.Application.Commands.Applications.UpdateStatus;
using RecruitmentAPI.Application.DTOs.Applications;
using RecruitmentAPI.Application.Queries.Applications;

namespace RecruitmentAPI.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class ApplicationsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ApplicationsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("vacancy/{vacancyId}")]
    [Authorize(Roles = RecruitmentPortalRoles.Staff)]
    public async Task<IActionResult> GetByVacancy(
        long vacancyId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await _mediator.Send(new GetApplicationsByVacancyQuery
        {
            VacancyId = vacancyId,
            PageNumber = pageNumber,
            PageSize = pageSize
        });
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(long id)
    {
        var result = await _mediator.Send(new GetApplicationByIdQuery { ApplicationId = id });
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "Candidate")]
    public async Task<IActionResult> Apply([FromBody] CreateApplicationCommand command)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _mediator.Send(command);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPut("{id}/status")]
    [Authorize(Roles = RecruitmentPortalRoles.Staff)]
    public async Task<IActionResult> UpdateStatus(long id, [FromBody] UpdateApplicationStatusDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _mediator.Send(new UpdateApplicationStatusCommand { ApplicationId = id, Status = dto });
        return Ok(result);
    }

    [HttpGet("{id}/match-result")]
    [Authorize(Roles = RecruitmentPortalRoles.Staff)]
    public async Task<IActionResult> GetMatchResult(long id)
    {
        var result = await _mediator.Send(new GetMatchResultQuery { ApplicationId = id });
        return Ok(result);
    }
}
