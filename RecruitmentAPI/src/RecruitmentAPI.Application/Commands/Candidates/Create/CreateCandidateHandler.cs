using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using RecruitmentAPI.Application.DTOs;
using RecruitmentAPI.Domain.Entities;
using RecruitmentAPI.Domain.Enums;
using RecruitmentAPI.Domain.Repositories.Command.Base;
using RecruitmentAPI.Domain.Repositories.Query.Base;
using System.Security.Claims;

namespace RecruitmentAPI.Application.Commands.Candidates.Create;

public class CreateCandidateHandler : IRequestHandler<CreateCandidateCommand, Response<long>>
{
    private readonly ICommandRepository<Candidate> _commandRepository;
    private readonly IQueryRepository<Candidate> _queryRepository;
    private readonly IMapper _mapper;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CreateCandidateHandler(
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

    public async Task<Response<long>> Handle(CreateCandidateCommand request, CancellationToken cancellationToken)
    {
        var userId = _httpContextAccessor.HttpContext?.User?.FindFirst("UserId")?.Value
            ?? _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
            return Response<long>.FailureResponse("User not authenticated.");

        // Check if candidate already exists for this user
        var existing = _queryRepository.GetQueryable()
            .Any(c => c.UserId == userId && !c.IsDeleted);

        if (existing)
            return Response<long>.FailureResponse("Candidate profile already exists for this user.");

        var candidate = _mapper.Map<Candidate>(request.Candidate);
        candidate.UserId = userId;
        candidate.ProfileStatus = CandidateProfileStatus.Incomplete;
        candidate.IsProfileLocked = false;
        candidate.CreatedBy = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Name)?.Value;

        await _commandRepository.AddAsync(candidate);

        return Response<long>.SuccessResponse(candidate.CandidateId, "Candidate profile created successfully.");
    }
}
