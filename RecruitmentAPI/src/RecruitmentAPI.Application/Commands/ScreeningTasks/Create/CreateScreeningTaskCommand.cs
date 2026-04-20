using MediatR;
using RecruitmentAPI.Application.DTOs;
using RecruitmentAPI.Application.DTOs.Applications;
using RecruitmentAPI.Application.DTOs.ScreeningTasks;

namespace RecruitmentAPI.Application.Commands.ScreeningTasks.Create;

public class CreateScreeningTaskCommand : IRequest<Response<long>>
{
    public CreateScreeningTaskDto Task { get; set; } = null!;
}
