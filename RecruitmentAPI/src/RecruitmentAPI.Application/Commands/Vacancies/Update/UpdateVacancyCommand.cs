using MediatR;
using RecruitmentAPI.Application.DTOs;
using RecruitmentAPI.Application.DTOs.Vacancies;

namespace RecruitmentAPI.Application.Commands.Vacancies.Update;

public class UpdateVacancyCommand : IRequest<Response<string>>
{
    public long VacancyId { get; set; }
    public CreateVacancyDto Vacancy { get; set; } = null!;
}
