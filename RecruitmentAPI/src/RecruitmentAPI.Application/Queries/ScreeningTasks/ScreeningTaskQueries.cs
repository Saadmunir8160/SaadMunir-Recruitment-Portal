using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RecruitmentAPI.Application.DTOs;
using RecruitmentAPI.Application.DTOs.Applications;
using RecruitmentAPI.Domain.Entities;
using RecruitmentAPI.Domain.Repositories.Query.Base;

namespace RecruitmentAPI.Application.Queries.ScreeningTasks;

// Get screening tasks by application
public class GetScreeningTasksByApplicationQuery : IRequest<Response<List<ScreeningTaskDto>>>
{
    public long ApplicationId { get; set; }
}

public class GetScreeningTasksByApplicationHandler : IRequestHandler<GetScreeningTasksByApplicationQuery, Response<List<ScreeningTaskDto>>>
{
    private readonly IQueryRepository<ScreeningTask> _queryRepository;
    private readonly IMapper _mapper;

    public GetScreeningTasksByApplicationHandler(IQueryRepository<ScreeningTask> queryRepository, IMapper mapper)
    {
        _queryRepository = queryRepository;
        _mapper = mapper;
    }

    public async Task<Response<List<ScreeningTaskDto>>> Handle(GetScreeningTasksByApplicationQuery request, CancellationToken cancellationToken)
    {
        var tasks = await _queryRepository.GetQueryable()
            .Where(t => t.ApplicationId == request.ApplicationId && !t.IsDeleted)
            .OrderBy(t => t.CreatedDate)
            .ProjectTo<ScreeningTaskDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        return Response<List<ScreeningTaskDto>>.SuccessResponse(tasks);
    }
}
