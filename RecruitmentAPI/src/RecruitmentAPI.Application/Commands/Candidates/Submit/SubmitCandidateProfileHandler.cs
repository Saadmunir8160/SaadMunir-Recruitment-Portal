using MediatR;
using Microsoft.AspNetCore.Http;
using RecruitmentAPI.Application.Common.Exceptions;
using RecruitmentAPI.Application.DTOs;
using RecruitmentAPI.Domain.Entities;
using RecruitmentAPI.Domain.Enums;
using RecruitmentAPI.Domain.Repositories.Command.Base;
using RecruitmentAPI.Domain.Repositories.Query.Base;
using System.Security.Claims;

namespace RecruitmentAPI.Application.Commands.Candidates.Submit;

public class SubmitCandidateProfileHandler : IRequestHandler<SubmitCandidateProfileCommand, Response<string>>
{
    private readonly ICommandRepository<Candidate> _commandRepository;
    private readonly IQueryRepository<Candidate> _queryRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public SubmitCandidateProfileHandler(
        ICommandRepository<Candidate> commandRepository,
        IQueryRepository<Candidate> queryRepository,
        IHttpContextAccessor httpContextAccessor)
    {
        _commandRepository = commandRepository;
        _queryRepository = queryRepository;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<Response<string>> Handle(SubmitCandidateProfileCommand request, CancellationToken cancellationToken)
    {
        var candidate = await _queryRepository.GetByIdAsync(request.CandidateId);
        if (candidate is null || candidate.IsDeleted)
            throw new NotFoundException(nameof(Candidate), request.CandidateId);

        if (candidate.ProfileStatus != CandidateProfileStatus.Incomplete &&
            candidate.ProfileStatus != CandidateProfileStatus.CorrectionRequired)
        {
            throw new BadRequestException("Profile can only be submitted from Incomplete or CorrectionRequired status.");
        }

        candidate.ProfileStatus = CandidateProfileStatus.Submitted;
        candidate.IsProfileLocked = true;
        candidate.ModifiedDate = DateTime.UtcNow;
        candidate.ModifiedBy = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Name)?.Value;

        await _commandRepository.UpdateAsync(candidate);

        return Response<string>.SuccessResponse("Submitted", "Profile submitted successfully. Profile is now locked.");
    }
}
