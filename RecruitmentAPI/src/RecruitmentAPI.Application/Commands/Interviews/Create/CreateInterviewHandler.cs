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

namespace RecruitmentAPI.Application.Commands.Interviews.Create;

public class CreateInterviewHandler : IRequestHandler<CreateInterviewCommand, Response<long>>
{
    private readonly ICommandRepository<Interview> _commandRepository;
    private readonly ICommandRepository<AppEntity> _applicationCommandRepo;
    private readonly IQueryRepository<AppEntity> _applicationQueryRepo;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CreateInterviewHandler(
        ICommandRepository<Interview> commandRepository,
        ICommandRepository<AppEntity> applicationCommandRepo,
        IQueryRepository<AppEntity> applicationQueryRepo,
        IHttpContextAccessor httpContextAccessor)
    {
        _commandRepository = commandRepository;
        _applicationCommandRepo = applicationCommandRepo;
        _applicationQueryRepo = applicationQueryRepo;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<Response<long>> Handle(CreateInterviewCommand request, CancellationToken cancellationToken)
    {
        var dto = request.Interview;

        var application = await _applicationQueryRepo.GetByIdAsync(dto.ApplicationId);
        if (application is null || application.IsDeleted)
            throw new NotFoundException("Application", dto.ApplicationId);

        var userName = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Name)?.Value;

        var interview = new Interview
        {
            ApplicationId = dto.ApplicationId,
            InterviewType = dto.InterviewType,
            InterviewMode = dto.InterviewMode,
            ScheduledDate = dto.ScheduledDate,
            ScheduledEndDate = dto.ScheduledEndDate,
            Location = dto.Location,
            InterviewerUserId = dto.InterviewerUserId,
            InterviewerName = dto.InterviewerName,
            Status = InterviewStatus.Scheduled,
            CreatedBy = userName
        };

        await _commandRepository.AddAsync(interview);

        // Advance application status to InterviewScheduled
        application.Status = ApplicationStatus.InterviewScheduled;
        application.ModifiedDate = DateTime.UtcNow;
        application.ModifiedBy = userName;
        await _applicationCommandRepo.UpdateAsync(application);

        return Response<long>.SuccessResponse(interview.InterviewId, "Interview scheduled successfully.");
    }
}
