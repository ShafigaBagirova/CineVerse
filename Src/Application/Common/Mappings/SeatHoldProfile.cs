using Application.SeatHolds.Dtos;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;

namespace Application.Common.Mappings;

public sealed class SeatHoldMappingProfile : Profile
{
    public SeatHoldMappingProfile()
    {
        CreateMap<CreateSeatHoldRequest, SeatHold>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.UserId, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.Ignore())
            .ForMember(dest => dest.ExpiresAtUtc, opt => opt.Ignore())
            .ForMember(dest => dest.Screening, opt => opt.Ignore())
            .ForMember(dest => dest.Seat, opt => opt.Ignore());
        CreateMap<SeatHold, GetSeatHoldByIdResponse>()
    .ForMember(dest => dest.Status,
        opt => opt.MapFrom(src => src.Status.ToString()));
        CreateMap<SeatHold, GetAllSeatHoldsResponse>()
    .ForMember(dest => dest.Status,
        opt => opt.MapFrom(src => src.Status.ToString()));
        CreateMap<SeatHold, GetSeatHoldsByScreeningResponse>()
    .ForMember(dest => dest.Status,
        opt => opt.MapFrom(src => src.Status.ToString()));
    }
}