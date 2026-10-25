using Application.WatchRooms.Dtos;
using AutoMapper;
using Domain.Entities;

namespace Application.Common.Mappings;

public class WatchRoomMappingProfile : Profile
{
    public WatchRoomMappingProfile()
    {
        CreateMap<CreateWatchRoomRequest, WatchRoom>()
            .ForMember(dest => dest.ContentUrl,
                opt => opt.MapFrom(src => src.ContentUrl.Trim()))
            .ForMember(dest => dest.Title,
                opt => opt.MapFrom(src => string.IsNullOrWhiteSpace(src.Title) ? null : src.Title.Trim()))
            .ForMember(dest => dest.Code, opt => opt.Ignore())
            .ForMember(dest => dest.HostUserId, opt => opt.Ignore())
            .ForMember(dest => dest.Platform, opt => opt.Ignore())
            .ForMember(dest => dest.ContentId, opt => opt.Ignore())
            .ForMember(dest => dest.Members, opt => opt.Ignore());

        CreateMap<WatchRoom, GetWatchRoomByIdResponse>();
        CreateMap<WatchRoom, GetAllWatchRoomsResponse>()
            .ForMember(dest => dest.MemberCount, opt => opt.MapFrom(src => src.Members.Count));

        CreateMap<WatchRoomMember, WatchRoomMemberResponse>()
            .ForMember(dest => dest.UserName, opt => opt.Ignore());
    }
}
