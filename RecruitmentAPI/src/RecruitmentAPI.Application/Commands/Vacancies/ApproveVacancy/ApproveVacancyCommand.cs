using MediatR;
using RecruitmentAPI.Application.DTOs;

namespace RecruitmentAPI.Application.Commands.Vacancies.ApproveVacancy;

public class ApproveVacancyCommand : IRequest<Response<bool>>
{
    public long VacancyId { get; set; }
    public string? Comments { get; set; }
}
