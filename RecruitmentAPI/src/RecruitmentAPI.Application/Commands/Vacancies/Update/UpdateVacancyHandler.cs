using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using RecruitmentAPI.Application.Common.Exceptions;
using RecruitmentAPI.Application.DTOs;
using RecruitmentAPI.Domain.Entities;
using RecruitmentAPI.Domain.Repositories.Command.Base;
using RecruitmentAPI.Domain.Repositories.Query.Base;
using System.Security.Claims;

namespace RecruitmentAPI.Application.Commands.Vacancies.Update;

public class UpdateVacancyHandler : IRequestHandler<UpdateVacancyCommand, Response<string>>
{
    private readonly ICommandRepository<Vacancy> _commandRepository;
    private readonly IQueryRepository<Vacancy> _queryRepository;
    private readonly IMapper _mapper;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UpdateVacancyHandler(
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

    public async Task<Response<string>> Handle(UpdateVacancyCommand request, CancellationToken cancellationToken)
    {
        var vacancy = await _queryRepository.GetByIdAsync(request.VacancyId);
        if (vacancy is null || vacancy.IsDeleted)
            throw new NotFoundException(nameof(Vacancy), request.VacancyId);

        _mapper.Map(request.Vacancy, vacancy);
        vacancy.ModifiedDate = DateTime.UtcNow;
        vacancy.ModifiedBy = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Name)?.Value;

        await _commandRepository.UpdateAsync(vacancy);

        return Response<string>.SuccessResponse("Updated", "Vacancy updated successfully.");
    }
}
