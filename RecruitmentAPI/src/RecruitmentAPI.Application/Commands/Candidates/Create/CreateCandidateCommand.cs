using MediatR;
using RecruitmentAPI.Application.DTOs;
using RecruitmentAPI.Application.DTOs.Candidates;

namespace RecruitmentAPI.Application.Commands.Candidates.Create;

public class CreateCandidateCommand : IRequest<Response<long>>
{
    public CreateCandidateDto Candidate { get; set; } = null!;
}
