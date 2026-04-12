using MediatR;
using RecruitmentAPI.Application.DTOs;
using RecruitmentAPI.Application.DTOs.Candidates;

namespace RecruitmentAPI.Application.Commands.Candidates.UpdateStatus;

public class UpdateCandidateStatusCommand : IRequest<Response<string>>
{
    public long CandidateId { get; set; }
    public UpdateCandidateStatusDto Status { get; set; } = null!;
}
