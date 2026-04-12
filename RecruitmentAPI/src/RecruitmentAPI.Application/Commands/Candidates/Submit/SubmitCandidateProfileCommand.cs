using MediatR;
using RecruitmentAPI.Application.DTOs;
using RecruitmentAPI.Domain.Enums;

namespace RecruitmentAPI.Application.Commands.Candidates.Submit;

public class SubmitCandidateProfileCommand : IRequest<Response<string>>
{
    public long CandidateId { get; set; }
}
