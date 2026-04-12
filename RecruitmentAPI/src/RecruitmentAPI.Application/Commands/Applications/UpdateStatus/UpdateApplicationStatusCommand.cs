using MediatR;
using RecruitmentAPI.Application.DTOs;
using RecruitmentAPI.Application.DTOs.Applications;

namespace RecruitmentAPI.Application.Commands.Applications.UpdateStatus;

public class UpdateApplicationStatusCommand : IRequest<Response<string>>
{
    public long ApplicationId { get; set; }
    public UpdateApplicationStatusDto Status { get; set; } = null!;
}
