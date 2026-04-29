using Application.Chats.Dtos;
using Application.Common.Responses;
using MediatR;

namespace Application.Chats.Queries;

public sealed record GetChatMessagesQuery(int ChatId) : IRequest<BaseResponse<List<MessageDto>>>;
