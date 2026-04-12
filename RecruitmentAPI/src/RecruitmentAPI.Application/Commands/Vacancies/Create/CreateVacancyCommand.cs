using MediatR;
using RecruitmentAPI.Application.DTOs;
using RecruitmentAPI.Application.DTOs.Vacancies;

namespace RecruitmentAPI.Application.Commands.Vacancies.Create;

public class CreateVacancyCommand : IRequest<Response<long>>
{
    public CreateVacancyDto Vacancy { get; set; } = null!;
}
