using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RecruitmentAPI.Application.Common.Exceptions;
using RecruitmentAPI.Application.Common.Extensions;
using RecruitmentAPI.Application.DTOs;
using RecruitmentAPI.Application.DTOs.Candidates;
using RecruitmentAPI.Domain.Entities;
using RecruitmentAPI.Domain.Repositories.Query.Base;

namespace RecruitmentAPI.Application.Queries.Candidates;

// Get all candidates (paginated)
public class GetAllCandidatesQuery : IRequest<PaginatedResponse<CandidateDto>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public class GetAllCandidatesHandler : IRequestHandler<GetAllCandidatesQuery, PaginatedResponse<CandidateDto>>
{
    private readonly IQueryRepository<Candidate> _queryRepository;
    private readonly IMapper _mapper;

    public GetAllCandidatesHandler(IQueryRepository<Candidate> queryRepository, IMapper mapper)
    {
        _queryRepository = queryRepository;
        _mapper = mapper;
    }

    public async Task<PaginatedResponse<CandidateDto>> Handle(GetAllCandidatesQuery request, CancellationToken cancellationToken)
    {
        var query = _queryRepository.GetQueryable()
            .Where(c => !c.IsDeleted)
            .OrderByDescending(c => c.CreatedDate)
            .ProjectTo<CandidateDto>(_mapper.ConfigurationProvider);

        return await query.ToPaginatedResponseAsync(new PaginationParameters
        {
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        });
    }
}

// Get candidate by ID (with details)
public class GetCandidateByIdQuery : IRequest<Response<CandidateDetailDto>>
{
    public long CandidateId { get; set; }
}

public class GetCandidateByIdHandler : IRequestHandler<GetCandidateByIdQuery, Response<CandidateDetailDto>>
{
    private readonly IQueryRepository<Candidate> _queryRepository;
    private readonly IMapper _mapper;

    public GetCandidateByIdHandler(IQueryRepository<Candidate> queryRepository, IMapper mapper)
    {
        _queryRepository = queryRepository;
        _mapper = mapper;
    }

    public async Task<Response<CandidateDetailDto>> Handle(GetCandidateByIdQuery request, CancellationToken cancellationToken)
    {
        var candidate = await _queryRepository.GetQueryable()
            .Include(c => c.Educations.Where(e => !e.IsDeleted))
            .Include(c => c.Experiences.Where(e => !e.IsDeleted))
            .Include(c => c.Documents.Where(d => !d.IsDeleted))
            .FirstOrDefaultAsync(c => c.CandidateId == request.CandidateId && !c.IsDeleted, cancellationToken);

        if (candidate is null)
            throw new NotFoundException(nameof(Candidate), request.CandidateId);

        var dto = _mapper.Map<CandidateDetailDto>(candidate);
        return Response<CandidateDetailDto>.SuccessResponse(dto);
    }
}
