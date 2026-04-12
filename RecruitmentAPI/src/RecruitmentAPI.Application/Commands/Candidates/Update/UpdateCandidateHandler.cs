using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using RecruitmentAPI.Application.Common.Exceptions;
using RecruitmentAPI.Application.DTOs;
using RecruitmentAPI.Domain.Entities;
using RecruitmentAPI.Domain.Enums;
using RecruitmentAPI.Domain.Repositories.Command.Base;
using RecruitmentAPI.Domain.Repositories.Query.Base;
using System.Security.Claims;

namespace RecruitmentAPI.Application.Commands.Candidates.Update;

public class UpdateCandidateHandler : IRequestHandler<UpdateCandidateCommand, Response<string>>
{
    private readonly ICommandRepository<Candidate> _commandRepository;
    private readonly IQueryRepository<Candidate> _queryRepository;
    private readonly IMapper _mapper;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UpdateCandidateHandler(
        ICommandRepository<Candidate> commandRepository,
        IQueryRepository<Candidate> queryRepository,
        IMapper mapper,
        IHttpContextAccessor httpContextAccessor)
    {
        _commandRepository = commandRepository;
        _queryRepository = queryRepository;
        _mapper = mapper;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<Response<string>> Handle(UpdateCandidateCommand request, CancellationToken cancellationToken)
    {
        var candidate = await _queryRepository.GetByIdAsync(request.CandidateId);
        if (candidate is null || candidate.IsDeleted)
            throw new NotFoundException(nameof(Candidate), request.CandidateId);

        // Profile lock check — only Incomplete and CorrectionRequired allow edits
        if (candidate.ProfileStatus == CandidateProfileStatus.Submitted ||
            candidate.ProfileStatus == CandidateProfileStatus.UnderReview ||
            candidate.ProfileStatus == CandidateProfileStatus.Approved)
        {
            throw new BadRequestException("Profile is locked. Contact HR for corrections.");
        }

        _mapper.Map(request.Candidate, candidate);
        candidate.ModifiedDate = DateTime.UtcNow;
        candidate.ModifiedBy = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Name)?.Value;

        await _commandRepository.UpdateAsync(candidate);

        return Response<string>.SuccessResponse("Updated", "Candidate profile updated successfully.");
    }
}
