using AutoMapper;
using Domain.Entities;
using Application.DTOs;

namespace Application.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<CoverageArea, CoverageAreaResponseDTO>()
                .ForMember(dest => dest.CoverageAreaId, opt => opt.MapFrom(src => src.CoverageAreaId))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name));
        }
    }
} 