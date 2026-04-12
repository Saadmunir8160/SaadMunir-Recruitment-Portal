using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using RecruitmentAPI.Application.DTOs;
using RecruitmentAPI.Domain.Entities;
using RecruitmentAPI.Domain.Enums;
using RecruitmentAPI.Domain.Repositories.Command.Base;
using RecruitmentAPI.Domain.Repositories.Query.Base;
using System.Security.Claims;

namespace RecruitmentAPI.Application.Commands.Vacancies.Create;

public class CreateVacancyHandler : IRequestHandler<CreateVacancyCommand, Response<long>>
{
    private readonly ICommandRepository<Vacancy> _commandRepository;
    private readonly IQueryRepository<Vacancy> _queryRepository;
    private readonly IMapper _mapper;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CreateVacancyHandler(
        ICommandRepository<Vacancy> commandRepository,
        IQueryRepository<Vacancy> queryRepository,
        IMapper mapper,
        IHttpContextAccessor httpContextAccessor)
    {
        _commandRepository = commandRepository;
        _queryRepository = queryRepository;
        _mapper = mapper;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<Response<long>> Handle(CreateVacancyCommand request, CancellationToken cancellationToken)
    {
        var userId = _httpContextAccessor.HttpContext?.User?.FindFirst("UserId")?.Value;
        var userName = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Name)?.Value;

        var vacancy = _mapper.Map<Vacancy>(request.Vacancy);

        // Auto-generate requisition number: REQ-{YEAR}-{SEQUENCE:0000}
        var year = DateTime.UtcNow.Year;
        var count = await _queryRepository.GetQueryable()
            .CountAsync(v => v.CreatedDate.Year == year, cancellationToken) + 1;
        vacancy.RequisitionNumber = $"REQ-{year}-{count:D4}";

        vacancy.PublishStatus = VacancyPublishStatus.Draft;
        vacancy.CreatedByUserId = userId;
        vacancy.CreatedBy = userName;

        await _commandRepository.AddAsync(vacancy);

        return Response<long>.SuccessResponse(vacancy.VacancyId, "Vacancy created successfully.");
    }
}
