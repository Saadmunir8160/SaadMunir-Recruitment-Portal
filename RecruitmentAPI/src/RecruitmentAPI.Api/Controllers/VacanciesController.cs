using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RecruitmentAPI.Application.Commands.Vacancies.Create;
using RecruitmentAPI.Application.Commands.Vacancies.Update;
using RecruitmentAPI.Application.Commands.Vacancies.SubmitVacancyForApproval;
using RecruitmentAPI.Application.Commands.Vacancies.ApproveVacancy;
using RecruitmentAPI.Application.Commands.Vacancies.RejectVacancy;
using RecruitmentAPI.Application.DTOs.Vacancies;
using RecruitmentAPI.Application.Queries.Vacancies;
using RecruitmentAPI.Domain.Enums;

namespace RecruitmentAPI.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class VacanciesController : ControllerBase
{
    private readonly IMediator _mediator;

    public VacanciesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] VacancyPublishStatus? status = null)
    {
        var result = await _mediator.Send(new GetAllVacanciesQuery
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            StatusFilter = status
        });
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(long id)
    {
        var result = await _mediator.Send(new GetVacancyByIdQuery { VacancyId = id });
        return Ok(result);
    }

    [HttpGet("published")]
    [AllowAnonymous]
    public async Task<IActionResult> GetPublished(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null)
    {
        var result = await _mediator.Send(new GetPublishedVacanciesQuery
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            SearchTerm = search
        });
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = RecruitmentPortalRoles.Staff)]
    public async Task<IActionResult> Create([FromBody] CreateVacancyDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var result = await _mediator.Send(new CreateVacancyCommand { Vacancy = dto });
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = RecruitmentPortalRoles.Staff)]
    public async Task<IActionResult> Update(long id, [FromBody] CreateVacancyDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var result = await _mediator.Send(new UpdateVacancyCommand { VacancyId = id, Vacancy = dto });
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // ========================
    // VACANCY APPROVAL WORKFLOW
    // ========================

    [HttpPost("{id}/submit-for-approval")]
    [Authorize(Roles = RecruitmentPortalRoles.Staff)]
    public async Task<IActionResult> SubmitForApproval(long id)
    {
        var result = await _mediator.Send(new SubmitVacancyForApprovalCommand { VacancyId = id });
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("{id}/approve")]
    [Authorize(Roles = "Admin,HRSupervisor,HR Manager,HR Section Head")]
    public async Task<IActionResult> Approve(long id, [FromBody] ApproveRejectDto dto)
    {
        var result = await _mediator.Send(new ApproveVacancyCommand
        {
            VacancyId = id,
            Comments = dto?.Comments
        });
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("{id}/reject")]
    [Authorize(Roles = "Admin,HRSupervisor,HR Manager,HR Section Head")]
    public async Task<IActionResult> Reject(long id, [FromBody] ApproveRejectDto dto)
    {
        var result = await _mediator.Send(new RejectVacancyCommand
        {
            VacancyId = id,
            Reason = dto?.Comments
        });
        return result.Success ? Ok(result) : BadRequest(result);
    }
}

// Build error CS0246 fix karne ke liye class yahan honi chahiye
public class ApproveRejectDto
{
    public string? Comments { get; set; }
}