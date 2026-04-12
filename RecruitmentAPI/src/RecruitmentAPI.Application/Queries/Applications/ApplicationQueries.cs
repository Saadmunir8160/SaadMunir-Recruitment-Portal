using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RecruitmentAPI.Application.Common.Exceptions;
using RecruitmentAPI.Application.Common.Extensions;
using RecruitmentAPI.Application.DTOs;
using RecruitmentAPI.Application.DTOs.Applications;
using RecruitmentAPI.Domain.Entities;
using RecruitmentAPI.Domain.Repositories.Query.Base;
using AppEntity = RecruitmentAPI.Domain.Entities.Application;

namespace RecruitmentAPI.Application.Queries.Applications;

// Get all applications for a vacancy (paginated)
public class GetApplicationsByVacancyQuery : IRequest<PaginatedResponse<ApplicationDto>>
{
    public long VacancyId { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public class GetApplicationsByVacancyHandler : IRequestHandler<GetApplicationsByVacancyQuery, PaginatedResponse<ApplicationDto>>
{
    private readonly IQueryRepository<AppEntity> _queryRepository;
    private readonly IMapper _mapper;

    public GetApplicationsByVacancyHandler(IQueryRepository<AppEntity> queryRepository, IMapper mapper)
    {
        _queryRepository = queryRepository;
        _mapper = mapper;
    }

    public async Task<PaginatedResponse<ApplicationDto>> Handle(GetApplicationsByVacancyQuery request, CancellationToken cancellationToken)
    {
        var query = _queryRepository.GetQueryable()
            .Where(a => a.VacancyId == request.VacancyId && !a.IsDeleted)
            .OrderByDescending(a => a.ApplicationDate)
            .ProjectTo<ApplicationDto>(_mapper.ConfigurationProvider);

        return await query.ToPaginatedResponseAsync(new PaginationParameters
        {
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        });
    }
}

// Get application by ID (with details)
public class GetApplicationByIdQuery : IRequest<Response<ApplicationDetailDto>>
{
    public long ApplicationId { get; set; }
}

public class GetApplicationByIdHandler : IRequestHandler<GetApplicationByIdQuery, Response<ApplicationDetailDto>>
{
    private readonly IQueryRepository<AppEntity> _queryRepository;
    private readonly IMapper _mapper;

    public GetApplicationByIdHandler(IQueryRepository<AppEntity> queryRepository, IMapper mapper)
    {
        _queryRepository = queryRepository;
        _mapper = mapper;
    }

    public async Task<Response<ApplicationDetailDto>> Handle(GetApplicationByIdQuery request, CancellationToken cancellationToken)
    {
        var application = await _queryRepository.GetQueryable()
            .Include(a => a.Candidate)
            .Include(a => a.Vacancy)
            .Include(a => a.MatchResult)
            .Include(a => a.ScreeningTasks.Where(s => !s.IsDeleted))
            .Include(a => a.Interviews.Where(i => !i.IsDeleted))
            .FirstOrDefaultAsync(a => a.ApplicationId == request.ApplicationId && !a.IsDeleted, cancellationToken);

        if (application is null)
            throw new NotFoundException(nameof(AppEntity), request.ApplicationId);

        var dto = _mapper.Map<ApplicationDetailDto>(application);
        return Response<ApplicationDetailDto>.SuccessResponse(dto);
    }
}

// Get match result for an application
public class GetMatchResultQuery : IRequest<Response<MatchResultDto>>
{
    public long ApplicationId { get; set; }
}

public class GetMatchResultHandler : IRequestHandler<GetMatchResultQuery, Response<MatchResultDto>>
{
    private readonly IQueryRepository<MatchResult> _queryRepository;
    private readonly IMapper _mapper;

    public GetMatchResultHandler(IQueryRepository<MatchResult> queryRepository, IMapper mapper)
    {
        _queryRepository = queryRepository;
        _mapper = mapper;
    }

    public async Task<Response<MatchResultDto>> Handle(GetMatchResultQuery request, CancellationToken cancellationToken)
    {
        var matchResult = await _queryRepository.GetQueryable()
            .FirstOrDefaultAsync(mr => mr.ApplicationId == request.ApplicationId, cancellationToken);

        if (matchResult is null)
            throw new NotFoundException("MatchResult", request.ApplicationId);

        var dto = _mapper.Map<MatchResultDto>(matchResult);
        return Response<MatchResultDto>.SuccessResponse(dto);
    }
}
