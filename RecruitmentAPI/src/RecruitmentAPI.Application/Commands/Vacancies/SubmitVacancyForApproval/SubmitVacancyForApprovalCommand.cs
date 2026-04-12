using MediatR;
using RecruitmentAPI.Application.DTOs;

namespace RecruitmentAPI.Application.Commands.Vacancies.SubmitVacancyForApproval;

public class SubmitVacancyForApprovalCommand : IRequest<Response<bool>>
{
    public long VacancyId { get; set; }
}
