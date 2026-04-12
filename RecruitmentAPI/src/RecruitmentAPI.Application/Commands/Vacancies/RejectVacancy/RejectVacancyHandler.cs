using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using RecruitmentAPI.Application.Common;
using RecruitmentAPI.Application.DTOs;
using RecruitmentAPI.Domain.Entities;
using RecruitmentAPI.Domain.Enums;
using RecruitmentAPI.Domain.Repositories.Command.Base;
using RecruitmentAPI.Domain.Repositories.Query.Base;
using System.Security.Claims;

namespace RecruitmentAPI.Application.Commands.Vacancies.RejectVacancy;

public class RejectVacancyHandler : IRequestHandler<RejectVacancyCommand, Response<bool>>
{
    private readonly ICommandRepository<Vacancy> _vacancyCommand;
    private readonly IQueryRepository<Vacancy> _vacancyQuery;
    private readonly ICommandRepository<VacancyApproval> _approvalCommand;
    private readonly IQueryRepository<VacancyApproval> _approvalQuery;
    private readonly ICommandRepository<Notification> _notificationCommand;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public RejectVacancyHandler(
        ICommandRepository<Vacancy> vacancyCommand,
        IQueryRepository<Vacancy> vacancyQuery,
        ICommandRepository<VacancyApproval> approvalCommand,
        IQueryRepository<VacancyApproval> approvalQuery,
        ICommandRepository<Notification> notificationCommand,
        IHttpContextAccessor httpContextAccessor)
    {
        _vacancyCommand = vacancyCommand;
        _vacancyQuery = vacancyQuery;
        _approvalCommand = approvalCommand;
        _approvalQuery = approvalQuery;
        _notificationCommand = notificationCommand;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<Response<bool>> Handle(RejectVacancyCommand request, CancellationToken cancellationToken)
    {
        var vacancy = await _vacancyQuery.GetQueryable()
            .FirstOrDefaultAsync(v => v.VacancyId == request.VacancyId, cancellationToken);

        if (vacancy == null)
            return Response<bool>.FailureResponse("Vacancy not found.");

        if (vacancy.PublishStatus != VacancyPublishStatus.PendingApproval)
            return Response<bool>.FailureResponse("Vacancy is not pending approval.");

        var userName = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Name)?.Value;
        var userId = _httpContextAccessor.HttpContext?.User?.FindFirst("UserId")?.Value;

        // Find the current pending approval step
        var currentApproval = await _approvalQuery.GetQueryable()
            .Where(a => a.VacancyId == request.VacancyId && a.Status == ApprovalStatus.Pending)
            .OrderBy(a => a.ApprovalStep)
            .FirstOrDefaultAsync(cancellationToken);

        if (currentApproval == null)
            return Response<bool>.FailureResponse("No pending approval step found.");

        var roles = VacancyApprovalAuthorization.GetRoleSet(_httpContextAccessor.HttpContext?.User);
        if (!VacancyApprovalAuthorization.CanActOnApprovalStep(roles, currentApproval.ApprovalStep))
            return Response<bool>.FailureResponse(
                $"Your role cannot reject step {currentApproval.ApprovalStep} ({currentApproval.ApprovalStepName}).");

        // Mark the current step as rejected
        currentApproval.Status = ApprovalStatus.Rejected;
        currentApproval.ApproverUserId = userId;
        currentApproval.ApproverName = userName;
        currentApproval.Comments = request.Reason;
        currentApproval.ActionDate = DateTime.UtcNow;
        await _approvalCommand.UpdateAsync(currentApproval);

        // Revert vacancy back to Draft
        vacancy.PublishStatus = VacancyPublishStatus.Draft;
        await _vacancyCommand.UpdateAsync(vacancy);

        if (!string.IsNullOrWhiteSpace(vacancy.CreatedByUserId))
        {
            var reason = string.IsNullOrWhiteSpace(request.Reason) ? "No reason provided." : request.Reason.Trim();
            var note = new Notification
            {
                RecipientUserId = vacancy.CreatedByUserId,
                Channel = NotificationChannel.InApp,
                Subject = $"Vacancy \"{vacancy.JobTitle}\" was rejected",
                Body =
                    $"Your vacancy (requisition {vacancy.RequisitionNumber}) was rejected at step {currentApproval.ApprovalStep} ({currentApproval.ApprovalStepName}). Reason: {reason}",
                EntityType = nameof(Vacancy),
                EntityId = vacancy.VacancyId,
                TemplateCode = "VacancyApprovalRejected",
                Status = NotificationStatus.Pending,
                CreatedDate = DateTime.UtcNow
            };
            await _notificationCommand.AddAsync(note);
        }

        return Response<bool>.SuccessResponse(true, $"Vacancy rejected at Step {currentApproval.ApprovalStep} ({currentApproval.ApprovalStepName}). Reverted to Draft.");
    }
}
