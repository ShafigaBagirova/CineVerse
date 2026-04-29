using Application.Common.Responses;
using MediatR;

namespace Application.Chats.Commands;

public sealed record MarkMessagesAsReadCommand(int ChatId) : IRequest<BaseResponse>;
