using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RecruitmentAPI.Application.Common.Exceptions;
using RecruitmentAPI.Application.Common.Extensions;
using RecruitmentAPI.Application.DTOs;
using RecruitmentAPI.Application.DTOs.Vacancies;
using RecruitmentAPI.Domain.Entities;
using RecruitmentAPI.Domain.Enums;
using RecruitmentAPI.Domain.Repositories.Query.Base;

namespace RecruitmentAPI.Application.Queries.Vacancies;

// Get all vacancies (paginated)
public class GetAllVacanciesQuery : IRequest<PaginatedResponse<VacancyDto>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public VacancyPublishStatus? StatusFilter { get; set; }
}

public class GetAllVacanciesHandler : IRequestHandler<GetAllVacanciesQuery, PaginatedResponse<VacancyDto>>
{
    private readonly IQueryRepository<Vacancy> _queryRepository;
    private readonly IMapper _mapper;

    public GetAllVacanciesHandler(IQueryRepository<Vacancy> queryRepository, IMapper mapper)
    {
        _queryRepository = queryRepository;
        _mapper = mapper;
    }

    public async Task<PaginatedResponse<VacancyDto>> Handle(GetAllVacanciesQuery request, CancellationToken cancellationToken)
    {
        var query = _queryRepository.GetQueryable()
            .Where(v => !v.IsDeleted);

        if (request.StatusFilter.HasValue)
            query = query.Where(v => v.PublishStatus == request.StatusFilter.Value);

        var projected = query
            .OrderByDescending(v => v.CreatedDate)
            .ProjectTo<VacancyDto>(_mapper.ConfigurationProvider);

        return await projected.ToPaginatedResponseAsync(new PaginationParameters
        {
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        });
    }
}

// Get vacancy by ID (with details)
public class GetVacancyByIdQuery : IRequest<Response<VacancyDetailDto>>
{
    public long VacancyId { get; set; }
}

public class GetVacancyByIdHandler : IRequestHandler<GetVacancyByIdQuery, Response<VacancyDetailDto>>
{
    private readonly IQueryRepository<Vacancy> _queryRepository;
    private readonly IMapper _mapper;

    public GetVacancyByIdHandler(IQueryRepository<Vacancy> queryRepository, IMapper mapper)
    {
        _queryRepository = queryRepository;
        _mapper = mapper;
    }

    public async Task<Response<VacancyDetailDto>> Handle(GetVacancyByIdQuery request, CancellationToken cancellationToken)
    {
        var vacancy = await _queryRepository.GetQueryable()
            .Include(v => v.Recruiters.Where(r => !r.IsDeleted))
            .Include(v => v.Approvals.Where(a => !a.IsDeleted))
            .FirstOrDefaultAsync(v => v.VacancyId == request.VacancyId && !v.IsDeleted, cancellationToken);

        if (vacancy is null)
            throw new NotFoundException(nameof(Vacancy), request.VacancyId);

        var dto = _mapper.Map<VacancyDetailDto>(vacancy);

        var pending = vacancy.Approvals
            .Where(a => !a.IsDeleted && a.Status == ApprovalStatus.Pending)
            .OrderBy(a => a.ApprovalStep)
            .FirstOrDefault();
        if (pending != null)
        {
            dto.PendingApprovalStep = pending.ApprovalStep;
            dto.PendingApprovalStepName = pending.ApprovalStepName;
        }

        return Response<VacancyDetailDto>.SuccessResponse(dto);
    }
}

// Get published vacancies (for candidates)
public class GetPublishedVacanciesQuery : IRequest<PaginatedResponse<VacancyDto>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SearchTerm { get; set; }
}

public class GetPublishedVacanciesHandler : IRequestHandler<GetPublishedVacanciesQuery, PaginatedResponse<VacancyDto>>
{
    private readonly IQueryRepository<Vacancy> _queryRepository;
    private readonly IMapper _mapper;

    public GetPublishedVacanciesHandler(IQueryRepository<Vacancy> queryRepository, IMapper mapper)
    {
        _queryRepository = queryRepository;
        _mapper = mapper;
    }

    public async Task<PaginatedResponse<VacancyDto>> Handle(GetPublishedVacanciesQuery request, CancellationToken cancellationToken)
    {
        var query = _queryRepository.GetQueryable()
            .Where(v => !v.IsDeleted && v.PublishStatus == VacancyPublishStatus.Published);

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = $"%{request.SearchTerm.Trim()}%";
            query = query.Where(v =>
                EF.Functions.Like(v.JobTitle, term) ||
                (v.DepartmentName != null && EF.Functions.Like(v.DepartmentName, term)) ||
                (v.Location != null && EF.Functions.Like(v.Location, term)));
        }

        var projected = query
            .OrderByDescending(v => v.PublishedDate)
            .ProjectTo<VacancyDto>(_mapper.ConfigurationProvider);

        return await projected.ToPaginatedResponseAsync(new PaginationParameters
        {
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        });
    }
}
