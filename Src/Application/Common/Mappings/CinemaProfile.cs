using Application.Cinemas.Dtos;
using AutoMapper;
using Domain.Entities;

namespace Application.Common.Mappings;

public class CinemaProfile : Profile
{
    public CinemaProfile()
    {
        CreateMap<CreateCinemaRequest, Cinema>()
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name.Trim()))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description != null ? src.Description.Trim() : null))
            .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Address.Trim()))
            .ForMember(dest => dest.City, opt => opt.MapFrom(src => src.City.Trim()))
            .ForMember(dest => dest.Country, opt => opt.MapFrom(src => src.Country.Trim()));
        CreateMap<UpdateCinemaRequest, Cinema>()
           .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name.Trim()))
           .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description != null ? src.Description.Trim() : null))
           .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Address.Trim()))
           .ForMember(dest => dest.Phone, opt => opt.MapFrom(src => src.Phone))
           .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email));
        CreateMap<Cinema, GetAllCinemasResponse>();
        CreateMap<Cinema, GetCinemaByIdResponse>();
    }

}