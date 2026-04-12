using MediatR;
using RecruitmentAPI.Application.DTOs;
using RecruitmentAPI.Application.DTOs.Candidates;

namespace RecruitmentAPI.Application.Commands.Candidates.Update;

public class UpdateCandidateCommand : IRequest<Response<string>>
{
    public long CandidateId { get; set; }
    public UpdateCandidateDto Candidate { get; set; } = null!;
}
