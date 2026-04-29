using Application.Chats.Commands;
using Application.Chats.Dtos;
using AutoMapper;
using Domain.Entities;

namespace Application.Common.Mappings;

public sealed class ChatMappingProfile : Profile
{
    public ChatMappingProfile()
    {
        CreateMap<ChatParticipant, ChatParticipantDto>();
        CreateMap<Message, MessageDto>();
        CreateMap<Chat, ChatDto>()
            .ForMember(dest => dest.LastMessage,
                opt => opt.MapFrom(src => src.Messages
                    .OrderByDescending(m => m.CreatedAt)
                    .FirstOrDefault()));

        CreateMap<CreatePrivateChatCommand, Chat>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.IsGroup, opt => opt.MapFrom(_ => false))
            .ForMember(dest => dest.Participants, opt => opt.Ignore())
            .ForMember(dest => dest.Messages, opt => opt.Ignore());

        CreateMap<SendMessageCommand, Message>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.ChatId, opt => opt.MapFrom(src => src.Request.ChatId))
            .ForMember(dest => dest.Content, opt => opt.MapFrom(src => src.Request.Content))
            .ForMember(dest => dest.SenderId, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.IsRead, opt => opt.Ignore())
            .ForMember(dest => dest.Chat, opt => opt.Ignore());
    }
}
