using MediatR;
using RecruitmentAPI.Application.DTOs;

namespace RecruitmentAPI.Application.Commands.Vacancies.RejectVacancy;

public class RejectVacancyCommand : IRequest<Response<bool>>
{
    public long VacancyId { get; set; }
    public string? Reason { get; set; }
}
