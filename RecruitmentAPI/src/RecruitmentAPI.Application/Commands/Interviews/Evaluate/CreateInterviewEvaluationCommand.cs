using MediatR;
using RecruitmentAPI.Application.DTOs;
using RecruitmentAPI.Application.DTOs.Applications;
using RecruitmentAPI.Application.DTOs.Interviews;

namespace RecruitmentAPI.Application.Commands.Interviews.Evaluate;

public class CreateInterviewEvaluationCommand : IRequest<Response<long>>
{
    public long InterviewId { get; set; }
    public CreateInterviewEvaluationDto Evaluation { get; set; } = null!;
}
