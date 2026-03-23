using Application.Seats.Dtos;
using AutoMapper;
using Domain.Entities;

namespace Application.Common.Mappings;

public sealed class CreateSeatMappingProfile : Profile
{
    public CreateSeatMappingProfile()
    {
        CreateMap<CreateSeatRequest, Seat>()
            .ForMember(dest => dest.Row, opt => opt.MapFrom(src => src.Row.Trim()));

        CreateMap<UpdateSeatRequest, Seat>()
            .ForMember(dest => dest.Row,
                opt => opt.MapFrom(src => src.Row != null ? src.Row.Trim().ToUpper() : null))
            .ForMember(dest => dest.HallId, opt => opt.Ignore())
            .ForAllMembers(opt =>
                opt.Condition((src, dest, srcMember) => srcMember != null));
             

        CreateMap<Seat, GetSeatByIdResponse>();
        CreateMap<Seat, GetAllSeatsResponse>();
    }
}