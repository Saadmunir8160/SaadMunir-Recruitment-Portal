using MediatR;
using Microsoft.AspNetCore.Http;
using RecruitmentAPI.Application.Common.Exceptions;
using RecruitmentAPI.Application.DTOs;
using RecruitmentAPI.Domain.Entities;
using RecruitmentAPI.Domain.Enums;
using RecruitmentAPI.Domain.Repositories.Command.Base;
using RecruitmentAPI.Domain.Repositories.Query.Base;
using System.Security.Claims;

namespace RecruitmentAPI.Application.Commands.Candidates.UpdateStatus;

public class UpdateCandidateStatusHandler : IRequestHandler<UpdateCandidateStatusCommand, Response<string>>
{
    private readonly ICommandRepository<Candidate> _commandRepository;
    private readonly IQueryRepository<Candidate> _queryRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UpdateCandidateStatusHandler(
        ICommandRepository<Candidate> commandRepository,
        IQueryRepository<Candidate> queryRepository,
        IHttpContextAccessor httpContextAccessor)
    {
        _commandRepository = commandRepository;
        _queryRepository = queryRepository;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<Response<string>> Handle(UpdateCandidateStatusCommand request, CancellationToken cancellationToken)
    {
        var candidate = await _queryRepository.GetByIdAsync(request.CandidateId);
        if (candidate is null || candidate.IsDeleted)
            throw new NotFoundException(nameof(Candidate), request.CandidateId);

        candidate.ProfileStatus = request.Status.NewStatus;
        candidate.IsProfileLocked = request.Status.NewStatus is
            CandidateProfileStatus.Submitted or
            CandidateProfileStatus.UnderReview or
            CandidateProfileStatus.Approved;

        if (request.Status.NewStatus == CandidateProfileStatus.Rejected)
        {
            candidate.RejectionReason = request.Status.Reason;
            candidate.RejectionDate = DateTime.UtcNow;
            candidate.RejectedByUserId = _httpContextAccessor.HttpContext?.User?.FindFirst("UserId")?.Value;
        }
        else if (request.Status.NewStatus == CandidateProfileStatus.CorrectionRequired)
        {
            candidate.RejectionReason = request.Status.Reason;
        }

        candidate.ModifiedDate = DateTime.UtcNow;
        candidate.ModifiedBy = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Name)?.Value;

        await _commandRepository.UpdateAsync(candidate);

        return Response<string>.SuccessResponse("Updated", "Candidate status updated successfully.");
    }
}
