using MediatR;
using RecruitmentAPI.Application.Common.Interfaces;
using RecruitmentAPI.Application.DTOs;

namespace RecruitmentAPI.Application.Commands.Applications.Match;

public class MatchApplicationCommand : IRequest<Response<bool>>
{
    public long ApplicationId { get; set; }
}

public class MatchApplicationHandler : IRequestHandler<MatchApplicationCommand, Response<bool>>
{
    private readonly IMatchingService _matchingService;

    public MatchApplicationHandler(IMatchingService matchingService)
        => _matchingService = matchingService;

    public async Task<Response<bool>> Handle(MatchApplicationCommand request, CancellationToken cancellationToken)
    {
        await _matchingService.MatchAsync(request.ApplicationId, cancellationToken);
        return Response<bool>.SuccessResponse(true, "Matching completed.");
    }
}
