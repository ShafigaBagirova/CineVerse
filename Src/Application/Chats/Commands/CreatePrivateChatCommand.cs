using Application.Chats.Dtos;
using Application.Common.Responses;
using MediatR;

namespace Application.Chats.Commands;

public sealed record CreatePrivateChatCommand(string ReceiverUserId) : IRequest<BaseResponse<ChatDto>>;
