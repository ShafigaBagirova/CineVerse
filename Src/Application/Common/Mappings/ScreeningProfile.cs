using Application.Screenings.Dtos;
using AutoMapper;
using Domain.Entities;

namespace Application.Common.Mappings;

public class ScreeningMappingProfile : Profile
{
    public ScreeningMappingProfile()
    {
        CreateMap<CreateScreeningRequest, Screening>()
            .ForMember(dest => dest.Language,
                opt => opt.MapFrom(src => src.Language.Trim()))
            .ForMember(dest => dest.SubtitleLanguage,
                opt => opt.MapFrom(src => string.IsNullOrWhiteSpace(src.SubtitleLanguage)
                    ? null
                    : src.SubtitleLanguage.Trim()));

        CreateMap<UpdateScreeningRequest, Screening>()
             .ForMember(dest => dest.Language, opt => opt.MapFrom(src => src.Language != null ? src.Language.Trim() : null))
           .ForMember(dest => dest.SubtitleLanguage,opt => opt.MapFrom(src => src.SubtitleLanguage != null ? src.SubtitleLanguage.Trim() : null))
           .ForMember(dest => dest.MovieId, opt => opt.Ignore())
           .ForMember(dest => dest.HallId, opt => opt.Ignore())
         .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

        CreateMap<Screening, GetScreeningByIdResponse>()
            .ForMember(dest => dest.MovieTitle,opt => opt.MapFrom(src => src.Movie.Title))
           .ForMember(dest => dest.HallName,opt => opt.MapFrom(src => src.Hall.Name));
      
        CreateMap<Screening, GetAllScreeningsResponse>()
          .ForMember(dest => dest.MovieTitle,opt => opt.MapFrom(src => src.Movie.Title))
             .ForMember(dest => dest.HallName,opt => opt.MapFrom(src => src.Hall.Name));
    }
}