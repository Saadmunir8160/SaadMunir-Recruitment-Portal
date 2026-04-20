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

namespace RecruitmentAPI.Application.Commands.Interviews.Evaluate;

public class CreateInterviewEvaluationHandler : IRequestHandler<CreateInterviewEvaluationCommand, Response<long>>
{
    private readonly ICommandRepository<InterviewEvaluation> _commandRepository;
    private readonly ICommandRepository<Interview> _interviewCommandRepo;
    private readonly IQueryRepository<Interview> _interviewQueryRepo;
    private readonly ICommandRepository<AppEntity> _applicationCommandRepo;
    private readonly IQueryRepository<AppEntity> _applicationQueryRepo;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CreateInterviewEvaluationHandler(
        ICommandRepository<InterviewEvaluation> commandRepository,
        ICommandRepository<Interview> interviewCommandRepo,
        IQueryRepository<Interview> interviewQueryRepo,
        ICommandRepository<AppEntity> applicationCommandRepo,
        IQueryRepository<AppEntity> applicationQueryRepo,
        IHttpContextAccessor httpContextAccessor)
    {
        _commandRepository = commandRepository;
        _interviewCommandRepo = interviewCommandRepo;
        _interviewQueryRepo = interviewQueryRepo;
        _applicationCommandRepo = applicationCommandRepo;
        _applicationQueryRepo = applicationQueryRepo;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<Response<long>> Handle(CreateInterviewEvaluationCommand request, CancellationToken cancellationToken)
    {
        var interview = await _interviewQueryRepo.GetByIdAsync(request.InterviewId);
        if (interview is null || interview.IsDeleted)
            throw new NotFoundException("Interview", request.InterviewId);

        var dto = request.Evaluation;
        var userId = _httpContextAccessor.HttpContext?.User?.FindFirst("UserId")?.Value;
        var userName = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Name)?.Value;

        var evaluation = new InterviewEvaluation
        {
            InterviewId = request.InterviewId,
            ApplicationId = interview.ApplicationId,
            EvaluationType = dto.EvaluationType,
            EvaluatorUserId = userId,
            EvaluatorName = userName,
            TechnicalScore = dto.TechnicalScore,
            CommunicationScore = dto.CommunicationScore,
            ProblemSolvingScore = dto.ProblemSolvingScore,
            LeadershipScore = dto.LeadershipScore,
            CulturalFitScore = dto.CulturalFitScore,
            OverallScore = dto.OverallScore,
            Strengths = dto.Strengths,
            Weaknesses = dto.Weaknesses,
            Notes = dto.Notes,
            Recommendation = dto.Recommendation,
            Decision = dto.Decision,
            RejectionReason = dto.RejectionReason,
            EvaluationDate = DateTime.UtcNow
        };

        await _commandRepository.AddAsync(evaluation);

        // Mark interview as Completed
        interview.Status = InterviewStatus.Completed;
        interview.ModifiedDate = DateTime.UtcNow;
        interview.ModifiedBy = userName;
        await _interviewCommandRepo.UpdateAsync(interview);

        // Advance application status based on decision
        var application = await _applicationQueryRepo.GetByIdAsync(interview.ApplicationId);
        if (application is not null)
        {
            application.Status = dto.Decision switch
            {
                EvaluationDecision.Rejected => ApplicationStatus.Rejected,
                EvaluationDecision.Accepted => ApplicationStatus.InterviewCompleted,
                _ => application.Status // Hold — no change
            };

            if (dto.Decision == EvaluationDecision.Rejected)
            {
                application.RejectionReason = dto.RejectionReason;
                application.RejectionPhase = "Interview";
            }

            application.ModifiedDate = DateTime.UtcNow;
            application.ModifiedBy = userName;
            await _applicationCommandRepo.UpdateAsync(application);
        }

        return Response<long>.SuccessResponse(evaluation.InterviewEvaluationId, "Interview evaluation submitted successfully.");
    }
}
