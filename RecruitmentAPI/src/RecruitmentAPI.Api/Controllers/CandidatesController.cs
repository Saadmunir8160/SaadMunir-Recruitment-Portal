using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RecruitmentAPI.Application.Commands.Candidates.Create;
using RecruitmentAPI.Application.Commands.Candidates.MarkOcrVerified;
using RecruitmentAPI.Application.Commands.Candidates.Submit;
using RecruitmentAPI.Application.Commands.Candidates.Update;
using RecruitmentAPI.Application.Commands.Candidates.UpdateStatus;
using RecruitmentAPI.Application.DTOs.Candidates;
using RecruitmentAPI.Application.Queries.Candidates;

namespace RecruitmentAPI.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class CandidatesController : ControllerBase
{
    private readonly IMediator _mediator;

    public CandidatesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    // Roles updated to match your Angular RoleService
    [Authorize(Roles = RecruitmentPortalRoles.Staff)]
    public async Task<IActionResult> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _mediator.Send(new GetAllCandidatesQuery
        {
            PageNumber = pageNumber,
            PageSize = pageSize
        });
        return Ok(result);
    }

    [HttpGet("{id}")]
    [Authorize(Roles = RecruitmentPortalRoles.Staff)]
    public async Task<IActionResult> GetById(long id)
    {
        var result = await _mediator.Send(new GetCandidateByIdQuery { CandidateId = id });
        return Ok(result);
    }

    [HttpPost]
    [AllowAnonymous] // Candidates aksar login ke baghair register karte hain
    public async Task<IActionResult> Create([FromBody] CreateCandidateDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _mediator.Send(new CreateCandidateCommand { Candidate = dto });
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPut("{id}")]
    // Candidate khud update kar sakay ya HR team
    [Authorize(Roles = RecruitmentPortalRoles.StaffWithCandidate)]
    public async Task<IActionResult> Update(long id, [FromBody] UpdateCandidateDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _mediator.Send(new UpdateCandidateCommand
        {
            CandidateId = id,
            Candidate = dto
        });
        return Ok(result);
    }

    [HttpPost("{id}/submit")]
    [Authorize(Roles = "Candidate")]
    public async Task<IActionResult> Submit(long id)
    {
        var result = await _mediator.Send(new SubmitCandidateProfileCommand { CandidateId = id });
        return Ok(result);
    }

    [HttpPut("{id}/status")]
    [Authorize(Roles = RecruitmentPortalRoles.Staff)]
    public async Task<IActionResult> UpdateStatus(long id, [FromBody] UpdateCandidateStatusDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _mediator.Send(new UpdateCandidateStatusCommand { CandidateId = id, Status = dto });
        return Ok(result);
    }

    [HttpPost("{id}/mark-ocr-verified")]
    [Authorize(Roles = RecruitmentPortalRoles.Staff)]
    public async Task<IActionResult> MarkOcrVerified(long id)
    {
        var result = await _mediator.Send(new MarkOcrVerifiedCommand { CandidateId = id });
        return result.Success ? Ok(result) : BadRequest(result);
    }
}