using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RecruitmentAPI.Application.Commands.Applications.Match;
using RecruitmentAPI.Application.Common.Exceptions;
using RecruitmentAPI.Application.Common.Interfaces;
using RecruitmentAPI.Application.DTOs;
using RecruitmentAPI.Domain.Entities;
using RecruitmentAPI.Domain.Enums;
using RecruitmentAPI.Domain.Repositories.Command.Base;
using RecruitmentAPI.Domain.Repositories.Query.Base;
using System.Security.Claims;
using AppEntity = RecruitmentAPI.Domain.Entities.Application;

namespace RecruitmentAPI.Application.Commands.Applications.Create;

public class CreateApplicationHandler : IRequestHandler<CreateApplicationCommand, Response<long>>
{
    private readonly ICommandRepository<AppEntity> _commandRepository;
    private readonly IQueryRepository<AppEntity> _applicationQueryRepo;
    private readonly IQueryRepository<Candidate> _candidateQueryRepo;
    private readonly IQueryRepository<Vacancy> _vacancyQueryRepo;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IServiceScopeFactory _scopeFactory;

    public CreateApplicationHandler(
        ICommandRepository<AppEntity> commandRepository,
        IQueryRepository<AppEntity> applicationQueryRepo,
        IQueryRepository<Candidate> candidateQueryRepo,
        IQueryRepository<Vacancy> vacancyQueryRepo,
        IHttpContextAccessor httpContextAccessor,
        IServiceScopeFactory scopeFactory)
    {
        _commandRepository = commandRepository;
        _applicationQueryRepo = applicationQueryRepo;
        _candidateQueryRepo = candidateQueryRepo;
        _vacancyQueryRepo = vacancyQueryRepo;
        _httpContextAccessor = httpContextAccessor;
        _scopeFactory = scopeFactory;
    }

    public async Task<Response<long>> Handle(CreateApplicationCommand request, CancellationToken cancellationToken)
    {
        var userId = _httpContextAccessor.HttpContext?.User?.FindFirst("UserId")?.Value;
        if (string.IsNullOrEmpty(userId))
            return Response<long>.FailureResponse("User not authenticated.");

        // Get candidate for current user
        var candidate = await _candidateQueryRepo.GetQueryable()
            .FirstOrDefaultAsync(c => c.UserId == userId && !c.IsDeleted, cancellationToken);

        if (candidate is null)
            throw new NotFoundException("Candidate profile not found. Please create a profile first.");

        if (candidate.ProfileStatus != CandidateProfileStatus.Submitted &&
            candidate.ProfileStatus != CandidateProfileStatus.Approved)
        {
            throw new BadRequestException("Profile must be submitted before applying to vacancies.");
        }

        // Verify vacancy exists and is published
        var vacancy = await _vacancyQueryRepo.GetByIdAsync(request.VacancyId);
        if (vacancy is null || vacancy.IsDeleted)
            throw new NotFoundException(nameof(Vacancy), request.VacancyId);

        if (vacancy.PublishStatus != VacancyPublishStatus.Published)
            throw new BadRequestException("Vacancy is not open for applications.");

        if (vacancy.ClosingDate.HasValue && vacancy.ClosingDate.Value < DateTime.UtcNow)
            throw new BadRequestException("Vacancy application deadline has passed.");

        // Check duplicate application (CandidateId, VacancyId unique)
        var existingApplication = await _applicationQueryRepo.GetQueryable()
            .AnyAsync(a => a.CandidateId == candidate.CandidateId
                && a.VacancyId == request.VacancyId
                && !a.IsDeleted, cancellationToken);

        if (existingApplication)
            throw new BadRequestException("You have already applied to this vacancy.");

        var application = new AppEntity
        {
            CandidateId = candidate.CandidateId,
            VacancyId = request.VacancyId,
            ApplicationDate = DateTime.UtcNow,
            Status = ApplicationStatus.Applied,
            CreatedBy = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Name)?.Value
        };

        await _commandRepository.AddAsync(application);

        // Fire AI matching in background using its own DI scope to avoid DbContext disposal issues.
        var applicationId = application.ApplicationId;
        _ = Task.Run(async () =>
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            var matchingService = scope.ServiceProvider.GetRequiredService<IMatchingService>();
            await matchingService.MatchAsync(applicationId);
        }, CancellationToken.None);

        return Response<long>.SuccessResponse(application.ApplicationId, "Application submitted successfully.");
    }
}
