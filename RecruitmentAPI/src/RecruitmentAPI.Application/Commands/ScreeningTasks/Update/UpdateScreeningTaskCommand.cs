using MediatR;
using RecruitmentAPI.Application.DTOs;
using RecruitmentAPI.Application.DTOs.Applications;
using RecruitmentAPI.Application.DTOs.ScreeningTasks;

namespace RecruitmentAPI.Application.Commands.ScreeningTasks.Update;

public class UpdateScreeningTaskCommand : IRequest<Response<string>>
{
    public long ScreeningTaskId { get; set; }
    public UpdateScreeningTaskDto Task { get; set; } = null!;
}
