using MediatR;
using Microsoft.AspNetCore.Http;
using RecruitmentAPI.Application.Common.Exceptions;
using RecruitmentAPI.Application.DTOs;
using RecruitmentAPI.Domain.Entities;
using RecruitmentAPI.Domain.Enums;
using RecruitmentAPI.Domain.Repositories.Command.Base;
using RecruitmentAPI.Domain.Repositories.Query.Base;
using System.Security.Claims;
using AppEntity = RecruitmentAPI.Domain.Entities.Application;

namespace RecruitmentAPI.Application.Commands.ScreeningTasks.Create;

public class CreateScreeningTaskHandler : IRequestHandler<CreateScreeningTaskCommand, Response<long>>
{
    private readonly ICommandRepository<ScreeningTask> _commandRepository;
    private readonly IQueryRepository<AppEntity> _applicationQueryRepo;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CreateScreeningTaskHandler(
        ICommandRepository<ScreeningTask> commandRepository,
        IQueryRepository<AppEntity> applicationQueryRepo,
        IHttpContextAccessor httpContextAccessor)
    {
        _commandRepository = commandRepository;
        _applicationQueryRepo = applicationQueryRepo;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<Response<long>> Handle(CreateScreeningTaskCommand request, CancellationToken cancellationToken)
    {
        var dto = request.Task;

        var application = await _applicationQueryRepo.GetByIdAsync(dto.ApplicationId);
        if (application is null || application.IsDeleted)
            throw new NotFoundException("Application", dto.ApplicationId);

        var userName = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Name)?.Value;

        var task = new ScreeningTask
        {
            ApplicationId = dto.ApplicationId,
            AssignedToUserId = dto.AssignedToUserId,
            AssignedToName = dto.AssignedToName,
            TaskDescription = dto.TaskDescription,
            Deadline = dto.Deadline,
            Notes = dto.Notes,
            Status = ScreeningTaskStatus.Pending,
            CreatedBy = userName
        };

        await _commandRepository.AddAsync(task);

        return Response<long>.SuccessResponse(task.ScreeningTaskId, "Screening task created successfully.");
    }
}
