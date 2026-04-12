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

namespace RecruitmentAPI.Application.Commands.Vacancies.SubmitVacancyForApproval;

public class SubmitVacancyForApprovalHandler : IRequestHandler<SubmitVacancyForApprovalCommand, Response<bool>>
{
    private readonly ICommandRepository<Vacancy> _vacancyCommand;
    private readonly IQueryRepository<Vacancy> _vacancyQuery;
    private readonly IQueryRepository<VacancyApproval> _approvalQuery;
    private readonly ICommandRepository<VacancyApproval> _approvalCommand;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public SubmitVacancyForApprovalHandler(
        ICommandRepository<Vacancy> vacancyCommand,
        IQueryRepository<Vacancy> vacancyQuery,
        IQueryRepository<VacancyApproval> approvalQuery,
        ICommandRepository<VacancyApproval> approvalCommand,
        IHttpContextAccessor httpContextAccessor)
    {
        _vacancyCommand = vacancyCommand;
        _vacancyQuery = vacancyQuery;
        _approvalQuery = approvalQuery;
        _approvalCommand = approvalCommand;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<Response<bool>> Handle(SubmitVacancyForApprovalCommand request, CancellationToken cancellationToken)
    {
        var roles = VacancyApprovalAuthorization.GetRoleSet(_httpContextAccessor.HttpContext?.User);
        if (!VacancyApprovalAuthorization.CanSubmitForApproval(roles))
            return Response<bool>.FailureResponse("You are not authorized to submit vacancies for approval.");

        var vacancy = await _vacancyQuery.GetQueryable()
            .FirstOrDefaultAsync(v => v.VacancyId == request.VacancyId, cancellationToken);

        if (vacancy == null)
            return Response<bool>.FailureResponse("Vacancy not found.");

        if (vacancy.PublishStatus != VacancyPublishStatus.Draft)
            return Response<bool>.FailureResponse("Only Draft vacancies can be submitted for approval.");

        var alreadyPending = await _approvalQuery.GetQueryable()
            .AnyAsync(
                a => a.VacancyId == request.VacancyId && !a.IsDeleted && a.Status == ApprovalStatus.Pending,
                cancellationToken);
        if (alreadyPending)
            return Response<bool>.FailureResponse("This vacancy already has a pending approval step.");

        var userName = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Name)?.Value;

        // Update vacancy status to PendingApproval
        vacancy.PublishStatus = VacancyPublishStatus.PendingApproval;
        await _vacancyCommand.UpdateAsync(vacancy);

        // Create Step 1 approval row for HR Manager
        var approval = new VacancyApproval
        {
            VacancyId = request.VacancyId,
            ApprovalStep = 1,
            ApprovalStepName = "HR Manager Approval",
            Status = ApprovalStatus.Pending,
            CreatedBy = userName
        };

        await _approvalCommand.AddAsync(approval);

        return Response<bool>.SuccessResponse(true, "Vacancy submitted for approval. Awaiting HR Manager review.");
    }
}
