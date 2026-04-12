using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RecruitmentAPI.Application.Common.Extensions;
using RecruitmentAPI.Application.DTOs;
using RecruitmentAPI.Application.DTOs.Applications;
using RecruitmentAPI.Domain.Entities;
using RecruitmentAPI.Domain.Repositories.Query.Base;

namespace RecruitmentAPI.Application.Queries.Interviews;

// Get all interviews (paginated)
public class GetAllInterviewsQuery : IRequest<PaginatedResponse<InterviewListDto>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public class GetAllInterviewsHandler : IRequestHandler<GetAllInterviewsQuery, PaginatedResponse<InterviewListDto>>
{
    private readonly IQueryRepository<Interview> _queryRepository;
    private readonly IMapper _mapper;

    public GetAllInterviewsHandler(IQueryRepository<Interview> queryRepository, IMapper mapper)
    {
        _queryRepository = queryRepository;
        _mapper = mapper;
    }

    public async Task<PaginatedResponse<InterviewListDto>> Handle(GetAllInterviewsQuery request, CancellationToken cancellationToken)
    {
        var query = _queryRepository.GetQueryable()
            .Where(i => !i.IsDeleted)
            .OrderByDescending(i => i.ScheduledDate)
            .ProjectTo<InterviewListDto>(_mapper.ConfigurationProvider);

        return await query.ToPaginatedResponseAsync(new PaginationParameters
        {
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        });
    }
}

// Get interviews by application
public class GetInterviewsByApplicationQuery : IRequest<Response<List<InterviewListDto>>>
{
    public long ApplicationId { get; set; }
}

public class GetInterviewsByApplicationHandler : IRequestHandler<GetInterviewsByApplicationQuery, Response<List<InterviewListDto>>>
{
    private readonly IQueryRepository<Interview> _queryRepository;
    private readonly IMapper _mapper;

    public GetInterviewsByApplicationHandler(IQueryRepository<Interview> queryRepository, IMapper mapper)
    {
        _queryRepository = queryRepository;
        _mapper = mapper;
    }

    public async Task<Response<List<InterviewListDto>>> Handle(GetInterviewsByApplicationQuery request, CancellationToken cancellationToken)
    {
        var interviews = await _queryRepository.GetQueryable()
            .Where(i => i.ApplicationId == request.ApplicationId && !i.IsDeleted)
            .OrderByDescending(i => i.ScheduledDate)
            .ProjectTo<InterviewListDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        return Response<List<InterviewListDto>>.SuccessResponse(interviews);
    }
}
