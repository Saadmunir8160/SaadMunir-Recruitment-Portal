using MediatR;
using Microsoft.AspNetCore.Http;
using RecruitmentAPI.Application.Common.Exceptions;
using RecruitmentAPI.Application.DTOs;
using RecruitmentAPI.Domain.Entities;
using RecruitmentAPI.Domain.Enums;
using RecruitmentAPI.Domain.Repositories.Command.Base;
using RecruitmentAPI.Domain.Repositories.Query.Base;
using System.Security.Claims;

namespace RecruitmentAPI.Application.Commands.Candidates.MarkOcrVerified;

public class MarkOcrVerifiedCommand : IRequest<Response<string>>
{
    public long CandidateId { get; set; }
}

public class MarkOcrVerifiedHandler : IRequestHandler<MarkOcrVerifiedCommand, Response<string>>
{
    private readonly IQueryRepository<Candidate> _queryRepository;
    private readonly ICommandRepository<Candidate> _commandRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public MarkOcrVerifiedHandler(
        IQueryRepository<Candidate> queryRepository,
        ICommandRepository<Candidate> commandRepository,
        IHttpContextAccessor httpContextAccessor)
    {
        _queryRepository = queryRepository;
        _commandRepository = commandRepository;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<Response<string>> Handle(MarkOcrVerifiedCommand request, CancellationToken cancellationToken)
    {
        var candidate = await _queryRepository.GetByIdAsync(request.CandidateId);
        if (candidate is null || candidate.IsDeleted)
            throw new NotFoundException(nameof(Candidate), request.CandidateId);

        candidate.OcrVerificationStatus = OcrVerificationStatus.Verified;

        // If profile was locked in CorrectionRequired due to OCR, clear it back to UnderReview
        if (candidate.ProfileStatus == CandidateProfileStatus.CorrectionRequired)
            candidate.ProfileStatus = CandidateProfileStatus.UnderReview;

        candidate.ModifiedDate = DateTime.UtcNow;
        candidate.ModifiedBy = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Name)?.Value;

        await _commandRepository.UpdateAsync(candidate);

        return Response<string>.SuccessResponse("Verified", "Candidate OCR verification manually overridden to Verified.");
    }
}
