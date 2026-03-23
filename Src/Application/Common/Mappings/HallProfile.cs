using Application.Halls.Commands;
using Application.Halls.Dtos;
using AutoMapper;
using Domain.Entities;
using FluentValidation;

namespace Application.Common.Mappings;

public class HallMappingProfile : Profile
{
    public HallMappingProfile()
    {
        CreateMap<CreateHallRequest, Hall>()
            .ForMember(dest => dest.Name,
                opt => opt.MapFrom(src => src.Name.Trim()));

        CreateMap<UpdateHallRequest, Hall>()
    .ForMember(dest => dest.Name,
        opt => opt.MapFrom(src => src.Name != null ? src.Name.Trim() : null))
    .ForMember(dest => dest.CinemaId, opt => opt.Ignore())
    .ForMember(dest => dest.Cinema, opt => opt.Ignore())
    .ForAllMembers(opt =>
        opt.Condition((src, dest, srcMember) => srcMember != null));
        CreateMap<Hall, GetHallByIdResponse>();
        CreateMap<Hall,GetAllHallsResponse>();
    }
}
