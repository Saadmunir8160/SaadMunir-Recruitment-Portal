using MediatR;
using RecruitmentAPI.Application.DTOs;
using RecruitmentAPI.Application.DTOs.Applications;

namespace RecruitmentAPI.Application.Commands.Applications.Create;

public class CreateApplicationCommand : IRequest<Response<long>>
{
    public long VacancyId { get; set; }
}
