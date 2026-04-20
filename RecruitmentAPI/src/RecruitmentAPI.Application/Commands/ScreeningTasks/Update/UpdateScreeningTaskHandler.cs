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

namespace RecruitmentAPI.Application.Commands.ScreeningTasks.Update;

public class UpdateScreeningTaskHandler : IRequestHandler<UpdateScreeningTaskCommand, Response<string>>
{
    private readonly ICommandRepository<ScreeningTask> _commandRepository;
    private readonly IQueryRepository<ScreeningTask> _queryRepository;
    private readonly ICommandRepository<AppEntity> _applicationCommandRepo;
    private readonly IQueryRepository<AppEntity> _applicationQueryRepo;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UpdateScreeningTaskHandler(
        ICommandRepository<ScreeningTask> commandRepository,
        IQueryRepository<ScreeningTask> queryRepository,
        ICommandRepository<AppEntity> applicationCommandRepo,
        IQueryRepository<AppEntity> applicationQueryRepo,
        IHttpContextAccessor httpContextAccessor)
    {
        _commandRepository = commandRepository;
        _queryRepository = queryRepository;
        _applicationCommandRepo = applicationCommandRepo;
        _applicationQueryRepo = applicationQueryRepo;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<Response<string>> Handle(UpdateScreeningTaskCommand request, CancellationToken cancellationToken)
    {
        var task = await _queryRepository.GetByIdAsync(request.ScreeningTaskId);
        if (task is null || task.IsDeleted)
            throw new NotFoundException("ScreeningTask", request.ScreeningTaskId);

        var dto = request.Task;
        var userName = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Name)?.Value;

        if (dto.ExpectedSalary.HasValue) task.ExpectedSalary = dto.ExpectedSalary;
        if (dto.NoticePeriodDays.HasValue) task.NoticePeriodDays = dto.NoticePeriodDays;
        if (dto.WillingnessToRelocate.HasValue) task.WillingnessToRelocate = dto.WillingnessToRelocate;
        if (dto.CandidateAvailability is not null) task.CandidateAvailability = dto.CandidateAvailability;
        if (dto.PreferredLocation is not null) task.PreferredLocation = dto.PreferredLocation;
        if (dto.Notes is not null) task.Notes = dto.Notes;
        if (dto.Status.HasValue) task.Status = dto.Status.Value;
        if (dto.Deadline.HasValue) task.Deadline = dto.Deadline;

        if (task.Status == ScreeningTaskStatus.Completed && task.CompletedDate is null)
            task.CompletedDate = DateTime.UtcNow;

        task.ModifiedDate = DateTime.UtcNow;
        task.ModifiedBy = userName;

        await _commandRepository.UpdateAsync(task);

        // When task is completed, move application to Shortlisted if still in Screening
        if (task.Status == ScreeningTaskStatus.Completed)
        {
            var application = await _applicationQueryRepo.GetByIdAsync(task.ApplicationId);
            if (application is not null && application.Status == ApplicationStatus.Screening)
            {
                application.Status = ApplicationStatus.Shortlisted;
                application.ModifiedDate = DateTime.UtcNow;
                application.ModifiedBy = userName;
                await _applicationCommandRepo.UpdateAsync(application);
            }
        }

        return Response<string>.SuccessResponse("Updated", "Screening task updated successfully.");
    }
}
