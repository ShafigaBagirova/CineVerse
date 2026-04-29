using Application.Chats.Dtos;
using Application.Common.Responses;
using MediatR;

namespace Application.Chats.Commands;

public sealed class SendMessageRequest
{
    public int ChatId { get; set; }
    public string Content { get; set; } = string.Empty;
}

public sealed record SendMessageCommand(SendMessageRequest Request) : IRequest<BaseResponse<MessageDto>>;
