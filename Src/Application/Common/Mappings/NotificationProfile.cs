using Application.Notifications.Dtos;
using AutoMapper;
using Domain.Entities;

namespace Application.Common.Mappings;

public class NotificationProfile:Profile
{
    public NotificationProfile()
    {
        CreateMap<Notification, GetMyNotificationsResponse>()
                    .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type.ToString()));
    }
}
