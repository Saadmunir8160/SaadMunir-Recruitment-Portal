using MediatR;
using Microsoft.AspNetCore.Http;
using RecruitmentAPI.Application.Common.Exceptions;
using RecruitmentAPI.Application.DTOs;
using RecruitmentAPI.Domain.Enums;
using RecruitmentAPI.Domain.Repositories.Command.Base;
using RecruitmentAPI.Domain.Repositories.Query.Base;
using System.Security.Claims;
using AppEntity = RecruitmentAPI.Domain.Entities.Application;

namespace RecruitmentAPI.Application.Commands.Applications.UpdateStatus;

public class UpdateApplicationStatusHandler : IRequestHandler<UpdateApplicationStatusCommand, Response<string>>
{
    private readonly ICommandRepository<AppEntity> _commandRepository;
    private readonly IQueryRepository<AppEntity> _queryRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UpdateApplicationStatusHandler(
        ICommandRepository<AppEntity> commandRepository,
        IQueryRepository<AppEntity> queryRepository,
        IHttpContextAccessor httpContextAccessor)
    {
        _commandRepository = commandRepository;
        _queryRepository = queryRepository;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<Response<string>> Handle(UpdateApplicationStatusCommand request, CancellationToken cancellationToken)
    {
        var application = await _queryRepository.GetByIdAsync(request.ApplicationId);
        if (application is null || application.IsDeleted)
            throw new NotFoundException("Application", request.ApplicationId);

        application.Status = request.Status.NewStatus;

        if (request.Status.NewStatus == ApplicationStatus.Rejected)
        {
            application.RejectionReason = request.Status.Reason;
            application.RejectionPhase = "ManualOverride";
        }

        application.ModifiedDate = DateTime.UtcNow;
        application.ModifiedBy = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Name)?.Value;

        await _commandRepository.UpdateAsync(application);

        return Response<string>.SuccessResponse("Updated", "Application status updated successfully.");
    }
}
