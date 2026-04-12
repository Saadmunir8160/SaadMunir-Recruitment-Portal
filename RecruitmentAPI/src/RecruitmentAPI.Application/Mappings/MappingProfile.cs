using AutoMapper;
using RecruitmentAPI.Application.DTOs.Applications;
using RecruitmentAPI.Application.DTOs.Candidates;
using RecruitmentAPI.Application.DTOs.Documents;
using RecruitmentAPI.Application.DTOs.Vacancies;
using RecruitmentAPI.Domain.Entities;

namespace RecruitmentAPI.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Candidate
        CreateMap<Candidate, CandidateDto>();
        CreateMap<Candidate, CandidateDetailDto>();
        CreateMap<CandidateEducation, CandidateEducationDto>();
        CreateMap<CandidateExperience, CandidateExperienceDto>();
        CreateMap<CandidateDocument, CandidateDocumentDto>();
        CreateMap<CreateCandidateDto, Candidate>();
        CreateMap<UpdateCandidateDto, Candidate>();

        // Vacancy
        CreateMap<Vacancy, VacancyDto>();
        CreateMap<Vacancy, VacancyDetailDto>();
        CreateMap<VacancyRecruiter, VacancyRecruiterDto>();
        CreateMap<CreateVacancyDto, Vacancy>();

        // Application
        CreateMap<global::RecruitmentAPI.Domain.Entities.Application, ApplicationDto>()
            .ForMember(d => d.CandidateName, opt => opt.MapFrom(s => s.Candidate.FullName))
            .ForMember(d => d.VacancyTitle, opt => opt.MapFrom(s => s.Vacancy.JobTitle));
        CreateMap<global::RecruitmentAPI.Domain.Entities.Application, ApplicationDetailDto>()
            .ForMember(d => d.CandidateName, opt => opt.MapFrom(s => s.Candidate.FullName))
            .ForMember(d => d.VacancyTitle, opt => opt.MapFrom(s => s.Vacancy.JobTitle));
        CreateMap<MatchResult, MatchResultDto>();
        CreateMap<ScreeningTask, ScreeningTaskDto>();
        CreateMap<Interview, InterviewDto>();
        CreateMap<Interview, InterviewListDto>()
            .ForMember(d => d.CandidateName, opt => opt.MapFrom(s => s.Application.Candidate.FullName));
    }
}
