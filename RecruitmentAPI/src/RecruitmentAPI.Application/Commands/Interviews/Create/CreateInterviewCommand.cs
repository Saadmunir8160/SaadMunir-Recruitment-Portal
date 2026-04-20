using MediatR;
using RecruitmentAPI.Application.DTOs;
using RecruitmentAPI.Application.DTOs.Applications;
using RecruitmentAPI.Application.DTOs.Interviews;

namespace RecruitmentAPI.Application.Commands.Interviews.Create;

public class CreateInterviewCommand : IRequest<Response<long>>
{
    public CreateInterviewDto Interview { get; set; } = null!;
}
